
using System;
using UnityEngine;

namespace Siroshton.Masaya.Component
{
    public class ParticleSystemController : MonoBehaviour
    {
        [SerializeField] private ParticleSystem _particles;

        private void Awake()
        {
            if( _particles == null )  _particles = GetComponent<ParticleSystem>();
            if( _particles == null ) Debug.LogWarning("ParticleSystem not found.");
        }
        
        [Obsolete("We should just use Play on the ParticleSystem directly.")]
        public void Play()
        {
            if (_particles != null)
            {
                _particles.Play();
            }
        }

        [Obsolete("We should just use Stop on the ParticleSystem directly.")]
        public void Stop()
        {
            if( _particles != null )
            {
                _particles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }
        }

        public void StopAndClear()
        {
            if (_particles != null)
            {
                _particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }

        public void SetEmissionsEnabled(bool enable)
        {
            if (_particles != null)
            {
                ParticleSystem.EmissionModule emissions = _particles.emission;
                emissions.enabled = enable;
            }
        }

    }
}