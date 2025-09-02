using Siroshton.Masaya.Audio;
using Siroshton.Masaya.Item;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Siroshton.Masaya.Player
{
    public class CharacterSheet : MonoBehaviour
    {
        [SerializeField] private int _bottleCount = 0;
        [SerializeField] private int _firstBottleBlockSize = 100;
        [SerializeField] private float _difficultyRamp = 0.01f;
        [Tooltip("Percentage of bottles to destroy when the player dies.")]
        [SerializeField] private float _destroyBottlesOnDeath = 0.5f;
        [SerializeField] private ExperiencePointAudio _experienceAudio;

        [SerializeField] private UnityEvent<int> _onExperienceGained = new UnityEvent<int>();
        [SerializeField] private UnityEvent<int> _onBottleCountChange = new UnityEvent<int>();
        [SerializeField] private UnityEvent<float> _onBottleFillChange = new UnityEvent<float>();

        [SerializeField] private UnityEvent<SkillCard> _onCardReceived = new UnityEvent<SkillCard>();
        [SerializeField] private UnityEvent<SkillCard> _onCardRemoved = new UnityEvent<SkillCard>();
        [SerializeField] private UnityEvent<SkillCard> _onCardEquipped = new UnityEvent<SkillCard>();
        [SerializeField] private UnityEvent<SkillCard> _onCardUnEquipped = new UnityEvent<SkillCard>();

        private int _blockLevel = 1;
        private int _totalExperience = 0;
        private int _blockExperience = 0;

        [Serializable]
        private class Cards
        {
            public List<SkillCard> collected = new List<SkillCard>();
            public HashSet<SkillCard> equipped = new HashSet<SkillCard>();
            public int equipPoints = 2;
        }

        [SerializeField] private Cards _cards = new Cards();

        public float destroyBottlesOnDeath { get => _destroyBottlesOnDeath; set => _destroyBottlesOnDeath = value; }
        public int blockLevel { get => _blockLevel; }
        public float difficultyRamp { get => _difficultyRamp; }
        public UnityEvent<int> onExperienceGained { get => _onExperienceGained; set => _onExperienceGained = value; }
        public UnityEvent<int> onBottleCountChange { get => _onBottleCountChange; set => _onBottleCountChange = value; }
        public UnityEvent<float> onBottleFillChange { get => _onBottleFillChange; set => _onBottleFillChange = value; }
        public UnityEvent<SkillCard> onCardReceived { get => _onCardReceived; set => _onCardReceived = value; }
        public UnityEvent<SkillCard> onCardRemoved { get => _onCardRemoved; set => _onCardRemoved = value; }
        public UnityEvent<SkillCard> onCardEquipped { get => _onCardEquipped; set => _onCardEquipped = value; }
        public UnityEvent<SkillCard> onCardUnEquipped { get => _onCardUnEquipped; set => _onCardUnEquipped = value; }

        public List<SkillCard> cards { get => _cards.collected; }
        public int equippedCardCount { get => _cards.equipped.Count; }
        public int availableEquipPoints { get => _cards.equipPoints - equippedCardCount; }
        public int totalEquipPoints { get => _cards.equipPoints; }

        public int bottleCount 
        { 
            get => _bottleCount; 
            set
            {
                if( _bottleCount == value ) return;

                _bottleCount = System.Math.Max(0, value); 
                _onBottleCountChange.Invoke(_bottleCount);
            }
        }

        public SkillCard[] GetEquippedCards()
        {
            SkillCard[] cards = new SkillCard[_cards.equipped.Count];
            _cards.equipped.CopyTo(cards);
            return cards;
        }

        public SkillCard FindCollectedCard(SkillCard searchCard)
        {
            return _cards.collected.Find((search) => search.title == searchCard.title);
        }

        public void GiveCard(SkillCard card)
        {
            if( card == null )
            {
                Debug.LogError("card is null.");
                return;
            }

            SkillCard found = FindCollectedCard(card);
            if( found != null )
            {
                found.OnAdditionalPickup();
            }
            else
            {
                _cards.collected.Add(card);
                card.onCardChange.AddListener(OnCardChange);
                card.OnInitialPickup();
            }

            _onCardReceived.Invoke(card);
        }

        public bool EquipCard(SkillCard card)
        {
            if( _cards.equipped.Count >= _cards.equipPoints )
            {
                Debug.Log("No available equip points.");
                return false;
            }

            SkillCard found = FindCollectedCard(card);
            if( found == null )
            {
                Debug.Log("Card is not available for equipping.");
                return false;
            }

            if( _cards.equipped.Contains(found) )
            {
                Debug.Log("Card is already equipped.");
                return false;
            }

            _cards.equipped.Add(found);
            _onCardEquipped?.Invoke(found);
            return true;
        }

        public bool UnEquipCard(SkillCard card)
        {
            if( !_cards.equipped.Contains(card) )
            {
                Debug.Log("Card is not equipped.");
                return false;
            }

            _cards.equipped.Remove(card);
            _onCardUnEquipped?.Invoke(card);
            return true;
        }

        public bool IsCardEquippped(SkillCard card)
        {
            if( card == null ) return false;

            return _cards.equipped.Contains(card);
        }

        private void OnCardChange(SkillCard card)
        {
            if( card.maxLevel == 0 )
            {
                // last card was burnt
                RemoveCard(card);
            }
        }

        public void RemoveCard(SkillCard card)
        {
            if (card == null)
            {
                Debug.LogError("card is null.");
                return;
            }

            SkillCard found = FindCollectedCard(card);
            if( found == null )
            {
                Debug.LogError("card not found in set.");
                return;
            }

            found.onCardChange.RemoveListener(OnCardChange);

            _cards.collected.Remove(found);
            _onCardRemoved.Invoke(found);
        }

        public bool UseBottles(int count)
        {
            if( _bottleCount <= 0 || count > _bottleCount ) return false;

            _bottleCount -= count;
            _onBottleCountChange.Invoke(_bottleCount);
            return true;
        }
        
        public void DestroyBottlesFromCharacterDeath()
        {
            DestroyBottles(Mathf.CeilToInt((float)_bottleCount * _destroyBottlesOnDeath));
        }
        
        public void DestroyBottles(int count)
        {
            _bottleCount -= count;
            if( _bottleCount < 0 ) _bottleCount = 0;
            _onBottleCountChange.Invoke(_bottleCount);
        }

        public void ClearCurrentBlockExperience()
        {
            if(_blockExperience <= 0) return;

            _blockExperience = 0;
            _onBottleFillChange.Invoke(GetCurrentBottleFillLevel());
        }

        public int GetCurrentBlockSize()
        {
            return GetBlockSizeForLevel(_blockLevel);
        }

        public int GetBlockSizeForLevel(int blockLevel)
        {
            return (int)(Mathf.Pow(blockLevel, _difficultyRamp) * (float)_firstBottleBlockSize);
        }

        /// <summary>
        /// Gives experience to the player, for each block level gained a bottle is awarded.
        /// </summary>
        /// <param name="experience"></param>
        public void GiveExperience(int experience)
        {
            if( experience <= 0 ) return;

            _totalExperience += _totalExperience;
            _blockExperience += experience;
            while (_blockExperience >= GetBlockSizeForLevel(_blockLevel))
            {
                _blockExperience -= GetBlockSizeForLevel(_blockLevel);
                _blockLevel++;
                _bottleCount++;
                _onBottleCountChange.Invoke(_bottleCount);
            }

            _onBottleFillChange.Invoke(GetCurrentBottleFillLevel());
            _onExperienceGained.Invoke(experience);

            _experienceAudio.PlayNotes(experience);
        }

        public float GetCurrentBottleFillLevel()
        {
            return (float)_blockExperience / (float)GetBlockSizeForLevel(_blockLevel);
        }
    }
}