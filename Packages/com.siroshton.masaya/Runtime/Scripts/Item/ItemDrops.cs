using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Siroshton.Masaya.Motion;
using Siroshton.Masaya.Extension;
using UnityEngine.AI;

namespace Siroshton.Masaya.Item
{
    public class ItemDrops : MonoBehaviour
    {
        [SerializeField] private List<ItemDropGroup> _dropGroups = new List<ItemDropGroup>();
        [Tooltip("Minimum Y Offset to use, this helps reduce z-fighting for stacked cards.")]
        [SerializeField] private float _minYOffset = 0.01f;
        [Tooltip("Maximum Y Offset to use, this helps reduce z-fighting for stacked cards.")]
        [SerializeField] private float _maxYOffset = 0.05f;
        [SerializeField] private UnityEvent _onItemsDropped = new UnityEvent();

        private float _yOffset;

        private void Awake()
        {
            _yOffset = _minYOffset;
        }

        static public int GetChanceRoll()
        {
            return UnityEngine.Random.Range(1, 101);
        }

        public List<GameObject> TryDropItems(Vector3 atPosition, ItemDropOptions p)
        {
            List<GameObject> allItems = new List<GameObject>();

            if (p.attemptChanceDrops)
            {
                //Debug.Log("Trying for drops...");

                for (int i = 0; i < _dropGroups.Count; i++)
                {
                    ItemDropGroup group = _dropGroups[i];
                    if( group.chance <= 0 ) continue;

                    int roll = GetChanceRoll();
                    int need = (int)((float)group.chance * p.baseChanceBonus) + p.chanceBonus;
                    bool success = roll <= need;
                    
                    if( p.useGroupGauranteeRules && group.chance >= group.disableGauranteeWhenChanceIsBelow )
                    {
                        if( !success ) group.failedDrops++;
                        success |= group.gauranteeDropAfterXFails > 0 && group.failedDrops >= group.gauranteeDropAfterXFails;
                    }

                    //Debug.Log($"Group [{i}], Rolled: {roll}, Needed <= {need} ({group.chance} + {p.chanceBonus} bonus), Success: {success}, Failed Drops: {group.failedDrops} of {group.gauranteeDropAfterXFails}");

                    if (!success) continue;

                    allItems.AddRange(CollectItems(group.items, group.dropCount.random + p.itemCountModifier));
                    group.failedDrops = 0;
                    group.chance = Mathf.Max(group.chance - group.chanceReductionAfterDrop, group.mininumChanceAfterReductions);
                }
            }

            if ( p.mightDrop != null )
            {
                int roll = GetChanceRoll();
                if( roll <= p.mightDropChance )
                {
                    allItems.AddRange(CollectItems(p.mightDrop, p.mightDropCount.random));
                }
            }

            if (p.alwaysDrop != null)
            {
                foreach (GameObject o in p.alwaysDrop)
                {
                    if (o != null) allItems.Add(o);
                }
            }

            if (allItems.Count > 0)
            {
                _onItemsDropped.Invoke();
                allItems.Shuffle<GameObject>();
                return InstantiateDrops(p, atPosition, allItems);
            }

            return null;
        }

        /// <summary>
        /// Choose randomly [count] items from the list.
        /// </summary>
        private GameObject[] CollectItems(GameObject[] items, int count)
        {
            GameObject[] allItems = new GameObject[count];

            for (int i=0;i<count;i++)
            {
                allItems[i] = items[Random.Range(0, items.Length)];
            }

            return allItems;
        }

        private List<GameObject> InstantiateDrops(ItemDropOptions p, Vector3 atPosition, List<GameObject> items)
        {
            float yStep = (_maxYOffset - _minYOffset) / 10.0f;
            if (_yOffset < _minYOffset || _yOffset > _maxYOffset) _yOffset = _minYOffset;

            float dropAngle = Random.Range(0, Mathf.PI * 2.0f);
            float dropAngleStep = Mathf.PI * 2.0f / (float)items.Count;

            List<GameObject> drops = new List<GameObject>();
            for (int i = 0; i < items.Count; i++)
            {
                Vector3 position = atPosition + Vector3.up * _yOffset;
                GameObject o = GameObject.Instantiate(
                        items[i],
                        position,
                        Quaternion.AngleAxis(UnityEngine.Random.Range(0.0f, 365.0f), Vector3.up));

                DestroyIfDropOnPlayerDeath d = o.GetComponent<DestroyIfDropOnPlayerDeath>();
                if (d != null) d.isDrop = true;

                // Setup a Jump animation for the drop
                JumpTo jump = o.AddComponent<JumpTo>();
                float angle = dropAngle;
                dropAngle += dropAngleStep;
                Vector3 dir = new Vector3(Mathf.Sin(angle), 0, Mathf.Cos(angle));
                jump.targetPosition = position + dir * p.dropRadius;

                if ( p.sampleNavMesh )
                {
                    NavMeshHit hit;
                    if( NavMesh.SamplePosition(jump.targetPosition, out hit, 1, NavMesh.AllAreas) )
                    {
                        jump.targetPosition = hit.position;
                    }
                    else if( NavMesh.FindClosestEdge(jump.targetPosition, out hit, NavMesh.AllAreas) )
                    {
                       jump.targetPosition = hit.position;
                    } 
                    else
                    {
                        // Unable to Place drop, skip it
                        Debug.LogWarning("Unable to drop item, no where to put it.");
                        Destroy(o);
                        continue;
                    }
                }

                jump.targetPosition += Vector3.up * _yOffset;;
                jump.duration = 0.75f;
                jump.StartJump();

                drops.Add(o);

                _yOffset += yStep;
                if (_yOffset > _maxYOffset) _yOffset = _minYOffset;
            }

            return drops;

        }

    }
}
