using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Siroshton.Masaya.Effect
{
    [RequireComponent(typeof(ParticleSystem))]
    public class ParticleEvents : MonoBehaviour
    {
        [SerializeField] private UnityEvent<Vector3> _onParticleTrigger;

        private ParticleSystem _ps;
        private List<ParticleSystem.Particle> _enteredParticles = new List<ParticleSystem.Particle>();

        private void Awake()
        {
            _ps = GetComponent<ParticleSystem>();
        }

        private void OnParticleTrigger()
        {
            if( _onParticleTrigger == null ) return;

            int count = _ps.GetTriggerParticles(ParticleSystemTriggerEventType.Enter, _enteredParticles);
            for(int i=0;i<count;i++)
            {
                _onParticleTrigger.Invoke(_enteredParticles[i].position);
            }
        }

    }

}