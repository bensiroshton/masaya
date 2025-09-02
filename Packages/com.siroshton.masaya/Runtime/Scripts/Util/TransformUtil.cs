

using UnityEngine;

namespace Siroshton.Masaya.Util
{

    static public class TransformUtil
    {

        static public Transform GetRoot(Transform transform)
        {
            while( transform.parent != null )
            {
                transform = transform.parent;
            }

            return transform; 
        }
    }
}