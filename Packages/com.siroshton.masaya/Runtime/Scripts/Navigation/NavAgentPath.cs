using Siroshton.Masaya.Core;
using Siroshton.Masaya.Math;
using Siroshton.Masaya.Path;
using Siroshton.Masaya.Util;
using System;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using static Siroshton.Masaya.Core.Types;

namespace Siroshton.Masaya.Navigation
{

    [RequireComponent(typeof(NavMeshAgent))]
    public class NavAgentPath : MonoBehaviour
    {
        [Serializable]
        public class Events
        {
            [Tooltip("The parameter will be true when the destination has been reached or false otherwise")]
            [SerializeField] public UnityEvent<bool> onPathFinished;
            [SerializeField] public UnityEvent onNewPathSet;
            [SerializeField] public UnityEvent onNewPathFailed;
        }

        [Serializable]
        public class PathProviderEvents
        {
            [SerializeField] public UnityEvent onStartingPath;
            [SerializeField] public UnityEvent onLeavingPath;
        }

        [Serializable]
        public class PathProvider
        {
            [SerializeField, HideInInspector] public string name;

            [Tooltip("If the Refresh Interval is less than zero then the provider will only be called once to a get a position when that provider is selected, otherwise it will update at the set interval.")]
            [SerializeField] public IntervalFloat refreshInterval = new IntervalFloat(-1, -1);
            [SerializeField] public TargetProvider provider;
            [Tooltip("When Auto Select Path is enabled, this duration will be added to the Auto Switch Interval time before a new provider is selected.")]
            [SerializeField] public IntervalFloat additionalDuration = new IntervalFloat();
            [SerializeField] public PathProviderEvents events = new PathProviderEvents();

            [HideInInspector] public float timeSinceRefresh;
            [HideInInspector] public float currentRefreshInterval;
        }

        [Tooltip("When set the NavMeshAgent will be given new paths as soon as the current one is finished.")]
        [SerializeField] private bool _autoSelectPath = true;
        [Tooltip("The path index to start with, if the value is not valid then Start Path is ignored.  Set to -1 to purposefully not use a start path.")]
        [SerializeField] private int _startPath = 0;
        [Tooltip("When Auto Select Path is enabled, a new provider will be selected after the interval time has passed (random between A and B).")]
        [SerializeField] private IntervalFloat _autoSwitchInterval = new IntervalFloat();
        [SerializeField] private NextSelectionType _autoPathSelectionType = NextSelectionType.Incremental;
        [SerializeField] private PathProvider[] _pathProviders;
        [SerializeField] private Events _events;

        private NavMeshAgent _agent;
        private bool _hasPath;
        private int _pathIndex = -1;
        private PathProvider _currentProvider;
        private TargetProvider _lastTargetProvider;
        private float _timeOnPath;
        private float _nextInterval;
        private Vector3 _lastTarget;
        private Creature.Creature _creature;

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            _creature = GetComponentInParent<Creature.Creature>();
        }

        private void Start()
        {
            if( _startPath >=0 && _startPath < _pathProviders.Length)
            {
                SetDestination(_pathProviders[_startPath]);
            }
        }

        public void PauseAgentPath()
        {
            _agent.isStopped = true;
        }

        public void PlayAgentPath()
        {
            _agent.isStopped = false;
        }

        public void SetDestination()
        {
            if(_pathProviders != null )
            {
                SetDestination(SelectionUtil.SelectNext(_autoPathSelectionType, _pathIndex, _pathProviders.Length));
            }
        }

        public void SetDestination(int index)
        {
            if (_pathProviders == null) return;
            else if (_currentProvider != null && _pathProviders.Length == 1) return;

            if (index >= 0 && index < _pathProviders.Length)
            {
                _pathIndex = index;
                SetDestination(_pathProviders[_pathIndex]);
            }
        }

        public void SetDestinationFromRandomProvider()
        {
            int i = UnityEngine.Random.Range(0, _pathProviders.Length);

            SetDestination(_pathProviders[i]);
        }

        public void SetDestination(TargetProvider provider)
        {
            _currentProvider = null;
            SetDestination(provider, false, 0);
        }

        private void SetDestination(PathProvider provider)
        {
            if( provider.provider == null ) return;

            bool isChangingProvider = _currentProvider != provider;
            SetDestination(provider.provider, isChangingProvider, isChangingProvider ? 0 : _currentProvider.timeSinceRefresh);
            if( isChangingProvider )
            {
                _timeOnPath = 0;
                _currentProvider?.events.onLeavingPath?.Invoke();
                provider.events.onStartingPath?.Invoke();
                _nextInterval += provider.additionalDuration.random;
            }
            _currentProvider = provider;
            _currentProvider.timeSinceRefresh = 0;
            _currentProvider.currentRefreshInterval = _currentProvider.refreshInterval.random;
        }

        private void SetDestination(TargetProvider provider, bool triggerEvents, float timeSinceLastCall)
        {
            if (!(_agent.isOnNavMesh || _agent.isOnOffMeshLink) || !_agent.enabled || _agent.isStopped) return;

            Vector3 target;
            
            if (provider.GetTarget(transform, out target, timeSinceLastCall))
            {
                if (_agent.SetDestination(target))
                {
                    _lastTarget = target;

                    if ( provider != _lastTargetProvider )
                    {
                        _timeOnPath = 0;
                        _nextInterval = _autoSwitchInterval.random;
                        _lastTargetProvider = provider;
                    }
                    _hasPath = true;
                    if (triggerEvents) _events.onNewPathSet?.Invoke();
                }
                else
                {
                    _hasPath = false;
                    if (triggerEvents) _events.onNewPathFailed?.Invoke();
                }
            }
        }

        private void Update()
        {
            if(_creature != null && !_creature.isEngaged)
            {
                return;
            }

            if (_currentProvider != null && _currentProvider.currentRefreshInterval >= 0)
            {
                _currentProvider.timeSinceRefresh += GameState.deltaTime;
                if(_currentProvider.timeSinceRefresh >= _currentProvider.currentRefreshInterval)
                {
                    SetDestination(_currentProvider.provider, false, _currentProvider.timeSinceRefresh);
                    _currentProvider.timeSinceRefresh = 0;
                    _currentProvider.currentRefreshInterval = _currentProvider.refreshInterval.random;
                }
            }

            if ( _hasPath )
            {
                if( !_agent.hasPath && !_agent.pathPending )
                {
                    _hasPath = false;
                    _events.onPathFinished?.Invoke(true);
                }
                else if( _agent.pathStatus == NavMeshPathStatus.PathPartial )
                {
                    _hasPath = false;
                    _events.onPathFinished?.Invoke(false);
                }
            }

            if( _autoSelectPath )
            {
                _timeOnPath += GameState.deltaTime;
                if( _timeOnPath >= _nextInterval || !_hasPath )
                {
                    SetDestination();
                }
            }
        }

        private void OnValidate()
        {
            if(_pathProviders!=null)
            {
                for(int i=0;i< _pathProviders.Length;i++)
                {
                    _pathProviders[i].name = $"[{i}] {_pathProviders[i].provider?.ToString()}";
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            if(_currentProvider!=null)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawSphere(_lastTarget, 0.1f);
            }
        }

    }

}