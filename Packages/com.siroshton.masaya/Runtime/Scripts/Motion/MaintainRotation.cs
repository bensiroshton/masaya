using Siroshton.Masaya.Core;
using Siroshton.Masaya.Math;
using UnityEngine;

namespace Siroshton.Masaya.Motion
{

    public class MaintainRotation : MonoBehaviour
    {

        public enum RotationSource
        {
            CaptureOnStartup,
            Fixed,
        }

        [SerializeField] RotationSource _rotationSource = RotationSource.CaptureOnStartup;
        [SerializeField] private Vector3 _fixedRotation;
        [SerializeField] private IntervalVec3 _shakeExtents;
        [SerializeField] private float _maxDegreesPerSecond;
        [SerializeField] private bool _isLocal = true;

        private Quaternion _targetRotation;

        public void Shake()
        {
            ApplyRandomRotation(_shakeExtents);
        }

        public void ApplyRandomRotation(IntervalVec3 range)
        {
            Vector3 euler = range.randomExtents;

            if (_isLocal)
            {
                transform.localRotation = _targetRotation * Quaternion.Euler(euler);
            }
            else
            {
                transform.rotation = _targetRotation * Quaternion.Euler(euler);
            }
        }

        private void Start()
        {
            if( _rotationSource == RotationSource.CaptureOnStartup )
            {
                if( _isLocal ) _targetRotation = transform.localRotation;
                else _targetRotation = transform.rotation;
            }
            else if( _rotationSource == RotationSource.Fixed )
            {
                _targetRotation = Quaternion.Euler(_fixedRotation);
            }
        }

        private void Update()
        {
            if( _isLocal )
            {
                transform.localRotation = Quaternion.RotateTowards(transform.localRotation, _targetRotation, _maxDegreesPerSecond * GameState.deltaTime);
            }
            else
            {
                transform.rotation = Quaternion.RotateTowards(transform.rotation, _targetRotation, _maxDegreesPerSecond * GameState.deltaTime);
            }
        }

    }

}