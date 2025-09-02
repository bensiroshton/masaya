using Siroshton.Masaya.Item;
using Siroshton.Masaya.Player;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Siroshton.Masaya.UI
{
    [ExecuteInEditMode]
    public class CardLayout : MonoBehaviour
    {
        // all position/sizes are in percentages of the pixel size.

        [SerializeField] private Vector2 _pixelSize = new Vector2(548, 896);
        [SerializeField] private Image _cardBackgroundImage;

        [SerializeField] private float _borderThickness = 0.05f;
        [SerializeField] private float _textSideMargin = 0.02f;

        [SerializeField] private RectTransform _titleArea;
        [SerializeField] private TMP_Text _titleText;
        [SerializeField] private TMP_Text _levelText;
        [SerializeField] private float _titlePosition = 0.1f;
        [SerializeField] private float _titleHeight = 0.1f;
        [SerializeField] private float _titleToLevelRatio = 0.6f;

        [SerializeField] private RectTransform _descriptionArea;
        [SerializeField] private TMP_Text _descriptionText;
        [SerializeField] private float _descriptionMaxHeight = 0.5f;
        [SerializeField] private float _descriptionTextSize = 12;

        [SerializeField] private RectTransform _costArea;
        [SerializeField] private TMP_Text _costLevelText;
        [SerializeField] private TMP_Text _costValueText;
        [SerializeField] private float _costAreaPosition = 0.8f;
        [SerializeField] private float _costAreaHeight = 0.1f;

        [SerializeField] private Image _borderImage;
        [SerializeField] private Image _equippedImage;
        [SerializeField] private Image _starImage;
        [SerializeField] private Image _tokenImage;

        // buttons
        [SerializeField] private RectTransform _buttons;

        [SerializeField] private RectTransform _equip;
        [SerializeField] private Image _equipButton;
        [SerializeField] private TMP_Text _equipText;

        [SerializeField] private RectTransform _levelUp;
        [SerializeField] private Image _levelUpButton;
        [SerializeField] private TMP_Text _levelUpText;

        [SerializeField] private RectTransform _burnCard;
        [SerializeField] private Image _burnCardButton;
        [SerializeField] private TMP_Text _burnCardText;

        [SerializeField] private float _buttonWidth = 0.8f;
        [SerializeField] private float _buttonHeight = 0.1f;

        // collider for joy card selection
        [SerializeField] private BoxCollider2D _collider;

        private Vector2 _area;
        private SkillCard _card;
        private bool _needsResize;
        private bool _showEquipUnEquip;

        public Vector2 pixelSize 
        { 
            get => _pixelSize; 
            set
            {
                _pixelSize = value;
                Resize(value);
            }
        }

        public SkillCard card {
            get => _card; 
            set 
            {
                if( _card != null )
                {
                    _card.onCardChange.RemoveListener(OnCardChange);
                }
                
                _card = value;
                
                if( _card != null )
                {
                    _card.onCardChange.AddListener(OnCardChange);
                }
                
                RefreshData();
            }
        }

        public bool showLevelUp 
        { 
            get => _levelUp.gameObject.activeSelf; 
            set
            {
                _levelUp.gameObject.SetActive(value);
                _buttons.gameObject.SetActive(value || showBurnCard);
                _needsResize = true;
            }
        }

        public bool showBurnCard
        { 
            get => _burnCard.gameObject.activeSelf; 
            set
            {
                _burnCard.gameObject.SetActive(value); 
                _buttons.gameObject.SetActive(value || showLevelUp);
                _needsResize = true;
            }
        }

        private void Start()
        {
            _starImage.transform.localRotation = Quaternion.Euler(0, 0, Random.Range(-180.0f, 180.0f));
        }

        private void OnCardChange(SkillCard card)
        {
            RefreshData();
        }

        public void RefreshData()
        {
            if(_card==null)
            {
                Debug.LogError("Card is null.");
                return;
            }

            _titleText.text = _card.title;
            _descriptionText.text = _card.description;
            _levelText.text = $"{_card.currentLevel} / {_card.maxLevel}";
            _cardBackgroundImage.sprite = _card.backgroundImage;

            if ( !_card.atMaxLevel )
            {
                _costValueText.text = $"x {_card.costToLevelUp}";
                _costLevelText.text = _card.perLevelModifierString;
                _costArea.gameObject.SetActive(true);
            }
            else
            {
                _costArea.gameObject.SetActive(false);
            }

            _starImage.gameObject.SetActive(_card.CanLevelUp(Player.Player.instance.characterSheet));

            CharacterSheet sheet = Player.Player.instance.characterSheet;
            if (sheet.IsCardEquippped(_card))
            {
                _tokenImage.gameObject.SetActive(true);
                _equippedImage.gameObject.SetActive(true);
                _equipText.text = "UnEquip";
                _showEquipUnEquip = true;
            }
            else
            {
                _tokenImage.gameObject.SetActive(false);
                _equippedImage.gameObject.SetActive(false);
                _equipText.text = "Equip";

                _showEquipUnEquip = sheet.availableEquipPoints > 0;
            }

            _equip.gameObject.SetActive(_showEquipUnEquip);

            _levelUpText.text = $"Level Up (- <sprite=0>x {card.costToLevelUp})";
            _burnCardText.text = $"Burn (+ <sprite=0>x {card.burnCardRefundAmount})";
            _needsResize = true;
        }

        void Update()
        {
            Vector2 area = (transform as RectTransform).rect.size;
            if(_needsResize || area != _area )
            {
                Resize(area);
            }
        }

        private void Resize(Vector2 area)
        {
            float heightRatio = _pixelSize.y / _pixelSize.x;
            _area = area;
            _needsResize = false;

            RectTransform rt = transform as RectTransform;
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, area.x);
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, area.y);

            Vector2 cardSize;
            if( area.y / area.x > heightRatio)
            {
                // fit width
                cardSize.x = area.x;
                cardSize.y = area.x * heightRatio;
            }
            else
            {
                // fit height
                cardSize.y = area.y;
                cardSize.x = area.y / heightRatio;
            }

            float contentPixelWidth = cardSize.x - cardSize.x * _borderThickness * 2;
            float sideMargin = cardSize.x * _textSideMargin;

            _cardBackgroundImage.rectTransform.anchoredPosition = new Vector2(0, 0);
            _cardBackgroundImage.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, cardSize.x);
            _cardBackgroundImage.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, cardSize.y);

            float titlePixelHeight = _titleHeight * cardSize.y;
            _titleArea.anchoredPosition = new Vector2(0, -cardSize.y * _titlePosition);
            _titleArea.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, contentPixelWidth);
            _titleArea.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, titlePixelHeight);

            _titleText.rectTransform.anchoredPosition = new Vector2(0, 0);
            _titleText.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, contentPixelWidth);
            _titleText.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, titlePixelHeight * _titleToLevelRatio);
            _titleText.margin = new Vector4(sideMargin, 0, sideMargin, 0);

            _levelText.rectTransform.anchoredPosition = new Vector2(0, 0);
            _levelText.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, contentPixelWidth);
            _levelText.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, titlePixelHeight * (1.0f - _titleToLevelRatio));
            _levelText.margin = new Vector4(sideMargin, 0, sideMargin, 0);

            _descriptionText.fontSize = cardSize.x / _pixelSize.x * _descriptionTextSize;
            float descMaxPixelHeight = _descriptionMaxHeight * cardSize.y;
            Vector2 textSize = _descriptionText.GetPreferredValues(contentPixelWidth - sideMargin * 2, descMaxPixelHeight);
            
            float descPixelHeight = Mathf.Min(descMaxPixelHeight, textSize.y);
            _descriptionArea.anchoredPosition = new Vector2(0, 0);
            _descriptionArea.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, contentPixelWidth);
            _descriptionArea.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, descPixelHeight);

            _descriptionText.rectTransform.anchoredPosition = new Vector2(0, 0);
            _descriptionText.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, contentPixelWidth);
            _descriptionText.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, descPixelHeight);
            _descriptionText.margin = new Vector4(sideMargin, 0, sideMargin, 0);

            _costArea.anchoredPosition = new Vector2(0, -cardSize.y * _costAreaPosition);
            _costArea.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, contentPixelWidth);
            _costArea.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, _costAreaHeight * cardSize.y);

            _borderImage.rectTransform.anchoredPosition = _cardBackgroundImage.rectTransform.anchoredPosition;
            _borderImage.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, cardSize.x);
            _borderImage.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, cardSize.y);

            _equippedImage.rectTransform.anchoredPosition = _cardBackgroundImage.rectTransform.anchoredPosition;
            _equippedImage.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, cardSize.x);
            _equippedImage.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, cardSize.y);

            _starImage.rectTransform.anchoredPosition = new Vector2(cardSize.x / 2 - cardSize.x * _borderThickness, -cardSize.y * _titlePosition);
            _starImage.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, cardSize.x * 0.25f);
            _starImage.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, cardSize.x * 0.25f);

            _tokenImage.rectTransform.anchoredPosition = new Vector2(0, -cardSize.y * 0.025f);
            _tokenImage.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, cardSize.x * 0.2f);
            _tokenImage.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, cardSize.x * 0.2f);

            // buttons
            float rowWidth = cardSize.x * _buttonWidth;
            float rowHeight = cardSize.y * _buttonHeight;
            float buttonSize = rowHeight;
            
            // textSize = _burnCardText.GetPreferredValues(rowWidth - buttonSize, rowHeight); <--- broken, returns huge width values.
            // rowWidth = textSize.x + buttonSize;

            int count = 0;
            if (_showEquipUnEquip) count++; // show equip/unequip
            if (showLevelUp) count++;
            if (showBurnCard) count++;
            _buttons.anchoredPosition = new Vector2(0, 0);
            _buttons.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, rowWidth);
            _buttons.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, rowHeight * count);

            _equip.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, rowWidth);
            _equip.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, rowHeight);
            _equipButton.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, rowHeight);
            _equipButton.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, rowHeight);
            _equipText.rectTransform.anchoredPosition = new Vector2(buttonSize, 0);
            _equipText.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, rowWidth - buttonSize);
            _equipText.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, rowHeight);

            _levelUp.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, rowWidth);
            _levelUp.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, rowHeight);
            _levelUpButton.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, rowHeight);
            _levelUpButton.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, rowHeight);
            _levelUpText.rectTransform.anchoredPosition = new Vector2(buttonSize, 0);
            _levelUpText.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, rowWidth - buttonSize);
            _levelUpText.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, rowHeight);
            
            _burnCard.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, rowWidth);
            _burnCard.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, rowHeight);
            _burnCardButton.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, rowHeight);
            _burnCardButton.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, rowHeight);
            _burnCardText.rectTransform.anchoredPosition = new Vector2(buttonSize, 0);
            _burnCardText.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, rowWidth - buttonSize);
            _burnCardText.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, rowHeight);

            _collider.size = cardSize;
        }
    }
}
