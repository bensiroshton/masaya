using Siroshton.Masaya.Core;
using Siroshton.Masaya.Environment;
using Siroshton.Masaya.Item;
using Siroshton.Masaya.Math;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

namespace Siroshton.Masaya.Entity
{
    [RequireComponent(typeof(GameStateHandler))]
    public class Entity : MonoBehaviour
    {
        public enum ReleaseEvent
        {
            OnDeath,
            OnDestroy,
            Manual
        }

        [Serializable]
        public class Events
        {
            public UnityEvent<GameObject> onHit;
            public UnityEvent<GameObject> onHealthChanged;
            public UnityEvent<GameObject> onKilled;
            public UnityEvent<GameObject> onDestroyed;
            public UnityEvent<GameObject> onNoFireZoneEnter;
            public UnityEvent<GameObject> onNoFireZoneExit;
            public UnityEvent<GameObject> onWaterEnter;
            public UnityEvent<GameObject> onWaterExit;
            public UnityEvent onItemsDropped;
        }

        [Serializable]
        public class Agent
        {
            public NavMeshAgent agent;
            public bool autoUpdateSpeed = true;
            public bool disableRotationsOnStart;
        }

        public enum OnPlayerDeathAction
        {
            DoNothing,
            Destroy,
            DestroyIfDead
        }

        [Serializable]
        public class Lifecycle
        {
            public int experience;
            public ReleaseEvent releaseExperienceMethod = ReleaseEvent.OnDeath;
            public bool destroyOnDeath;
            public OnPlayerDeathAction onPlayerDeath = OnPlayerDeathAction.Destroy;
            public float killWhenBelowY = -30;
            public bool moveToAreaOnDeath;
            [Tooltip("When hit apply this damage to this Entity.  Used by bullets for example that might die (or not) when they hit another object.")]
            public int selfDamageWhenHit;
            public bool showDamageText = true;
        }

        [SerializeField] private bool _isInvincible;
        [SerializeField] private LayerMask _allowHitMask;
        [SerializeField] private LayerMask _ignoreDamageMask;
        [SerializeField] private Attributes _attributes = new Attributes();
        [SerializeField] private AttributeModifiers _attributeModifiers = new AttributeModifiers();
        [SerializeField] private Lifecycle _lifecycle = new Lifecycle();
        [SerializeField] private Agent _agent = new Agent();
        [Tooltip("This is used for spawning experience points to be within the bounds of the given collider.")]
        [SerializeField] private ReleaseEvent _itemDropEvent = ReleaseEvent.OnDeath;
        [SerializeField] private ItemDropOptions _itemDrops;
        [SerializeField] private Events _events = new Events();

        private struct MotionInfo
        {
            public Vector3 lastPosition;
            public float currentSpeed;
            public Vector3 velocity;
            public Vector3 velocitySmoothVelocity;

            public float relativeTurnAngle;
            public float relativeTurnAngleVelocity;
            public float lastYRotation;
        }

        private int _health;
        private bool _inNoFireZone;
        private bool _inWater;

        private MotionInfo _motion;

        protected NavMeshAgent agent => _agent.agent;

        virtual protected void OnHit() { }
        virtual protected void OnHealthChanged() { }
        virtual protected void OnKilled() { }

        public Attributes attributes { get => _attributes; }
        public AttributeModifiers attributeModifiers { get => _attributeModifiers; }
        public bool isInvincible { get => _isInvincible; set => _isInvincible = value; }
        public bool isDead { get => _health <= 0; }
        public int experience { get => _lifecycle.experience; set => _lifecycle.experience = value; }
        public bool destroyOnDeath { get => _lifecycle.destroyOnDeath; set => _lifecycle.destroyOnDeath = value; }
        public bool inNoFireZone { get => _inNoFireZone; }
        public bool inWater { get => _inWater; }
        public float killWhenBelowY { get => _lifecycle.killWhenBelowY; set => _lifecycle.killWhenBelowY = value; }
        public float speed { get => _attributes.GetSpeed(_attributeModifiers); }
        public float currentSpeed => _motion.currentSpeed;
        public float relativeTurnAngle => _motion.relativeTurnAngle;
        public Vector3 velocity => _motion.velocity;

