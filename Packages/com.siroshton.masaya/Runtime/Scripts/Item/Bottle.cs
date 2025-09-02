
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

namespace Siroshton.Masaya.Item
{
    public class Bottle : MonoBehaviour, IItem
    {
        [SerializeField] private int _bottleCount = 1;
        [SerializeField] private AudioClip _soundOnPickup;
        [SerializeField] private Sprite _pickupImage;

        public int bottleCount => _bottleCount;
        public Core.Types.Rarity rarity => Core.Types.Rarity.Common;
        public Sprite pickupImage => _pickupImage;
        public string itemName => "Mana Bottle";
        public AudioClip soundOnPickup => _soundOnPickup;

        private void OnTriggerEnter(Collider other)
        {
            // make sure the Physics masks only allow the player to collide with this item.
            Player.Player.instance.GiveItem(this);
        }

    }

}
