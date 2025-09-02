using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Siroshton.Masaya.Entity
{

    public class EntityDeathTracker : MonoBehaviour
    {
        [SerializeField] private UnityEvent _onAllEntitiesKilled;

        private HashSet<GameObject> _entities = new HashSet<GameObject>();

        public UnityEvent onAllEntitiesKilled { get => _onAllEntitiesKilled; set => _onAllEntitiesKilled = value; }

        public void Reset()
        {
            _entities.Clear();
        }

        public void AddEntity(GameObject obj)
        {
            Entity e = obj.GetComponent<Entity>();
            if( e == null )
            {
                Debug.LogError("Object does not contain an Entity component.");
                return;
            }

            if( e.isDead )
            {
                Debug.LogWarning("Entity is already dead.");
                return;
            }

            _entities.Add(obj);
            e.onKilled.AddListener(OnEntityKilled);
        }

        private void OnEntityKilled(GameObject obj)
        {
            _entities.Remove(obj);

            if( _entities.Count == 0 && _onAllEntitiesKilled != null ) _onAllEntitiesKilled.Invoke();
        }
    }

}