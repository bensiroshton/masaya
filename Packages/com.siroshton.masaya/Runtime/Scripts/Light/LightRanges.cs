using Siroshton.Masaya.Math;
using UnityEngine;

namespace Siroshton.Masaya.Light
{

    public class LightRanges : MonoBehaviour
    {
        [SerializeField] private UnityEngine.Light _light;
        [SerializeField] private IntervalFloat _intensityRange = new IntervalFloat(0, 1);
        [SerializeField] private IntervalFloat _rangeRange = new IntervalFloat(1, 1);

        private void Awake()
        {
            if( _light == null ) _light = GetComponent<UnityEngine.Light>();
        }

        public void SetIntensity(float normalizedRange)
        {
            _light.intensity = _intensityRange.Lerp(normalizedRange);
        }

        public void SetRange(float normalizedRange)
        {
            _light.range = _rangeRange.Lerp(normalizedRange);
        }

    }

}