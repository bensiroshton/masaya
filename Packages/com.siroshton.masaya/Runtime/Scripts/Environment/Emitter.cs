

using Siroshton.Masaya.Core;
using Siroshton.Masaya.Entity;
using Siroshton.Masaya.Math;
using Siroshton.Masaya.Motion;
using System.Reflection.Emit;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using UnityEngine.Events;

namespace Siroshton.Masaya.Environment
{

    public class Emitter : MonoBehaviour
    {
        public enum EmitterMode
        {
            Auto,
            Manual
        }

        [SerializeField] private EmitterMode _mode = EmitterMode.Auto;
        
        [Tooltip("If not set this objects transform will be used.")]
        [SerializeField] private Transform _spawnPoint;

        [Tooltip("Prefab to instantiate. Set 'prefabs' to None if using this field.")]
        [SerializeField] private GameObject _prefab;

        [Tooltip("Prefab Insantiator to instantiate prefabs. Set 'prefab' to None if using this field.")]
        [SerializeField] private PrefabInstantiator _prefabs;

        [Tooltip("Give emitted objects references to other objects, objects need to implement IEmitterObjectReference to use.")]
        [SerializeField] private GameObject[] _references;

        [Tooltip("When true the interval range will be ignored and the first emission will be immediate.")]
        [SerializeField] private bool _emitImmediatelyOnStart = true;
        [SerializeField] private IntervalFloat _intervalRange;

        [Tooltip("Max emissions ever generated.  Set to < 0 to disable the check.  0 disables the emitter.")]
        [SerializeField] private int _maxEmissions = -1;

        [SerializeField] private UnityEvent<GameObject> _onObjectEmitted = new UnityEvent<GameObject>();

        private float _timeSinceEmission;
        private int _emissionCount = 0;
        private float _nextInterval;

        public EmitterMode mode { get => _mode; set => _mode = value; }
        public GameObject prefab { get => _prefab; set => _prefab = value; }
        public IntervalFloat interval { get => _intervalRange; set => _intervalRange = value; }
        public int maxEmissions { get => _maxEmissions; set => _maxEmissions = value; }
        public int emissionCount { get => _emissionCount; }
        public Vector3 spawnPosition { get => _spawnPoint != null ? _spawnPoint.position : transform.position; }
        public Quaternion spawnRotation { get => _spawnPoint != null ? _spawnPoint.rotation : transform.rotation; }

        virtual protected void OnObjectEmitted(GameObject o) {}
        virtual protected bool isEmissionOk { get => true; }

        virtual public void ResetEmitter()
        {
            ResetEmissionCount();
        }

        public void ResetEmissionCount()
        {
            _emissionCount = 0;
        }

        protected void Awake()
        {
        }

        protected void Start()
        {
            _nextInterval = _intervalRange.random;
            if(_emitImmediatelyOnStart ) _timeSinceEmission = _nextInterval; // emit first wave immediately
        }

        protected void Update()
        {
            if(_mode == EmitterMode.Auto)
            {
                _timeSinceEmission += GameState.deltaTime;
                if (_timeSinceEmission < _nextInterval) return;

                EmitObject();
            }
        }

        public void EmitObjects(int count = 1)
        {
            EmitObject(spawnPosition, spawnRotation);
        }

        public GameObject EmitObject()
        {
            return EmitObject(spawnPosition, spawnRotation);
        }

        public GameObject EmitObject(Vector3 position, Quaternion rotation)
        {
            if (!isEmissionOk) return null;

            if (_maxEmissions == 0) return null;
            else if (_maxEmissions > 0 && _emissionCount >= _maxEmissions) return null;

            GameObject obj = null;
            if (_prefab != null) obj = GameObject.Instantiate(_prefab, position, rotation);
            else if (_prefabs != null)
            {
                obj = _prefabs.Instantiate(position, rotation);
            }

            if (obj != null)
            {
                if(_references != null )
                {
                    IEmitterObjectReference objRef = obj.GetComponent<IEmitterObjectReference>();
                    if( objRef != null )
                    {
                        for(int i=0;i< _references.Length;i++)
                        {
                            objRef.AddReference(_references[i]);
                        }
                    }
                }

                OnObjectEmitted(obj);
                _onObjectEmitted.Invoke(obj);
                _emissionCount++;
            }

            _timeSinceEmission = 0;
            _nextInterval = _intervalRange.random;
            return obj;
        }

        protected void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(transform.position, new Vector3(0.2f, 0.2f, 0.2f));
        }
    }

}
