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

    public class Slug : Entity.Entity
    {
        [SerializeField] private IntervalFloat _fireInterval;
        [SerializeField] private Gun _gun;
        [SerializeField] private GameObject _bulletCharge;
        [SerializeField] private UnityEvent _onChargeStarted;
        [SerializeField] private float _newPositionRadius = 2;
        [SerializeField] private float _newPositionForwardOffset = 2;
        [SerializeField, Range(0, 90)] private float _animationTurnAngleFactor = 50;
        [SerializeField, Range(0, 90)] private float _animationTurnAngleExtent = 50;

        private struct AnimProperties
        {
            public Animator animator;
            public int speedId;
            public int chargeId;
            public int hitStateId;
            public int turnId;

            public TimeClip chargeTime;
        }

        private struct ShootState
        {
            public bool isShooting;
            public float timeSinceFired;
            public float nextInterval;
            public bool isCharging;
            public Vector3 bulletChargeSize;
            public UnityEngine.Light chargeLight;
            public float chargeLightIntensity;
        }

        private AnimProperties _anim;
        private ShootState _fire;

        protected new void Awake()
        {
            base.Awake();

            _anim.animator = GetComponent<Animator>();
            _anim.speedId = Animator.StringToHash("speed");
            _anim.chargeId = Animator.StringToHash("charge");
            _anim.hitStateId = Animator.StringToHash("Hit");
            _anim.turnId = Animator.StringToHash("turn");
            AnimUtil.GetClipRange(_anim.animator, "Slug_Charging", out _anim.chargeTime, Anim_StartCharging, Anim_StopCharging);

            _fire.bulletChargeSize = _bulletCharge.transform.localScale;
            _fire.nextInterval = _fireInterval.random;
            _fire.chargeLight = _bulletCharge.GetComponent<UnityEngine.Light>();
            _fire.chargeLightIntensity = _fire.chargeLight.intensity;

            _bulletCharge.SetActive(false);
            _bulletCharge.transform.localScale = Vector3.zero;
            _fire.chargeLight.intensity = 0;
        }

        protected override void OnHit() 
        { 
            if( !_fire.isShooting ) _anim.animator.Play(_anim.hitStateId, 0, 0);
        }

        public void Anim_CrawlAddSpeed(AnimationEvent e)
        {
            agent.speed = speed;
        }

        public void Anim_CrawlRemoveSpeed(AnimationEvent e)
        {
            agent.speed = 0;
        }

        private void Fire()
        {
            // check if player is in sight
            if( _fire.isShooting ) return;

            _anim.animator.SetTrigger(_anim.chargeId);
            _fire.isShooting = true;

            agent.ResetPath();
        }

        public void Anim_OnChargingStart()
        {
        }

        public void Anim_StartCharging()
        {
            _fire.isCharging = true;
            _bulletCharge.SetActive(true);
            if( _onChargeStarted != null ) _onChargeStarted.Invoke();
        }

        public void Anim_StopCharging()
        {
            _fire.isCharging = false;
            _bulletCharge.transform.localScale = Vector3.zero;
            _bulletCharge.SetActive(false);
            _fire.chargeLight.intensity = 0;

            Vector3 target = Player.Player.instance.GetFutureLocation(_gun.GetGunBulletProperties().speed, _gun.transform.position);
            target.y = Player.Player.instance.torso.position.y;
            _gun.transform.LookAt(target);
            _gun.Trigger();
        }

        public void Anim_OnChargingEnd()
        {
            _fire.isShooting = false;
            _fire.nextInterval = _fireInterval.random;
            _fire.timeSinceFired = 0;
        }

        public void Anim_OnHitFinished()
        {
        }

        private void UpdateShooting()
        {
            if( _fire.isCharging )
            {
                AnimatorStateInfo info = _anim.animator.GetCurrentAnimatorStateInfo(0);
                float normalizedTime = AnimUtil.SplitNormalizedTime(info.normalizedTime)[1];
                float pos = _anim.chargeTime.GetClipPositionFromNormalizedTime(normalizedTime);

                _bulletCharge.transform.localScale = Vector3.Lerp(Vector3.zero, _fire.bulletChargeSize, pos);
                _fire.chargeLight.intensity = Mathf.Lerp(0, _fire.chargeLightIntensity, pos);
            }
        }

        protected new void Update()
        {
            base.Update();

            _fire.timeSinceFired += GameState.deltaTime;

            // disable going to/from other states based on speed.
            // currentSpeed is conflicting with the pause/go of the agent to do motion only
            // at certain times for the slug animation
            //_anim.animator.SetFloat(_anim.speedId, currentSpeed);

            if ( _fire.isShooting )
            {
                UpdateShooting();
            }
            else if( _fire.timeSinceFired >= _fire.nextInterval )
            {
                Fire();
            }
            else if( !agent.hasPath )
            {
                NavMeshHit hit;
                Vector3 target;
                Vector3 point = MathUtil.GetRandomPointInSphere(transform.position + transform.forward * _newPositionForwardOffset, _newPositionRadius);
                point.y = 0;
                if ( NavMesh.SamplePosition(point, out hit, 1, NavMesh.AllAreas) )
                {
                    target = hit.position;
                    if( NavMesh.FindClosestEdge(hit.position, out hit, NavMesh.AllAreas) )
                    {
                        if( (hit.position - target).sqrMagnitude < agent.radius * agent.radius )
                        {
                            target = hit.position + (target - hit.position).normalized * agent.radius;
                        }
                    }
                    agent.SetDestination(target);
                }
            }

            if( currentSpeed > 0 )
            {
                float angle = relativeTurnAngle * _animationTurnAngleFactor;
                float pos = MathUtil.Normalize(-_animationTurnAngleExtent, _animationTurnAngleExtent, angle);
                _anim.animator.SetFloat(_anim.turnId, pos);
            }
            else
            {
                _anim.animator.SetFloat(_anim.turnId, 0.5f);
            }
        }

    }

}