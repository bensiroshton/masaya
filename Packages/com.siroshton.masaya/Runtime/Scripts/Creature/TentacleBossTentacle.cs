using Siroshton.Masaya.Core;
using Siroshton.Masaya.Math;
using Siroshton.Masaya.Weapon;
using UnityEngine;
using UnityEngine.Events;

namespace Siroshton.Masaya.Creature
{

    public class TentacleBossTentacle : Entity.Entity
    {
        [SerializeField] private IntervalFloat _fireInterval = new IntervalFloat(2, 3);

        [SerializeField] private UnityEvent _onWakingUpStarted;
        [SerializeField] private UnityEvent _onWakingUpEnded;
        [SerializeField] private UnityEvent _onShootStarted;
        [SerializeField] private UnityEvent _onShoot;
        [SerializeField] private UnityEvent _onShootEnded;
        [SerializeField] private UnityEvent _onSlapStarted;
        [SerializeField] private UnityEvent _onSlapWhoosh;
        [SerializeField] private UnityEvent _onSlapHitGround;
        [SerializeField] private UnityEvent _onSlapSlide;
        [SerializeField] private UnityEvent _onSlapEnded;
        [SerializeField] private UnityEvent _onDeathStarted;
        [SerializeField] private UnityEvent _onDeathEnded;

        private Gun _gun;

        private enum State
        {
            WakingUp,
            Idle,
            Slapping,
            Shooting,
            Dying
        }

        private struct AnimState
        {
            public Animator animator;
            public int hitId;
            public int slapId;
            public int shootId;
            public int dieId;
            public int speedId;
        }

        private struct FireState
        {
            public float timeSinceFire;
            public float nextInterval;
        }

        private State _state;
        private AnimState _anim;
        private FireState _fire;

        protected new void Awake()
        {
            base.Awake();

            _state = State.WakingUp;

            _anim.animator = GetComponentInChildren<Animator>();
            _anim.hitId = Animator.StringToHash("hit");
            _anim.slapId = Animator.StringToHash("slap");
            _anim.shootId = Animator.StringToHash("shoot");
            _anim.dieId = Animator.StringToHash("die");
            _anim.speedId = Animator.StringToHash("speed");
            _anim.animator.SetFloat(_anim.speedId, attributes.speed);

            _gun = GetComponentInChildren<Gun>();
            _fire.nextInterval = _fireInterval.random;
        }

        protected override void OnHit()
        {
            if( _state == State.Idle ) _anim.animator.SetTrigger(_anim.hitId);
        }

        protected override void OnKilled()
        {
            _state = State.Dying;

            Collider[] colliders = GetComponentsInChildren<Collider>();
            for (int i = 0; i < colliders.Length; i++)
            {
                colliders[i].enabled = false;
            }

            _anim.animator.SetTrigger(_anim.dieId);
        }

        public void Slap()
        {
            if( _state != State.Idle ) return;

            _anim.animator.SetTrigger(_anim.slapId);
            _state = State.Slapping;
        }

        public bool Shoot()
        {
            if (_state != State.Idle) return false;

            _anim.animator.SetTrigger(_anim.shootId);
            _state = State.Shooting;
            return true;
        }

        public void Anim_WakingUpStarted()
        {
            if (_onWakingUpStarted != null ) _onWakingUpStarted.Invoke();
        }

        public void Anim_WakingUpEnded()
        {
            _state = State.Idle;

            if (_onWakingUpEnded != null) _onWakingUpEnded.Invoke();
        }

        public void Anim_OnShootStarted()
        {
            if (_onShootStarted != null) _onShootStarted.Invoke();
        }

        public void Anim_OnShootBullet()
        {
            _gun.Trigger();

            if (_onShoot != null) _onShoot.Invoke();
        }

        public void Anim_OnShootEnded()
        {
            _state = State.Idle;

            if (_onShootEnded != null) _onShootEnded.Invoke();

            _fire.timeSinceFire = 0;
            _fire.nextInterval = _fireInterval.random;
        }

        public void Anim_OnSlapStarted()
        {
            if (_onSlapStarted != null) _onSlapStarted.Invoke();
        }

        public void Anim_OnSlapWhoosh()
        {
            if (_onSlapWhoosh != null) _onSlapWhoosh.Invoke();
        }

        public void Anim_OnSlapHitGround()
        {
            if (_onSlapHitGround != null) _onSlapHitGround.Invoke();
        }

        public void Anim_OnSlapSlide()
        {
            if (_onSlapSlide != null) _onSlapSlide.Invoke();
        }

        public void Anim_OnSlapEnded()
        {
            _state = State.Idle;
            
            if (_onSlapEnded != null) _onSlapEnded.Invoke();
        }

        public void Anim_OnDeathStarted()
        {
            if (_onDeathStarted != null) _onDeathStarted.Invoke();
        }

        public void Anim_OnDeathEnded()
        {
            if (_onDeathEnded != null) _onDeathEnded.Invoke();
        }

        protected new void Update()
        {
            base.Update();

            _fire.timeSinceFire += GameState.deltaTime;

            if( _fire.timeSinceFire >= _fire.nextInterval )
            {
                Shoot();
            }

        }

    }

}
