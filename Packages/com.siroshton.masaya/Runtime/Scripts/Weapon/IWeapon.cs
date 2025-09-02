
using Siroshton.Masaya.Entity;
using System.Numerics;

namespace Siroshton.Masaya.Weapon
{

    public interface IWeapon 
    {
        public AttributeModifiers ownerAttributeModifiers { get; set; }
        public float timeSinceTriggered { get; }
        public bool canTrigger { get; }
        public bool Trigger();
    }

}