        public int GetDamage(out bool isCritical)
        {
            return _attributes.GetDamage(_attributeModifiers, out isCritical);
        }

        public UnityEvent<GameObject> onHit { get => _events.onHit; set => _events.onHit = value; }
        public UnityEvent<GameObject> onHealthChanged { get => _events.onHealthChanged; set => _events.onHealthChanged = value; }
        public UnityEvent<GameObject> onKilled { get => _events.onKilled; set => _events.onKilled = value; }
        public UnityEvent<GameObject> onDestroyed { get => _events.onDestroyed; set => _events.onDestroyed = value; }
        public UnityEvent<GameObject> onNoFireZoneEnter { get => _events.onNoFireZoneEnter; set => _events.onNoFireZoneEnter = value; }
        public UnityEvent<GameObject> onNoFireZoneExit { get => _events.onNoFireZoneExit; set => _events.onNoFireZoneExit = value; }
        public UnityEvent onItemsDropped { get => _events.onItemsDropped; set => _events.onItemsDropped = value; }
        public UnityEvent<GameObject> onWaterEnter { get => _events.onWaterEnter; set => _events.onWaterEnter = value; }
        public UnityEvent<GameObject> onWaterExit { get => _events.onWaterExit; set => _events.onWaterExit = value; }

        internal void ResetMotionInfo()
        {
            _motion.lastPosition = transform.position;
            _motion.currentSpeed = 0;
            _motion.velocity = Vector3.zero;
            _motion.velocitySmoothVelocity = Vector3.zero;

            _motion.relativeTurnAngle = 0;
            _motion.relativeTurnAngleVelocity = 0;
            _motion.lastYRotation = 0;
        }

        public int health 
        { 
            get => _health; 
            set
            {
                bool wasZero = _health == 0;
                _health = System.Math.Max(0, value);

                OnHealthChanged();
                _events.onHealthChanged?.Invoke(gameObject);

                if ( _health == 0 && !wasZero )
                {
                    OnKilled();
                    _events.onKilled?.Invoke(gameObject);

                    if(_lifecycle.releaseExperienceMethod == ReleaseEvent.OnDeath) ReleaseExperience();
                    if(_itemDropEvent == ReleaseEvent.OnDeath) TryItemDrops();

                    if(_lifecycle.moveToAreaOnDeath)
                    {
                        Area area = Area.FindNearest(transform.position);
                        if( area != null )
                        {
                            transform.SetParent(area.transform, true);
                        }
                    }
                }
            }
        }

        protected void Awake()
        {
            // keep Awake() even if empty since we use this class as a base class and those should call call base.Awake()

            if( _agent.agent == null ) _agent.agent = GetComponentInChildren<NavMeshAgent>();
            if( _agent.agent != null ) 
            {
                if(_agent.disableRotationsOnStart) _agent.agent.updateRotation = false;
                if(_agent.autoUpdateSpeed) StartCoroutine(UpdateAgent());
            }

            _health = _attributes.maxHealth;
        }

        protected void Start()
        {
            // keep Start() even if empty since we use this class as a base class and those should call call base.Start()
            _motion.lastPosition = transform.position;
            _motion.velocity = transform.forward;
            _motion.lastYRotation = transform.rotation.eulerAngles.y;

            Player.Player.instance.onPlayerRevived.AddListener(OnPlayerRevived);
        }

        private void OnPlayerRevived()
        {
            if( _lifecycle.onPlayerDeath == OnPlayerDeathAction.Destroy )
            {
                Destroy();
            }
            else if( _lifecycle.onPlayerDeath == OnPlayerDeathAction.DestroyIfDead )
            {
                DestroyIfDead();
            }
        }

