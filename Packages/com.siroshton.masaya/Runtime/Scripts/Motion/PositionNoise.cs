using Siroshton.Masaya.Core;
using UnityEditor;
using UnityEngine;

namespace Siroshton.Masaya.Motion
{

    public class PositionNoise : MonoBehaviour
    {
        [SerializeField] private Vector3 _extents;
        [SerializeField] private float _interval = 0.25f;
        [SerializeField] private bool _smoothStep;

        private float _time;
        private Vector3 _position;
        private Vector3 _lastOffset;
        private Vector3 _nextOffset;

        protected void Start()
        {
            _position = transform.localPosition;
        }

        protected void Update()
        {            
            _time += GameState.deltaTime;
            if( _time >= _interval )
            {
                _lastOffset = _nextOffset;
                _nextOffset = new Vector3(Random.Range(-_extents.x, _extents.x), Random.Range(-_extents.y, _extents.y), Random.Range(-_extents.z, _extents.z));

                if( !_smoothStep || _interval <= 0) transform.localPosition = _position + _nextOffset;
                _time = 0;
            }

            if( _smoothStep && _interval > 0)
            {
                transform.localPosition = _position + Vector3.Lerp(_lastOffset, _nextOffset, _time / _interval);
            }
        }

    }
}