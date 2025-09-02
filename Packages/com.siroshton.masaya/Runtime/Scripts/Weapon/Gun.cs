using Siroshton.Masaya.Core;
using Siroshton.Masaya.Entity;
using UnityEngine;
using UnityEngine.Events;

namespace Siroshton.Masaya.Weapon
{

    public class Gun : MonoBehaviour, IWeapon
    {
        public struct GunBulletProperties
        {
            public float speed;
            public float range;
        }

        [SerializeField] private Bullet _bulletPrefab;
        [SerializeField] private Transform _projectileLaunchPoint;
        [SerializeField] private float _triggerCooldown = 1;
        [SerializeField, Range(0, 360)] private float _spread = 0;
        [SerializeField] private float _spreadOffset = 0;
        [SerializeField] private int _bulletsPerTrigger = 1;
        [SerializeField] private float _timeBeforeTriggerRelease = 0.1f;
        [SerializeField] private UnityEvent _onFire = new UnityEvent();
        [SerializeField] private UnityEvent<GameObject> _onBulletSpawned;
        [Tooltip("The float is in normalized time, 0 < 1 = Can Not Trigger, 1 = Ready To Trigger")]
        [SerializeField] private UnityEvent<float> _onTriggerCooldown;
        [SerializeField] private UnityEvent _onTriggerDown;
        [SerializeField] private UnityEvent _onTriggerRelease;

        private float _timeSinceTriggered;
        private float _timeSinceLastTriggerCall;
        private bool _isTriggerDown;

        private AttributeModifiers _ownerAttributeModifiers;

        public float triggerCooldown { get => _triggerCooldown; set => _triggerCooldown = value; }
        public float timeTillTriggerReady { get => Mathf.Max(0, _triggerCooldown - _timeSinceTriggered); }
        public float timeSinceTriggered { get => _timeSinceTriggered; }
        public Bullet bulletPrefab { get => _bulletPrefab; set => _bulletPrefab = value; }
        public AttributeModifiers ownerAttributeModifiers { get => _ownerAttributeModifiers; set => _ownerAttributeModifiers = value; }
        public UnityEvent<GameObject> onBulletSpawned { get => _onBulletSpawned; set => _onBulletSpawned = value; }

        private void Awake()
        {
            if( _projectileLaunchPoint == null ) _projectileLaunchPoint = transform;

            if(_bulletPrefab == null)
            {
                Debug.LogWarning("Gun bullet prefab is missing. Disabling gun.");
                enabled = false;
            }
        }

        public bool canTrigger
        {
            get
            {
                return _timeSinceTriggered >= _triggerCooldown;
            }
        }

        public void PointAt(Vector3 target, bool xzOnly = true)
        {
            if(xzOnly) target.y = transform.position.y;

            transform.rotation = Quaternion.LookRotation(target - transform.position, Vector3.up);
        }

        public void SetDirection(Vector3 direction)
        {
            transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
        }

        public GunBulletProperties GetGunBulletProperties()
        {
            GunBulletProperties p;

            if (_ownerAttributeModifiers != null)
            {
                p.speed = _bulletPrefab.attributes.speed * _ownerAttributeModifiers.bulletSpeedModifier;
                p.range = _bulletPrefab.range * _ownerAttributeModifiers.bulletRangeModifier;
            }
            else
            {
                p.speed = _bulletPrefab.attributes.speed;
                p.range = _bulletPrefab.range;
            }

            return p;
        }

        public bool Trigger()
        {
            return Trigger(false);
        }

        public bool Trigger(bool overrideTriggerTime = false)
        {
            _timeSinceLastTriggerCall = 0;
            if( !_isTriggerDown )
            {
                _isTriggerDown = true;
                _onTriggerDown?.Invoke();
            }

            if ( !canTrigger && !overrideTriggerTime || _bulletsPerTrigger < 1) return false;

            Quaternion rotation = _projectileLaunchPoint.rotation;
            Vector3 fwd = _projectileLaunchPoint.forward;
            float angleStep = 0;
            Quaternion angle = Quaternion.identity;

            if ( _spread > 0 && _bulletsPerTrigger > 1)
            {
                if( _spread == 360 )
                {
                    angleStep = _spread / (float)(_bulletsPerTrigger);
                }
                else
                {
                    angleStep = _spread / (float)(_bulletsPerTrigger - 1.0f);
                }
                angle = Quaternion.AngleAxis(-_spread / 2.0f, Vector3.up);
                rotation = rotation * angle;
                fwd = angle * fwd;
                angle = Quaternion.AngleAxis(angleStep, Vector3.up);
            }

            for (int i=0;i<_bulletsPerTrigger;i++)
            {
                Bullet bullet = GameObject.Instantiate(_bulletPrefab, _projectileLaunchPoint.position + fwd * _spreadOffset, rotation);
                if( _ownerAttributeModifiers != null )
                {
                    bullet.attributeModifiers.bulletSpeedModifier *= _ownerAttributeModifiers.bulletSpeedModifier;
                    bullet.attributeModifiers.damageModifier *= _ownerAttributeModifiers.damageModifier;
                    bullet.attributeModifiers.criticalChance = _ownerAttributeModifiers.criticalChance;
                    bullet.attributeModifiers.criticalDamageModifier *= _ownerAttributeModifiers.criticalDamageModifier;
                    bullet.range *= _ownerAttributeModifiers.bulletRangeModifier;

                    bullet.transform.localScale *= _ownerAttributeModifiers.bulletSizeModifier;
                }
                 
                if (_spread > 0 && _bulletsPerTrigger > 1)
                {
                    rotation = rotation * angle;
                    fwd = angle * fwd;
                }

                _onBulletSpawned?.Invoke(bullet.gameObject);
            }

            _timeSinceTriggered = 0;
            _onFire.Invoke();
            _onTriggerCooldown?.Invoke(0);
            return true;
        }

        private void Update()
        {
            _timeSinceLastTriggerCall += GameState.deltaTime;
            if( _timeSinceLastTriggerCall >= _timeBeforeTriggerRelease && _isTriggerDown )
            {
                _isTriggerDown = false;
                _onTriggerRelease?.Invoke();
            }

            if (_ownerAttributeModifiers != null)
            {
                _timeSinceTriggered += GameState.deltaTime * _ownerAttributeModifiers.fireRateModifier;
            }
            else
            {
                _timeSinceTriggered += GameState.deltaTime;
            }

            if(_onTriggerCooldown!=null)
            {
                if (_triggerCooldown > 0 ) _onTriggerCooldown.Invoke(Mathf.Min(_timeSinceTriggered / _triggerCooldown, 1.0f)); 
                else _onTriggerCooldown.Invoke(1);
            }
        }

#if UNITY_EDITOR
        protected void OnDrawGizmosSelected()
        {
            if( _spreadOffset > 0 )
            {
                UnityEditor.Handles.color = Color.yellow;

                if (_projectileLaunchPoint != null ) UnityEditor.Handles.DrawWireDisc(_projectileLaunchPoint.position, Vector3.up, _spreadOffset);
                else UnityEditor.Handles.DrawWireDisc(transform.position, Vector3.up, _spreadOffset);

                // TODO: draw spread lines and launch points.

            }
        }
#endif

        protected void OnDrawGizmos()
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(transform.position, transform.position + transform.forward * 1.0f);
        }

    }

}