        public void DisableAllComponents(bool recursive)
        {
            MonoBehaviour[] components;
            if( recursive ) components = GetComponentsInChildren<MonoBehaviour>();
            else components = GetComponents<MonoBehaviour>();

            for(int i=0;i<components.Length;i++)
            {
                components[i].enabled = false;
            }
        }

        protected void OnDestroy()
        {
            if (_lifecycle.releaseExperienceMethod == ReleaseEvent.OnDestroy) ReleaseExperience();
            if (_itemDropEvent == ReleaseEvent.OnDestroy) TryItemDrops();

            _events.onDestroyed?.Invoke(gameObject);
        }

        public void TryItemDrops()
        {
            if (_itemDrops.attemptChanceDrops && GameManager.instance.TryDropItems(transform.position, _itemDrops)?.Count > 0)
            {
                _events.onItemsDropped?.Invoke();
            }
        }

        public void ReleaseExperience()
        {
            if (_lifecycle.experience > 0)
            {
                Collider[] colliders = GetComponentsInChildren<Collider>();
                int count = 0;
                for(int i=0;i<colliders.Length;i++)
                {
                    if( (colliders[i].gameObject.layer & GameLayers.enemiesMask) > 0 )
                    {
                        ++count;
                    }
                    else
                    {
                        colliders[i] = null;
                    }
                }

                if( count == 0 )
                {
                    Player.Player.instance.SpawnExperience(transform.position + Vector3.up * _itemDrops.dropRadius, _itemDrops.dropRadius, _lifecycle.experience);
                }
                else
                {
                    int expEach = (int)((float)_lifecycle.experience / (float)count);

                    for(int i=0;i<colliders.Length;i++)
                    {
                        if( colliders[i] != null )
                        {
                            Bounds b = colliders[i].bounds;
                            Player.Player.instance.SpawnExperience(b.center, MathUtil.GetRadius(b), expEach);
                        }
                    }

                }

                _lifecycle.experience = 0;
            }
        }

        public Vector3 GetFutureLocation(float speedTraveling, Vector3 fromPosition)
        {
            return GetFutureLocation(speedTraveling / (transform.position - fromPosition).magnitude);
        }

        public Vector3 GetFutureLocation(float futureSeconds)
        {            
            return transform.position + velocity * futureSeconds;
        }

        private IEnumerator UpdateAgent()
        {
            while( _agent.agent != null && _agent.autoUpdateSpeed )
            {
                _agent.agent.speed = speed;
                yield return new WaitForSeconds(1.0f);
            }
        }

        public void UpdateLayerInfo()
        {
            _inNoFireZone = false;
            _inWater = false;

            int layerMask = GameLayers.noFireZoneMask | GameLayers.waterMask;
            Collider[] colliders = UnityEngine.Physics.OverlapBox(transform.position, new Vector3(0.5f, 0.5f, 0.5f), transform.rotation, layerMask, QueryTriggerInteraction.Collide);
            foreach(Collider c in colliders)
            {
                if( (c.gameObject.layer & GameLayers.noFireZone) > 0 ) _inNoFireZone = true;
                if ((c.gameObject.layer & GameLayers.water) > 0) _inWater = true;
            }
        }

        protected void Update()
        {
            float deltaTime = GameState.deltaTime;
            if (deltaTime == 0) return;

            if (transform.position.y < _lifecycle.killWhenBelowY) Kill();

            // update velocity
            Vector3 newVelocity = (transform.position - _motion.lastPosition) / deltaTime;
            newVelocity = Vector3.SmoothDamp(velocity, newVelocity, ref _motion.velocitySmoothVelocity, 0.1f, Mathf.Infinity, deltaTime);
               
            _motion.currentSpeed = newVelocity.magnitude;
            _motion.velocity = newVelocity;
            _motion.lastPosition = transform.position;

            // update relative turn angle
            float yRotation = transform.rotation.eulerAngles.y;
            float turnAngle = yRotation - _motion.lastYRotation;
            _motion.relativeTurnAngle = Mathf.SmoothDampAngle(_motion.relativeTurnAngle, turnAngle, ref _motion.relativeTurnAngleVelocity, 0.1f, Mathf.Infinity, deltaTime);
            _motion.lastYRotation = yRotation;
        }

