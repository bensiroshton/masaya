using Siroshton.Masaya.Core;
using UnityEngine;

namespace Siroshton.Masaya.Effect
{
    [RequireComponent(typeof(Camera))]
    public class CameraReflection : MonoBehaviour
    {
        [Tooltip("Update the camera reflection every X seconds.")]
        [SerializeField] private float _updateInterval = 0.25f;

        // NOTE: we are currently assuming this plane is an XZ plane
        [SerializeField] private Transform _reflectionPlane;
        [SerializeField, Range(-10, 10)] private float _planeOffset;

        private Camera _camera;
        private Camera _cameraToReflect;
        private RenderTexture _renderTexture;
        private float _timeSinceUpdate;

        private void Awake()
        {
            _cameraToReflect = Camera.main;
            _camera = GetComponent<Camera>();
            _camera.enabled = false;
            _renderTexture = _camera.targetTexture;
        }

        private void Start()
        {
            _camera.nearClipPlane = _cameraToReflect.nearClipPlane;
            _camera.farClipPlane = _cameraToReflect.farClipPlane;
            _camera.cameraType = _cameraToReflect.cameraType;
            _camera.fieldOfView = _cameraToReflect.fieldOfView;
            _camera.aspect = _cameraToReflect.aspect;
        }

        private void Update()
        {
            _timeSinceUpdate += GameState.deltaTime;
            if ( _timeSinceUpdate >= _updateInterval )
            {
                Vector3 pos = _cameraToReflect.transform.position;
                pos.y = _reflectionPlane.position.y - pos.y;
                transform.position = pos;

                Vector3 fwd = Vector3.Reflect(_cameraToReflect.transform.forward, Vector3.up);
                transform.rotation = Quaternion.LookRotation(fwd, Vector3.down);

                var clipPlane = CameraSpacePlane(_camera, _reflectionPlane.position + _reflectionPlane.up * _planeOffset, Vector3.up, 1.0f);
                var projection = _cameraToReflect.CalculateObliqueMatrix(clipPlane);
                _camera.projectionMatrix = projection;

                UpdateTexture();
                _timeSinceUpdate = 0;
            }
        }

        private void UpdateTexture()
        {
            if( _renderTexture.width != _camera.pixelWidth || _renderTexture.height != _camera.pixelHeight )
            {
                _renderTexture.width = _camera.pixelHeight;
                _renderTexture.height = _camera.pixelHeight;
            }

            _camera.Render();
        }

        // This is taken from the Boat Attack (PlanarReflections.cs) example.
        // Given position/normal of the plane, calculates plane in camera space.
        private Vector4 CameraSpacePlane(Camera cam, Vector3 pos, Vector3 normal, float sideSign)
        {
            var offsetPos = pos + normal;
            var m = cam.worldToCameraMatrix;
            var cameraPosition = m.MultiplyPoint(offsetPos);
            var cameraNormal = m.MultiplyVector(normal).normalized * sideSign;
            return new Vector4(cameraNormal.x, cameraNormal.y, cameraNormal.z, -Vector3.Dot(cameraPosition, cameraNormal));
        }
    }

}