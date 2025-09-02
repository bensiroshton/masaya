
using System.Drawing;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Siroshton.Masaya.UI
{
    [RequireComponent(typeof(RectTransform))]
    public class MessageOverlayUI : MonoBehaviour
    {
        [SerializeField] private Transform _attachTo;
        [SerializeField] private TMP_Text _text;
        [SerializeField] private float _paddingVertical = 4;
        [SerializeField] private float _paddingHorizontal = 10;
        [SerializeField] private Image _OKButton;

        private RectTransform _rectTransform;
        private float _scale = 1;
        private bool _isDirty = true;

        public RectTransform rectTransform => _rectTransform;
        public Transform attachTo { get => _attachTo; set => _attachTo = value; }

        public string message
        {
            get => _text.text;
            set
            {
                _text.text = value;
                _isDirty = true;
            }
        }

        public bool showButton
        {
            get => _OKButton.enabled;
            set => _OKButton.enabled = value;
        }

        public float scale 
        { 
            get => _scale; 
            set
            {
                _scale = value;
                transform.localScale = new Vector3(_scale, _scale, _scale);
            }
        }

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
        }

        private void LateUpdate()
        {
            Vector2 pos = Camera.main.WorldToScreenPoint(attachTo.position);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(transform.parent as RectTransform, pos, null, out pos);
            rectTransform.anchoredPosition = pos;

            if(_isDirty)
            {
                ResizeToFitText();
                _isDirty = false;
            }
        }

        private void ResizeToFitText()
        {
            _text.ForceMeshUpdate();

            Bounds bounds = _text.textBounds;
            float width = bounds.size.x + _paddingHorizontal * 2;
            float height = bounds.size.y + _paddingVertical * 2;

            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
        }

    }
}