using UnityEngine;

namespace Siroshton.Masaya.Motion
{

    public class IgnoreParentRotation : MonoBehaviour
    {

        public void LateUpdate()
        {
            if( transform.parent == null ) return;

            transform.rotation *= Quaternion.Inverse(transform.parent.rotation);
        }

    }
}