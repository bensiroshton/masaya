using Siroshton.Masaya.Entity;
using Siroshton.Masaya.Player;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace Siroshton.Masaya.Item
{
    [Serializable]
    public class SkillCard : IItem
    {
        [SerializeField] private string _title;

        [Tooltip("Use {modifier} to be replaced with total modifier value.")]
        [Multiline]
        [SerializeField] private string _description;

        [SerializeField] private Core.Types.Rarity _rarity;

        [SerializeField] private AttributeModifiers.Modifier _modifier;

        [Tooltip("Cost is the number of bottles it takes to level up, modifiers round up.")]
        [SerializeField] private int _costAtLevelOne = 4;
        [SerializeField] private int _additionalCostEachLevel = 0;
        [SerializeField] private float _perLevelModifier = 0.02f;

        [SerializeField] private Sprite _pickupImage;
        [SerializeField] private Sprite _backgroundImage;

        [SerializeField, HideInInspector] private int _currentLevel = 0;
        [Tooltip("The level of the card when the player picks up this card for the first time.")]
        [SerializeField] private int _initialLevel = 1;
        [Tooltip("Add additional levels to this card if the player is already holding it.")]
        [SerializeField] private int _additionalLevel = 0;
        [SerializeField, HideInInspector] private int _maxLevel = 1;
        [SerializeField] private int _maxLevelCap = -1;

        [Tooltip("Percentage of current level cost to refund.")]
        [SerializeField] private float _burnCardRefund = 0.75f;

        [SerializeField] private UnityEvent<SkillCard> _onCardChange = new UnityEvent<SkillCard>();

        [SerializeField] private AudioClip _soundOnPickup;

        public Core.Types.Rarity rarity { get => _rarity; set => _rarity = value; }
        public string title { get => _title; }
        public string description { get => _description.Replace("{modifier}", currentLevelModifierString); }
        public AttributeModifiers.Modifier modifier { get => _modifier; }
        public int currentLevel { get => _currentLevel; }
        public int initialLevel { get => _initialLevel; }
        public int additionaLevel { get => _additionalLevel; }
        public int maxLevel { get => _maxLevel; }
        public bool atMaxLevel { get => _currentLevel == _maxLevel || _maxLevelCap > 1 && _currentLevel == _maxLevelCap; }
        public float perLevelModifier { get => _perLevelModifier; }
        public string perLevelModifierString { get => (_perLevelModifier > 0 ? "+" : "") + _perLevelModifier.ToString("0.0%"); }
        public float currentLevelModifier { get => (float)_currentLevel * _perLevelModifier; }
        public string currentLevelModifierString { get => currentLevelModifier.ToString("0.0%"); }
        public bool canBurnCard { get => _maxLevel > 0; }
        public int burnCardRefundAmount { get => Mathf.CeilToInt((float)GetCostAtLevel(_currentLevel) * _burnCardRefund); }
        public Sprite backgroundImage { get => _backgroundImage; }
        public UnityEvent<SkillCard> onCardChange { get => _onCardChange; set => _onCardChange = value; }

        public int costToLevelUp
        {
            get
            {
                if( _currentLevel == _maxLevel || _maxLevelCap > 1 && _currentLevel == _maxLevelCap ) return -1;
                else return GetCostAtLevel(_currentLevel + 1);
            }
        }

        public Sprite pickupImage => _pickupImage;
        public string itemName => _title;
        public AudioClip soundOnPickup { get => _soundOnPickup; set => _soundOnPickup = value; }

        private int GetCostAtLevel(int level)
        {
            //return PlayerUtil.GetBlockSizeForLevel(_costAtLevelOne, level, Player.Player.instance.characterSheet.difficultyRamp);
            return _costAtLevelOne + (level - 1) + _additionalCostEachLevel;
        }

        public bool CanLevelUp(CharacterSheet sheet)
        {
            if (atMaxLevel) return false;
            else if (sheet.bottleCount <  costToLevelUp) return false;
            
            return true;
        }

        public void OnInitialPickup()
        {
            _currentLevel = _initialLevel;
            _maxLevel = Mathf.Max(1, _initialLevel);
            if (_maxLevelCap >= 0) _maxLevel = Mathf.Min(_maxLevel, _maxLevelCap);

            _onCardChange.Invoke(this);
        }

        public void OnAdditionalPickup()
        {
            _currentLevel += _additionalLevel;
            _maxLevel += Mathf.Max(1, _additionalLevel);
            if(_maxLevelCap >= 0) _maxLevel = Mathf.Min(_maxLevel, _maxLevelCap);

            _onCardChange.Invoke(this);
        }

        /// <summary>
        /// Increase the max level of this card.
        /// </summary>
        /// <returns>true if successful.</returns>
        public bool IncreaseMaxLevel()
        {
            if (_maxLevel == _maxLevelCap) return false;

            _maxLevel++;
            _onCardChange.Invoke(this);
            return true;
        }

        /// <summary>
        /// LevelUp the skill card, will only succeed if its not already at the max level and the CharacterSheet can afford it.
        /// </summary>
        /// <returns>true if successful.</returns>
        public bool LevelUp(CharacterSheet sheet)
        {
            if( !CanLevelUp(sheet) ) return false;
            else if (!sheet.UseBottles(costToLevelUp)) return false;

            _currentLevel++;
            _onCardChange.Invoke(this);
            return true;
        }

        /// <summary>
        /// Burn the card, will only succeed if the max level is greater than 0.  The CharacterSheet will be refunded.
        /// </summary>
        /// <returns>new card max level, when 0 the card should be destroyed.</returns>
        public int BurnCard(CharacterSheet sheet)
        {
            if( !canBurnCard ) return _maxLevel;

            sheet.bottleCount += burnCardRefundAmount;
            _maxLevel--;
            if( _currentLevel > _maxLevel ) _currentLevel = _maxLevel;
            _onCardChange.Invoke(this);
            return _maxLevel;
        }

    }
}