using Siroshton.Masaya.Item;
using Siroshton.Masaya.Weapon;
using UnityEngine;
using UnityEngine.Events;

namespace Siroshton.Masaya.Player
{
    public class Bangles : MonoBehaviour, IEquipment, IItem
    {
        [SerializeField] private string _name = "Bangles";
        [SerializeField] private Sprite _pickupImage;

        [SerializeField] private GameObject _leftHandModel;
        [SerializeField] private GameObject _rightHandModel;
        [SerializeField] private GameObject _worldItem;
        [SerializeField] private GameObject _gunPrefab;
        [SerializeField] private Collider _collectionTrigger;
        
        [SerializeField] private AudioClip _soundOnPickup;
        [SerializeField] private AudioClip _soundOnEquip;

        [SerializeField] private UnityEvent _onFoundBegin;
        [SerializeField] private UnityEvent _onFoundHoldStart;
        [SerializeField] private UnityEvent _onFoundHoldEnd;
        [SerializeField] private UnityEvent _onFoundEnd;
        [SerializeField] private UnityEvent _onHitFloor;
        
        public GameObject gunPrefab => _gunPrefab;
        public GameObject leftHandModel => _leftHandModel;
        public GameObject rightHandModel => _rightHandModel;
        public GameObject worldItem => _worldItem;

        public UnityEvent onFoundBegin { get => _onFoundBegin; set => _onFoundBegin = value; }
        public UnityEvent onFoundHoldStart { get => _onFoundHoldStart; set => _onFoundHoldStart = value; }
        public UnityEvent onFoundHoldEnd { get => _onFoundHoldEnd; set => _onFoundHoldEnd = value; }
        public UnityEvent onFoundEnd { get => _onFoundEnd; set => _onFoundEnd = value; }
        public UnityEvent onHitFloor { get => _onHitFloor; set => _onHitFloor = value; }

        public Core.Types.Rarity rarity => Core.Types.Rarity.Legendary;
        public Sprite pickupImage => _pickupImage;
        public string itemName => _name;
        public AudioClip soundOnPickup => _soundOnPickup;
        public AudioClip soundOnEquip => _soundOnEquip;

        public void OnTriggerEnter(Collider other)
        {
            // make sure the Physics masks only allow the player to collide with this item.
            Player.instance.GiveItem(this);
        }

        void IEquipment.DisableCollectionTriggers()
        {
            _collectionTrigger.enabled = false;
        }

        void IEquipment.EnableCollectionTriggers()
        {
            _collectionTrigger.enabled = true;
        }

        void IEquipment.TriggerFoundBegin()
        {
            if( _onFoundBegin != null ) _onFoundBegin.Invoke();
        }

        void IEquipment.TriggerFoundHoldStart()
        {
            if (_onFoundHoldStart != null) _onFoundHoldStart.Invoke();
        }

        void IEquipment.TriggerFoundHoldEnd()
        {
            if (_onFoundHoldEnd != null) _onFoundHoldEnd.Invoke();
        }

        void IEquipment.TriggerFoundEnd()
        {
            if (_onFoundEnd != null) _onFoundEnd.Invoke();
        }

        void IEquipment.TriggerHitFloor()
        {
            if (_onHitFloor != null) _onHitFloor.Invoke();
        }
    }
}