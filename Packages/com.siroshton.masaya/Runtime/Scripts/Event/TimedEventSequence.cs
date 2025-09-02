using Siroshton.Masaya.Core;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace Siroshton.Masaya.Event
{

    public class TimedEventSequence : MonoBehaviour
    {
        [Serializable]
        public class Step
        {
            [Tooltip("Time to wait since the last event before firing the events.")]
            public float delay;
            public UnityEvent events;
        }

        [SerializeField] private string _name;
        [SerializeField] private bool _startImmediately = true;
        [SerializeField] private bool _disableWhenFinished = false;

        [Tooltip("Sequence steps.")]
        [SerializeField] private Step[] _steps;

        private int _step;
        private float _time;
        private bool _isPlaying;
        private int _timesPlayed;

        public bool isPlaying => _isPlaying;

        private void Start()
        {
            //Debug.Log($"{name} enabled: {enabled}, start immediately: {_startImmediately}");
            if( _startImmediately ) Restart();
        }

        public void Restart()
        {
            if (_steps == null || _steps.Length == 0)
            {
                _isPlaying = false;
                enabled = !_disableWhenFinished;
                return;
            }

            _step = 0;
            _time = 0;
            _isPlaying = true;
            _timesPlayed++;

            // Call Update, this ensures the steps run immediately if the first delay is zero otherwise we won't get them until the next
            // frame which can cause visual artificats (ie., enabling an object but settings its scale to zero in the sequence for example
            // would flash the object next frame until its scale gets set).
            if ( enabled ) Update();
        }

        public void StartIfNotPlaying()
        {
            if( !_isPlaying ) Restart();
        }

        public void StartIfNeverPlayed()
        {
            if (!_isPlaying && _timesPlayed == 0) Restart();
        }

        private void Update()
        {
            if( !_isPlaying ) return;

            _time += GameState.deltaTime;

            if( _time >= _steps[_step].delay )
            {
                if( _steps[_step].events != null ) _steps[_step].events.Invoke();
                _step++;
                if( _step >= _steps.Length )
                {
                    _isPlaying = false;
                    enabled = !_disableWhenFinished;
                }
                _time = 0;
            }
        }
    }

}