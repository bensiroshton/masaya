
using Siroshton.Masaya.Audio;
using Siroshton.Masaya.Component;
using Siroshton.Masaya.Core;
using Siroshton.Masaya.Environment;
using Siroshton.Masaya.Item;
using Siroshton.Masaya.Math;
using Siroshton.Masaya.Weapon;
using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace Siroshton.Masaya.Player
{
    [RequireComponent(typeof(CharacterController))]
    //[RequireComponent(typeof(Animator))]
    public class Player : Entity.Entity
    {
        [Serializable]
        public class Attachments
        {
            public Transform leftWristPoint;
            public Transform rightWristPoint;
            public Transform primaryRootLeft;
            public Transform primaryRootRight;
            public Transform primaryRoot;
            public Transform primaryWeaponPoint;
            public Transform equipmentFoundPoint;
            public float foundEquipmentHoldTime = 4.0f;
            public GameObject leftFootItem;
            public GameObject rightFootItem;
        }

        // aiming
        [SerializeField, Range(0, 1)] private float _aimSmoothingFactor = 0;
        [SerializeField, Range(0, 1)] private float _aimSmoothingTime = 0.1f;
        [SerializeField, Range(0, 1)] private float _autoTargetFactor = 0.0f;
        [SerializeField, Range(0, 10)] private float _autoTargetMinDistance = 2.0f;
        [SerializeField, Range(0, 10)] private float _autoTargetMinDistanceTargetFactor = 0.25f;
        [SerializeField, Range(0, 3)] private float _autoTargetMinCastRadius = 0.5f;
        [SerializeField, Range(0, 5)] private float _autoTargetMaxCastRadius = 2.0f;
        [SerializeField, Range(0, 45)] private float _maxAutoTargetAngle = 35;
        [SerializeField] private LayerMask _autoTargetCastMask;
        [Tooltip("If the next fire angle is over this threshold then ignore the guns trigger time requirements. 0 disables the check.")]
        [SerializeField, Range(0, 180)] private float _fireTriggerAngleThreshold = 0;
        // animation
        [SerializeField] private float _moveToIdleTime = 0.3f;
        [SerializeField, Range(0, 2)] private float _lookAtTime = 1.0f;
        [SerializeField, Range(0, 3)] private float _deathLingerTime = 1.0f;
        // scene
        [SerializeField] private PlayerCamera _camera;
        // model
        [SerializeField] private Transform _modelRoot;
        [SerializeField] private Transform _head;
        [SerializeField] private Transform _torso;
        // attachments
        [SerializeField] private Attachments _attachments;
        // audio
        [SerializeField] private AudioClip _footStepStandard;
        [SerializeField] private AudioClip _footStepWater;
        [SerializeField] private IntervalFloat _footStepPitchRange;
        [SerializeField] private float _maxFootStepVolume = 0.15f;
        [SerializeField] private AudioSourceController _footStepAudio;
        // events
        [SerializeField] private UnityEvent _onFootStep = new UnityEvent();
        [SerializeField] private UnityEvent _onLeftFootStep = new UnityEvent();
        [SerializeField] private UnityEvent _onRightFootStep = new UnityEvent();
        [SerializeField] private UnityEvent _onPlayerDied = new UnityEvent();
        [SerializeField] private UnityEvent _onPlayerRevived = new UnityEvent();
        [SerializeField] private UnityEvent _onEquipmentFoundBegin = new UnityEvent();
        [SerializeField] private UnityEvent _onEquipmentFoundHoldStart = new UnityEvent();
        [SerializeField] private UnityEvent _onEquipmentFoundHoldEnd = new UnityEvent();
        [SerializeField] private UnityEvent _onEquipmentFoundEnd = new UnityEvent();

        private Gun _primaryWeapon;

        private CharacterController _charCon;
        private CharacterSheet _characterSheet;
        private AudioSource _audioSource;
        private CircleZone _circleZone;
        private Transform _autoLockTarget;
        private ExperiencePointParticles _experiencePoints;

        private enum PlayerState
        {
            Playing,
            FoundEquipment,
            Dying,
            BeingCarried,
            Waiting,
        }

        private struct MotionState
        {
            public Vector3 requestedMoveDir;
            public Vector3 moveDir;
            public float moveRotation;
            public float moveSpeed;

            public Vector3 requestedAimDir;
            public Vector3 aimDir;
            public float aimRotation;
            public float aimMoveAngle;

            public void Clear()
            {
                requestedMoveDir = Vector3.zero;
                moveDir = Vector3.zero;
                moveRotation = 0;
                moveSpeed = 0;

                requestedAimDir = Vector3.zero;
                aimDir = Vector3.zero;
                aimRotation = 0;
                aimMoveAngle = 0;
            }
        }

        private struct Aiming
        {
            public Vector3 smoothedAimDir;
            public Vector3 smoothAimVelocity;
            public Vector3 autoTargetDir;
            public Vector3 finalDir;
            public bool hasLock;
            public float autoTargetDistance;
            public Vector3 rawLockPoint;
            public Vector3 lastFireDirection;
            public float fireAngleChange;
            public int fireAngleBursts;
            public Collider[] hits;
            public Collider lockCollider;
            public Entity.Entity lockEntity;

            public void Clear()
            {
                hasLock = false;
                smoothedAimDir = Vector3.zero;
                autoTargetDir = Vector3.zero;
                finalDir = Vector3.zero;
                smoothAimVelocity = Vector3.zero;
            }
        }


        private struct AnimState
        {
            public Animator animator;

            public int speedId;
            public float speedBlendTarget;
            public float speedBlendTargetVelocity;

            public int aimId;
            public float aimBlendTarget;
            public float aimBlendTargetVelocity;
            public int aimLayer;
            public float aimLayerWeightTarget;
            public float aimWeightTargetVelocity;

            public int equipmentFoundStartId;
            public int equipmentFoundEndId;

            public int dieId;
            public int reviveId;

            public Vector3 lookAtTarget;
            public float lookAtWeight;
            public float lookAtTargetWeight;
            public float lookAtTargetWeightVelocity;
        }

        private struct FoundEquipmentSequence
        {
            public float time;
            public int step;
            public IEquipment equipemnt;
        }

        private struct DeathSequence
        {
            public float time;
            public int step;
        }

        private PlayerState _state = PlayerState.Playing;
        private MotionState _motion;
        private Aiming _aiming;
        private AnimState _anim;
        private FoundEquipmentSequence _foundEquipment;
        private DeathSequence _death;

        static private Player _instance;

        static public Player instance
        {
            get
            {
                if( _instance == null )
                {
                    GameObject o = GameObject.Find("Player");
                    if( o == null ) Debug.LogError("Unable to find object `Player`.");
                    _instance = o.GetComponent<Player>();
                    if (_instance == null) Debug.LogError("`Player` GameObject is missing 'Player' Component.");
                }
                return _instance;
            }
        }

        public Transform head { get => _head; }
        public Transform torso { get => _torso; }
        public CharacterSheet characterSheet { get => _characterSheet; }
        public Gun primaryWeapon { get => _primaryWeapon; set => _primaryWeapon = value; }
        public UnityEvent onPlayerDied { get => _onPlayerDied; set => _onPlayerDied = value; }
        public UnityEvent onPlayerRevived { get => _onPlayerRevived; set => _onPlayerRevived = value; }
        public CircleZone circleZone { get => _circleZone; }
        public float height { get => _charCon.height; }
        public Vector3 inputAimDirection { get => _motion.requestedAimDir; }
        public Vector3 lookAtTarget { get => _anim.lookAtTarget; set => _anim.lookAtTarget = value; }
        public float lookAtTargetWeight { get => _anim.lookAtTargetWeight; set => _anim.lookAtTargetWeight = value; }
        public bool isWaiting => _state == PlayerState.Waiting;

        protected new void Awake()
        {
            base.Awake();

            if( _instance == null ) _instance = GetComponent<Player>();
            if( _camera == null ) _camera = Camera.main.GetComponent<PlayerCamera>();
            _experiencePoints = GetComponentInChildren<ExperiencePointParticles>();

            // move the AudioListener to the scene root, it has a parent constraint to follow the player around.
            // this is so we can lock its rotation.
            AudioListener al = GetComponentInChildren<AudioListener>();
            al.gameObject.transform.SetParent(null);
            al.gameObject.transform.rotation = Quaternion.identity;

            _charCon = GetComponent<CharacterController>();

            _characterSheet = GetComponent<CharacterSheet>();
            _characterSheet.onCardReceived.AddListener(OnCardReceived);
            _characterSheet.onCardRemoved.AddListener(OnCardRemoved);
            _characterSheet.onCardEquipped.AddListener(OnCardEquipped);
            _characterSheet.onCardUnEquipped.AddListener(OnCardUnEquipped);
            if ( _characterSheet == null ) Debug.LogError("CharacterSheet must exist!");
            _audioSource = GetComponent<AudioSource>();
            _circleZone = GetComponent<CircleZone>();

            // animation
            _anim.animator = GetComponentInChildren<Animator>();
            _anim.speedId = Animator.StringToHash("speed");
            _anim.aimId = Animator.StringToHash("aim");
            _anim.aimLayer = _anim.animator.GetLayerIndex("Aiming");
            _anim.equipmentFoundStartId = Animator.StringToHash("startEquipmentFound");
            _anim.equipmentFoundEndId = Animator.StringToHash("endEquipmentFound");
            _anim.dieId = Animator.StringToHash("die");
            _anim.reviveId = Animator.StringToHash("revive");

            _aiming.hits = new Collider[10];
        }

        public void PlayAudio(AudioClip clip)
        {
            _audioSource.PlayOneShot(clip);
        }

        public void SpawnExperience(Vector3 position, float radius, int experiencePoints)
        {
            _experiencePoints.SpawnPoints(position, radius, experiencePoints);
        }

        public bool Raycast(Ray ray, out RaycastHit hitInfo, float maxDistance)
        {
            return _charCon.Raycast(ray, out hitInfo, maxDistance);
        }

        private void OnCardReceived(SkillCard card)
        {
            RecalculateModifiers();
            card.onCardChange.AddListener(OnCardChange);
        }

        private void OnCardChange(SkillCard card)
        {
            RecalculateModifiers();
        }

        private void OnCardRemoved(SkillCard card)
        {
            card.onCardChange.RemoveListener(OnCardChange);
            RecalculateModifiers();
        }

        private void OnCardEquipped(SkillCard card)
        {
            RecalculateModifiers();
        }

        private void OnCardUnEquipped(SkillCard card)
        {
            RecalculateModifiers();
        }

        private void RecalculateModifiers()
        {
            attributeModifiers.Reset();

            SkillCard[] cards = _characterSheet.GetEquippedCards();
            for(int i=0;i<cards.Length;i++)
            {
                SkillCard card = cards[i];
                attributeModifiers.AddModifier(card.modifier, card.currentLevelModifier);
            }
        }

        private void PlayFootStepAudio()
        {
            _footStepAudio.volumeTarget = _maxFootStepVolume * currentSpeed / speed;

            if (currentSpeed > 0.01f)
            {
                if (inWater) _footStepAudio.PlayClip(_footStepWater);
                else _footStepAudio.PlayClip(_footStepStandard);
            }
        }

        public void Anim_OnLeftFootStep()
        {
            PlayFootStepAudio();
            _onFootStep?.Invoke();
            _onLeftFootStep?.Invoke();
        }

        public void Anim_OnRightFootStep()
        {
            PlayFootStepAudio();
            _onFootStep?.Invoke();
            _onRightFootStep?.Invoke();
        }

        protected override void OnKilled() 
        {
            if (_state != PlayerState.Playing) return;

            HandleDeath();
        }

        private void ZeroAnimations()
        {
            _anim.speedBlendTarget = 0;
            _anim.aimLayerWeightTarget = 0;
            _anim.aimBlendTarget = 0;

            _anim.animator.SetFloat(_anim.speedId, 0);
            _anim.animator.SetFloat(_anim.aimId, 0.5f);
            _anim.animator.SetLayerWeight(_anim.aimLayer, 0);
        }

        private void HandleDeath()
        {
            Debug.Log("Player died.");

            _state = PlayerState.Dying;
            onPlayerDied.Invoke();
            GameEvents.TriggerPlayerDeathEvents();

            SetDefaultParent();
            ZeroAnimations();
            _anim.animator.SetTrigger(_anim.dieId);
            isInvincible = true;

            _death.time = 0;
            _death.step = 0;
        }

        public void Anim_OnDeathFinished()
        {
            _death.time = 0;
            _death.step++;
        }

        public void Anim_OnReviveFinished()
        {
            _death.time = 0;
            _death.step++;
        }

        private void UpdateDeath()
        {
            _death.time += GameState.deltaTime;

            // step 0 gets triggered by animation event Anim_OnDeathFinished
            // step 2 gets triggered by animation event Anim_OnReviveFinished

            if ( _death.step == 1)
            {
                if (_death.time > _deathLingerTime)
                {
                    // revive player at the last checkpoint
                    Transport(Checkpoint.activeCheckpoint.spawnPoint);

                    _characterSheet.DestroyBottlesFromCharacterDeath();
                    _characterSheet.ClearCurrentBlockExperience();

                    _anim.animator.SetTrigger(_anim.reviveId);

                    Revive(attributes.maxHealth);

                    _onPlayerRevived.Invoke();
                    GameEvents.TriggerPlayerRevivedEvents();

                    _death.time = 0;
                    _death.step++;
                }

            }
            else if ( _death.step == 3)
            {
                _state = PlayerState.Playing;
                isInvincible = false;
            }
        }

        public void GiveItem(IItem item)
        {
            if (item.soundOnPickup != null) PlayAudio(item.soundOnPickup);

            if (item is IEquipment e)
            {
                FoundEquipment(e);
            }
            else if (item is Bottle bottle)
            {
                characterSheet.bottleCount += bottle.bottleCount;
                Destroy(bottle.gameObject);
            }
            else if (item is SkillCard card)
            {
                characterSheet.GiveCard(card);
            }
            else
            {
                Debug.LogError($"Not handling {item} in Player.GiveItem.");
            }

            GameManager.instance.ui.PickedUpItem(item);
        }

        public void FoundEquipment(IEquipment equipment)
        {
            if( _state != PlayerState.Playing )
            {
                Debug.LogError($"Player state must be Playing to find equipment.  Current State: {_state}");
                return;
            }

            _state = PlayerState.FoundEquipment;

            _foundEquipment.time = 0;
            _foundEquipment.step = 0;
            _foundEquipment.equipemnt = equipment;
            
            equipment.gameObject.SetActive(false);
            equipment.gameObject.transform.SetParent(_attachments.equipmentFoundPoint);
            equipment.gameObject.transform.localPosition = Vector3.zero;
            equipment.gameObject.transform.localRotation = Quaternion.identity;

            _anim.speedBlendTarget = 0;
            _anim.aimBlendTarget = 0;

            _onEquipmentFoundBegin.Invoke();
            _foundEquipment.equipemnt.TriggerFoundBegin();
        }

        public void Equip(IEquipment equipment)
        {
            if( equipment as Bangles ) Equip(equipment as Bangles);
            else Destroy(equipment.gameObject);
        }

        public void Equip(Bangles bangles)
        {
            if( _primaryWeapon != null )
            {
                _primaryWeapon.onBulletSpawned.RemoveListener(OnBulletSpawned);
            }

            _primaryWeapon = GameObject.Instantiate<GameObject>(bangles.gunPrefab).GetComponent<Gun>();
            _primaryWeapon.transform.SetParent(_attachments.primaryWeaponPoint);
            _primaryWeapon.transform.localPosition = Vector3.zero;
            _primaryWeapon.transform.localRotation = Quaternion.identity;

            _primaryWeapon.ownerAttributeModifiers = attributeModifiers;
            primaryWeapon.onBulletSpawned.AddListener(OnBulletSpawned);

            GameObject model = GameObject.Instantiate(bangles.leftHandModel);
            model.transform.SetParent(_attachments.leftWristPoint);
            model.transform.localPosition = Vector3.zero;
            model.transform.localRotation = Quaternion.identity;

            model = GameObject.Instantiate(bangles.rightHandModel);
            model.transform.SetParent(_attachments.rightWristPoint);
            model.transform.localPosition = Vector3.zero;
            model.transform.localRotation = Quaternion.identity;

            if( bangles.soundOnEquip != null ) PlayAudio(bangles.soundOnEquip);

            // These attachments should probably come from the equipment itself, but for now were just enabling the foot particles.
            if (_attachments.leftFootItem != null) _attachments.leftFootItem.SetActive(true);
            if (_attachments.rightFootItem != null) _attachments.rightFootItem.SetActive(true);

            Destroy(bangles.worldItem);
            Destroy(bangles);
        }

        public void Transport(Transform to)
        {
#if UNITY_EDITOR
            if( to == null )
            {
                Debug.LogError("Trying to Transport to a null location.");
            }
#endif
            Transport(to.position, to.rotation);
        }

        public void Transport(Vector3 position, Quaternion rotation)
        {
            CharacterController c = GetComponent<CharacterController>();
            c.enabled = false;
            transform.position = position;
            transform.rotation = rotation;
            c.enabled = true;
            _camera.SnapToTarget();
            _motion.Clear();
            _motion.moveRotation = transform.rotation.eulerAngles.y;
            _modelRoot.transform.localRotation = Quaternion.identity;
            base.ResetMotionInfo();

            UpdateLayerInfo();
        }

        public void Move(Vector3 direction)
        {
            if( _state != PlayerState.Playing ) direction = Vector3.zero;
            _motion.requestedMoveDir = direction;
        }

        public void Aim(Vector3 direction)
        {
            if (_state != PlayerState.Playing) direction = Vector3.zero;
            _motion.requestedAimDir = direction;
        }

        public void SetBeingCarried(bool isCarried)
        {
            if( isCarried ) _state = PlayerState.BeingCarried;
            else _state = PlayerState.Playing;
        }

        public void Wait()
        {
            _state = PlayerState.Waiting;
            ZeroAnimations();
        }

        public void SetPlaying()
        {
            if( _state == PlayerState.Playing ) return;

            _state = PlayerState.Playing;
            _motion.moveRotation = transform.rotation.eulerAngles.y;
        }

        public void SetDefaultParent()
        {
            transform.SetParent(null, true);
            SceneManager.MoveGameObjectToScene(gameObject, GameManager.instance.gameObject.scene);
        }

        protected new void Update()
        {
            base.Update();

            if (_state == PlayerState.Dying)
            {
                UpdateDeath();
            }
            else if (_state == PlayerState.Playing)
            {
                UpdateMotion();
            }
            else if (_state == PlayerState.FoundEquipment)
            {
                UpdateFoundEquipmentSequence();
            }

            // update look at weight
            _anim.lookAtWeight = Mathf.SmoothDamp(_anim.lookAtWeight, _anim.lookAtTargetWeight, ref _anim.lookAtTargetWeightVelocity, _lookAtTime);
            _anim.lookAtTargetWeight = 0; // this will get reset elsewhere but in case it doesn't we remove the look at target over time.
        }

        public void OnAnimatorIK(int layerIndex)
        {
            _anim.animator.SetLookAtPosition(_anim.lookAtTarget);
            _anim.animator.SetLookAtWeight(_anim.lookAtWeight);
        }

        private int CollectAimingHits(float maxDistance)
        {
            int steps = 5;

            Vector3 fwd = _motion.aimDir.normalized;
            Vector3 startTop = transform.position + Vector3.up * _charCon.height + fwd * _autoTargetMinDistance;
            Vector3 startBottom = startTop;
            startBottom.y = transform.position.y;

            Vector3 endTop = transform.position + Vector3.up * _charCon.height + fwd * maxDistance;
            Vector3 endBottom = endTop;
            endBottom.y = transform.position.y;

            for (int i=0;i<steps;i++)
            {
                float step = i / (float)steps;
                Vector3 top = Vector3.Lerp(startTop, endTop, step);
                Vector3 bottom = Vector3.Lerp(startBottom, endBottom, step);
                float radius = Mathf.Lerp(_autoTargetMinCastRadius, _autoTargetMaxCastRadius, step);

                int count = UnityEngine.Physics.OverlapCapsuleNonAlloc(top, bottom, radius, _aiming.hits, _autoTargetCastMask);
                if( count > 0 ) return count;
            }

            return 0;
        }

        private void UpdateAiming(float aimMag)
        {
            _aiming.finalDir = _motion.aimDir;
            _aiming.autoTargetDir = Vector3.zero;
            _aiming.hasLock = false;
            _aiming.lockCollider = null;
            _aiming.lockEntity = null;

            if (aimMag > 0 && _autoTargetFactor > 0)
            {
                Gun.GunBulletProperties bulletProps = _primaryWeapon.GetGunBulletProperties();

                Vector3 top = transform.position + Vector3.up * _charCon.height + _motion.aimDir * _autoTargetMinDistance;
                Vector3 bottom = top;
                bottom.y = transform.position.y;

                int hitCount = CollectAimingHits(bulletProps.range);
                if (hitCount > 0)
                {
                    // find nearest hit to our users aim line
                    float d = (_aiming.hits[0].bounds.center - _primaryWeapon.transform.position).magnitude;
                    Vector3 checkPoint = _primaryWeapon.transform.position + _motion.aimDir * d;
                    float nearest = 10000000;
                    Vector3 hitPoint = Vector3.zero;

                    for(int i=0;i<hitCount;i++)
                    {
                        Vector3 point = _aiming.hits[i].ClosestPoint(checkPoint);
                        float sqrMag = (point - checkPoint).sqrMagnitude;
                        if (  sqrMag < nearest )
                        {
                            hitPoint = point;
                            nearest = sqrMag;
                            _aiming.lockCollider = _aiming.hits[i];
                        }
                    }

                    _aiming.rawLockPoint = hitPoint;

                    _aiming.lockEntity = _aiming.lockCollider.GetComponentInChildren<Entity.Entity>();
                    if(_aiming.lockEntity == null ) _aiming.lockEntity = _aiming.lockCollider.GetComponentInParent<Entity.Entity>();

                    if(_aiming.lockEntity != null )
                    {
                        lookAtTarget = hitPoint;
                        lookAtTargetWeight = 1.0f;

                        // Calculate the future impact point based on the gun position, enemy position
                        // and bullet speed.

                        Vector3 gunLine = _aiming.lockEntity.transform.position - _primaryWeapon.transform.position;
                        gunLine.y = _aiming.rawLockPoint.y;

                        Vector3 futurePoint = _aiming.lockEntity.GetFutureLocation(gunLine.magnitude / bulletProps.speed);
                        Vector3 futurePointOffset = futurePoint - _aiming.lockEntity.transform.position;
                        hitPoint += futurePointOffset;
                        hitPoint.y = _aiming.rawLockPoint.y;
                    }

                    _aiming.autoTargetDir = hitPoint - _primaryWeapon.transform.position;
                    _aiming.autoTargetDistance = _aiming.autoTargetDir.magnitude;
                    _aiming.autoTargetDir = _aiming.autoTargetDir.normalized;
                    _aiming.hasLock = true;

                    // factor in how much auto aiming is wanted to our final direction
                    _aiming.finalDir = Vector3.Lerp(_motion.aimDir, _aiming.autoTargetDir, _autoTargetFactor);

                    // adjust finalDir if we have a max target angle.
                    if (_maxAutoTargetAngle > 0)
                    {
                        float distanceFactor = _aiming.autoTargetDistance / bulletProps.range;
                        if( distanceFactor > 1 ) distanceFactor = 1;

                        distanceFactor = Mathf.Lerp(_autoTargetMinDistanceTargetFactor, 1.0f, distanceFactor); // adjust the factor

                        float maxTargetAngle = _maxAutoTargetAngle * distanceFactor;

                        // only check our Y rotation.
                        float angleOffset = Vector3.SignedAngle(_motion.aimDir, new Vector3(_aiming.finalDir.x, 0, _aiming.finalDir.z), Vector3.up);
                        if( Mathf.Abs(angleOffset) > maxTargetAngle)
                        {
                            // reverse finalDir rotation back towards aimDir, we do this to maintain our vertical angle.
                            Quaternion r;
                            if ( angleOffset > 0 ) r = Quaternion.Euler(0, maxTargetAngle - angleOffset, 0);
                            else r = Quaternion.Euler(0, -(maxTargetAngle + angleOffset), 0);

                            _aiming.finalDir = r * _aiming.finalDir;
                        }
                    }
                }
            }
    
            _aiming.fireAngleChange = Vector3.Angle(_aiming.finalDir, _aiming.lastFireDirection); // lastFireDirection is updated when the gun is actually fired.

            // check if we are snapping to the new fire angle, if we're not then add smoothing
            if (_aiming.fireAngleChange < _fireTriggerAngleThreshold)
            {
                // smooth towards our final direction
                _aiming.smoothedAimDir = Vector3.SmoothDamp(_aiming.smoothedAimDir, _aiming.finalDir, ref _aiming.smoothAimVelocity, _aimSmoothingTime);
                // factor in however much smoothing is wanted
                _aiming.finalDir = Vector3.Lerp(_aiming.finalDir, _aiming.smoothedAimDir, _aimSmoothingFactor).normalized * aimMag;
            }

            // aim the gun toward our final direction
            if ( aimMag > 0 ) 
            {
                _primaryWeapon.SetDirection(_aiming.finalDir);
            }
            else
            {
                _primaryWeapon.SetDirection(_primaryWeapon.transform.parent.forward);
                _aiming.smoothedAimDir = _aiming.finalDir;
            }
        }

        private void OnBulletSpawned(GameObject bulletObj)
        {
            Bullet bullet = bulletObj.GetComponent<Bullet>();

            if ( _aiming.hasLock )
            {
                bullet.SetTarget(_aiming.lockCollider);
                bullet.targetEntity = _aiming.lockEntity;
            }
            else
            {
                bullet.targetType = Bullet.Target.AutoEnemy;
            }

        }

        private void UpdateMotion()
        {
            // setup move direction based on input
            _motion.moveDir = _motion.requestedMoveDir;
            float moveMag = _motion.moveDir.magnitude;

            // setup aim direction based on input (aim basis and visuals / animations)
            _motion.aimDir = _motion.requestedAimDir;
            float aimMag = _motion.requestedAimDir.magnitude;

            if( aimMag <= Mathf.Epsilon || inNoFireZone )
            {
                aimMag = 0;
                _motion.aimDir = _motion.moveDir;
            } 

            // Change our move direction, but only if we are not in the stick dead zone
            if (moveMag > 0.1f) _motion.moveRotation = Vector3.SignedAngle(Vector3.forward, _motion.moveDir, Vector3.up);
            _motion.moveSpeed = _motion.moveDir.magnitude * speed;

            transform.localRotation = Quaternion.Euler(0, _motion.moveRotation, 0);

            if( aimMag > 0 )
            {
                _motion.aimMoveAngle = Vector3.SignedAngle(transform.forward, _motion.aimDir, Vector3.up);
            }
            else if( moveMag > 0 )
            {
                _motion.aimMoveAngle = Vector3.SignedAngle(transform.forward, _motion.moveDir, Vector3.up);
            }

            // update Primary Attachment Root
            _attachments.primaryRoot.position = _attachments.primaryRootLeft.position * 0.5f + _attachments.primaryRootRight.position * 0.5f;
            Vector3 dir = (_attachments.primaryRoot.position - transform.position);
            dir.y = 0;
            _attachments.primaryRoot.rotation = Quaternion.LookRotation(dir, Vector3.up);

            // are we moving backwards?
            if (_primaryWeapon != null && Mathf.Abs(_motion.aimMoveAngle) > 90)
            {
                // flipped animations

                // aimMoveAngle Range: -90 to -180 (aiming left), 180 to 90 (aiming right)
                // change to: 90 -> 0 -> -90
                if (_motion.aimMoveAngle < 0) _anim.aimBlendTarget = (_motion.aimMoveAngle + 180.0f) / 90.0f;
                else _anim.aimBlendTarget = (_motion.aimMoveAngle - 180.0f) / 90.0f;

                _modelRoot.transform.localRotation = Quaternion.Euler(_modelRoot.transform.localRotation.x, 180, _modelRoot.transform.localRotation.z);

                _anim.speedBlendTarget = -moveMag;
                _anim.speedBlendTarget = Mathf.Min(moveMag, _anim.speedBlendTarget);
            }
            else
            {
                // non flipped animations
                _modelRoot.transform.localRotation = Quaternion.Euler(_modelRoot.transform.localRotation.x, 0, _modelRoot.transform.localRotation.z);
                _anim.aimBlendTarget = _motion.aimMoveAngle / 90.0f;
                _anim.speedBlendTarget = Mathf.Max(moveMag, _anim.speedBlendTarget);
            }
            
            // Adjust our aiming animation layer weight and direction pose
            if (_primaryWeapon == null) _anim.aimLayerWeightTarget = 0;
            else _anim.aimLayerWeightTarget = Mathf.Max(aimMag, _anim.aimLayerWeightTarget);

            _anim.animator.SetFloat(_anim.aimId, MathUtil.Normalize(-1, 1, _anim.aimBlendTarget));
            _anim.animator.SetLayerWeight(_anim.aimLayer, _anim.aimLayerWeightTarget);
            _anim.aimLayerWeightTarget = Mathf.SmoothDamp(_anim.aimLayerWeightTarget, 0, ref _anim.aimBlendTargetVelocity, _moveToIdleTime);

            // move our character
            _charCon.Move(_motion.moveDir * (_motion.moveSpeed * GameState.deltaTime) + UnityEngine.Physics.gravity * Time.deltaTime);

            // adjust the animation walk/running speed
            _anim.animator.SetFloat(_anim.speedId, _anim.speedBlendTarget);
            _anim.speedBlendTarget = Mathf.SmoothDamp(_anim.speedBlendTarget, 0, ref _anim.speedBlendTargetVelocity, _moveToIdleTime);

            // aim and fire
            if (aimMag > 0.2f && primaryWeapon != null)
            {
                // we also don't want to fire in no fire zones, but our aimMag will be 0 so we don't need to check for it again

                // aim gun (for smoothing / auto targeting)
                UpdateAiming(aimMag);

                // check if the player aim angle changed drastically, if so possibly allow to override the gun trigger time and fire immediately.
                bool overrideTime = false;
                if(_fireTriggerAngleThreshold > 0 && _aiming.fireAngleBursts < 1)
                {
                    overrideTime = _aiming.fireAngleChange >= _fireTriggerAngleThreshold;
                    if( overrideTime )
                    {
                        _aiming.fireAngleBursts++;
                    }
                }

                // fire
                if( primaryWeapon.Trigger(overrideTime) )
                {
                    _aiming.lastFireDirection = _aiming.finalDir;
                    if( !overrideTime) _aiming.fireAngleBursts = 0;
                }

                _anim.aimLayerWeightTarget = 1;
            }
            else
            {
                _aiming.hasLock = false;
            }
        }

        private void UpdateFoundEquipmentSequence()
        {
            _foundEquipment.time += GameState.deltaTime;
            
            if (_foundEquipment.step == 0) // snap instantly to vicotry pose
            {
                transform.localRotation = Quaternion.Euler(0, 180, 0);
                _motion.moveRotation = 180;
                _motion.moveSpeed = 0;

                _anim.animator.SetTrigger(_anim.equipmentFoundStartId); // this transition has 0 time.
                _foundEquipment.equipemnt.gameObject.SetActive(true);

                _onEquipmentFoundHoldStart.Invoke();
                _foundEquipment.equipemnt.TriggerFoundHoldStart();

                _foundEquipment.step++;
                _foundEquipment.time = 0;
            }
            else if (_foundEquipment.step == 1) // hold victory pose for some time
            {
                if( _foundEquipment.time > _attachments.foundEquipmentHoldTime)
                {
                    // Trigger Events _before_ we Equip otherwise the transforms will change.
                    _onEquipmentFoundHoldEnd.Invoke();
                    _foundEquipment.equipemnt.TriggerFoundHoldEnd();

                    _anim.animator.SetTrigger(_anim.equipmentFoundEndId);
                    Equip(_foundEquipment.equipemnt);

                    _foundEquipment.step++;
                    _foundEquipment.time = 0;
                }
            }
            else if (_foundEquipment.step == 2) // transition back to playing state
            {
                if (_foundEquipment.time > 0.25f)
                {
                    _state = PlayerState.Playing;

                    _foundEquipment.equipemnt.TriggerFoundEnd();
                    _onEquipmentFoundEnd.Invoke();
                    _foundEquipment.equipemnt = null;
                }
            }
        }

#if UNITY_EDITOR
        protected new void OnDrawGizmosSelected()
        {
            base.OnDrawGizmosSelected();

            // Move Direction
            Vector3 center = torso.position;
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(center, center + _motion.moveDir * 2);

            // Auto Target Cast

            // Draw Target Physics Overlay (this needs to be in sync with CollectAimingHits)
            if( Application.isPlaying && _primaryWeapon != null)
            {
                Vector3 fwd = _motion.aimDir.normalized;
                Gun.GunBulletProperties bulletProps = _primaryWeapon.GetGunBulletProperties();
                Gizmos.color = Color.magenta;
                Gizmos.DrawLine(center + fwd * _autoTargetMinDistance, center + fwd * bulletProps.range);

                int steps = 5;

                Vector3 start = center + fwd * _autoTargetMinDistance;
                Vector3 end = center + fwd * bulletProps.range;

                for (int i = 0; i < steps; i++)
                {
                    float step = i / (float)steps;
                    Vector3 point = Vector3.Lerp(start, end, step);
                    float radius = Mathf.Lerp(_autoTargetMinCastRadius, _autoTargetMaxCastRadius, step);

                    Gizmos.DrawWireSphere(point, radius);
                }
            }

            // Aim Direction
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(center, center + _motion.aimDir * 2);

            Gizmos.color = Color.green;
            Gizmos.DrawLine(center, center + _aiming.smoothedAimDir * 1.75f);

            if (_aiming.hasLock)
            {                
                Gizmos.color = Color.red;
                Vector3 targetPoint = _primaryWeapon.transform.position + _aiming.autoTargetDir * _aiming.autoTargetDistance;
                Gizmos.DrawLine(_primaryWeapon.transform.position, targetPoint);
                Gizmos.DrawWireSphere(targetPoint, 0.08f);

                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(_aiming.rawLockPoint, 0.08f);
            }

            Gizmos.color = Color.white;
            Gizmos.DrawLine(center, center + _aiming.finalDir * 1.25f);

            if(_maxAutoTargetAngle > 0)
            {
                Gizmos.color = Color.yellow;
                Vector3 fwd;
                Quaternion rotateBy = Quaternion.Euler(0, _maxAutoTargetAngle, 0);
                fwd = rotateBy * transform.forward;
                Gizmos.DrawLine(transform.position, transform.position + fwd * 2);

                rotateBy = Quaternion.Euler(0, -_maxAutoTargetAngle, 0);
                fwd = rotateBy * transform.forward;
                Gizmos.DrawLine(transform.position, transform.position + fwd * 2);
            }
        }
#endif

    }
}