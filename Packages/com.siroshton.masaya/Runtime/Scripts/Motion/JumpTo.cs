using Siroshton.Masaya.Core;
using UnityEngine;
using UnityEngine.Events;

namespace Siroshton.Masaya.Motion
{
    public class JumpTo : MonoBehaviour
    {
        [SerializeField] private bool _startImmediately = false;
        [SerializeField] private Vector3 _targetPosition;
        [SerializeField] private float _height = 1;
        [SerializeField] private float _duration = 1;
        [SerializeField] private UnityEvent _onFinished = new UnityEvent();

        private float _time;
        private bool _isJumping;
        private Vector3 _start;

        public bool startImmediately { get => _startImmediately; set => _startImmediately = value; }
        public Vector3 targetPosition { get => _targetPosition; set => _targetPosition = value; }
        public float height { get => _height; set => _height = value; }
        public float duration { get => _duration; set => _duration = value; }
        public UnityEvent onFinished { get => _onFinished; set => _onFinished = value; }

        private void Start()
        {
            if (_startImmediately) StartJump();
        }

        public void StartJump()
        {
            if (_duration <= 0)
            {
                transform.position = _targetPosition;
                return;
            }

            _time = 0;
            _isJumping = true;
            _start = transform.position;
        }

        private void Update()
        {
            if (!_isJumping) return;

            _time += GameState.deltaTime;
            if (_time >= _duration)
            {
                _time = _duration;
                _isJumping = false;
            }

            float ntime = _time / _duration;
            Vector3 pos = Vector3.Lerp(_start, _targetPosition, ntime);
            pos.y += Mathf.Sin(Mathf.PI * ntime) * _height;
            transform.position = pos;

            if (!_isJumping)
            {
                _onFinished.Invoke();
            }
        }

    }

}