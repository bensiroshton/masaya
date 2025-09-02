using Siroshton.Masaya.Core;
using UnityEngine;
using UnityEngine.Events;

namespace Siroshton.Masaya.Effect
{
    [RequireComponent(typeof(UnityEngine.Light))]
    public class GrowLight : MonoBehaviour
    {
        [SerializeField] private bool _getSizeAndIntensityOnStart = true;
        [SerializeField] private float _maxIntensity = 1;
        [SerializeField] private Vector3 _maxSize = Vector3.one;
        [SerializeField] private float _duration = 1;
        [SerializeField] private UnityEvent _onFinished;

        private enum Action
        {
            Waiting,
            Growing,
            Shrinking
        }

        private Action _action;
        private UnityEngine.Light _light;
        private float _time;
        private float _thisDuration;
        private float _startIntensity;
        private Vector3 _startSize;

        private void Start()
        {
            _light = GetComponent<UnityEngine.Light>();

            if( _getSizeAndIntensityOnStart )
            {
                _maxIntensity = _light.intensity;
                _maxSize = transform.localScale;
            }
        }
        
        public void StartGrowing()
        {
            StartGrowing(_duration);
        }

        public void StartGrowing(float duration)
        {
            _thisDuration = duration;

            if(_thisDuration <= 0)
            {
                InstantGrowth();
            }
            else
            {
                _action = Action.Growing;
                _time = 0;
                _startIntensity = _light.intensity;
                _startSize = transform.localScale;
            }
        }

        public void InstantGrowth()
        {
            _light.intensity = _maxIntensity;
            transform.localScale = _maxSize;
            _action = Action.Waiting;
        }

        public void StartShrinking()
        {
            StartShrinking(_duration);
        }

        public void StartShrinking(float duration)
        {
            _thisDuration = duration;

            if (_thisDuration <= 0)
            {
                InstantShrink();
            }
            else
            {
                _action = Action.Shrinking;
                _time = 0;
                _startIntensity = _light.intensity;
                _startSize = transform.localScale;
            }
        }

        public void InstantShrink()
        {
            _light.intensity = 0;
            transform.localScale = Vector3.zero;
            _action = Action.Waiting;
        }

        private void Update()
        {
            if( _action == Action.Waiting ) return;

            _time += GameState.deltaTime;
            float pos = _time / _thisDuration;
            bool finish = false;

            if( pos > 1 )
            {
                pos = 1;
                finish = true;
            }

            if( _action == Action.Growing )
            {
                _light.intensity = Mathf.Lerp(_startIntensity, _maxIntensity, pos);
                transform.localScale = Vector3.Lerp(_startSize, _maxSize, pos);
            }
            else
            {
                _light.intensity = Mathf.Lerp(_startIntensity, 0, pos);
                transform.localScale = Vector3.Lerp(_startSize, Vector3.zero, pos);
            }

            if ( finish )
            {
                _action = Action.Waiting;
                _onFinished?.Invoke();
            }
        }

    }

}