using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Siroshton.Masaya.UI
{
    public class Bottle : MonoBehaviour
    {
        [SerializeField] private RectMask2D _mask;
        [SerializeField] private TMP_Text _fillText;
        [SerializeField] private float _fillLevel;

        private Rect _parentRect;

        private void Start()
        {
            _parentRect = (_mask.rectTransform.parent as RectTransform).rect;
            fillLevel = _fillLevel;
        }

        public float fillLevel 
        { 
            get => _fillLevel; 
            set
            {
                if( value < 0 ) _fillLevel = 0;
                else if( value > 1) _fillLevel = 1;
                else _fillLevel = value;

                _mask.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, _parentRect.height * _fillLevel);
                _fillText.text = $"{(int)(_fillLevel * 100.0)}%";
            }
        }

    }
}