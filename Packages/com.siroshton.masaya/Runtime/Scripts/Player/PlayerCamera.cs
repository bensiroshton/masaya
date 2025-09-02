using Siroshton.Masaya.Core;
using Siroshton.Masaya.Environment;
using Siroshton.Masaya.Math;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Siroshton.Masaya.Player
{
    [RequireComponent(typeof(Camera))]
    [RequireComponent(typeof(SphereCollider))]
    public class PlayerCamera : MonoBehaviour
    {
        public enum CameraZone
        {
            Standard,
            NoFireZone
        }

        [Serializable]
        public struct CameraParameters
        {
            [Tooltip("Distance camera should be away from the player.")]
            public float targetDistance;

            [Tooltip("Minimum distance from camera center projection allowed to drift.")]
            [Range(0, 5)]
            public float minTargetRadius;

            [Tooltip("Maxium distance from camera center projection allowed to drift.")]
            [Range(0, 5)]
            public float maxTargetRadius;

            [Tooltip("Set target point blend between players movement vs aim direction. 0=move direction, 1=aim direction.")]
            [Range(0, 1)]
            public float moveAimBlend;

            [Tooltip("Time it takes to move to the desired camera position when idle.")]
            [Range(0, 5)] public float idleMotionTime;

            [Tooltip("Time it takes to move to the desired camera position when running.")]
            [Range(0, 5)] public float runningMotionTime;
        }

        [SerializeField, Range(0, 40)] private float _focusPointDistanceInfluence = 20.0f;
        [SerializeField, Range(0, 10)] private float _zoneTransitionDuration = 2.0f;
        [SerializeField] private CameraZone _activeZone = CameraZone.Standard;
        [SerializeField] private CameraParameters _standardZone;
        [SerializeField] private CameraParameters _noFireZone;

        private Camera _camera;
        private SphereCollider _viewCollider;
        private Rect _viewport = new Rect(0, 0, 1, 1);
        private Vector3[] _frustumCorners = new Vector3[4];
        private Player _player;
        private Vector3 _trackingVelocity;
        private float _smoothRadius;
        private float _radiusVelocity;
        private Vector3 _smoothedAim;
        private Vector3 _smoothedAimVelocity;
        private Vector3 _targetPoint;

        private CameraZone _lastZone;
        private CameraParameters _currentParameters;
        private CameraParameters _targetParameters;
        private CameraParameters _velocityParameters;

        private HashSet<Collider> _focusObjects = new HashSet<Collider>();

        private bool _hasStartBeenCalled;

        public CameraZone activeZone
        { 
            get => _activeZone; 
            set
            {
                if( _activeZone == value ) return;

                _lastZone = _activeZone;
                _activeZone = value;
                _targetParameters = activeZoneParams;
            }
        }

        public CameraParameters currentZoneParams => _currentParameters;

        private CameraParameters activeZoneParams
        {
            get
            {
                if (_activeZone == CameraZone.NoFireZone) return _noFireZone;
                else return _standardZone;
            }
        }

        private void Awake()
        {
            _currentParameters = activeZoneParams;
            _targetParameters = _currentParameters;

            _camera = GetComponent<Camera>();
            _viewCollider = GetComponent<SphereCollider>();
        }

        private void Start()
        {
            _hasStartBeenCalled = true;
            _player = Player.instance;
            _lastZone = _activeZone;
            SnapToTarget();
        }

        private void UpdateTargetPoint()
        {
#if UNITY_EDITOR
            if( _player == null ) _player = Player.instance;
#endif
            Vector3 dir = Vector3.Lerp(_player.transform.forward, _smoothedAim, _currentParameters.moveAimBlend);
            _targetPoint = _player.transform.position + dir * _smoothRadius;

            // Add in Focus points if we have them
            if (_focusObjects.Count > 0)
            {
                _focusObjects.RemoveWhere((obj) => obj == null);
                
                Vector3 focusPoint = Vector3.zero;
                float d;
                int count = 0;
                var it = _focusObjects.GetEnumerator();

                float targetDistance = activeZoneParams.targetDistance;
                int targetDistCount = 1;

                while (it.MoveNext())
                {
                    if( !it.Current.enabled ) continue;
                    else if (!it.Current.gameObject.activeInHierarchy ) continue;

                    d = (it.Current.transform.position - _targetPoint).magnitude;
                    if( d <= _focusPointDistanceInfluence )
                    {
                        float influence = 1.0f - d / _focusPointDistanceInfluence;
                        focusPoint += Vector3.Lerp(_targetPoint, it.Current.transform.position, influence);
                        count++;

                        if( it.Current.GetComponent<CameraFocus>() is CameraFocus focus )
                        {
                            targetDistance += Mathf.Lerp(_targetParameters.targetDistance, focus.targetDistance, influence);
                            targetDistCount++;
                        }
                    }
                }

                if( count > 0 )
                {
                    _targetPoint = focusPoint / (float)count;
                }

                _targetParameters.targetDistance = targetDistance / (float)targetDistCount;
            }

            // now make sure we are no further than maxTargetRadius from our player.
            Vector3 offset = _targetPoint - _player.transform.position;
            if( offset.sqrMagnitude > _currentParameters.maxTargetRadius * _currentParameters.maxTargetRadius)
            {
                _targetPoint = _player.transform.position + offset.normalized * _currentParameters.maxTargetRadius;
            }
        }

        private Vector3 desiredCameraPos => _targetPoint - transform.forward * _currentParameters.targetDistance;

        private CameraZone GetPlayerZone()
        {
            if( _player.inNoFireZone ) return CameraZone.NoFireZone;
            else return CameraZone.Standard;
        }

        public void SnapToTarget()
        {
            if( !_hasStartBeenCalled ) return;

#if UNITY_EDITOR
            if ( !Application.isPlaying )
            {
                _currentParameters = activeZoneParams;
            }
#endif
            UpdateTargetPoint();
            _trackingVelocity = Vector3.zero;
            _smoothRadius = 0;
            _radiusVelocity = 0;
            _smoothedAim = Vector3.zero;
            _smoothedAimVelocity = Vector3.zero;
            _targetParameters = _currentParameters;
            transform.position = desiredCameraPos;
        }

#if UNITY_EDITOR
        public void Editor_Update()
        {
            // update fields to reflect editor changes
            //if( !_isTransitioning ) _currentParameters = activeZoneParams;
            //else _transitionTo = activeZoneParams;
        }
#endif
        
        public void LateUpdate()
        {
            float deltaTime = GameState.deltaTime;
            if( deltaTime == 0 ) return;

            activeZone = GetPlayerZone();

            _currentParameters.targetDistance = Mathf.SmoothDamp(_currentParameters.targetDistance, _targetParameters.targetDistance, ref _velocityParameters.targetDistance, _zoneTransitionDuration, Mathf.Infinity, deltaTime);
            _currentParameters.minTargetRadius = Mathf.SmoothDamp(_currentParameters.minTargetRadius, _targetParameters.minTargetRadius, ref _velocityParameters.minTargetRadius, _zoneTransitionDuration, Mathf.Infinity, deltaTime);
            _currentParameters.maxTargetRadius = Mathf.SmoothDamp(_currentParameters.maxTargetRadius, _targetParameters.maxTargetRadius, ref _velocityParameters.maxTargetRadius, _zoneTransitionDuration, Mathf.Infinity, deltaTime);
            _currentParameters.moveAimBlend = Mathf.SmoothDamp(_currentParameters.moveAimBlend, _targetParameters.moveAimBlend, ref _velocityParameters.moveAimBlend, _zoneTransitionDuration, Mathf.Infinity, deltaTime);
            _currentParameters.idleMotionTime = Mathf.SmoothDamp(_currentParameters.idleMotionTime, _targetParameters.idleMotionTime, ref _velocityParameters.idleMotionTime, _zoneTransitionDuration, Mathf.Infinity, deltaTime);
            _currentParameters.runningMotionTime = Mathf.SmoothDamp(_currentParameters.runningMotionTime, _targetParameters.runningMotionTime, ref _velocityParameters.runningMotionTime, _zoneTransitionDuration, Mathf.Infinity, deltaTime);

            float speedPos = _player.currentSpeed / _player.speed;
            float smoothTime = Mathf.Lerp(_currentParameters.idleMotionTime, _currentParameters.runningMotionTime, speedPos);

            // update target aim
            Vector3 aimTarget;
            if( _player.inputAimDirection.sqrMagnitude > 0 ) aimTarget = _player.inputAimDirection.normalized;
            else aimTarget = _player.transform.forward;

            _smoothedAim = Vector3.SmoothDamp(_smoothedAim, aimTarget, ref _smoothedAimVelocity, smoothTime, Mathf.Infinity, deltaTime);

            // update target radius
            float targetRadius = Mathf.Lerp(_currentParameters.minTargetRadius, _currentParameters.maxTargetRadius, speedPos);
            _smoothRadius = Mathf.SmoothDamp(_smoothRadius, targetRadius, ref _radiusVelocity, smoothTime, Mathf.Infinity, deltaTime);

            // update target point
            UpdateTargetPoint();

            // update motion
            transform.position = Vector3.SmoothDamp(transform.position, desiredCameraPos, ref _trackingVelocity, smoothTime, Mathf.Infinity, deltaTime);

            // update View Collider
            UpdateViewCollider();
        }

        private void UpdateViewCollider()
        {
            _camera.CalculateFrustumCorners(_viewport, (_targetPoint - transform.position).magnitude, Camera.MonoOrStereoscopicEye.Mono, _frustumCorners);
            _viewCollider.center = (_frustumCorners[0] + _frustumCorners[1] + _frustumCorners[2] + _frustumCorners[3]) / 4.0f;
            _viewCollider.radius = (_frustumCorners[0] - _viewCollider.center).magnitude;
        }

        private void OnTriggerEnter(Collider collider)
        {
            if( collider.gameObject.layer == GameLayers.area )
            {
                Area area = collider.GetComponent<Area>();
                if( area != null ) area.Activate();
            }
            else if( collider.gameObject.layer == GameLayers.cameraFocus )
            {
                _focusObjects.Add(collider);
            }
        }

        private void OnTriggerExit(Collider collider)
        {
            if (collider.gameObject.layer == GameLayers.area)
            {
                Area area = collider.GetComponent<Area>();
                if (area != null) area.Deactivate();
            }
            else if (collider.gameObject.layer == GameLayers.cameraFocus)
            {
                _focusObjects.Remove(collider);
            }
        }

        private void OnDrawGizmosSelected()
        {
            if( _player != null ) // happens when not playing in editor mode
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawWireSphere(_targetPoint, 0.1f);
            }
        }
    }

}