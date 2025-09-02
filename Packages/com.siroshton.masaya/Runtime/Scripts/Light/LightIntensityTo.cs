using Siroshton.Masaya.Core;
using Siroshton.Masaya.Math;
using UnityEngine;
using UnityEngine.Events;

namespace Siroshton.Masaya.Light
{
    public class LightIntensityTo : MonoBehaviour
    {
        [SerializeField] private bool _startImmediately = false;
        [SerializeField] private UnityEngine.Light _light;
        [SerializeField] private float _targetIntensity;
        [SerializeField] private float _duration = 1;
        [SerializeField] private bool _smoothTime = false;
        [SerializeField] private UnityEvent _onFinished;

        public bool startImmediately { get => _startImmediately; set => _startImmediately = value; }
        public new UnityEngine.Light light { get => _light; set => _light = value; }
        public float targetIntensity { get => _targetIntensity; set => _targetIntensity = value; }
        public float duration { get => _duration; set => _duration = value; }
        public bool smoothTime { get => _smoothTime; set => _smoothTime = value; }
        public float intensity { get => _light.intensity; set => _light.intensity = value; }
        public UnityEvent onFinished { get => _onFinished; set => _onFinished = value; }

        private float _time;
        private bool _isChanging;
        private float _startValue;
        private float _toIntensity;

        private void Awake()
        {
            if (_light == null) _light = GetComponent<UnityEngine.Light>();
        }

        private void Start()
        {
            if (_startImmediately) StartChanging();
        }

        public void StartChanging()
        {
            StartChanging(_targetIntensity);
        }

        public void StopChanging()
        {
            _isChanging = false;
        }

        public void StartChanging(float toIntensity)
        {
            if (_duration <= 0 || toIntensity == _light.intensity)
            {
                _light.intensity = toIntensity;
                _isChanging = false;
                _onFinished?.Invoke();
                return;
            }

            _time = 0;
            _isChanging = true;
            _startValue = _light.intensity;
            _toIntensity = toIntensity;
        }

        private void Update()
        {
            if (!_isChanging) return;

            _time += GameState.deltaTime;
            if (_time >= _duration)
            {
                _time = _duration;
                _isChanging = false;
            }

            float pos = _time / _duration;

            if (_smoothTime) pos = MathUtil.SmoothNormalizedTime(pos);
            _light.intensity = Mathf.Lerp(_startValue, _toIntensity, pos);

            if (!_isChanging) _onFinished?.Invoke();
        }

    }

}