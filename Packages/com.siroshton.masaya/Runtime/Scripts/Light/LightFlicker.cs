using Siroshton.Masaya.Core;
using Siroshton.Masaya.Math;
using UnityEngine;

namespace Siroshton.Masaya.Light
{

    [RequireComponent(typeof(UnityEngine.Light))]
    public class LightFlicker : MonoBehaviour
    {
        [SerializeField] private bool _enableIntensity = true;
        [SerializeField] private IntervalFloat _intensityRange = new IntervalFloat(1, 2);
        [SerializeField] private bool _enableRange = true;
        [SerializeField] private IntervalFloat _rangeRange = new IntervalFloat(1, 1);
        [SerializeField] private float _frequency = 0.1f;

        private UnityEngine.Light _light;
        
        private float _lastRange;
        private float _targetRange;
        private float _lastIntensity;
        private float _targetIntensity;
        private float _timeSinceChange;

        private void Start()
        {
            _light = GetComponent<UnityEngine.Light>();
            SetNewTarget();
        }

        private void SetNewTarget()
        {
            if( _frequency <= 0 ) _frequency = 0.0001f;
            _lastRange = _light.range;
            _targetRange = _rangeRange.random;
            _lastIntensity = _light.intensity;
            _targetIntensity = _intensityRange.random;
            _timeSinceChange = 0;
        }

        private void Update()
        {
            _timeSinceChange += GameState.deltaTime;

            if (_enableIntensity) _light.intensity = Mathf.Lerp(_lastIntensity, _targetIntensity, _timeSinceChange / _frequency);
            if (_enableRange) _light.range = Mathf.Lerp(_lastRange, _targetRange, _timeSinceChange / _frequency);
            if (_timeSinceChange >= _frequency) SetNewTarget();
        }
    }

}