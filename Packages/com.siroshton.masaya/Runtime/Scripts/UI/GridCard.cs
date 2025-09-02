using Siroshton.Masaya.Item;
using UnityEngine;
using UnityEngine.UI;

namespace Siroshton.Masaya.UI
{
    public class GridCard : MonoBehaviour
    {
        [SerializeField] private Image _selectedImage;
        [SerializeField] private CardLayout _card;

        public bool selected { get => _selectedImage.enabled; set => _selectedImage.enabled = value; }
        public SkillCard card { get => _card.card; set => _card.card = value; }
        public float heigtRatio => _card.pixelSize.y / _card.pixelSize.x;

        public void SetSize(Vector2 size)
        {
            _selectedImage.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size.x);
            _selectedImage.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size.y);
        }
    }
}
