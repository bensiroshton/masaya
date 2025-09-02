using Siroshton.Masaya.Core;
using Siroshton.Masaya.Item;
using Siroshton.Masaya.Math;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Siroshton.Masaya.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class ItemPickup : MonoBehaviour
    {
        [SerializeField] private Image _image;
        [SerializeField] private TMP_Text _text;
        [SerializeField] private float _textHeightPercentage = 0.2f;

        public delegate void OnFadeFinishedDelegate(ItemPickup ip);

        private CanvasGroup _canvasGroup;
        private float _time;
        private float _duration;
        private bool _isFading;
        private OnFadeFinishedDelegate _onFadeFinished;

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
        }

        public void InitItem(IItem item)
        {
            _image.type = Image.Type.Simple;
            _image.preserveAspect = true;
            _image.sprite = item.pickupImage;

            _text.text = item.itemName;

            UpdateLayout();
        }

        public void UpdateLayout()
        {
            RectTransform myrt = transform as RectTransform;

            _image.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, myrt.rect.height * (1.0f - _textHeightPercentage));
            _image.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, myrt.rect.width);
            _image.rectTransform.anchoredPosition = new Vector2(myrt.rect.width / 2.0f, myrt.rect.height * _textHeightPercentage);

            _text.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, myrt.rect.height * _textHeightPercentage);
            _text.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, myrt.rect.width);
        }

        public void FadeOut(float duration, OnFadeFinishedDelegate onFadeFinished)
        {
            _time = 0;
            _duration = duration;
            _isFading = true;
            _onFadeFinished = onFadeFinished;
        }

        private void Update()
        {
            if( !_isFading ) return;
            
            _time += GameState.deltaTime;
            if( _time >= _duration )
            {
                _time = _duration;
                _isFading = false;
                _onFadeFinished?.Invoke(this);
            }

            _canvasGroup.alpha = 1.0f - MathUtil.Normalize(_duration / 2.0f, _duration, _time);
        }
    }
}