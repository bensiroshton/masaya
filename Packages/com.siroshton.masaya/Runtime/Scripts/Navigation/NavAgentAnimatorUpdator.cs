using Siroshton.Masaya.Core;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.AI;

namespace Siroshton.Masaya.Navigation
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class NavAgentAnimatorUpdator : MonoBehaviour
    {
        [SerializeField] private Animator _animator;
        [SerializeField] private string _speedProperty = "speed";
        [SerializeField] private float _multiplier = 1;

        private NavMeshAgent _agent;
        private int _speedId;
        private Vector3 _lastPosition;

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            if( _animator == null ) _animator = GetComponent<Animator>();
            _speedId = Animator.StringToHash(_speedProperty);
        }

        private void Update()
        {
            if (_agent.speed == 0) return;

            float distance = (transform.position - _lastPosition).magnitude;
            float speed = distance / GameState.deltaTime;

            if (_agent.speed > 0) _animator.SetFloat(_speedId, speed * _multiplier / _agent.speed);
            else _animator.SetFloat(_speedId, 0);

            _lastPosition = transform.position;
        }
    }
}