using Siroshton.Masaya.Animation;
using Siroshton.Masaya.Core;
using Siroshton.Masaya.Math;
using UnityEngine;
using UnityEngine.Events;
using static Siroshton.Masaya.Core.Types;

namespace Siroshton.Masaya.Creature
{

    public class HellToad : Entity.Entity
    {
        [Tooltip("The amount of time in seconds between each jump.")]
        [SerializeField] private float _jumpFrequency = 5;
        [SerializeField] private float _maxJumpDistance = 3;
        [Tooltip("Track and update the target position during the jump.  0=don't track at all., 1=track the whole time.")]
        [SerializeField] private float _trackTargetDuringJump = 0.75f;
        [SerializeField] private int _launchFrame = 14;
        [SerializeField] private int _jumpAtPeakFrame = 31;
        [SerializeField] private bool _jumpWhenHit = false;
        [SerializeField] private ParticleSystem _landingParticles;
        [SerializeField] private int _landingParticleCount = 20;
        [SerializeField] private UnityEvent _onLanded = new UnityEvent();

        private SimpleTransform _launch;
        private SimpleTransform _target;

        private struct AnimProperties
        {
            public Animator animator;
            public int jumpLayer;
            public int jumpId;
            public float timeSinceJump;
            public bool isJumping;
            public bool hasJumpEvent;
            public float launchPos;
            public float peakPos;
        }

        AnimProperties _anim;

        protected new void Start()
        {
            base.Start();
            _anim.animator = GetComponent<Animator>();
            _anim.jumpLayer = 0;
            _anim.jumpId = Animator.StringToHash("jump");
            foreach (AnimationClip clip in _anim.animator.runtimeAnimatorController.animationClips)
            {
                if (clip.name.Contains("jump", System.StringComparison.CurrentCultureIgnoreCase))
                {
                    _anim.launchPos = (float)_launchFrame / clip.frameRate / clip.length;
                    _anim.peakPos = (float)_jumpAtPeakFrame / clip.frameRate / clip.length;
                    break;
                }
            }
        }

        protected override void OnHit()
        {
            if( _jumpWhenHit ) Jump();
        }

        public void Jump()
        {
            if( _anim.isJumping ) return;

            //Debug.Log("Jump Called");
            _anim.animator.SetBool(_anim.jumpId, true);
            _anim.timeSinceJump = 0;
        }

        private void OnJumpAnimationStarted()
        {
            //Debug.Log("Jump Animation Started");
            _anim.isJumping = true;
            _anim.hasJumpEvent = false;
        }

        /// <summary>
        /// OnJumped
        /// Triggered by Animation Event.
        /// </summary>
        private void OnJumped()
        {
            //Debug.Log("Jumped");
            _launch.position = transform.position;
            _launch.rotation = transform.rotation;
            UpdateTargetPos();

            _anim.hasJumpEvent = true;
        }

        private void UpdateTargetPos()
        {
            _target = MathUtil.GetPointAndRotationTowards(transform.position, Player.Player.instance.transform.position, _maxJumpDistance);
        }

        /// <summary>
        /// OnLanded
        /// Triggered by Animation Event.
        /// </summary>
        private void OnLanded()
        {
            //Debug.Log("Landed");
            _landingParticles.Emit(_landingParticleCount);
            _onLanded.Invoke();
        }

        private void OnJumpAnimationFinished()
        {
            //Debug.Log("Jump Animation Finished.");
            _anim.timeSinceJump = 0;
            _anim.isJumping = false;
        }

        protected new void Update()
        {
            base.Update();
            
            if (_anim.isJumping)
            {
                if ( _anim.hasJumpEvent )
                {
                    AnimatorStateInfo info = _anim.animator.GetCurrentAnimatorStateInfo(_anim.jumpLayer);
                    float pos = AnimUtil.SplitNormalizedTime(info.normalizedTime)[1];

                    if( pos >= _anim.peakPos )
                    {
                        // Frog Drop!
                        _anim.isJumping = false;
                        pos = _anim.peakPos;
                    }

                    if( pos > _anim.launchPos )
                    {
                        pos = (pos - _anim.launchPos) / (_anim.peakPos - _anim.launchPos); // pos is now between [0.0, 1.0] of jump position.

                        if( pos < _trackTargetDuringJump) UpdateTargetPos();

                        transform.position = Vector3.Lerp(_launch.position, _target.position, pos);
                        transform.rotation = Quaternion.Slerp(_launch.rotation, _target.rotation, pos);
                    }
                }
            }
            else if ( _anim.timeSinceJump >= _jumpFrequency && _jumpFrequency > 0)
            {
                Jump();
            }
            else
            {
                _anim.timeSinceJump += GameState.deltaTime;
            }
        }
    }
}

