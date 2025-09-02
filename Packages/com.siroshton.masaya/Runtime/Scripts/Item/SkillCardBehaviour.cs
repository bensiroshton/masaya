using UnityEngine;
using UnityEngine.Events;

namespace Siroshton.Masaya.Item
{
    public class SkillCardBehaviour : MonoBehaviour
    {
        [SerializeField] private SkillCard _card;
        [SerializeField] private UnityEvent _onPickup;

        public SkillCard card => _card;

        public void OnTriggerEnter(Collider other)
        {
            // make sure the Physics masks only allow the player to collide with this item.
            Player.Player.instance.GiveItem(_card);
            _onPickup?.Invoke();
            Destroy(gameObject);
        }
    }
}