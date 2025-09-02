using Siroshton.Masaya.Core;
using Siroshton.Masaya.Math;
using UnityEngine;
using UnityEngine.Events;

namespace Siroshton.Masaya.Motion
{
    public class ScaleTo : MonoBehaviour
    {
        [SerializeField] private bool _startImmediately = false;
        [SerializeField] private Vector3 _targetScale;
        [SerializeField] private float _duration = 1;
        [Tooltip("Add a random value between -durationRandomness and +durationRandomness to duration.")]
        [SerializeField] private float _durationRandomness = 0;
        [SerializeField] private bool _easeInOut = false;
        [SerializeField] private bool _enableDisableOnStartStopCalls = true;
        [SerializeField] private bool _useGameStateTime = true;
        [SerializeField] private UnityEvent _onFinished = new UnityEvent();

        public bool startImmediately { get => _startImmediately; set => _startImmediately = value; }
        public Vector3 targetScale { get => _targetScale; set => _targetScale = value; }
        public float duration { get => _duration; set => _duration = value; }
        public float durationRandomness { get => _durationRandomness; set => _durationRandomness = value; }
        public bool easeInOut { get => _easeInOut; set => _easeInOut = value; }
        public UnityEvent onFinished { get => _onFinished; set => _onFinished = value; }
        
        private float _time;
        private bool _isScaling;
        private Vector3 _start;
        private float _thisDuration;

        public void SetUniformLocalScale(float scale)
        {
            transform.localScale = new Vector3(scale, scale, scale);
        }

        private void Start()
        {
            if( _startImmediately ) StartScaling();
        }

        public void StartScaling()
        {
            _thisDuration = _duration + Random.Range(-_durationRandomness, _durationRandomness);
            if (_thisDuration <= 0)
            {
                transform.localScale = _targetScale;
                return;
            }

            _time = 0;
            _isScaling = true;
            _start = transform.localScale;
            if( _enableDisableOnStartStopCalls ) enabled = true;
        }

        public void StopScaling()
        {
            _isScaling = false;
            if (_enableDisableOnStartStopCalls) enabled = false;
            _onFinished.Invoke();
        }

        public void ScaleImmediate()
        {
            transform.localScale = _targetScale;
        }

        private void Update()
        {
            if (!_isScaling) return;

            if(_useGameStateTime ) _time += GameState.deltaTime;
            else _time += Time.deltaTime;

            if( _time >= _thisDuration)
            {
                _time = _thisDuration;
                _isScaling = false;
            }

            float pos = _time / _thisDuration;
            if (_easeInOut) pos = MathUtil.SmoothNormalizedTime(pos);

            transform.localScale = Vector3.Lerp(_start, _targetScale, pos);

            if (!_isScaling)
            {
                StopScaling();
            }
        }

    }

}