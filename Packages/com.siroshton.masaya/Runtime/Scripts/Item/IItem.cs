
using UnityEngine;
using static Siroshton.Masaya.Core.Types;

namespace Siroshton.Masaya.Item
{

    public interface IItem
    {
        public string itemName { get; }
        public Rarity rarity { get; }
        public AudioClip soundOnPickup { get; }
        public Sprite pickupImage { get; }
    }

}