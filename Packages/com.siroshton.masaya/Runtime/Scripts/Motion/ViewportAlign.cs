using Siroshton.Masaya.Core;
using Siroshton.Masaya.Math;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace Siroshton.Masaya.Motion
{
    [ExecuteInEditMode]
    public class ViewportAlign : MonoBehaviour
    {
        private void Update()
        {
#if UNITY_EDITOR
            if( Camera.current != null )
            {
                transform.rotation = Quaternion.LookRotation(-Camera.current.transform.forward, Vector3.up);
            }
#else
            transform.rotation = Quaternion.LookRotation(-Camera.main.transform.forward, Vector3.up);
#endif
        }
    }

}