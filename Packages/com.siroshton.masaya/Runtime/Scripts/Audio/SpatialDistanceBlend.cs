using Siroshton.Masaya.Math;
using UnityEngine;

namespace Siroshton.Masaya.Audio
{
    public class SpatialDistanceBlend : MonoBehaviour
    {
        [SerializeField] private AudioSource _audioSource;
        [SerializeField] private Transform _testPoint;
        [SerializeField] private bool _usePlayerTransform = true;
        [SerializeField] private IntervalFloat _innerOuterRadius = new IntervalFloat(0, 1);
        [SerializeField] private IntervalFloat _innerOuterRadiusValue = new IntervalFloat(0, 1);

        private void Awake()
        {
            if( _audioSource == null ) _audioSource = GetComponent<AudioSource>();
            if( _usePlayerTransform ) _testPoint = Player.Player.instance.transform;
        }

        private void Update()
        {
            float pos = MathUtil.GetNormalizedPosition(transform.position, _innerOuterRadius.a, _innerOuterRadius.b, _testPoint.position);
            _audioSource.spatialBlend = _innerOuterRadiusValue.Lerp(pos);
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if( _innerOuterRadius.a > 0 )
            {
                UnityEditor.Handles.color = new Color(0, 1, 1, 0.1f);
                UnityEditor.Handles.DrawSolidDisc(transform.position, Vector3.up, _innerOuterRadius.a);
            }

            if (_innerOuterRadius.b > 0)
            {
                UnityEditor.Handles.color = Color.cyan;
                UnityEditor.Handles.DrawWireDisc(transform.position, Vector3.up, _innerOuterRadius.b);
            }
        }
#endif
    }

}