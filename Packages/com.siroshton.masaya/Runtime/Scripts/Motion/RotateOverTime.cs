using Siroshton.Masaya.Core;
using UnityEngine;
using UnityEngine.Events;

namespace Siroshton.Masaya.Motion
{

    public class RotateOverTime : MonoBehaviour
    {
        [SerializeField] private bool _startImmediately = false;
        [SerializeField] private Vector3 _degreesPerSecond;
        [SerializeField] private float _duration;
        [SerializeField] private UnityEvent _onFinished = new UnityEvent();

        private float _timeRotating;
        private bool _isRotating;

        private void Start()
        {
            if( _startImmediately ) StartRotating();
        }

        public void StartRotating()
        {
            _timeRotating = 0;
            _isRotating = true;
        }

        private void Update()
        {
            if( !_isRotating ) return;

            _timeRotating += GameState.deltaTime;
            Vector3 rotation = transform.localRotation.eulerAngles;
            rotation += _degreesPerSecond * GameState.deltaTime;
            transform.localRotation = Quaternion.Euler(rotation);

            if (_timeRotating > _duration)
            {
                _onFinished.Invoke();
                _isRotating = false;
            }
        }

    }

}