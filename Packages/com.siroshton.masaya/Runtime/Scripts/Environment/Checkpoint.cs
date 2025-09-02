using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace Siroshton.Masaya.Environment
{
    public class Checkpoint : MonoBehaviour
    {
        [SerializeField] private Transform _spawnPoint;
        [SerializeField] private UnityEvent _onCheckpointSet;
        [SerializeField] private UnityEvent _onCheckpointUnSet;

        static private Checkpoint _activeCheckpoint;
        static public Checkpoint activeCheckpoint { get => _activeCheckpoint; }

        public Transform spawnPoint { get => _spawnPoint; set => _spawnPoint = value; }

        public void SetAsActive()
        {
            _activeCheckpoint = this;
        }

        private void Start()
        {
            if( _spawnPoint == null ) _spawnPoint = transform;
        }

        private void OnTriggerEnter(Collider other)
        {
            if( other.gameObject == Player.Player.instance.gameObject )
            {
                Checkpoint[] checks = GameObject.FindObjectsByType<Checkpoint>(FindObjectsSortMode.None);
                for(int i=0; i<checks.Length; i++)
                {
                    if( checks[i] != this ) checks[i]._onCheckpointUnSet?.Invoke();
                }

                _activeCheckpoint = this;
                _onCheckpointSet?.Invoke();
            }
        }

#if UNITY_EDITOR
        SphereCollider _cachedCollider;
        private void OnDrawGizmos()
        {
            if ( _spawnPoint != null )
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawRay(_spawnPoint.position, _spawnPoint.forward);
                Gizmos.DrawWireSphere(_spawnPoint.position, 0.1f);
            }

            if( _cachedCollider == null ) _cachedCollider = GetComponentInChildren<SphereCollider>();
            if (_cachedCollider != null)
            {
                Handles.color = Color.cyan;
                Handles.DrawWireDisc(_cachedCollider.bounds.center, Vector3.up, _cachedCollider.radius);
            }
        }
#endif

    }
}