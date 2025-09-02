using Siroshton.Masaya.Mesh;
using UnityEngine;

namespace Siroshton.Masaya.Effect
{
    [RequireComponent(typeof(Camera))]
    public class CameraAlignPlanarUV : MonoBehaviour
    {
        [SerializeField] private MeshRenderer _renderer;

        private Camera _camera;

        private void Start()
        {
            FitCamera();
        }

        public void FitCamera()
        {
            if( _camera == null )
            {
                _camera = GetComponent<Camera>();
                if( !_camera.orthographic )
                {
                    Debug.LogWarning("Camera should be orthographic.");
                }
            }

            if( _renderer == null )
            {
                Debug.LogWarning("Renderer is null.");
                return;
            }

            //bool hasBounds;
            //Rect uvs = MeshUtil.GetUVBounds(_renderer.GetComponent<MeshFilter>(), out hasBounds);
            //if( !hasBounds ) return;

            Bounds bounds = _renderer.bounds;

            _camera.transform.position = bounds.center + Vector3.up * 5;
            _camera.aspect = bounds.size.x / bounds.size.z;
            _camera.orthographicSize = bounds.extents.z;
            _camera.fieldOfView = 90;




        }

    }

}