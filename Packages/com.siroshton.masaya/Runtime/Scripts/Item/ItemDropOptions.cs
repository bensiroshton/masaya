using Siroshton.Masaya.Math;
using System;
using UnityEngine;

namespace Siroshton.Masaya.Item
{
    [Serializable]
    public class ItemDropOptions
    {
        [Tooltip("Increase the chance something from this group will drop, 0=No Bonus, 100=Gauranteed drop.")]
        [SerializeField, Range(0, 100)] private int _chanceBonus = 0;
        [Tooltip("Percentage of the base bonus applied, 0 = None, 1 = 100%.")]
        [SerializeField, Range(0, 1)] private float _baseChanceBonus = 1;
        [Tooltip("Add this modifier to the min/max number of items dropped.")]
        [SerializeField] private int _itemCountModifier = 0;
        [Tooltip("When turned off the random chance to drop items is disabled.")]
        [SerializeField] private bool _attemptChanceDrops = false;
        [SerializeField] private bool _useGroupGauranteeRules = true;

        [Tooltip("Drop items at this radius around dropper.")]
        [SerializeField, Range(0, 10)] private float _dropRadius = 0.25f;

        [Tooltip("When set the drop location will be checked to see if we are dropping to a nav mesh surface to try and find a suitable location.")]
        [SerializeField] private bool _sampleNavMesh = true;

        [Tooltip("The chance something from the Might Drop list will drop, the chance bonus above does not get added to the roll for these.")]
        [SerializeField, Range(0, 100)] private int _mightDropChance = 0;
        [Tooltip("The number of items from that Might Drop list that will drop on a succesfull roll.")]
        [SerializeField] private IntervalInt _mightDropCount = new IntervalInt(1, 1);

        [Tooltip("These items might drop based on a chance roll.")]
        [SerializeField] private GameObject[] _mightDrop;

        [Tooltip("Everything in the always drop list will be dropped regardless of chance roll.  If you want something dropped multiple times then add it multiple times to the list.")]
        [SerializeField] private GameObject[] _alwaysDrop;

        public int chanceBonus => _chanceBonus;
        public float baseChanceBonus => _baseChanceBonus;
        public int itemCountModifier => _itemCountModifier;
        public bool attemptChanceDrops => _attemptChanceDrops;
        public bool useGroupGauranteeRules => _useGroupGauranteeRules;
        public float dropRadius => _dropRadius;
        public bool sampleNavMesh => _sampleNavMesh;

        public int mightDropChance { get => _mightDropChance; }
        public IntervalInt mightDropCount { get => _mightDropCount; }
        public GameObject[] mightDrop { get => _mightDrop; }
        
        public GameObject[] alwaysDrop { get => _alwaysDrop; }
    }

}