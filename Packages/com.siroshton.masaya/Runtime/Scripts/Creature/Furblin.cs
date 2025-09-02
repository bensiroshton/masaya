using Siroshton.Masaya.Animation;
using Siroshton.Masaya.Core;
using Siroshton.Masaya.Math;
using Siroshton.Masaya.Weapon;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using static Siroshton.Masaya.Core.Types;

namespace Siroshton.Masaya.Creature
{

    public class Furblin : Entity.Entity
    {
        [SerializeField] private Gun _gun;
        [SerializeField] private IntervalFloat _throwInterval;
        [SerializeField] private GameObject _bulletCharge;
        [SerializeField, Range(0, 1)] private float _lookAtPlayerWeight = 0.5f;
        [SerializeField, Range(0, 90)] private float _maxTwistAngle = 90;
        [SerializeField] private float _playerRadiusTarget = 3;
        [SerializeField] private float _targetPositionRotationSpeed = 10;
        [SerializeField] private float _targetUpdateInterval = 1;
        [SerializeField] private IntervalFloat _changeDirectionInterval;
        [SerializeField] private float _maxDistanceFromHome = 15;
        [SerializeField] private float _delayBeforeTryingToReturnHome = 2;
        [SerializeField] private float _alertOthersRange = 5;
        [SerializeField] private UnityEvent _onThrowStarted;
        [SerializeField] private UnityEvent _onChargeStarted;
        [SerializeField] private UnityEvent _onChargeEnded;
        [SerializeField] private UnityEvent _onThrowEnded;
        [SerializeField] private UnityEvent _onReturnedHome;

        private enum State
        {
            WaitingToEngage,
            Engaged,
            ReturningHome
        }

        private struct AnimProperties
        {
            public Animator animator;
            public int throwStateId;
            public int throwLayer;
            public TimeClip throwChargeTime;
            public int twistId;
            public int speedMultiplierId;
            public int turnId;
            public int speedId;
        }

        private struct ThrowState
        {
            public bool isThrowing;
            public bool isCharging;
            public float timeSinceThrow;
            public float nextInterval;
            public float throwLayerBlend;
            public float throwLayerBlendVelocity;
            public float throwLayerTargetWeight;
            public UnityEngine.Light chargeLight;
            public float chargeLightIntensity;
        }

        private struct LookAtPlayerState
        {
            public float weight;
            public float targetWeight;
            public float velocity;
        }

        private struct MotionState
        {
            public float idleWalk;
            public float idleWalkTarget;
            public float idleWalkVelocity;
            public float targetRotation;
            public float updateTime;
            public Vector3 targetPosition;
            public float changeDirectionTime;
            public float nextChangeDirectionInterval;
            public Vector3 homePosition;
            public float timeSinceTryingToReturnHome;
        }

        private State _state = State.WaitingToEngage;
        private AnimProperties _anim;
        private ThrowState _throw;
        private LookAtPlayerState _lookAt;
        private MotionState _motion;

        protected new void Awake()
        {
            base.Awake();

            _anim.animator = GetComponentInChildren<Animator>();
            _anim.throwStateId = Animator.StringToHash("Throw");
            _anim.throwLayer = _anim.animator.GetLayerIndex("Throw");
            _anim.twistId = Animator.StringToHash("twist");
            _anim.speedMultiplierId = Animator.StringToHash("speedMultiplier");
            _anim.turnId = Animator.StringToHash("turn");
            _anim.speedId = Animator.StringToHash("speed");
            AnimUtil.GetClipRange(_anim.animator, "Furblin_Throw", out _anim.throwChargeTime, Anim_OnThrowStartCharging, Anim_OnThrowStopCharging);
            
            _throw.nextInterval = _throwInterval.random;
            _throw.chargeLight = _bulletCharge.GetComponentInChildren<UnityEngine.Light>();
            _throw.chargeLightIntensity = _throw.chargeLight.intensity;
            _bulletCharge.SetActive(false);

            _lookAt.targetWeight = _lookAtPlayerWeight;

            agent.updateRotation = false;

            _motion.targetRotation = Random.Range(0.0f, 360.0f);
            _motion.nextChangeDirectionInterval = _changeDirectionInterval.random;
            _motion.homePosition = transform.position;
        }

        protected void OnAnimatorIK()
        {
            _anim.animator.SetLookAtPosition(Player.Player.instance.head.position);
            _anim.animator.SetLookAtWeight(_lookAt.weight);
        }

        protected override void OnHit()
        {
            Engage();
        }

