using Siroshton.Masaya.Core;
using Siroshton.Masaya.Player;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace Siroshton.Masaya.Creature
{
    [RequireComponent(typeof(Animator))]
    public class HollowOne : MonoBehaviour
    {
        [SerializeField, Range(0, 1)] private float _lookAtPlayerWeight;
        [SerializeField] private Vector3 _lookAtPlayerOffset;
        [SerializeField] private GameObject _itemToGivePrefab;
        [SerializeField] private Transform _itemAttachmentPoint;
        [SerializeField] private UnityEvent _onItemDropBegin = new UnityEvent();
        [SerializeField] private UnityEvent _onItemReachedFloor = new UnityEvent();
        [SerializeField] private UnityEvent _onItemCaughtByPlayer = new UnityEvent();
        [SerializeField] private UnityEvent _onItemDropFinish = new UnityEvent();

        private GameObject _itemToGive;
        private bool _isDropping;


        private struct AnimProperties
        {
            public Animator animator;
            public int giveItem;
        }

        private AnimProperties _anim;

        protected void Awake()
        {
            _anim.animator = GetComponent<Animator>();
            _anim.giveItem = Animator.StringToHash("giveItem");
        }

        protected void OnAnimatorIK()
        {
            _anim.animator.SetLookAtWeight(_lookAtPlayerWeight);
            _anim.animator.SetLookAtPosition(Player.Player.instance.transform.position + _lookAtPlayerOffset);
        }

        public void GiveItem()
        {
            _itemToGive = GameObject.Instantiate(_itemToGivePrefab);
            _itemToGive.transform.SetParent(_itemAttachmentPoint);
            _itemToGive.transform.localPosition = Vector3.zero;
            _itemToGive.transform.localRotation = Quaternion.identity;

            if (_itemToGive.GetComponent<IEquipment>() is IEquipment e)
            {
                e.DisableCollectionTriggers();
            }

            _anim.animator.SetTrigger(_anim.giveItem);
        }

        public void DropItem()
        {
            _itemToGive.transform.SetParent(null, true);
            _isDropping = true;
            _onItemDropBegin.Invoke();

            if (_itemToGive.GetComponent<IEquipment>() is IEquipment e)
            {
                e.EnableCollectionTriggers();
            }
        }

        protected void Update()
        {
            if( _isDropping )
            {
                if( _itemToGive.transform.parent != null )
                {
                    // player grabbed it before it hit the ground.
                    _isDropping = false;
                    _onItemCaughtByPlayer.Invoke();
                    _onItemDropFinish.Invoke();
                    return;
                }

                _itemToGive.transform.position += UnityEngine.Physics.gravity * GameState.deltaTime;
                if ( _itemToGive.transform.position.y <= 0 )
                {
                    _itemToGive.transform.position = new Vector3(_itemToGive.transform.position.x, 0, _itemToGive.transform.position.z);
                    _itemToGive.transform.rotation = Quaternion.identity;

                    if (_itemToGive.GetComponent<IEquipment>() is IEquipment e)
                    {
                        e.TriggerHitFloor();
                    }

                    _onItemReachedFloor.Invoke();
                    _onItemDropFinish.Invoke();

                    _itemToGive = null;
                    _isDropping = false;
                }

            }
        }

    }
}
