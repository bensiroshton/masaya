using System;
using UnityEngine;

namespace Siroshton.Masaya.Path
{
    public class BlendProvider : TargetProvider
    {
        [Serializable]
        public struct Provider
        {
            public TargetProvider provider;
            [Range(0, 1)] public float blendWeight;
        }

        [SerializeField] private Provider[] _providers;

        public override bool GetTarget(Transform current, out Vector3 target, float timeSinceLastCall)
        {
            if ( _providers == null || _providers.Length == 0 )
            {
                target = current.position;
                return false;
            }

            target = Vector3.zero;
            Vector3 newTarget;
            float totalWeight = 0;

            for (int i=0;i<_providers.Length;i++)
            {
                if( _providers[i].provider.GetTarget(current, out newTarget, timeSinceLastCall) )
                {
                    target += newTarget * _providers[i].blendWeight;
                    totalWeight += _providers[i].blendWeight;
                }
            }

            if( totalWeight == 0) return false;

            target /= totalWeight;

            return true;
        }

        public override string ToString()
        {
            return $"BlendProvider";
        }

    }
}
