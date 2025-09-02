using Siroshton.Masaya.Core;
using Siroshton.Masaya.Math;
using Siroshton.Masaya.Motion;
using Siroshton.Masaya.Weapon;
using UnityEngine;
using UnityEngine.Events;

namespace Siroshton.Masaya.Creature
{
    [RequireComponent(typeof(LookAtPlayer))]
    public class HoleCreature : Entity.Entity
    {
        [SerializeField] private bool _awakeOnStart = true;
        [SerializeField] private bool _isHostile = false;
        [SerializeField] private bool _isHostileAfterHit = true;
        [SerializeField] private float _retreatWhenNotHitTime = 10.0f;
        [SerializeField] private IntervalFloat _attackInterval = new IntervalFloat(2, 4);
        [SerializeField] private float _attackDuration =  4.0f;
        [SerializeField] private float _rotationSpeedWhileIdle = 60;
        [SerializeField] private float _rotationSpeedWhileAttacking = 15;
        [SerializeField] private Gun _gun;

        [SerializeField] private UnityEvent _onRoar;
        [SerializeField] private UnityEvent _onAttackBeginStarted;
        [SerializeField] private UnityEvent _onAttackBeginEnded;
        [SerializeField] private UnityEvent _onAttackEndStarted;
        [SerializeField] private UnityEvent _onAttackEndEnded;
        [SerializeField] private UnityEvent _onFriendlyRetreatStarted;
        [SerializeField] private UnityEvent _onFriendlyRetreatEnded;
        [SerializeField] private UnityEvent _onAngryRetreatStarted;
        [SerializeField] private UnityEvent _onAngryRetreatEnded;

        private enum State
        {
            Hiding,
            WakingUp,
            Idle,
            Attacking,
            Killed,
            RetreatingFriendly,
            RetreatingAngry
        }

        private struct AnimProperties
        {
            public Animator animator;
            public int wakeUpId;
            public int attackBeginId;
            public int attackEndId;
            public int killedId;
            public int retreatFriendlyId;
            public int retreatAngryId;
        }

        private struct AttackState
        {
            public float timeSinceAttack;
            public bool isAttacking;
            public float nextInterval;

            public bool isFiring;
            public float timeFiring;
        }

        private LookAtPlayer _lookAt;
        private State _state = State.Hiding;
        private AnimProperties _anim;
        private AttackState _attack;
        private float _timeNotHostile;

        protected new void Awake()
        {
            base.Awake();

            _lookAt = GetComponent<LookAtPlayer>();
            _lookAt.maxDegreesPerSecond = _rotationSpeedWhileIdle;

            _anim.animator = GetComponentInChildren<Animator>();
            _anim.wakeUpId = Animator.StringToHash("wakeUp");
            _anim.attackBeginId = Animator.StringToHash("attackBegin");
            _anim.attackEndId = Animator.StringToHash("attackEnd");
            _anim.killedId = Animator.StringToHash("killed");
            _anim.retreatFriendlyId = Animator.StringToHash("retreatFriendly");
            _anim.retreatAngryId = Animator.StringToHash("retreatAngry");

            _attack.nextInterval = _attackInterval.random;
        }

        protected new void Start()
        {
            base.Start();

            if(_awakeOnStart) WakeUp();
        }

        protected override void OnHit()
        {
            if( _state == State.RetreatingFriendly && _isHostileAfterHit ) Attack();

            _isHostile = _isHostileAfterHit;
            _attack.timeSinceAttack = _attack.nextInterval;
        }

        public void RetreatFriendly()
        {
            Retreat(true);
        }

        public void RetreatAngry()
        {
            Retreat(false);
        }

        public void Retreat(bool isFriendly)
        {
            if( isFriendly )
            {
                _anim.animator.SetTrigger(_anim.retreatFriendlyId);
                if (_onFriendlyRetreatStarted != null) _onFriendlyRetreatStarted.Invoke();
                _state = State.RetreatingFriendly;
            }
            else
            {
                _anim.animator.SetTrigger(_anim.retreatAngryId);
                if (_onAngryRetreatStarted != null) _onAngryRetreatStarted.Invoke();
                _state = State.RetreatingAngry;
            }

        }

        public void WakeUp()
        {
            if (_state != State.Hiding) return;

            _state = State.WakingUp;
            _anim.animator.SetTrigger(_anim.wakeUpId);
        }

        public void Anim_OnRoar()
        {
            if( _onRoar != null ) _onRoar.Invoke();
        }

        public void Anim_OnSurpriseEnded()
        {
            _state = State.Idle;
        }

        public void Anim_OnRetreatEnded()
        {
            if( _state == State.RetreatingFriendly && !_isHostile ) _onFriendlyRetreatEnded.Invoke();
            else _onAngryRetreatEnded.Invoke();
        }

        public void Attack()
        {
            if (_state != State.Idle && _state != State.RetreatingFriendly ) return;

            _state = State.Attacking;
            _anim.animator.SetTrigger(_anim.attackBeginId);
        }

        public void Anim_OnAttackBeginStarted()
        {
            _lookAt.maxDegreesPerSecond = _rotationSpeedWhileAttacking;

            if ( _onAttackBeginStarted != null ) _onAttackBeginStarted.Invoke();
        }

        public void Anim_OnAttackBeginEnded()
        {
            _attack.isFiring = true;
            _attack.timeFiring = 0;

            if (_onAttackBeginEnded != null) _onAttackBeginEnded.Invoke();
        }

        public void Anim_OnAttackEndStarted()
        {
            if (_onAttackEndStarted != null) _onAttackEndStarted.Invoke();
        }

        public void Anim_OnAttackEndEnded()
        {
            _state = State.Idle;
            _attack.timeSinceAttack = 0;
            _attack.nextInterval = _attackInterval.random;
            _lookAt.maxDegreesPerSecond = _rotationSpeedWhileIdle;

            if (_onAttackEndEnded != null) _onAttackEndEnded.Invoke();
        }

        private void UpdateAttack()
        {
            _attack.timeSinceAttack += GameState.deltaTime;

            if( _attack.isFiring )
            {
                _attack.timeFiring += GameState.deltaTime;
                float pos = _attack.timeFiring / _attackDuration;
                if( pos > 1 )
                {
                    pos = 1;
                    _attack.isFiring = false;
                    _anim.animator.SetTrigger(_anim.attackEndId);
                }

                _gun.Trigger(); // Adjust the fire interval on the gun itself if needed.
            }
        }

        protected override void OnKilled()
        {
            base.OnKilled();

            _state = State.Killed;
            _anim.animator.SetTrigger(_anim.killedId);
        }

        protected new void Update()
        {
            base.Update();

            if (_state == State.Idle)
            {
                if (_isHostile)
                {
                    _attack.timeSinceAttack += GameState.deltaTime;
                    if( _attack.timeSinceAttack > _attack.nextInterval ) Attack();
                }
                else
                {
                    _timeNotHostile += GameState.deltaTime;
                    if( _timeNotHostile >= _retreatWhenNotHitTime )
                    {
                        Retreat(true);
                    }
                }
            }
            else if ( _state == State.Attacking ) UpdateAttack();

        }
    }


}