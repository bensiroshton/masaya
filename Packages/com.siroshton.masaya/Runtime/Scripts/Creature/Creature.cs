using Siroshton.Masaya.Animation;
using Siroshton.Masaya.Attribute;
using Siroshton.Masaya.Core;
using Siroshton.Masaya.Math;
using Siroshton.Masaya.Weapon;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Siroshton.Masaya.Creature
{
    public class Creature : Entity.Entity
    {
        private enum State
        {
            WaitingToEngage,
            Engaged,
            ReturningHome
        }

        [Serializable]
        public class Home
        {
            public bool enabled = true;
            public float maxDistanceFromHome = 15;
            public float delayBeforeTryingToReturnHome = 2;
            public bool returnHomeOnPlayerDeath = true;

            public UnityEvent onReturningToHome;
            public UnityEvent onReturnedHome;

            [HideInInspector] public Vector3 position;
            [HideInInspector] public float timeSinceTryingToReturn;
        }

        [Serializable]
        public class Engagement
        {
            public bool alwaysEngaged = false;
            public bool alertOthers = true;
            public float alertOthersRange = 5;
            public float othersInRangeInterval = 1.0f;
            public UnityEvent onEngaged;

            [HideInInspector] public float timeSinceOthersCheck;
            [HideInInspector] public Collider[] otherColliders = new Collider[20];
        }

        public enum AutoFireMode
        {
            Never,
            Always,
            PlayerTargetAquired,
        }

        public enum TargetSearchMethod
        {
            SimpleRay,
        }


        [Serializable]
        public class WeaponFire
        {
            public AutoFireMode autoFireMode;
            public TargetSearchMethod targetSearchMode = TargetSearchMethod.SimpleRay;
            public IntervalFloat fireInterval;

            [HideInInspector] public float nextFireInterval;
            [HideInInspector] public IWeapon weapon;
        }

        [SerializeField] private Home _home = new Home();
        [SerializeField] private Engagement _engagement = new Engagement();
        [SerializeField] private WeaponFire _weaponFire = new WeaponFire();
        [SerializeField, Interface(typeof(IWeapon))] private MonoBehaviour _weapon;
        [SerializeField] private UnityEvent _onFireTriggerd;

        private State _state;
        private bool _isPlayerDead;

        public bool isEngaged => _state == State.Engaged;

        protected new void Awake()
        {
            base.Awake();

            _weaponFire.weapon = _weapon.GetComponent<IWeapon>();
            _weaponFire.nextFireInterval = _weaponFire.fireInterval.random;

            Player.Player.instance.onPlayerDied.AddListener(OnPlayerDied);
            Player.Player.instance.onPlayerRevived.AddListener(OnPlayerRevived);

#if UNITY_EDITOR
            if ( !_engagement.alwaysEngaged )
            {
                if( GetComponentInChildren<EngageZone>() == null )
                {
                    Debug.LogWarning($"EngageZone not found on creature {name}");
                }
            }
#endif
        }

        protected new void Start()
        {
            base.Start();

            _home.position = transform.position;
            _weaponFire.nextFireInterval = _weaponFire.fireInterval.random;

            if (_engagement.alwaysEngaged)
            {
                _state = State.Engaged;
            }

            if(_engagement.alertOthers)
            {
                StartCoroutine(CheckToEngageOthers());
            }
        }

        public void Fire()
        {
            _weaponFire.weapon.Trigger();
            _onFireTriggerd?.Invoke();
            _weaponFire.nextFireInterval = _weaponFire.fireInterval.random;
        }

        private void OnPlayerDied()
        {
            _isPlayerDead = true;

            if (_home.enabled && _home.returnHomeOnPlayerDeath)
            {
                ReturnHome();
            }
        }

        private void OnPlayerRevived()
        {
            _isPlayerDead = false;
        }

        protected override void OnHit()
        {
            base.OnHit();
            Engage();
        }

        public void Engage()
        {
            if (_state == State.Engaged || _isPlayerDead) return;

            _state = State.Engaged;
            _engagement.onEngaged?.Invoke();
        }

        private IEnumerator CheckToEngageOthers()
        {
            while(_engagement.alertOthers && !isDead)
            {
                if( _state == State.Engaged )
                {
                    int count = UnityEngine.Physics.OverlapSphereNonAlloc(transform.position, _engagement.alertOthersRange, _engagement.otherColliders, 1 << gameObject.layer);
                    for(int i=0;i<count;i++)
                    {
                        if (_engagement.otherColliders[i].GetComponent<Creature>() is Creature c)
                        {
                            c.Engage();
                        }
                    }
                }
                yield return new WaitForSeconds(_engagement.othersInRangeInterval);
            }
        }

        public void ReturnHome()
        {
            if (!_home.enabled || _state == State.WaitingToEngage || _state == State.ReturningHome || isDead ) return; // already home.

            _state = State.ReturningHome;
            _home.timeSinceTryingToReturn = 0;
            agent.SetDestination(_home.position);
        }

        private void UpdateWeaponFire()
        {
            if( _weaponFire.weapon == null || _weaponFire.autoFireMode == AutoFireMode.Never ) return;
            if( _weaponFire.weapon.timeSinceTriggered < _weaponFire.nextFireInterval ) return;

            if( _weaponFire.autoFireMode == AutoFireMode.Always )
            {
                Fire();
                return;
            }

            // if we get here then we will fire only if we have a target.
            bool hasTarget = false;

            if( _weaponFire.targetSearchMode == TargetSearchMethod.SimpleRay )
            {
                hasTarget = Physics.Raycast(_weapon.transform.position, _weapon.transform.forward, 10, GameLayers.playerMask);
            }

            if( hasTarget )
            {
                Fire();
            }
        }

        protected new void Update()
        {
            if (_state == State.WaitingToEngage) attributeModifiers.speedModifier = 0;
            else attributeModifiers.speedModifier = 1;

            base.Update();

            if ( _state == State.Engaged )
            {
                UpdateWeaponFire();

                if( _home.enabled )
                {
                    _home.timeSinceTryingToReturn += GameState.deltaTime;

                    if (_home.timeSinceTryingToReturn >= _home.delayBeforeTryingToReturnHome)
                    {
                        if ((transform.position - _home.position).sqrMagnitude > _home.maxDistanceFromHome * _home.maxDistanceFromHome)
                        {
                            ReturnHome();
                        }
                    }
                }
            }
            else if (_state == State.ReturningHome)
            {
                if (!agent.hasPath)
                {
                    if (_engagement.alwaysEngaged) _state = State.Engaged;
                    else _state = State.WaitingToEngage;

                    _home.onReturnedHome?.Invoke();
                }
            }
        }

    }

}