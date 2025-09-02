using UnityEngine;

namespace Siroshton.Masaya.Path
{
    public abstract class TargetProvider : MonoBehaviour
    {
        public abstract bool GetTarget(Transform current, out Vector3 target, float timeSinceLastCall);
    }
}
