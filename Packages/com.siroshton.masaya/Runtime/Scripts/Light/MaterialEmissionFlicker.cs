using Siroshton.Masaya.Core;
using Siroshton.Masaya.Math;
using UnityEngine;

namespace Siroshton.Masaya.Light
{
    [RequireComponent(typeof(MeshRenderer))]
    public class MaterialEmissionFlicker : MonoBehaviour
    {
        [SerializeField] private int _materialIndex = 0;
        [SerializeField] private IntervalFloat _intensityRange = new IntervalFloat(1, 2);
        [SerializeField] private float _frequency = 0.1f;

        private float _lastIntensity;
        private float _targetIntensity;
        private float _timeSinceChange;

        private MeshRenderer _meshRenderer;
        private Material _material;
        private int _emissionId;
        private Color _emissionColor;

        private void Start()
        {
            _meshRenderer = GetComponent<MeshRenderer>();
            _material = _meshRenderer.materials[_materialIndex];
            if( !_material.IsKeywordEnabled("_EMISSION") )
            {
                Debug.LogWarning($"Emission is not available for this material {_material}");
            }
            _emissionId = Shader.PropertyToID("_EmissionColor");
            _emissionColor = _material.GetColor(_emissionId);
            _targetIntensity = _intensityRange.random;

            SetNewTarget();
        }

        private void SetNewTarget()
        {
            if (_frequency <= 0) _frequency = 0.0001f;
            _lastIntensity = _targetIntensity;
            _targetIntensity = _intensityRange.random;
            _timeSinceChange = 0;
        }

        private void Update()
        {
            _timeSinceChange += GameState.deltaTime;
            _material.SetColor(_emissionId, _emissionColor * Mathf.Lerp(_lastIntensity, _targetIntensity, _timeSinceChange / _frequency));

            if (_timeSinceChange >= _frequency) SetNewTarget();
        }
    }

}