        /// <summary>
        /// Restore health.
        /// </summary>
        /// <param name="newHealth">New health to revive to (does not increase max health if value given is greater than max).</param>
        public void Revive(int newHealth)
        {
            if( newHealth == 0 ) _health = _attributes.maxHealth;
            else _health = newHealth;
        }

        /// <summary>
        /// Kill the Entity, this is the same as setting the health to zero.
        /// </summary>
        public void Kill()
        {
            health = 0;
        }

        public void Destroy()
        {
            Destroy(gameObject);
        }

        public void DestroyIfDead()
        {
            if( isDead ) Destroy(gameObject);
        }

        public void OnCollisionEnter(Collision collision)
        {
            HandleCollision(collision.collider);
        }

        public void OnTriggerEnter(Collider collider)
        {
            HandleCollision(collider);
        }

        private void HandleCollision(Collider collider)
        {
            if (!enabled) return;
            if (!collider.gameObject.activeInHierarchy) return; // this only happens when we give the player items which are added to the player heiarchy as the result of a trigger.

            if (collider.gameObject.layer == GameLayers.noFireZone)
            {
                if (!_inNoFireZone)
                {
                    Debug.Log($"{name} entered no fire zone.");
                    _inNoFireZone = true;
                    _events.onNoFireZoneEnter?.Invoke(gameObject);
                }
                return;
            }
            else if (collider.gameObject.layer == GameLayers.water)
            {
                if (!_inWater)
                {
                    Debug.Log($"{name} entered water.");
                    _inWater = true;
                    _events.onWaterEnter?.Invoke(gameObject);
                }
                return;
            }

            if (!_isInvincible && !inNoFireZone && (_allowHitMask & 1 << collider.gameObject.layer) != 0)
            {
                OnHit();
                _events.onHit?.Invoke(gameObject);

                if (health > 0)
                {
                    Entity them = collider.gameObject.GetComponentInParent<Entity>();
                    if (them != null)
                    {
                        if (!them.enabled) return;
                        if ((_ignoreDamageMask & 1 << collider.gameObject.layer) == 0)
                        {
                            bool isCritical;
                            int damage = them.GetDamage(out isCritical);

                            if (_lifecycle.showDamageText)
                            {
                                bool colliderEnabled = collider.enabled;
                                collider.enabled = true; // if the collider is not enabled then ClosestPoint will return its gameObject.transform.position
                                GameManager.instance.damageText.SpawnDamage(collider.ClosestPoint(transform.position), damage, isCritical);
                                collider.enabled = colliderEnabled;
                            }
                            health -= damage;
                        }
                    }
                    health -= _lifecycle.selfDamageWhenHit;
                }
            }
        }

        public void OnTriggerExit(Collider collider)
        {
            if( collider.gameObject.layer == GameLayers.noFireZone )
            {
                if( _inNoFireZone )
                {
                    Debug.Log($"{name} left no fire zone.");
                    _inNoFireZone = false;
                    _events.onNoFireZoneExit?.Invoke(gameObject);
                }
                return;
            }
            else if ( collider.gameObject.layer == GameLayers.water )
            {
                if (_inWater)
                {
                    Debug.Log($"{name} left water.");
                    _inWater = false;
                    _events.onWaterExit?.Invoke(gameObject);
                }
                return;
            }

        }

        private void LateUpdate()
        {
            if (_lifecycle.destroyOnDeath && _health == 0) Destroy(gameObject);
        }

#if UNITY_EDITOR
        protected void OnDrawGizmosSelected()
        {
            UnityEditor.Handles.color = Color.magenta;
            UnityEditor.Handles.DrawWireDisc(transform.position, Vector3.up, _itemDrops.dropRadius);
        }
#endif

        protected void OnDrawGizmos()
        {
            // Future Location
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(GetFutureLocation(1), 0.1f);
        }

    }

}