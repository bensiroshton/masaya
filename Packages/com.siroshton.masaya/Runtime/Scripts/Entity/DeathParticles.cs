

using UnityEngine;

namespace Siroshton.Masaya.Entity
{
    public class DeathParticles : MonoBehaviour, IDeathHandler
    {
        [SerializeField] private string _handlerName;
        [SerializeField] private ParticleSystem _particles;
        [SerializeField] private int _emissionCount = 1;

        public string handlerName { get => _handlerName; set => _handlerName = value; }

        public void HandleDeath(Entity o)
        {
            if( _particles == null ) return;

            var emit = new ParticleSystem.EmitParams();
            emit.position = o.transform.position;
            _particles.Emit(emit, _emissionCount);
        }
    }

}