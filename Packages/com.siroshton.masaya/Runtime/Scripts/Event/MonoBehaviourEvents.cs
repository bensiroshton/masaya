using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Events;

namespace Siroshton.Masaya.Event
{

    public class MonoBehaviourEvents : MonoBehaviour
    {
        [SerializeField] private UnityEvent _onAwake;
        [SerializeField] private UnityEvent _onStart;
        [SerializeField] private UnityEvent<Collider> _onTriggerEnter;
        [SerializeField] private UnityEvent<Collider> _onTriggerExit;
        [SerializeField] private UnityEvent<int> _onAnimatorIK;

        public UnityEvent onAwake { get => _onAwake; set => _onAwake = value; }
        public UnityEvent onStart { get => _onStart; set => _onStart = value; }
        public UnityEvent<Collider> onTriggerEnter { get => _onTriggerEnter; set => _onTriggerEnter = value; }
        public UnityEvent<Collider> onTriggerExit { get => _onTriggerExit; set => _onTriggerExit = value; }
        public UnityEvent<int> onAnimatorIK { get => _onAnimatorIK; set => _onAnimatorIK = value; }

        public void Awake()
        {
            if (_onAwake != null) _onAwake.Invoke();
        }

        public void Start()
        {
            if (_onStart != null) _onStart.Invoke();
        }

        public void OnTriggerEnter(Collider other)
        {
            if (_onTriggerEnter != null) _onTriggerEnter.Invoke(other);
        }

        public void OnTriggerExit(Collider other)
        {
            if (_onTriggerExit != null) _onTriggerExit.Invoke(other);
        }

        public void OnAnimatorIK(int layerIndex)
        {
            if (_onAnimatorIK != null) _onAnimatorIK.Invoke(layerIndex);
        }
    }

}