        public void Engage()
        {
            if (_state == State.Engaged) return;

            EngageInternal();

            Collider[] others = UnityEngine.Physics.OverlapSphere(transform.position, _alertOthersRange);
            foreach(Collider other in others)
            {
                if( other.GetComponent<Furblin>() is Furblin f)
                {
                    f.EngageInternal();
                }
            }
        }

        private void EngageInternal()
        {
            if (_state == State.Engaged) return;

            _state = State.Engaged;
            _throw.timeSinceThrow = 0;
            _throw.nextInterval = _throwInterval.random;
        }

        public void ReturnHome()
        {
            if( _state == State.WaitingToEngage || _state == State.ReturningHome ) return; // already home.

            _state = State.ReturningHome;
            _motion.timeSinceTryingToReturnHome = 0;
            agent.SetDestination(_motion.homePosition);
        }

        public void Throw()
        {
            if( _throw.isThrowing || _state != State.Engaged ) return;

            _throw.isThrowing = true;
            _throw.throwLayerTargetWeight = 1;
            _anim.animator.Play(_anim.throwStateId, _anim.throwLayer, 0);
        }

        public void Anim_OnThrowStarted()
        {
            _throw.isThrowing = true;
            _bulletCharge.SetActive(true);
            _bulletCharge.transform.localScale = Vector3.zero;
            _throw.chargeLight.intensity = 0;
            _lookAt.targetWeight = 0;
            if( _onThrowStarted!=null ) _onThrowStarted.Invoke();
        }

        public void Anim_OnThrowStartCharging()
        {
            _throw.isCharging = true;
            if( _onChargeStarted != null ) _onChargeStarted.Invoke();
        }

        public void Anim_OnThrowStopCharging()
        {
            _throw.isCharging = false;
            _bulletCharge.transform.localScale = Vector3.one;
            _throw.chargeLight.intensity = _throw.chargeLightIntensity;
            if (_onChargeEnded != null) _onChargeEnded.Invoke();
        }

        public void Anim_ReleaseProjectile()
        {
            _bulletCharge.SetActive(false);
            _bulletCharge.transform.localScale = Vector3.zero;
            _throw.chargeLight.intensity = 0;
            
            // TODO: limit the angle so it looks correct, ie., we can't have the furblin pointing away from the player but throwing towards the player.
            Vector3 target = Player.Player.instance.GetFutureLocation(_gun.GetGunBulletProperties().speed, _gun.transform.position);
            target.y = Player.Player.instance.torso.position.y;
            _gun.transform.LookAt(target, Vector3.up);
            _gun.Trigger();

            _lookAt.targetWeight = _lookAtPlayerWeight;
        }

        public void Anim_OnThrowEnded()
        {
            _throw.isThrowing = false;
            _throw.throwLayerTargetWeight = 0;
            _throw.timeSinceThrow = 0;
            _throw.nextInterval = _throwInterval.random;
            if (_onThrowEnded != null) _onThrowEnded.Invoke();
        }

        private void UpdateThrow()
        {
            if (_throw.isCharging)
            {
                AnimatorStateInfo info = _anim.animator.GetCurrentAnimatorStateInfo(_anim.throwLayer);
                float normalizedTime = AnimUtil.SplitNormalizedTime(info.normalizedTime)[1];
                float pos = _anim.throwChargeTime.GetClipPositionFromNormalizedTime(normalizedTime);

                _bulletCharge.transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, pos);
                _throw.chargeLight.intensity = Mathf.Lerp(0, _throw.chargeLightIntensity, pos);
            }
        }


