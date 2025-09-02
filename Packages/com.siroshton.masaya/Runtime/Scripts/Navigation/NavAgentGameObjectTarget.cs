
using Siroshton.Masaya.Core;
using UnityEngine;
using UnityEngine.AI;

namespace Siroshton.Masaya.Navigation
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class NavAgentGameObjectTarget : MonoBehaviour
    {
        [SerializeField] private GameObject _target;
        [SerializeField] private float _randomRadiusAroundTarget;

        [Tooltip("Stop and rest every so often. Set to zero for no rests.")]
        [SerializeField] private float _restFrequencyMin = 0;
        [Tooltip("Stop and rest every so often. Set to zero for no rests.")]
        [SerializeField] private float _restFrequencyMax = 0;

        [Tooltip("Rest at least this long when resting.")]
        [SerializeField] private float _restDurationMin = 1;
        [Tooltip("Rest no longer than this much when resting.")]
        [SerializeField] private float _restDurationMax = 2;

        [Tooltip("Rate to update the nav agent destination to the target objects location.")]
        [SerializeField] private float _updateInterval = 0.5f;

        private NavMeshAgent _agent;
        private Entity.Entity _entity;
        private float _timeSinceUpdate;

        private bool _isResting;
        private float _timeSinceRest;
        private float _restInterval;
        private float _timeResting;
        private float _restDuration;

        public GameObject target { get => _target; set => _target = value; }

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            _entity = GetComponent<Entity.Entity>();
            _timeSinceUpdate = _updateInterval; // Get a destintation as soon as possible.
            _restInterval = Random.Range(_restFrequencyMin, _restFrequencyMax);
        }

        private void Update()
        {
            if( _target == null ) return;

            if( _isResting )
            {
                _timeResting += GameState.deltaTime;
                if( _timeResting >= _restDuration )
                {
                    _isResting = false;
                    _timeSinceRest = 0;
                    _restInterval = Random.Range(_restFrequencyMin, _restFrequencyMax);
                    SetDestination();
                }
                return;
            }

            if(_restFrequencyMin > 0 && _restFrequencyMax > 0)
            {
                _timeSinceRest += GameState.deltaTime;
                if(_timeSinceRest >= _restInterval)
                {
                    _timeResting = 0;
                    _restDuration = Random.Range(_restDurationMin, _restDurationMax);
                    _isResting = true;
                    if( _agent.enabled ) _agent.ResetPath();
                    return;
                }
            }

            _timeSinceUpdate += GameState.deltaTime;
            if ( _timeSinceUpdate >= _updateInterval )
            {
                SetDestination();
            }
        }

        private void SetDestination()
        {
            if( _agent.enabled )
            {
                if (_entity != null)
                {
                    _agent.speed = _entity.speed;
                }

                if (_randomRadiusAroundTarget > 0 && (_target.transform.position - transform.position).sqrMagnitude > (_randomRadiusAroundTarget * _randomRadiusAroundTarget))
                {
                    _agent.SetDestination(_target.transform.position + new Vector3(Random.Range(-_randomRadiusAroundTarget, _randomRadiusAroundTarget), 0, Random.Range(-_randomRadiusAroundTarget, _randomRadiusAroundTarget)));
                }
                else
                {
                    _agent.SetDestination(_target.transform.position);
                }
            }

            _timeSinceUpdate = 0;
        }
    }

}