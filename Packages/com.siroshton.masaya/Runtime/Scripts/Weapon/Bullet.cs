using Siroshton.Masaya.Core;
using Siroshton.Masaya.Math;
using UnityEngine;
using UnityEngine.Events;

namespace Siroshton.Masaya.Weapon
{
    public class Bullet : Entity.Entity, IBullet
    {
        public enum Target
        {
            None,
            Position,
            Player,
            Transform,
            Collider,
            AutoEnemy
        }

        [Tooltip("Max distance bullet can travel before it dies of natural causes.")]
        [SerializeField] private float _range = 2;
        [Tooltip("Modify the speed with a multiplier.")]
        [SerializeField] private float _speedMultiplier = 1;
        [SerializeField] private Target _targetType;
        [SerializeField] private Vector3 _targetPosition;
        [SerializeField] private Transform _targetTransform;
        [SerializeField] private Vector3 _targetTransformOffset;
        [SerializeField] private Collider _targetCollider;
        [SerializeField] private Entity.Entity _targetEntity;
        [SerializeField] private bool _levelOutIfTargetDies = true;
        [Tooltip("Specify the rate of rotation each axis can perform to try and reach its target.")]
        [SerializeField] private Vector3 _targetLockDegreesPerSecond;
        [SerializeField] private float _autoTargetRange = 5;
        [SerializeField] private float _autoTargetRadius = 0.5f;
        [SerializeField] private bool _destroyOnExpired = true;
        [SerializeField] private UnityEvent _onExpired;

        private Vector3 _startPos;
        private float _rangeSqr;
        private float _maxTime;
        private float _timeAlive;
        private RaycastHit[] _hitBuffer;

        public Vector3 startPos { get => _startPos; }
        public float range { get => _range; set => _range = value; }
        public Target targetType { get => _targetType; set => _targetType = value; }
        public Vector3 targetPosition { get => _targetPosition; set => _targetPosition = value; }
        public Transform targetTransform { get => _targetTransform; set => _targetTransform = value; }
        public Vector3 targetTransformOffset { get => _targetTransformOffset; set => _targetTransformOffset = value; }
        public Collider targetCollider { get => _targetCollider; set => _targetCollider = value; }
        public Entity.Entity targetEntity { get => _targetEntity; set => _targetEntity = value; }
        public Vector3 targetLockDegreesPerSecond { get => _targetLockDegreesPerSecond; set => _targetLockDegreesPerSecond = value; }

        public Vector3 target 
        {
            get
            {
                if (_targetType == Target.AutoEnemy && _targetTransform != null ) return _targetTransform.position + _targetTransformOffset;
                else if (_targetType == Target.Collider ) return _targetCollider.bounds.center;
                else if ( _targetType == Target.Player ) return Player.Player.instance.torso.position;
                else if ( _targetType == Target.Transform ) return _targetTransform.position + _targetTransformOffset;
                else if( _targetType == Target.Position ) return _targetPosition;
                else return Vector3.zero;
            }
        }

        public bool hasTarget
        {
            get
            {
                if (_targetType == Target.AutoEnemy) return _targetTransform != null;
                else if (_targetType == Target.Collider ) return _targetCollider != null && _targetCollider.enabled;
                else if (_targetType == Target.Player ) return true;
                else if (_targetType == Target.Transform) return _targetTransform != null;
                else if(_targetType == Target.Position ) return true;
                else return false;
            }
        }


    public void SetTarget(Vector3 target)
        {
            _targetPosition = target;
            _targetType = Target.Position;
        }

        public void SetTarget(Transform target)
        {
            _targetTransform = target;
            _targetType = Target.Transform;
        }

        public void SetTarget(Collider target)
        {
            _targetCollider = target;
            _targetType = Target.Collider;
        }

        protected new void Awake()
        {
            base.Awake();
        }

