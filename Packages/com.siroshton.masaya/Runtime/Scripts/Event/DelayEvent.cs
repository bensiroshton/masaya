
using Siroshton.Masaya.Core;
using Siroshton.Masaya.Math;
using System.Collections;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;

namespace Siroshton.Masaya.Event
{
    public class DelayEvent : MonoBehaviour
    {
        [Tooltip("A random value between A nd B will be used.")]
        [SerializeField] private IntervalFloat _delay;
        [SerializeField] private bool _startImmediately = false;
        [Tooltip("Set to -1 to repeat indefinitely.")]
        [SerializeField] private int _repeat;
        [SerializeField] private UnityEvent _onFinished;

        private bool _isRunning;
        private float _time;
        private float _nextDelay;
        private int _count;

        private void Start()
        {
            if( _startImmediately ) StartDelay();
        }

        public void StartDelay()
        {
            if(_isRunning) return;

            _nextDelay = _delay.random;
            _time = 0;
            _count = _repeat;
            _isRunning = true;
        }

        private void Update()
        {
            if( !_isRunning ) return;

            _time += GameState.deltaTime;
            if( _time >= _nextDelay )
            {
                if (_onFinished != null) _onFinished.Invoke();
                if (_count > 0) _count--;

                if( _count == 0 )
                {
                    _isRunning = false;
                }
                else
                {
                    _nextDelay = _delay.random;
                    _time = 0;
                }
            }
        }

    }
}