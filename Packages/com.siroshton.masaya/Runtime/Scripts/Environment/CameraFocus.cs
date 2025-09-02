using Siroshton.Masaya.Core;
using Siroshton.Masaya.Math;
using UnityEngine;

namespace Siroshton.Masaya.Environment
{
    [RequireComponent(typeof(SphereCollider))]
    public class CameraFocus : MonoBehaviour
    {
        [SerializeField] private float _targetDistance = 40;

        public float targetDistance => _targetDistance;

#if UNITY_EDITOR
        private void OnValidate()
        {
            gameObject.layer = GameLayers.cameraFocus;
        }
#endif

    }

}