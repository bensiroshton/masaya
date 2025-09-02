using Siroshton.Masaya.Item;
using Siroshton.Masaya.Math;
using Siroshton.Masaya.Motion;
using Siroshton.Masaya.Player;
using UnityEngine;
using UnityEngine.UI;

namespace Siroshton.Masaya.UI
{

    public class CardItem : MonoBehaviour
    {
        [SerializeField] private float _borderSize = 30;
        [SerializeField] private Image _selectedImage;
        [SerializeField] private ScaleTo _scaleDown;
        [SerializeField] private RotateTo _rotateDown;
        [SerializeField] private ScaleTo _scaleUp;
        [SerializeField] private RotateTo _rotateUp;
        [SerializeField] private Color _selectedColor;
        [SerializeField] private Color _selectedAndEquippedColor;

        private CardLayout _cardLayout;
        private bool _hasStartBeenCalled;

        public SkillCard card => _cardLayout != null ? _cardLayout.card : null;

        public bool selected
        {
            get => _selectedImage.enabled;
            set
            {
                if( _cardLayout == null )
                {
                    value = false;
                }

                _selectedImage.enabled = value;

                BoxCollider2D collider = GetComponentInChildren<BoxCollider2D>();
                if( collider != null ) collider.enabled = !value;

                if( value )
                {
                    if(_hasStartBeenCalled)
                    {
                        _scaleDown.StopScaling();
                        _rotateDown.StopRotating();

                        _scaleUp.StartScaling();
                        _rotateUp.StartRotating();
                    }

                    transform.SetAsLastSibling();
                }
                else
                {
                    if(_hasStartBeenCalled)
                    {
                        _scaleDown.StartScaling();
                        _rotateDown.StartRotating();

                        _scaleUp.StopScaling();
                        _rotateUp.StopRotating();
                    }
                }

                RefreshData();
            }
        }

        public void SetDefaultState()
        {
            if( !_hasStartBeenCalled ) return;
            _scaleDown.ScaleImmediate();
            _rotateDown.RotateImmediate();
        }

        public void RefreshData()
        {
            CharacterSheet sheet = Player.Player.instance.characterSheet;

            if( sheet.IsCardEquippped(card) )
            {
                _selectedImage.color = _selectedAndEquippedColor;
            }
            else
            {
                _selectedImage.color = _selectedColor;
            }

            if (_cardLayout != null)
            {
                _cardLayout.showBurnCard = selected && card.canBurnCard;
                _cardLayout.showLevelUp = selected && card.CanLevelUp(sheet);
                _cardLayout.RefreshData();
            }
        }

        private void Start()
        {
            _scaleDown.targetScale = transform.localScale;
            _scaleUp.targetScale = transform.localScale * 1.4f;
            float rotation = MathUtil.MapAngle180(transform.localRotation.eulerAngles.z);
            _rotateDown.targetRotation = new Vector3(0, 0, rotation);
            _rotateUp.targetRotation = new Vector3(0, 0, rotation * 0.2f);
            _hasStartBeenCalled = true;
        }

        public void SetCard(SkillCard card, CardLayout prefab)
        {
            if( card == null)
            {
                if( _cardLayout != null )
                {
                    transform.DetachChildren();
                    Destroy(_cardLayout.gameObject);
                    _cardLayout = null;
                }
                return;
            }

            if(_cardLayout == null )
            {
                _cardLayout = GameObject.Instantiate<CardLayout>(prefab);
                _cardLayout.transform.SetParent(transform);
                _cardLayout.transform.localPosition = Vector3.zero;
                _cardLayout.transform.localRotation = Quaternion.identity;
                _cardLayout.showLevelUp = false;
                _cardLayout.showBurnCard = false;
            }

            _cardLayout.card = card;
        }

        public void RemoveCard()
        {
            SetCard(null, null);
        }

        public void SetLayout(CardManager.LayoutItem layout)
        {
            RectTransform rt = transform as RectTransform;
            rt.pivot = new Vector2(0.5f, 0.5f);

            float extraSize = _borderSize * 2 * layout.scale;

            rt.position = layout.position;
            rt.rotation = Quaternion.Euler(0, 0, layout.rotation);
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, layout.cardSize.x + extraSize);
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, layout.cardSize.y + extraSize);

            if( _cardLayout != null ) _cardLayout.pixelSize = layout.cardSize;
        }

    }

}