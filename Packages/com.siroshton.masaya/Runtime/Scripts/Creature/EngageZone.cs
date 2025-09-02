using Siroshton.Masaya.Core;
using UnityEngine;

namespace Siroshton.Masaya.Creature
{
    [RequireComponent(typeof(Collider))]
    internal class EngageZone : MonoBehaviour
    {
        private Creature _creature;

        private void Start()
        {
            gameObject.layer = GameLayers.triggers;
            
            Collider collider = GetComponent<Collider>();
            collider.isTrigger = true;

            _creature = GetComponentInParent<Creature>();
        }

        private void OnTriggerEnter(Collider other)
        {
            _creature.Engage();
        }

    }

}