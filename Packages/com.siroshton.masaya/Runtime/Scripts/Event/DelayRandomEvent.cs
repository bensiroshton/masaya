
using Siroshton.Masaya.Core;
using Siroshton.Masaya.Math;
using Siroshton.Masaya.Util;
using System.Collections;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;
using static Siroshton.Masaya.Core.Types;

namespace Siroshton.Masaya.Event
{
    public class DelayRandomEvent : MonoBehaviour
    {
        [Tooltip("A random value between A nd B will be used.")]
        [SerializeField] private IntervalFloat _delay;
        [SerializeField] private bool _startImmediately = false;
        [Tooltip("Set to -1 to repeat indefinitely.")]
        [SerializeField] private int _repeat;
        [SerializeField] private NextSelectionType _eventSelection = NextSelectionType.Random;
        [SerializeField] private UnityEvent[] _onInterval;

        private bool _isRunning;
        private float _time;
        private float _nextDelay;
        private int _count;
        private int _lastSelected;

        private void Start()
        {
            if (_startImmediately) StartDelay();
        }

        public void StartDelay()
        {
            if (_isRunning) return;

            _nextDelay = _delay.random;
            _time = 0;
            _count = _repeat;
            _isRunning = true;
        }

        private void TriggerEvent()
        {            
            if (_onInterval == null || _onInterval.Length == 0) return;

            _lastSelected = SelectionUtil.SelectNext(_eventSelection, _lastSelected, _onInterval.Length);
            _onInterval[_lastSelected]?.Invoke();
        }

        private void Update()
        {
            if (!_isRunning) return;

            _time += GameState.deltaTime;
            if (_time >= _nextDelay)
            {
                TriggerEvent();
                if (_count > 0) _count--;

                if (_count == 0)
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