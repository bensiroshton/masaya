using Siroshton.Masaya.Item;
using Siroshton.Masaya.Player;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Siroshton.Masaya.UI
{

    public class CardManager : MonoBehaviour, IUIInput
    {
        [Serializable]
        public struct Audio
        {
            public AudioSource source;
            public AudioClip cardLeveledUp;
            public AudioClip cardBurned;
            public AudioClip cardSelected;
        }

        [SerializeField] private CardItem _cardItemPrefab;
        [SerializeField] private CardLayout _cardLayoutPrefab;
        [SerializeField] private GameObject[] _layoutProviders;
        [SerializeField] private Audio _audio;


        private CardItem _selected;

        public struct LayoutItem
        {
            public Vector2 position;
            public float rotation;
            public Vector2 cardSize;
            public float scale;
        }

        public interface ILayoutItemProvider
        {
            public CardManager.LayoutItem[] GetLayout();
        }

        private List<LayoutItem> _layout = new List<LayoutItem>();
        private List<CardItem> _cards = new List<CardItem>();
        private Vector2 _areaSize;
        private Canvas _canvas;

        private void OnEnable()
        {
            // Add player cards to grid
            RemoveCards();
            AddPlayerCards();
        }

        private void OnDisable()
        {
            RemoveCards();
        }

        private void Start()
        {
            _canvas = GetComponentInParent<Canvas>();

            CharacterSheet sheet = Player.Player.instance.characterSheet;
            sheet.onCardRemoved.AddListener(OnCardRemoved);
            sheet.onBottleCountChange.AddListener(OnBottleCountChange);
        }

        private void OnBottleCountChange(int count)
        {
            for(int i=0;i<_cards.Count;i++)
            {
                _cards[i].RefreshData();
            }
        }

        private void AddPlayerCards()
        {
            List<SkillCard> cards = Player.Player.instance.characterSheet?.cards;
            if (cards == null) return;

            int count = 0;
            foreach (SkillCard card in cards)
            {
                CardItem cardItem;
                if( count < _cards.Count )
                {
                    cardItem = _cards[count];
                }
                else
                {
                    cardItem = GameObject.Instantiate<CardItem>(_cardItemPrefab);
                    cardItem.name = $"Card {count}";
                    cardItem.transform.SetParent(transform);
                    cardItem.transform.localPosition = Vector3.zero;
                    cardItem.transform.rotation = Quaternion.identity;
                    _cards.Add(cardItem);
                }

                cardItem.SetCard(card, _cardLayoutPrefab);
                cardItem.SetDefaultState();
                cardItem.selected = false;
                count++;
            }

            UpdateLayout();
        }

        private void PlayAudio(AudioClip clip, CardItem item, float pitch = 1)
        {
            if( item == null )
            {
                _audio.source.panStereo = 0;
            }
            else
            {
                Vector2 pos = UIUtil.GetWorldCenter(item.transform as RectTransform);
                Vector3[] points = new Vector3[4];
                (_canvas.transform as RectTransform).GetWorldCorners(points);
                float n = (pos.x - points[0].x) / (points[2].x - points[0].x);
                _audio.source.panStereo = Mathf.Lerp(-1, 1, n);
            }

            _audio.source.pitch = pitch;
            _audio.source.PlayOneShot(clip);
        }

        private void OnCardRemoved(SkillCard card)
        {
            RemoveCard(card);
        }

        private CardItem GetCardItem(SkillCard card)
        {
            return _cards.Find( (a) => a.card == card );
        }

        private void RemoveCard(SkillCard card)
        {
            if( card == _selected?.card )
            {
                _selected.selected = false;
                _selected = null;
            }

            for (int i=0;i<_cards.Count;i++)
            {
                if( _cards[i].card == card )
                {
                    _cards[i].RemoveCard();
                    Destroy(_cards[i].gameObject);
                    _cards.RemoveAt(i);
                    return;
                }
            }
        }

        private void RemoveCards()
        {
            // Destroy Cards
            for (int i = 0; i < _cards.Count; i++)
            {
                _cards[i].RemoveCard();
                Destroy(_cards[i].gameObject);
            }

            _cards.Clear();
            _selected = null;
        }

        private int GetSelectedIndex()
        {
            if (_selected == null) return -1;
            return _cards.FindIndex((a) => a == _selected);
        }

        private void UpdateLayout()
        {
            _layout.Clear();

            if (_layoutProviders == null) return;

            for (int i = 0; i < _layoutProviders.Length; i++)
            {
                ILayoutItemProvider provider = _layoutProviders[i].GetComponent<ILayoutItemProvider>();
                if (provider != null)
                {
                    _layout.AddRange(provider.GetLayout());
                }
            }

            Vector2 center = UIUtil.GetWorldCenter(transform as RectTransform);
            _layout.Sort((a, b) => {
                return (a.position - center).sqrMagnitude.CompareTo((b.position - center).sqrMagnitude);
            });

            for (int i = 0; i < _cards.Count; i++)
            {
                _cards[i].SetLayout(_layout[i]);
            }
        }

        private void Update()
        {
            Vector2 size = (transform as RectTransform).rect.size;
            if (size != _areaSize)
            {
                _areaSize = size;
                UpdateLayout();
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            UpdateLayout();

            for (int i = 0; i < _layout.Count; i++)
            {
                LayoutItem item = _layout[i];
                Gizmos.color = Color.magenta;
                Gizmos.matrix = Matrix4x4.Translate(item.position) * Matrix4x4.Rotate(Quaternion.Euler(0, 0, item.rotation));
                Gizmos.DrawCube(Vector3.zero, item.cardSize);
            }
        }
#endif

        public void OnMovePushed(Vector2 direction)
        {
            Vector2 origin;
            if(_selected != null)
            {
                origin = UIUtil.GetWorldCenter(_selected.transform as RectTransform);
            }
            else
            {
                origin = UIUtil.GetWorldCenter(transform as RectTransform);
            }

            RaycastHit2D hit;
            hit = Physics2D.Raycast(origin, direction);

            // if nothing was hit then try rotating towards the y direction and try again, this
            // helps get a card in the next column if they don't exactly push towards a card
            if( hit.collider == null )
            {
                Vector2 originalDirection = direction;
                bool tryPositive = direction.y < 0;
                for(int i=0;i<2;i++)
                {
                    if(tryPositive)
                    {
                        direction = Quaternion.Euler(new Vector3(0, 0, 45)) * originalDirection;
                        tryPositive = false;
                    }
                    else
                    {
                        direction = Quaternion.Euler(new Vector3(0, 0, -45)) * originalDirection;
                        tryPositive = true;
                    }

                    hit = Physics2D.Raycast(origin, direction);
                    if( hit.collider != null ) break;
                }
            }

            // handle a hit if we have one.
            if (hit.collider != null)
            {
                if (_selected != null) _selected.selected = false;

                CardItem ci = hit.collider.GetComponentInParent<CardItem>();
                ci.selected = true;
                _selected = ci;
                PlayAudio(_audio.cardSelected, _selected);
            }
        }

        public void OnMoveUpPushed()
        {
        }

        public void OnMoveDownPushed()
        {
        }

        public void OnMoveLeftPushed()
        {
        }

        public void OnMoveRightPushed()
        {
        }

        public void OnButton1Pushed()
        {
            // Equip/UnEquip Card
            if (_selected != null && _selected.card != null)
            {
                CharacterSheet sheet = Player.Player.instance.characterSheet;
                if( sheet.IsCardEquippped(_selected.card) )
                {
                    sheet.UnEquipCard(_selected.card);
                }
                else
                {
                    if( !sheet.EquipCard(_selected.card) )
                    {
                        // TODO
                        Debug.Log("Show message the player needs to unequip a card first.");
                    }
                }

                _selected.RefreshData();
            }

        }

        public void OnButton2Pushed()
        {
            // Level Card
            if (_selected != null && _selected.card != null)
            {
                if (_selected.card.LevelUp(Player.Player.instance.characterSheet))
                {
                    PlayAudio(_audio.cardLeveledUp, _selected, 1 + (float)_selected.card.currentLevel * 0.02f);
                    _selected.RefreshData();
                }
            }
        }

        public void OnButton3Pushed()
        {
            // Burn Card
            if (_selected != null && _selected.card != null)
            {
                int selectedIndex = GetSelectedIndex();

                PlayAudio(_audio.cardBurned, _selected);

                if (_selected.card.BurnCard(Player.Player.instance.characterSheet) > 0)
                {
                    _selected.RefreshData();
                }
                else
                {
                    if (_cards.Count > 0)
                    {
                        if (selectedIndex >= _cards.Count) selectedIndex = _cards.Count - 1;

                        _selected = _cards[selectedIndex];
                        _selected.selected = true;
                        PlayAudio(_audio.cardSelected, _selected);
                    }
                }
            }
        }

        public void OnButton4Pushed()
        {
        }

    }

}