using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Siroshton.Masaya.Entity
{

    public class EntityDestroyedTracker : MonoBehaviour
    {
        public enum DeathEvent
        {
            OnDestroyed,
            OnKilled
        }

        [SerializeField] private DeathEvent _deathEvent = DeathEvent.OnDestroyed;
        [SerializeField] private Entity[] _entities;
        [SerializeField] private UnityEvent _onAllEntitiesDestroyed;

        private HashSet<GameObject> _entityHash = new HashSet<GameObject>();

        public DeathEvent deathEvent { get => _deathEvent; set => _deathEvent = value; }
        public UnityEvent onAllEntitiesDestroyed { get => _onAllEntitiesDestroyed; set => _onAllEntitiesDestroyed = value; }

        private void Awake()
        {
            AddEntities();
        }

        public void Reset()
        {
            foreach(GameObject o in _entityHash)
            {
                if( o == null ) continue;
                ClearEntityEvents(o.GetComponent<Entity>());
            }

            if( _entities != null )
            {
                for (int i = 0; i < _entities.Length; i++)
                {
                    ClearEntityEvents(_entities[i]);
                }
            }

            _entityHash.Clear();
            AddEntities();
        }

        private void ClearEntityEvents(Entity e)
        {
            if( e == null ) return;

            e.onDestroyed.RemoveListener(OnEntityDestroyed);
            e.onKilled.RemoveListener(OnEntityDestroyed);
        }

        private void AddEntities()
        {
            if (_entities == null) return;

            for (int i = 0; i < _entities.Length; i++)
            {
                AddEntity(_entities[i]?.gameObject);
            }
        }

        public void AddEntity(GameObject obj)
        {
            Entity e = obj?.GetComponent<Entity>();
            if (e == null)
            {
                Debug.LogError("Object is null or does not contain an Entity component.");
                return;
            }

            if (e.isDead)
            {
                Debug.LogWarning("Entity is already dead.");
                return;
            }

#if UNITY_EDITOR
            //Debug.Log($"Added {e.name} ({deathEvent})");
#endif

            _entityHash.Add(obj);

            if( deathEvent == DeathEvent.OnDestroyed ) e.onDestroyed.AddListener(OnEntityDestroyed);
            else e.onKilled.AddListener(OnEntityDestroyed);
        }

        private void OnEntityDestroyed(GameObject obj)
        {
#if UNITY_EDITOR
            //Debug.Log($"Destroyed {obj.name} ({deathEvent})");
#endif
            _entityHash.Remove(obj);

            if (_entityHash.Count == 0 && _onAllEntitiesDestroyed != null) _onAllEntitiesDestroyed.Invoke();
        }
    }

}