using Siroshton.Masaya.Core;
using Siroshton.Masaya.Item;
using Siroshton.Masaya.Player;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Siroshton.Masaya.UI
{
    public class CardsUI : MonoBehaviour, IUISection
    {
        [SerializeField] private string _title;
        [SerializeField] private CardManager _cardManager;
        [SerializeField] private TMP_Text _bottleCountText;
        [SerializeField] private PlayerStats _stats;
        [SerializeField] private HorizontalLayoutGroup _equipTokenLayout;
        [SerializeField] private Sprite _equipToken;
        [SerializeField] private Vector2 _tokenSize = new Vector2(30, 30);

        public string title { get => _title; }

        private CharacterSheet _sheet;

        private void OnEnable()
        {
            _stats.RefreshData();
        }

        private void Start()
        {
            _sheet = Player.Player.instance.characterSheet;
            _sheet.onBottleCountChange.AddListener(OnBottleCountChange);
            OnBottleCountChange(_sheet.bottleCount);

            _sheet.onCardEquipped.AddListener(OnCardEquippedChange);
            _sheet.onCardUnEquipped.AddListener(OnCardEquippedChange);
            OnCardEquippedChange(null);
        }

        private void OnCardEquippedChange(SkillCard card)
        {
            // Update tokens
            GameObject o;
            Image img;

            while ( _equipTokenLayout.transform.childCount < _sheet.totalEquipPoints )
            {
                o = new GameObject("Token");
                img = o.AddComponent<Image>();
                img.sprite = _equipToken;
                o.transform.SetParent(_equipTokenLayout.transform);
            }

            for(int i=0;i<_equipTokenLayout.transform.childCount;i++)
            {
                o = _equipTokenLayout.transform.GetChild(i).gameObject;
                img = o.GetComponent<Image>();
                img.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _tokenSize.x);
                img.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, _tokenSize.y);
                o.SetActive(i < _sheet.availableEquipPoints);
            }
        }

        private void OnBottleCountChange(int count)
        {
            _bottleCountText.text = $"x {count}";
        }

        public void OnMovePushed(Vector2 direction)
        {
            _cardManager.OnMovePushed(direction);
        }

        public void OnMoveUpPushed()
        {
            _cardManager.OnMoveUpPushed();
        }

        public void OnMoveDownPushed()
        {
            _cardManager.OnMoveDownPushed();
        }

        public void OnMoveLeftPushed()
        {
            _cardManager.OnMoveLeftPushed();
        }

        public void OnMoveRightPushed()
        {
            _cardManager.OnMoveRightPushed();
        }

        public void OnButton1Pushed()
        {
            _cardManager.OnButton1Pushed();
            _stats.RefreshData();
        }

        public void OnButton2Pushed()
        {
            _cardManager.OnButton2Pushed();
        }

        public void OnButton3Pushed()
        {
            _cardManager.OnButton3Pushed();
            _stats.RefreshData();
        }

        public void OnButton4Pushed()
        {
            _cardManager.OnButton4Pushed();
        }

    }
}