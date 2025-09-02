using UnityEngine;

namespace Siroshton.Masaya.Path
{
    public class StationaryProvider : TargetProvider
    {
        public override bool GetTarget(Transform current, out Vector3 target, float timeSinceLastCall)
        {
            target = current.position;
            return true;
        }

        public override string ToString()
        {
            return $"StationaryProvider";
        }

    }
}
