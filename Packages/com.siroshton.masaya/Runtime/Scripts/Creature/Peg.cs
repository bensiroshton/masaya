
using Siroshton.Masaya.Animation;
using Siroshton.Masaya.Core;
using Siroshton.Masaya.Math;
using Siroshton.Masaya.Motion;
using Siroshton.Masaya.Weapon;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using static Siroshton.Masaya.Core.Types;

namespace Siroshton.Masaya.Creature
{

    public class Peg : Entity.Entity
    {
        [SerializeField] private LookAtPlayer _eyeball;

        [SerializeField] private float _jumpHeightAddition = 1;
        [SerializeField] private string _jumpAnimClipName = "Peg_Jump";
        [SerializeField] private IntervalFloat _jumpInterval;
        [SerializeField] private float _maxJumpDistance = 4;
        
        [SerializeField] private IntervalFloat _fireInterval;
        [SerializeField] private float _fireWarmupTime = 1;
        [SerializeField] private float _fireUpAngle = 0;
        [SerializeField] private Gun _gun;
        [SerializeField] private UnityEngine.Light _fireLight;
        [SerializeField] private float _fireLightIntensity = 1;

        [SerializeField] private UnityEvent _onJumpStarted;
        [SerializeField] private UnityEvent _onJumpLaunch;
        [SerializeField] private UnityEvent _onJumpLanded;
        [SerializeField] private UnityEvent _onJumpFinished;

        [SerializeField] private UnityEvent _onFireSequenceStarted;
        [SerializeField] private UnityEvent _onShotFired;
        [SerializeField] private UnityEvent _onFireSequenceEnded;

        private struct AnimProperties
        {
            public Animator animator;
            public int speedId;
            public int jumpId;
            public float jumpLaunchPos;
            public float jumpLandedPos;
            public float jumpLaunchDuration;
        }

        private struct JumpState
        {
            public float timeSinceJumped;
            public bool isJumping;
            public bool isLaunched;
            public float nextJumpInterval;
            public SimpleTransform start;
            public SimpleTransform end;
        }

        private struct ShootState
        {
            public bool isShooting;
            public float timeSinceFired;
            public float nextFireInterval;
            public float sequence;
            public float sequenceTime;
            public Vector3 storedEyeAngleOffset;
        }

        private AnimProperties _anim;
        private JumpState _jump;
        private ShootState _fire;

        protected new void Awake()
        {
            base.Awake();
        }

        protected new void Start()
        {
            base.Start();

            _anim.animator = GetComponent<Animator>();
            _anim.speedId = Animator.StringToHash("speed");
            _anim.jumpId = Animator.StringToHash("jump");

            AnimationClip clip = null;
            AnimationEvent[] events = AnimUtil.GetEvents(_anim.animator, _jumpAnimClipName, ref clip);
            if( events == null )
            {
                Debug.LogError($"Unable to get {_jumpAnimClipName} animation events.");
            }
            else
            {                
                foreach (AnimationEvent e in events)
                {
                    if( e.functionName == "Anim_OnJumpLaunch" ) _anim.jumpLaunchPos = e.time / clip.length;
                    else if (e.functionName == "Anim_OnJumpLanded") _anim.jumpLandedPos = e.time / clip.length;
                }

                _anim.jumpLaunchDuration = _anim.jumpLandedPos - _anim.jumpLaunchPos;
            }

            _jump.nextJumpInterval = _jumpInterval.random;
            _fire.nextFireInterval = _fireInterval.random;

            _gun.onBulletSpawned.AddListener(OnBulletSpawned);

        }

        public void Jump()
        {
            if( _jump.isJumping ) return;

            NavMeshHit hit;
            Vector3 point = MathUtil.GetPointTowards(transform.position, Player.Player.instance.GetFutureLocation(_anim.jumpLaunchDuration / 2.0f), _maxJumpDistance);
            if (!NavMesh.SamplePosition(point, out hit, 1, NavMesh.AllAreas))
            {
                return;
            }

            _jump.end = MathUtil.GetPointAndRotationTowards(transform.position, hit.position, _maxJumpDistance);
            _anim.animator.SetTrigger(_anim.jumpId);
            _jump.isJumping = true;
            agent.ResetPath();
        }

        public void Anim_OnJumpStarted()
        {
            _jump.isJumping = true;
            if (_onJumpStarted != null) _onJumpStarted.Invoke();
        }

        public void Anim_OnJumpLaunch()
        {
            _jump.start.position = transform.position;
            _jump.start.rotation = transform.rotation;
            _jump.isLaunched = true;
            if (_onJumpLaunch != null) _onJumpLaunch.Invoke();
        }

        public void Anim_OnJumpLanded()
        {
            _jump.isLaunched = false;
            if (_onJumpLanded != null) _onJumpLanded.Invoke();
        }

