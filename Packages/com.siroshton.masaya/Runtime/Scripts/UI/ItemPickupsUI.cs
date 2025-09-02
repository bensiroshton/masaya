using Siroshton.Masaya.Item;
using System.Collections.Generic;
using UnityEngine;

namespace Siroshton.Masaya.UI
{

    public class ItemPickupsUI : MonoBehaviour
    {
        [SerializeField] private float _itemDuration = 2;
        [SerializeField] private float _padding = 10;
        [SerializeField] private float _widthRatio = 1.0f;
        [SerializeField] private GameObject _itemPickupPrefab;

        private Queue<IItem> _items = new Queue<IItem>();
        private ItemPickup _currentItem;

        public void PickedUpItem(IItem item)
        {
            if( item.pickupImage == null ) return;

            _items.Enqueue(item);
            ProcessQueue();
        }

        private void ProcessQueue()
        {
            if( _currentItem != null )
            {
                Destroy(_currentItem.gameObject);
                _currentItem = null;
            }

            if( _items.Count == 0 ) return;

            IItem item = _items.Dequeue();

            RectTransform prt = transform as RectTransform;

            GameObject o = GameObject.Instantiate<GameObject>(_itemPickupPrefab);
            RectTransform ort = o.transform as RectTransform;
            ort.SetParent(prt);
            ort.localScale = Vector3.one;
            ort.anchoredPosition = new Vector2(_padding, _padding);

            float height = prt.rect.height - _padding;
            ort.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
            ort.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, height * _widthRatio);

            _currentItem = o.GetComponent<ItemPickup>();
            _currentItem.InitItem(item);
            _currentItem.FadeOut(_itemDuration, OnFadeFinished);
        }

        private void OnFadeFinished(ItemPickup ip)
        {
            Destroy(_currentItem.gameObject);
            _currentItem = null;

            ProcessQueue();
        }

    }

}