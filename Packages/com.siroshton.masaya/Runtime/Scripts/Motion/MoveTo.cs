using Siroshton.Masaya.Core;
using Siroshton.Masaya.Math;
using UnityEngine;
using UnityEngine.Events;

namespace Siroshton.Masaya.Motion
{
    public class MoveTo : MonoBehaviour
    {
        [SerializeField] private bool _startImmediately = false;
        [SerializeField] private bool _isRelative = false;
        [SerializeField] private Vector3 _targetPosition;
        [SerializeField] private bool _isLocal = true;
        [SerializeField] private float _duration = 1;
        [SerializeField] private bool _easeInOut = false;
        [SerializeField] private UnityEvent _onFinished;
        [SerializeField] private UnityEvent<GameObject> _onFinishedWithObject;

        private float _time;
        private bool _isMoving;
        private Vector3 _start;

        public Vector3 targetPosition { get => _targetPosition; set => _targetPosition = value; }
        public bool isRelative { get => _isRelative; set => _isRelative = value; }
        public bool isLocal { get => _isLocal; set => _isLocal = value; }
        public float duration { get => _duration; set => _duration = value; }
        public bool easeInOut { get => _easeInOut; set => _easeInOut = value; }
        public UnityEvent onFinished { get => _onFinished; set => _onFinished = value; }
        public UnityEvent<GameObject> onFinishedWithObject { get => _onFinishedWithObject; set => _onFinishedWithObject = value; }

        private void Start()
        {
            if (_startImmediately) StartMoving();
        }

        public void StartMoving()
        {
            StartMoving(_duration);
        }

        public void StartMoving(float duration)
        {
            _duration = duration;

            if (_duration <= 0)
            {
                if(_isLocal) transform.localPosition = _targetPosition;
                else transform.position = _targetPosition;
                return;
            }

            _time = 0;
            _isMoving = true;
            _start = transform.localPosition;
        }

        public void MoveImmediate()
        {
            transform.localPosition = _targetPosition;
        }

        private void Update()
        {
            if (!_isMoving) return;

            _time += GameState.deltaTime;
            if (_time >= _duration)
            {
                _time = _duration;
                _isMoving = false;
            }

            float pos = _time / _duration;
            if (_easeInOut) pos = MathUtil.SmoothNormalizedTime(pos);

            Vector3 target;
            if(_isRelative ) target = Vector3.Lerp(_start, _start + _targetPosition, pos);
            else target = Vector3.Lerp(_start, _targetPosition, pos);

            if(_isLocal) transform.localPosition = target;
            else transform.position = target;

            if (!_isMoving ) 
            {
                _onFinished?.Invoke();
                _onFinishedWithObject?.Invoke(gameObject);
            }
        }

    }

}