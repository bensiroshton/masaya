using Siroshton.Masaya.Audio;
using Siroshton.Masaya.Core;
using Siroshton.Masaya.Math;
using Siroshton.Masaya.Weapon;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

namespace Siroshton.Masaya.Creature
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class Bladderwort : Entity.Entity
    {
        [SerializeField] private Transform _modelTransform;

        [SerializeField] private float _rotationSpeed = 360;
        [SerializeField] private IntervalFloat _moveInterval = new IntervalFloat(0.5f, 2.0f);
        [SerializeField] private float _targetRadius = 2;

        [SerializeField] private Gun _gun;
        [SerializeField] private float _fireInterval = 3.0f;
        [SerializeField] private float _burstInterval = 0.5f;
        [SerializeField] private int _burstCount = 5;
        [SerializeField] private UnityEvent _onBurstsStarted = new UnityEvent();
        [SerializeField] private UnityEvent _onBurst = new UnityEvent();
        [SerializeField] private UnityEvent _onBurstsFinished = new UnityEvent();

        [SerializeField] private float _warningLightIntensity = 1;
        [SerializeField] private float _warningTime = 0.75f;
        [SerializeField] private UnityEngine.Light _warningLight;
        [SerializeField] private float _warningLightReductionSpeed = 0.5f;
        [SerializeField] private UnityEvent _onChargeStarted = new UnityEvent();

        [SerializeField] private float _shootLayerWeightReductionSpeed = 3;
        [SerializeField] private float _hurtLayerWeightReductionSpeed = 3;

        // moving
        private bool _isMoving;
        private float _timeSinceMove;
        private float _nextMoveInterval;
        private float _currentRotationSpeed;
        private float _rotationSpeedTarget;
        private float _rotationDirection = 1;
        private AudioSourceController _moveAudio;

        // firing
        private float _timeSinceFire;
        private float _timeSinceBurst;
        private int _burstsRemaining;
        private float _timeSinceWarning;
        private bool _isWarning;

        // animation
        private struct AnimProperties
        {
            public Animator animator;
            public int shootLayer;
            public float shootWeight;
            public int hurtLayer;
            public float hurtWeight;
        }

        AnimProperties _anim;

        protected new void Awake()
        {
            base.Awake();
            _anim.animator = GetComponentInChildren<Animator>();
            _anim.shootLayer = _anim.animator.GetLayerIndex("Shoot");
            _anim.hurtLayer = _anim.animator.GetLayerIndex("Hurt");

            _moveAudio = GetComponent<AudioSourceController>();

            _timeSinceMove = _moveInterval.random;
            _nextMoveInterval = _moveInterval.random;
            _warningLight.intensity = 0;
        }

        protected new void Update()
        {
            base.Update();

            // Handle Moving
            _timeSinceMove += GameState.deltaTime;
            if( _isMoving )
            {
                _isMoving = agent.hasPath || agent.pathPending;
                if( !_isMoving )
                {
                    _timeSinceMove = 0;
                    _nextMoveInterval = _moveInterval.random;
                    _rotationSpeedTarget = 0;
                }
            }
            else if( _timeSinceMove >= _nextMoveInterval)
            {
                Vector3 randomPos = Player.Player.instance.transform.position + new Vector3(Random.Range(-_targetRadius, _targetRadius), 0, Random.Range(-_targetRadius, _targetRadius));
                NavMeshHit hit;
                if( NavMesh.SamplePosition(randomPos, out hit, _targetRadius, NavMesh.AllAreas) )
                {
                    agent.SetDestination(hit.position);

                    _isMoving = true;
                    _rotationSpeedTarget = _rotationSpeed;
                    _rotationDirection = -_rotationDirection;
                }
            }

            if(_rotationSpeed != 0)
            {
                _currentRotationSpeed = Mathf.MoveTowards(_currentRotationSpeed, _rotationSpeedTarget, _rotationSpeed * GameState.deltaTime);
                _modelTransform.rotation *= Quaternion.AngleAxis(_currentRotationSpeed * GameState.deltaTime * _rotationDirection, Vector3.up);
            }

            _moveAudio.volumeTarget = _currentRotationSpeed / _rotationSpeed;

            // Constanty Reduce Warning Light
            _warningLight.intensity -= _warningLightReductionSpeed * GameState.deltaTime;
            if( _warningLight.intensity < 0 ) _warningLight.intensity = 0;

            // Handle Firing
            _timeSinceFire += GameState.deltaTime;
            _timeSinceBurst += GameState.deltaTime;

            if( _isWarning )
            {
                _timeSinceWarning += GameState.deltaTime;

                float pos = _timeSinceWarning / _warningTime;
                if ( _timeSinceWarning >= _warningTime )
                {
                    // Start Bursts
                    pos = 1;
                    _burstsRemaining = _burstCount;
                    _onBurstsStarted.Invoke();
                    _isWarning = false;
                }

                _warningLight.intensity = Mathf.Lerp(0, _warningLightIntensity, pos);
            }
            else if (_burstsRemaining > 0)
            {
                if (_timeSinceBurst > _burstInterval)
                {
                    _onBurst.Invoke();
                    _gun.Trigger();
                    _timeSinceBurst = 0;
                    _timeSinceFire = 0;
                    _burstsRemaining--;
                    _anim.shootWeight = 1;

                    if (_burstsRemaining==0) _onBurstsFinished.Invoke();
                }
            }
            else if( _timeSinceFire >= _fireInterval )
            {
                _isWarning = true;
                _timeSinceWarning = 0;
                if(_onChargeStarted != null ) _onChargeStarted.Invoke();
            }

            // Update Animation Layers
            _anim.animator.SetLayerWeight(_anim.shootLayer, _anim.shootWeight);
            _anim.shootWeight -= _shootLayerWeightReductionSpeed * GameState.deltaTime;
            if( _anim.shootWeight < 0 ) _anim.shootWeight = 0;

            _anim.animator.SetLayerWeight(_anim.hurtLayer, _anim.hurtWeight);
            _anim.hurtWeight -= _hurtLayerWeightReductionSpeed * GameState.deltaTime;
            if (_anim.hurtWeight < 0) _anim.hurtWeight = 0;

        }

        override protected void OnHit() 
        { 
            base.OnHit();

            _anim.hurtWeight = 1;
        }
    }

}