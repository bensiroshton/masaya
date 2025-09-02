using Siroshton.Masaya.Audio;
using Siroshton.Masaya.Core;
using Siroshton.Masaya.Math;
using Siroshton.Masaya.Weapon;
using System;
using System.Security.Cryptography;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Splines;

namespace Siroshton.Masaya.Creature
{

    public class Swoop : Entity.Entity
    {

        [SerializeField] private float _diveAngle = 30;
        [SerializeField] private float _turnWeightAngle = 20;
        [SerializeField] private float _turnBlendAngle = 10;
        [SerializeField, Range(0, 0.1f)] private float _nextPointOffset = 0.01f;
        [SerializeField] private IntervalFloat _fireInterval = new IntervalFloat(1, 2);
        [SerializeField, Range(0, 5)] private float _castRadius = 1;
        [SerializeField, Range(0, 5)] private float _minDistanceToShoot = 2;
        [SerializeField] private AudioClip _flapDown;
        [SerializeField] private IntervalFloat _screechInterval = new IntervalFloat(2, 4);
        [SerializeField] private UnityEvent _onPathEndReached;

        private Gun _gun;
        private AudioSourceController _audioController;

        private struct AnimProperties
        {
            public Animator animator;
            public int flapLayer;
            public int turnLayer;
            public int diveLayer;
            public int turnId;

            public float flapWeight;

            public float turnAngle;
            public float turnAngleVelocity;
            public float diveAngle;
            public float diveAngleVelocity;
        }

        private struct ShootState
        {
            public float timeSinceShoot;
            public float nextInterval;
            public float minDistanceToShootSqr;
        }

        private struct AudioState
        {
            public float timeSinceScreech;
            public float nextScreechInterval;
        }

        private AnimProperties _anim;
        private SplineAnimate _splineAnim;
        private Vector3 _nextPos;
        private ShootState _shoot;
        private AudioState _audio;

        protected new void Awake()
        {
            base.Awake();

            _anim.animator = GetComponent<Animator>();
            _anim.flapLayer = 0;
            _anim.turnLayer = _anim.animator.GetLayerIndex("Turn");
            _anim.diveLayer = _anim.animator.GetLayerIndex("Dive");
            _anim.turnId = Animator.StringToHash("turn");
            _anim.animator.Play("Flap", _anim.flapLayer, UnityEngine.Random.Range(0.0f, 1.0f));

            _splineAnim = GetComponent<SplineAnimate>();
            _splineAnim.MaxSpeed = speed;
            _splineAnim.Completed += OnSplineCompleted;

            _gun = GetComponentInChildren<Gun>();
            _shoot.nextInterval = _fireInterval.random;
            _shoot.minDistanceToShootSqr = _minDistanceToShoot * _minDistanceToShoot;

            _audioController = GetComponent<AudioSourceController>();
            _audio.nextScreechInterval = _screechInterval.random;
        }

        private void OnSplineCompleted()
        {
            if (_onPathEndReached != null) _onPathEndReached.Invoke();
        }

        public void Anim_OnFlapDown()
        {
            _audioController.PlayClip(_flapDown, _anim.flapWeight);
        }

        private void UpdateAnimation()
        {
            float deltaTime = GameState.deltaTime;

            // Dive Angle
            Vector3 rotation = transform.rotation.eulerAngles;
            Vector3 flatForwad = transform.forward;
            flatForwad.y = 0;
            float angle = Vector3.SignedAngle(flatForwad, transform.forward, transform.right);
            _anim.diveAngle = Mathf.SmoothDampAngle(_anim.diveAngle, angle, ref _anim.diveAngleVelocity, 0.5f, Mathf.Infinity, deltaTime);
            float pos = MathUtil.Normalize(0, _diveAngle, _anim.diveAngle);
            float climbPos = MathUtil.Normalize(0, -_diveAngle, _anim.diveAngle);
            _anim.flapWeight = 1.0f - pos;
            _anim.animator.SetLayerWeight(_anim.diveLayer, pos);

            // Turn Angle (remove as we climb at a steeper angle since the turn animation does not flap)
            float t = MathUtil.GetDecimal(_splineAnim.NormalizedTime) + _nextPointOffset;
            float3 np3 = _splineAnim.Container.Spline.EvaluatePosition(t);
            _nextPos = new Vector3(np3.x, np3.y, np3.z);
            _nextPos = _splineAnim.Container.transform.TransformPoint(_nextPos);
            angle = Vector3.SignedAngle(transform.forward, (_nextPos - transform.position).normalized, transform.up);
            _anim.turnAngle = Mathf.SmoothDampAngle(_anim.turnAngle, angle, ref _anim.turnAngleVelocity, 0.5f, Mathf.Infinity, deltaTime);

            // Set Flap / Turn Angle animation weights
            pos = Mathf.Min(Mathf.Abs(_anim.turnAngle / _turnWeightAngle), 1.0f) * (1.0f - climbPos);
            _anim.flapWeight = Mathf.Min(_anim.flapWeight, 1.0f - pos);
            _anim.animator.SetLayerWeight(_anim.turnLayer, pos);

            pos = MathUtil.Normalize(-_turnBlendAngle, _turnBlendAngle, _anim.turnAngle);
            pos = Mathf.Lerp(-1, 1, pos);
            _anim.animator.SetFloat(_anim.turnId, pos);
        }

        protected new void Update()
        {
            base.Update();

            UpdateAnimation();

            float deltaTime = GameState.deltaTime;

            _shoot.timeSinceShoot += deltaTime;
            _audio.timeSinceScreech += deltaTime;

            // shooting
            if ( _shoot.timeSinceShoot >= _shoot.nextInterval )
            {
                bool fire = (Player.Player.instance.torso.position - transform.position).sqrMagnitude >= _shoot.minDistanceToShootSqr;

                if ( fire && _castRadius > 0 )
                {
                    Ray ray = new Ray(transform.position, transform.forward);
                    fire = UnityEngine.Physics.SphereCast(ray, _castRadius, 10.0f, 1 << Player.Player.instance.gameObject.layer);
                }

                if( fire )
                {
                    _gun.Trigger();

                    _shoot.timeSinceShoot = 0;
                    _shoot.nextInterval = _fireInterval.random;
                }
            }

        }

#if UNITY_EDITOR
        protected new void OnDrawGizmosSelected()
        {
            base.OnDrawGizmosSelected();

            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(_nextPos, 0.1f);
        }
#endif

    }

}