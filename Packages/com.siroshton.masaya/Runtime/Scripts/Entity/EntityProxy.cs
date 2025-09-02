using Siroshton.Masaya.Environment;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace Siroshton.Masaya.Entity
{

    public class EntityProxy : MonoBehaviour, IEmitterObjectReference
    {
        [SerializeField] private Entity _entity;
        [SerializeField] private UnityEvent<GameObject> _onEntityDestroyed;
        [SerializeField] private UnityEvent<GameObject> _onEntityKilled;

        public Entity entity
        {
            get => _entity;
            set
            {
                if( _entity != null )
                {
                    _entity.onDestroyed.RemoveListener(OnEntityDestroyed);
                    _entity.onKilled.RemoveListener(OnEntityKilled);
                }

                _entity = value;

                if (_entity != null)
                {
                    _entity.onDestroyed.AddListener(OnEntityDestroyed);
                    _entity.onKilled.AddListener(OnEntityKilled);
                }

            }
        }

        private void OnEntityKilled(GameObject obj)
        {
            if (_onEntityKilled != null) _onEntityKilled.Invoke(obj);
        }

        private void OnEntityDestroyed(GameObject obj)
        {
            if(_onEntityDestroyed != null ) _onEntityDestroyed.Invoke(obj);
        }

        public void AddReference(GameObject o)
        {
            Entity e = o.GetComponent<Entity>();
            if( e != null ) entity = e;
        }
    }

}