        protected new void Start()
        {
            base.Start();

            _startPos = transform.position;
            _rangeSqr = _range * _range;
            _maxTime = _range / speed;
            _timeAlive = 0;
            attributeModifiers.speedModifier = _speedMultiplier * attributeModifiers.bulletSpeedModifier;
        }

        public void ClearTarget(bool levelOut)
        {
            if (levelOut)
            {
                // remove target and level out the bullet (ie., stop turning on the x axis [up/down])
                Vector3 fwdPos = transform.position + transform.forward * 10;
                fwdPos.y = transform.position.y;
                Vector3 newDir = (fwdPos - transform.position).normalized;
                transform.rotation = Quaternion.LookRotation(newDir, Vector3.up);
            }
            
            _targetType = Target.None;
        }

        public void Expire()
        {
            _onExpired?.Invoke();
            if (_destroyOnExpired) Destroy();
        }

        private void FixedUpdate()
        {
            if( isDead ) return;
            
            float deltaTime = GameState.deltaTime;

            _timeAlive += deltaTime;

            if( _targetType == Target.AutoEnemy && _targetTransform == null )
            {
                if (_hitBuffer == null) _hitBuffer = new RaycastHit[5];

                Ray ray = new Ray(transform.position, transform.forward);
                int count = UnityEngine.Physics.SphereCastNonAlloc(ray, _autoTargetRadius, _hitBuffer, _autoTargetRange, GameLayers.enemiesMask);
                if( count == 1 )
                {
                    _targetTransform = _hitBuffer[0].transform;
                    _targetTransformOffset = new Vector3(0, _hitBuffer[0].point.y - _targetTransform.position.y);
                }
                else if( count > 1 )
                {
                    // find nearest hit
                    int closest = 0;
                    float nearest = (_hitBuffer[0].point - transform.position).sqrMagnitude;
                    float ds;
                    for(int i=1;i<count;i++)
                    {
                        ds = (_hitBuffer[i].point - transform.position).sqrMagnitude;
                        if( ds < nearest )
                        {
                            nearest = ds;
                            closest = i;
                        }
                    }

                    _targetTransform = _hitBuffer[closest].transform;
                    _targetTransformOffset = new Vector3(0, _hitBuffer[closest].point.y - _targetTransform.position.y);
                }
            }

            if ( _targetType != Target.None && hasTarget )
            {
                if( _targetEntity != null && _targetEntity.health == 0 )
                {
                    ClearTarget(_levelOutIfTargetDies);
                }
                else
                {
                    Vector3 targetDir = (target - transform.position).normalized;
                    Quaternion rotation = Quaternion.LookRotation(targetDir, Vector3.up);
                    Vector3 currentAngles = transform.rotation.eulerAngles;
                    Vector3 newAngles = rotation.eulerAngles;
                
                    newAngles.x = Mathf.MoveTowardsAngle(currentAngles.x, newAngles.x, _targetLockDegreesPerSecond.x * deltaTime);
                    newAngles.y = Mathf.MoveTowardsAngle(currentAngles.y, newAngles.y, _targetLockDegreesPerSecond.y * deltaTime);
                    newAngles.z = Mathf.MoveTowardsAngle(currentAngles.z, newAngles.z, _targetLockDegreesPerSecond.z * deltaTime);

                    transform.rotation = Quaternion.Euler(newAngles);
                }
            }

            transform.position = transform.position + transform.forward * attributes.GetSpeed(attributeModifiers) * deltaTime;

            if (_timeAlive > _maxTime || Vector3.SqrMagnitude(transform.position - _startPos) > _rangeSqr)
            {
                Expire();
            }
        }

        protected new void OnDrawGizmos()
        {
            base.OnDrawGizmos();

            if( _targetType == Target.Position )
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireCube(target, new Vector3(0.1f, 0.1f, 0.1f));
            }
            else if (_targetType == Target.Player)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireCube(Player.Player.instance.torso.position, new Vector3(0.1f, 0.1f, 0.1f));
            }
        }
    }
}