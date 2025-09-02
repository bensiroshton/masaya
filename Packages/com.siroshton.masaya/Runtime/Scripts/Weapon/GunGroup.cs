using Siroshton.Masaya.Entity;
using UnityEngine;

namespace Siroshton.Masaya.Weapon
{

    public class GunGroup : MonoBehaviour, IWeapon
    {
        [SerializeField] private Gun[] _guns;

        private AttributeModifiers _ownerAttributeModifiers;

        public AttributeModifiers ownerAttributeModifiers
        { 
            get => _ownerAttributeModifiers; 
            set => _ownerAttributeModifiers = value;
        }

        public bool canTrigger => _guns != null && _guns.Length > 0 && _guns[0].canTrigger;
        public float timeSinceTriggered => (_guns != null && _guns.Length > 0) ? _guns[0].timeSinceTriggered : 0;

        public bool Trigger()
        {
            if( _guns == null ) return false;

            bool triggered = false;

            for(int i=0;i<_guns.Length;i++)
            {
                _guns[i].ownerAttributeModifiers = _ownerAttributeModifiers;
                triggered |= _guns[i].Trigger();
            }

            return triggered;
        }
    }

}