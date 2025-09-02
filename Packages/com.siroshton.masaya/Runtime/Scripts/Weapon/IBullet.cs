using UnityEngine;
using static UnityEngine.GraphicsBuffer;

namespace Siroshton.Masaya.Weapon
{
    public interface IBullet
    {
        public Vector3 startPos { get; }
        public float range { get; set; }

    }
}