        public void Anim_OnJumpFinished()
        {
            _jump.isJumping = false;
            _jump.timeSinceJumped = 0;
            _jump.nextJumpInterval = _jumpInterval.random;
            if (_onJumpFinished != null) _onJumpFinished.Invoke();
        }

        public void Fire()
        {
            if( _fire.isShooting ) return;

            _fire.isShooting = true;
            _fire.sequence = 0;
            _fire.storedEyeAngleOffset = _eyeball.offsetAngles;
            _fire.timeSinceFired = 0;
            _fire.sequenceTime = 0;

            if (_onFireSequenceStarted != null) _onFireSequenceStarted.Invoke();
        }

        private void SetNewPath()
        {
            NavMeshHit hit;
            Vector3 point = MathUtil.GetRandomPointInSphere(Player.Player.instance.transform.position, _maxJumpDistance);
            point.y = 0;
            if ( NavMesh.SamplePosition(point, out hit, 1, NavMesh.AllAreas) )
            {
                agent.SetDestination(hit.position);
            }
        }

        protected new void Update()
        {
            base.Update();

            _jump.timeSinceJumped += GameState.deltaTime;
            _fire.timeSinceFired += GameState.deltaTime;

            if ( _jump.isJumping )
            {
                UpdateJump();
            }
            else if (_jump.timeSinceJumped >= _jump.nextJumpInterval)
            {
                Jump();
            }
            else if( !agent.hasPath )
            {
                SetNewPath();
            }

            if ( _fire.isShooting )
            {
                UpdateShooting();
            }
            else if (_fire.timeSinceFired >= _fire.nextFireInterval)
            {
                Fire();
            }
        }

        private void UpdateJump()
        {
            if (_jump.isLaunched)
            {
                AnimatorStateInfo info = _anim.animator.GetCurrentAnimatorStateInfo(0);
                float normalizedTime = AnimUtil.SplitNormalizedTime(info.normalizedTime)[1];

                // make pos be between [0.0, 1.0] of position between animation start/stop event points.
                float pos = (normalizedTime - _anim.jumpLaunchPos) / _anim.jumpLaunchDuration;

                SimpleTransform t = SimpleTransform.Slerp(_jump.start, _jump.end, pos);
                t.position.y = Mathf.Sin(Mathf.Lerp(0, Mathf.PI, pos)) * _jumpHeightAddition;
                transform.position = t.position;
                transform.rotation = t.rotation;
            }
        }

        private void OnBulletSpawned(GameObject projectile)
        {
            Bullet bullet = projectile.GetComponent<Bullet>();
            Player.Player player = Player.Player.instance;
            //Vector3 target = MathUtil.GetRandomPointAround(player.GetFutureLocation(1), new Vector3(0.5f, 0, 0.5f));
            float time = (bullet.transform.position - player.transform.position).sqrMagnitude / (bullet.speed * bullet.speed);
            Vector3 target = player.GetFutureLocation(time);
            target.y = Player.Player.instance.height / 2.0f;
            bullet.SetTarget(target);
        }

        private void UpdateShooting()
        {
            _fire.sequenceTime += GameState.deltaTime;

            if (_fire.sequence == 0)
            {
                if(_fireWarmupTime == 0 )
                {
                    _fire.sequence++;
                    return;
                }

                float pos = _fire.sequenceTime / _fireWarmupTime;
                if (pos > 1)
                {
                    pos = 1;
                    _fire.sequence++;
                    _fire.sequenceTime = 0;
                    _gun.Trigger();

                    if (_onShotFired != null) _onShotFired.Invoke();
                }

                _fireLight.intensity = Mathf.Lerp(0, _fireLightIntensity, pos);
                _eyeball.offsetAngles = Vector3.Lerp(_fire.storedEyeAngleOffset, new Vector3(90 - _fireUpAngle, 0, 0), pos);
            }
            else if (_fire.sequence == 1)
            {
                float pos = _fire.sequenceTime / _fireWarmupTime;
                if( _fire.sequenceTime >= _fireWarmupTime)
                {
                    pos = 1;
                    _fire.sequence++;
                    _fire.sequenceTime = 0;
                }

                _fireLight.intensity = Mathf.Lerp(_fireLightIntensity, 0, pos);
                _eyeball.offsetAngles = Vector3.Lerp(new Vector3(90 - _fireUpAngle, 0, 0), _fire.storedEyeAngleOffset, pos);
            }
            else if (_fire.sequence == 2)
            {
                // end shooting sequence
                _eyeball.offsetAngles = _fire.storedEyeAngleOffset;
                _fire.nextFireInterval = _fireInterval.random;
                _fire.timeSinceFired = 0;
                _fire.isShooting = false;

                if (_onFireSequenceEnded != null) _onFireSequenceEnded.Invoke();
            }
        }

        protected new void OnDrawGizmos()
        {
            base.OnDrawGizmos();

            if (_jump.isLaunched)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(_jump.start.position + new Vector3(0, 0.001f, 0), _jump.end.position + new Vector3(0, 0.001f, 0));
            }
        }

    }

}