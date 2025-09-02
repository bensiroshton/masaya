using Siroshton.Masaya.Component;
using Siroshton.Masaya.Core;
using Siroshton.Masaya.Math;
using Siroshton.Masaya.Motion;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Siroshton.Masaya.Effect
{

    public class Splat : MonoBehaviour
    {
        public enum PlacementBehaviour
        {
            PlaceAsChild,
            PlaceInWorld,
            PlaceInWorldAndDestroyOnPlayerDeath
        }

        [SerializeField] private GameObject _prefab;
        [SerializeField] private float _startSize = 0;
        [SerializeField] private IntervalFloat _sizeRange = new IntervalFloat(0.5f, 1.0f);
        [SerializeField] private IntervalFloat _sizingDuration = new IntervalFloat(1.0f, 3.0f);
        [SerializeField] private IntervalFloat _distanceRange = new IntervalFloat(0, 0.5f);
        [SerializeField] private IntervalInt _countRange = new IntervalInt(1, 1);
        [SerializeField] private float _yOffset = 0.05f;
        [SerializeField] private PlacementBehaviour _placementBehaviour = PlacementBehaviour.PlaceAsChild;

        private GameObject _root;

        public void PlaceSplat()
        {
            CreateRootSplat();

            int count = _countRange.random;
            
            for (int i=0;i<count;i++)
            {
                Vector3 position = Vector3.Lerp(_distanceRange.a * Random.onUnitSphere, _distanceRange.b * Random.onUnitSphere, Random.value);
                position.y = _yOffset;
                AddSplatInternal(position);
            }
        }

        public void AddSplat(Vector3 worldPosition)
        {
            CreateRootSplat();

            worldPosition = _root.transform.worldToLocalMatrix * worldPosition;
            worldPosition.y = 0;
            AddSplatInternal(worldPosition);
        }

        private void CreateRootSplat()
        {
            if (_root != null) return;

            _root = new GameObject("Splat");
            if (_placementBehaviour == PlacementBehaviour.PlaceAsChild)
            {
                _root.transform.SetParent(transform);
                _root.transform.localPosition = new Vector3(0, _yOffset, 0);
                _root.transform.localRotation = Quaternion.identity;
            }
            else
            {
                _root.transform.position = new Vector3(transform.position.x, _yOffset, transform.position.z);
                _root.transform.rotation = transform.rotation;
            }

            _root.transform.localScale = Vector3.one;

            if (_placementBehaviour == PlacementBehaviour.PlaceInWorldAndDestroyOnPlayerDeath)
            {
                GameEvents ge = _root.AddComponent<GameEvents>();
                DestroyObjects d = _root.AddComponent<DestroyObjects>();
                d.objects = new GameObject[] { _root.gameObject };
                ge.onPlayerRevived = new UnityEngine.Events.UnityEvent();
                ge.onPlayerRevived.AddListener(d.Destroy);
            }
        }

        private void AddSplatInternal(Vector3 position)
        {
            // Create prefab
            GameObject o = GameObject.Instantiate(_prefab);
            o.transform.SetParent(_root.transform);
            o.transform.localPosition = position;
            o.transform.localRotation = Quaternion.Euler(new Vector3(0, Random.Range(0.0f, 360.0f), 0));

            // setup the ScaleTo transform
            float duration = _sizingDuration.random;
            float size = _sizeRange.random;
            Vector3 startSize = new Vector3(_startSize, 1.0f, _startSize);
            Vector3 endSize = new Vector3(size, 1.0f, size);

            if (duration <= 0)
            {
                o.transform.localScale = endSize;
            }
            else
            {
                o.transform.localScale = startSize;

                ScaleTo scaleTo = o.AddComponent<ScaleTo>();
                scaleTo.targetScale = endSize;
                scaleTo.duration = duration;
                scaleTo.startImmediately = true;
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            UnityEditor.Handles.color = Color.grey;
            UnityEditor.Handles.DrawWireDisc(transform.position, Vector3.up, _distanceRange.a);
            UnityEditor.Handles.DrawWireDisc(transform.position, Vector3.up, _distanceRange.b);
        }
#endif

    }

}