using UnityEngine;

namespace Siroshton.Masaya.Motion
{
    public struct PathPoint
    {
        public Vector3 position;
        public Quaternion rotation;
        public float speedMultiplier;

        public PathPoint(float speedMultiplier)
        {
            this.position = Vector3.zero;
            this.rotation = Quaternion.identity;
            this.speedMultiplier = speedMultiplier;
        }
    }
}