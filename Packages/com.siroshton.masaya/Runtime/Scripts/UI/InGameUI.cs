using Siroshton.Masaya.Player;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Siroshton.Masaya.UI
{
    [RequireComponent(typeof(RectTransform))]
    public class InGameUI : MonoBehaviour
    {
        [SerializeField] private Bottle _bottle;
        [SerializeField] private TMP_Text _bottleCount;

        private RectTransform _rectTransform;

        public RectTransform rectTransform => _rectTransform;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
        }

        private void Start()
        {
            CharacterSheet sheet = Player.Player.instance.characterSheet;

            OnBottleCountChange(sheet.bottleCount);
            OnFillLevelChange(sheet.GetCurrentBottleFillLevel());

            sheet.onBottleCountChange.AddListener(OnBottleCountChange);
            sheet.onBottleFillChange.AddListener(OnFillLevelChange);
        }

        private void OnBottleCountChange(int bottleCount)
        {
            _bottleCount.text = $"x {bottleCount}";
        }

        private void OnFillLevelChange(float fillLevel)
        {
            _bottle.fillLevel = fillLevel;
        }

    }
}
