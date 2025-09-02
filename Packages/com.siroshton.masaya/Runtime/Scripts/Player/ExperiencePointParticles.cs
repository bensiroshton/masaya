using Siroshton.Masaya.Core;
using Siroshton.Masaya.Math;
using UnityEngine;

namespace Siroshton.Masaya.Player
{
    [RequireComponent(typeof(ParticleSystem))]
    public class ExperiencePointParticles : MonoBehaviour
    {
        [SerializeField] private float _pointsPerParticle = 1.0f;
        [SerializeField, Range(0, 50)] private float _power1 = 10.0f;
        [SerializeField, Range(0, 50)] private float _maxVelocity = 3.0f;

        private ParticleSystem _system;
        private ParticleSystem.Particle[] _particles;
        private Player _player;
        private float _collectedSqrDistance;
        private float _maxSqrVelocity;

        public float pointsPerParticle { get => _pointsPerParticle; set => _pointsPerParticle = value; }

        private void Start()
        {
            _system = GetComponent<ParticleSystem>();
            _player = Player.instance;
            CharacterController charCon = _player.GetComponent<CharacterController>();
            _collectedSqrDistance = charCon.radius * charCon.radius;

            _particles = new ParticleSystem.Particle[_system.main.maxParticles];
        }

        public void SpawnPoints(Vector3 position, float radius, int experiencePoints)
        {
            _maxSqrVelocity = _maxVelocity * _maxVelocity; // do this here so it gets updated if we change _maxVelocity through the editor

            int count = (int)((float)experiencePoints / _pointsPerParticle);

            ParticleSystem.EmitParams p = new ParticleSystem.EmitParams();

            for (int i=0;i<count;i++)
            {
                p.position = MathUtil.GetRandomPointInSphere(position, radius);
                p.velocity = Vector3.zero;

                _system.Emit(p, 1);
            }
        }

        private void LateUpdate()
        {
            int count = _system.GetParticles(_particles);
            int collected = 0;
            float deltaTime = GameState.deltaTime;
            Vector3 dir;
            float sqrMag;

            for (int i = 0; i < count; i++)
            {
                dir = transform.position - _particles[i].position;
                sqrMag = dir.sqrMagnitude;

                if( sqrMag <= _collectedSqrDistance )
                {
                    _particles[i].remainingLifetime = 0;
                    ++collected;
                }
                else
                {
                    Vector3 v1 = _particles[i].velocity + dir * (_power1 / sqrMag * deltaTime);
                    if (v1.sqrMagnitude > _maxSqrVelocity) v1 = Vector3.Project(v1, dir);
                    _particles[i].velocity = v1;
                }
            }
            
            _system.SetParticles(_particles, count);

            if( collected > 0 )
            {
                _player.characterSheet.GiveExperience((int)((float)collected * _pointsPerParticle));
            }
        }


    }

}