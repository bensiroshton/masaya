

using UnityEngine;

namespace Siroshton.Masaya.Motion
{
    [ExecuteInEditMode]
    public class RelativeRotationOffset : MonoBehaviour
    {
        [SerializeField] private Transform _relativeTo;
        [SerializeField] private Vector3 _offset;

        private void Update()
        {
            transform.rotation = _relativeTo.rotation * Quaternion.Euler(_offset);
        }

    }

}