        private void UpdateMotion()
        {
            float deltaTime = GameState.deltaTime;

            _motion.timeSinceTryingToReturnHome += deltaTime;

            if( _state == State.Engaged )
            {
                _motion.updateTime += deltaTime;
                _motion.changeDirectionTime += deltaTime;

                // update target rotation
                if( _motion.nextChangeDirectionInterval > 0 && _motion.changeDirectionTime > _motion.nextChangeDirectionInterval )
                {
                    _targetPositionRotationSpeed = -_targetPositionRotationSpeed;
                    _motion.nextChangeDirectionInterval = _changeDirectionInterval.random;
                    _motion.changeDirectionTime = 0;
                }

                _motion.targetRotation += _targetPositionRotationSpeed * deltaTime;
                if( _motion.targetRotation > 360.0f) _motion.targetRotation -= 360.0f;
                else if(_motion.targetRotation < 0) _motion.targetRotation += 360.0f;

                if ( _motion.updateTime >= _targetUpdateInterval || !agent.hasPath )
                {
                    // reverse direction if the player is in our path
                    if (agent.velocity.sqrMagnitude > 0)
                    {
                        Ray ray = new Ray(transform.position + new Vector3(0, 1, 0), agent.velocity);
                        RaycastHit rayHit;
                        if (Player.Player.instance.Raycast(ray, out rayHit, 10))
                        {
                            _targetPositionRotationSpeed = -_targetPositionRotationSpeed;
                            _motion.changeDirectionTime = 0;
                        }
                    }

                    int tries = 10;
                    do
                    {
                        float r = Mathf.Deg2Rad * _motion.targetRotation;
                        _motion.targetPosition = Player.Player.instance.transform.position + new Vector3(Mathf.Sin(r), 0, Mathf.Cos(r)) * _playerRadiusTarget;
                        NavMeshHit hit;

                        if (NavMesh.SamplePosition(_motion.targetPosition, out hit, 1.0f, NavMesh.AllAreas))
                        {
                            agent.SetDestination(hit.position);
                            _motion.targetPosition = hit.position;
                            tries = 0;
                        }
                        else
                        {
                            _motion.targetRotation += _targetPositionRotationSpeed;
                        }

                        --tries;
                    } while(tries > 0);

                    _motion.updateTime = 0;
                }

                // Disengage, we are too far from our home position.
                if(_motion.timeSinceTryingToReturnHome >= _delayBeforeTryingToReturnHome)
                {
                    if ( (transform.position - _motion.homePosition).sqrMagnitude > _maxDistanceFromHome * _maxDistanceFromHome )
                    {
                        ReturnHome();
                    }
                }
            }
            else if( _state == State.ReturningHome )
            {
                if( !agent.hasPath )
                {
                    _state = State.WaitingToEngage;
                    if( _onReturnedHome != null ) _onReturnedHome.Invoke();
                }
            }

            // Set animation speed
            float velocity = agent.velocity.magnitude;
            _anim.animator.SetFloat(_anim.speedId, velocity);
            _anim.animator.SetFloat(_anim.speedMultiplierId, velocity + 0.2f);

            // Upate walk and twist animation angles to match our direction motion
            Vector3 forward = agent.velocity;
            if( velocity == 0 ) forward = transform.forward;

            float angleToPlayer = Vector3.SignedAngle(forward, Player.Player.instance.transform.position - transform.position, Vector3.up);
            float angle = Vector3.SignedAngle(Vector3.forward, forward, Vector3.up);

            // update our upper body twist animation to point towards the player
            float normAngle = MathUtil.Normalize(-_maxTwistAngle, _maxTwistAngle, angleToPlayer);
            _anim.animator.SetFloat(_anim.twistId, normAngle);

            if( velocity > 0 )
            {
                // rotate half way towards the player and make our turn animation angle match
                angleToPlayer *= 0.5f;
                transform.rotation = Quaternion.Euler(new Vector3(0, angle + angleToPlayer, 0));

                normAngle = MathUtil.Normalize(-90, 90, -angleToPlayer);
                _anim.animator.SetFloat(_anim.turnId, normAngle);
            }
        }

        protected new void Update()
        {
            base.Update();

            _throw.timeSinceThrow += GameState.deltaTime;

            if( _state == State.WaitingToEngage ) attributeModifiers.speedModifier = 0;
            else attributeModifiers.speedModifier = 1;

            if( _throw.isThrowing )
            {
                UpdateThrow();
            }
            else if( _throw.timeSinceThrow > _throw.nextInterval )
            {
                Throw();
            }

            UpdateMotion();

            _throw.throwLayerBlend = Mathf.SmoothDamp(_throw.throwLayerBlend, _throw.throwLayerTargetWeight, ref _throw.throwLayerBlendVelocity, 0.1f);
            _anim.animator.SetLayerWeight(_anim.throwLayer, _throw.throwLayerBlend);

            _lookAt.weight = Mathf.SmoothDamp(_lookAt.weight, _lookAt.targetWeight, ref _lookAt.velocity, 0.1f);
        }

#if UNITY_EDITOR
        protected new void OnDrawGizmosSelected()
        {
            base.OnDrawGizmosSelected();

            Gizmos.color = Color.magenta;
            Gizmos.DrawWireCube(_motion.targetPosition, new Vector3(0.1f, 0.1f, 0.1f));
        }
#endif

    }

}