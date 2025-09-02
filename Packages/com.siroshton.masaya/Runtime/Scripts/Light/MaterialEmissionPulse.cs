using Siroshton.Masaya.Core;
using Siroshton.Masaya.Math;
using UnityEngine;

namespace Siroshton.Masaya.Light
{
    [RequireComponent(typeof(MeshRenderer))]
    public class MaterialEmissionPulse : MonoBehaviour
    {
        [SerializeField] private int _materialIndex = 0;
        [SerializeField] private IntervalFloat _intensityRange = new IntervalFloat(1, 2);
        [SerializeField] private float _cycleDuration = 1.0f;

        private float _time;
        private MeshRenderer _meshRenderer;
        private Material _material;
        private int _emissionId;
        private Color _emissionColor;
        private float _emission;

        private void Start()
        {
            _meshRenderer = GetComponent<MeshRenderer>();
            _material = _meshRenderer.materials[_materialIndex];
            if (!_material.IsKeywordEnabled("_EMISSION"))
            {
                Debug.LogWarning($"Emission is not available for this material {_material}");
            }
            _emissionId = Shader.PropertyToID("_EmissionColor");
            _emissionColor = _material.GetColor(_emissionId);
            _emission = _intensityRange.a;
        }

        public void StopPulsing()
        {
            StopPulsing(_emission);
        }

        public void StopPulsing(float emission)
        {
            enabled = false;
            this.emission = emission;
        }

        public float emission
        {
            get => _emission;
            set
            {
                _emission = value;
                _material.SetColor(_emissionId, _emissionColor * _emission);
            }
        }

        private void Update()
        {
            float pos;
            
            _time += GameState.deltaTime;
            if( _time > _cycleDuration )
            {
                _time = 0f;
                pos = 1.0f;
            }
            else
            {
                pos = _time / _cycleDuration;
            }

            emission = Mathf.Lerp(_intensityRange.a, _intensityRange.b, Mathf.Sin(pos * Mathf.PI));
        }
    }

}