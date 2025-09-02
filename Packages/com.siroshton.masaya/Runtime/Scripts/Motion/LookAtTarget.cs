
using Siroshton.Masaya.Core;
using Unity.Jobs.LowLevel.Unsafe;
using UnityEngine;

namespace Siroshton.Masaya.Motion
{

    public class LookAtTarget : MonoBehaviour
    {
        [SerializeField] private Transform _target;
        [Tooltip("Set to 0 to lock the XZ plane.")]
        [SerializeField] private float _yDirFactor = 1;
        [Tooltip("If set the twin will also look at the target and the look from point will be the midpoint.  This is useful for pairing eyes to look at things.")]
        [SerializeField] private Transform _twin;
        [Tooltip("Offset the Look At rotation.  Useful for Blender exported objects on wonky axes.")]
        [SerializeField] private Vector3 _offsetAngles;
        [Tooltip("Max Rotation on the XY axis, these are relative starting rotation.  When set to 0 it will lock the axis.")]
        [SerializeField] private Vector2 _maxAnglesXY = new Vector2(360, 360);
        [SerializeField] private Transform _maxAnglesRelativeTo;
        [Tooltip("Set to 0 to look at target completely every frame.")]
        [SerializeField] private float _maxDegreesPerSecond = 0;
        [Tooltip("Start dampening the time to reach the target angle when the remaining angle is < the dampen angle.")]
        [SerializeField, Range(0, 180)] private float _dampenAngle = 0;

        private Quaternion _offset;
        private Vector3 _startAngles;

        public Transform target { get => _target; set => _target = value; }
        public float yDirFactor { get => _yDirFactor; set => _yDirFactor = value; }
        public Transform twin { get => _twin; set => _twin = value; }
        public float maxDegreesPerSecond { get => _maxDegreesPerSecond; set => _maxDegreesPerSecond = value; }

        public Vector3 offsetAngles 
        { 
            get => _offsetAngles; 
            set
            {
                _offsetAngles = value;
                _offset = Quaternion.Euler(_offsetAngles);
            }
        }

        protected void Start()
        {
            _offset = Quaternion.Euler(_offsetAngles);
            _startAngles = transform.localRotation.eulerAngles;
        }

        protected void LateUpdate()
        {
#if UNITY_EDITOR
            _offset = Quaternion.Euler(_offsetAngles);
#endif

            Vector3 dir;
            if( _twin != null )
            {
                dir = (_target.transform.position - (transform.position + _twin.position) * 0.5f).normalized;
            }
            else
            {
                dir = (_target.transform.position - transform.position).normalized;
            }

            dir.y *= _yDirFactor;

            Quaternion lookRotation = Quaternion.LookRotation(dir, Vector3.up) * _offset;

            if ( _maxDegreesPerSecond <= 0 )
            {
                transform.rotation = lookRotation;
            }
            else
            {
                if(_dampenAngle > 0 )
                {
                    float angle = Quaternion.Angle(transform.rotation, lookRotation);
                    float amount = 1;
                    if( angle < _dampenAngle )
                    {
                        amount = angle / _dampenAngle;
                    }

                    transform.rotation = Quaternion.RotateTowards(transform.rotation, lookRotation, _maxDegreesPerSecond * GameState.deltaTime * amount);
                }
                else
                {
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, lookRotation, _maxDegreesPerSecond * GameState.deltaTime);
                }
            }

            // Limit rotataion axes (do this on the localRotation, we used world rotation above).
            if(_maxAnglesRelativeTo != null )
            {   
                Vector3 relAngles = _maxAnglesRelativeTo.rotation.eulerAngles;
                Vector3 diff = transform.rotation.eulerAngles - relAngles;
                diff.x = Mathf.Repeat(diff.x + 180, 360) - 180;
                diff.y = Mathf.Repeat(diff.y + 180, 360) - 180;
                diff.x = Mathf.Clamp(diff.x, -_maxAnglesXY.x, _maxAnglesXY.x);
                diff.y = Mathf.Clamp(diff.y, -_maxAnglesXY.y, _maxAnglesXY.y);
                transform.rotation = Quaternion.Euler(relAngles + diff);
            }
            else
            {
                Vector3 diff = transform.localRotation.eulerAngles - _startAngles;
                diff.x = Mathf.Repeat(diff.x + 180, 360) - 180;
                diff.y = Mathf.Repeat(diff.y + 180, 360) - 180;
                diff.x = Mathf.Clamp(diff.x, -_maxAnglesXY.x, _maxAnglesXY.x);
                diff.y = Mathf.Clamp(diff.y, -_maxAnglesXY.y, _maxAnglesXY.y);
                transform.localRotation = Quaternion.Euler(_startAngles + diff);

                /*
                Vector3 angles = transform.localRotation.eulerAngles;
                angles.x = Mathf.Repeat(angles.x + 180, 360) - 180;
                angles.y = Mathf.Repeat(angles.y + 180, 360) - 180;
                angles.x = Mathf.Clamp(angles.x, -_maxAnglesXY.x + _startAngles.x, _maxAnglesXY.x + _startAngles.x);
                angles.y = Mathf.Clamp(angles.y, -_maxAnglesXY.y + _startAngles.y, _maxAnglesXY.y + _startAngles.y);
                transform.localRotation = Quaternion.Euler(angles);
                */
            }


            // rotate twin if we have
            if ( _twin != null ) _twin.rotation = transform.rotation;
        }

    }

}