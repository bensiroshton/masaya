
using UnityEngine;
using UnityEngine.Events;

namespace Siroshton.Masaya.Player
{
    public interface IEquipment
    {
        public GameObject gameObject { get; }
        public AudioClip soundOnEquip { get; }

        public UnityEvent onFoundBegin { get; set; }
        public UnityEvent onFoundHoldStart { get; set; }
        public UnityEvent onFoundHoldEnd { get; set; }
        public UnityEvent onFoundEnd { get; set; }
        public UnityEvent onHitFloor { get; set; }

        internal void DisableCollectionTriggers();
        internal void EnableCollectionTriggers();

        internal void TriggerFoundBegin();
        internal void TriggerFoundHoldStart();
        internal void TriggerFoundHoldEnd();
        internal void TriggerFoundEnd();
        internal void TriggerHitFloor();

    }
}