using System.Collections.Generic;
using UnityEngine;

namespace Siroshton.Masaya.UI
{
    [RequireComponent(typeof(ParticleSystem))]
    public class DamageTextParticles : MonoBehaviour
    {
        [SerializeField] private float _textSize = 1;
        [SerializeField] private Color _normalColor = Color.white;
        [SerializeField] private float _criticalTextSize = 1;
        [SerializeField] private Color _criticalColor = Color.yellow;

        private ParticleSystem _system;
        private ParticleSystem.Particle[] _particles;
        private ParticleSystem.EmitParams _emitParams = new ParticleSystem.EmitParams();
        private List<Vector4> _customData = new List<Vector4>();

        private void Start()
        {
            _system = GetComponent<ParticleSystem>();
            _particles = new ParticleSystem.Particle[_system.main.maxParticles];

            ParticleSystem.EmissionModule emission = _system.emission;
            emission.enabled = false;

            ParticleSystem.ShapeModule shape = _system.shape;
            shape.enabled = false;
        }

#if UNITY_EDITOR
        public void Editor_SpawnTestDamage(int damage)
        {
            if( _system == null ) _system = GetComponent<ParticleSystem>();
            if( _particles == null ) _particles = new ParticleSystem.Particle[_system.main.maxParticles];
            if( _customData == null ) _customData = new List<Vector4>();
            _emitParams = new ParticleSystem.EmitParams();

            SpawnDamage(transform.position, damage, false);
        }
#endif

        public void SpawnDamage(Vector3 position, int damage, bool isCritical)
        {
            string text = damage.ToString();
            int count = text.Length;

            float width = count * _textSize;
            Vector3 right = Camera.main.transform.right;
            position -= right * width * 0.5f;

            float size;
            Color color;

            if( isCritical )
            {
                size = _criticalTextSize;
                color = _criticalColor;
            }
            else
            {
                size = _textSize;
                color = _normalColor;
            }

            for (int i = 0; i < count; i++)
            {
                _emitParams.velocity = new Vector3(0, _system.main.startSpeed.Evaluate(0), 0);
                _emitParams.position = position;
                _emitParams.startSize3D = new Vector3(size, size, 100 + text[i] - '0');
                _emitParams.startColor  = color;
                _system.Emit(_emitParams, 1);

                position += right * size;
            }

            _system.GetCustomParticleData(_customData, ParticleSystemCustomData.Custom1);

            count = _system.GetParticles(_particles);
            for (int i = 0; i < count; i++)
            {
                if( _particles[i].startSize3D.z >= 100 )
                {
                    _customData[i] = new Vector4(_particles[i].startSize3D.z - 100, 0, 0, 0);
                    _particles[i].startSize3D = new Vector3(size, size, size);
                }
            }

            _system.SetCustomParticleData(_customData, ParticleSystemCustomData.Custom1);
        }

    }

}