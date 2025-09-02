
using UnityEngine;

namespace Siroshton.Masaya.Motion
{

    public class KeepUpright : MonoBehaviour
    {
        [SerializeField] private Transform _forwardReference;

        private void Awake()
        {
            if( _forwardReference == null ) _forwardReference = transform.parent;
            
            if( _forwardReference == null )
            {
                Debug.LogWarning("Disabling KeepUpright, no forward reference available.");
                enabled = false;
            }
        }

        private void Update()
        {
            Vector3 point = transform.position + _forwardReference.forward;
            point.y = transform.position.y;

            transform.rotation = Quaternion.LookRotation(point - transform.position, Vector3.up);
        }

    }

}