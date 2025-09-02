using Siroshton.Masaya.Math;
using System;
using UnityEngine;

namespace Siroshton.Masaya.Path
{
    public class OscillateProvider : TargetProvider
    {
        [SerializeField] private TargetProvider _providerA;
        [SerializeField] private TargetProvider _providerB;
        [SerializeField] private float _transitionTime = 4;
        [SerializeField] private bool _easeInOut = true;

        private float _time;
        private bool _a2b = true;

        public override bool GetTarget(Transform current, out Vector3 target, float timeSinceLastCall)
        {
            target = current.position;

            if( _providerA == null || _providerB == null ) return false;

            Vector3 a;
            if( !_providerA.GetTarget(current, out a, timeSinceLastCall) ) return false;

            Vector3 b;
            if (!_providerB.GetTarget(current, out b, timeSinceLastCall)) return false;

            _time += timeSinceLastCall;
            if( _time > _transitionTime )
            {
                _time = 0;
                _a2b = !_a2b;
            }

            float pos = _time / _transitionTime;
            if(_easeInOut) pos = MathUtil.SmoothNormalizedTime(pos);

            if( _a2b ) target = Vector3.Lerp(a, b, pos);
            else target = Vector3.Lerp(b, a, pos);

            return true;
        }

        public override string ToString()
        {
            return $"OscillateProvider({(_providerA ? _providerA.name : null)}, {(_providerB ? _providerB.name : null)})";
        }

    }
}
