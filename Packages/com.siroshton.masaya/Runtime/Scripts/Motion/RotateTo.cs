using Siroshton.Masaya.Core;
using Siroshton.Masaya.Math;
using UnityEngine;
using UnityEngine.Events;

namespace Siroshton.Masaya.Motion
{
    public class RotateTo : MonoBehaviour
    {
        public enum RotationMethod
        {
            Time,
            Speed
        }

        [SerializeField] private bool _startImmediately = false;
        [SerializeField] private Vector3 _targetRotation;
        [SerializeField] private bool _slerp = true;
        [SerializeField] private bool _easeInOut = false;
        [SerializeField] private RotationMethod _rotationMethod = RotationMethod.Time;
        [Tooltip("When Rotation Method is set to Time the rotation will occur over this duration.")]
        [SerializeField] private float _duration = 1;
        [Tooltip("When Rotation Method is set to Speed the rotation will use degrees per second to carry out the rotation.")]
        [SerializeField] private float _degreesPerSecond = 10;
        [SerializeField] private bool _useGameStateTime = true;
        [SerializeField] private bool _enableDisableOnStartStopCalls = true;
        [SerializeField] private UnityEvent _onFinished = new UnityEvent();

        private float _time;
        private bool _isRotating;
        private Quaternion _start;
        private Quaternion _target;
        private float _calculatedDuration;

        public Vector3 targetRotation { get => _targetRotation; set => _targetRotation = value; }

        private void Start()
        {
            if (_startImmediately) StartRotating();
        }

        public void StartRotating()
        {
            if (_duration <= 0)
            {
                transform.localRotation = _target;
                if (_enableDisableOnStartStopCalls) enabled = false;
                return;
            }

            _time = 0;
            _isRotating = true;
            _start = transform.localRotation;
            _target = Quaternion.Euler(_targetRotation);

            if (_rotationMethod == RotationMethod.Time) _calculatedDuration = _duration;
            else _calculatedDuration = Quaternion.Angle(_start, _target) / _degreesPerSecond;

            if(_calculatedDuration == 0)
            {
                _onFinished.Invoke();
                _isRotating = false;
                if (_enableDisableOnStartStopCalls) enabled = false;
            }
            else if (_enableDisableOnStartStopCalls) enabled = true;


        }

        public void StopRotating()
        {
            _isRotating = false;
            _onFinished.Invoke();
            if (_enableDisableOnStartStopCalls) enabled = false;
        }

        public void RotateImmediate()
        {
            transform.localRotation = _target;
        }

        private void Update()
        {
            if (!_isRotating) return;

            _time += _useGameStateTime ? GameState.deltaTime : Time.deltaTime;
            if (_time >= _calculatedDuration)
            {
                _time = _calculatedDuration;
                _isRotating = false;
            }

            float pos = _time / _calculatedDuration;
            if (_easeInOut) pos = MathUtil.SmoothNormalizedTime(pos);

            if ( _slerp )
            {
                transform.localRotation = Quaternion.Slerp(_start, _target, pos);
            }
            else transform.localRotation = Quaternion.Lerp(_start, _target, pos);

            if (!_isRotating)
            {
                _onFinished.Invoke();
                if (_enableDisableOnStartStopCalls) enabled = false;
            }
        }

    }

}