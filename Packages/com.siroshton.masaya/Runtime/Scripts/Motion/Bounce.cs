using Siroshton.Masaya.Core;
using Siroshton.Masaya.Math;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Events;

namespace Siroshton.Masaya.Motion
{
    public class Bounce : MonoBehaviour
    {
        public enum Direction
        {
            Forward,
            Backward,
            Random
        }

        [SerializeField] private bool _startImmediately = false;
        [SerializeField] private Vector3 _extents = Vector3.zero;
        [SerializeField] private IntervalFloat _duration = new IntervalFloat(1, 1);
        [Tooltip("Number of times to bounce, -1 = infinitely")]
        [SerializeField] private int _count = -1;
        [SerializeField] private Direction _direction;
        [SerializeField] private UnityEvent _onIterationFinished = new UnityEvent();
        [SerializeField] private UnityEvent _onFinished = new UnityEvent();

        private float _time;
        private bool _isBouncing;
        private Vector3 _start;
        private int _iterations;
        private float _nextDuration;
        private int _nextDirection;

        private void Start()
        {
            _nextDirection = (int)_direction;
            if(_direction == Direction.Random ) _nextDirection = Random.Range(0, 2);

            if (_startImmediately) StartBouncing();
        }

        public void StartBouncing()
        {
            if (_duration.a <= 0 || _duration.b <=0 )
            {
                Debug.LogWarning($"Not running, duration is not valid: {_duration}");
                return;
            }

            _time = 0;
            _isBouncing = true;
            _start = transform.localPosition;
            _iterations = _count;
            _nextDuration = _duration.random;
        }

        private void Update()
        {
            if (!_isBouncing) return;

            _time += GameState.deltaTime;
            if (_time >= _nextDuration)
            {
                _time = _nextDuration;
                _isBouncing = false;
            }

            float pos = _time / _nextDuration;
            if(_nextDirection == (int)Direction.Backward) pos = 1.0f - pos;

            pos = Mathf.Sin(Mathf.PI * 2 * pos);

            transform.localPosition = _start + _extents * pos;

            if (!_isBouncing)
            {
                transform.localPosition = _start;
                _onIterationFinished.Invoke();

                float d = _nextDuration; // maintain the same duration for multiple intervals
                if( _iterations > 0 )
                {
                    _iterations--;
                    if( _iterations > 0 ) StartBouncing();
                }
                else if( _iterations < 0 )
                {
                    StartBouncing();
                }
                _nextDuration = d;

                if( !_isBouncing ) _onFinished.Invoke();
            }
        }

    }

}