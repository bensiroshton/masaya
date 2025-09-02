using Siroshton.Masaya.Core;
using UnityEngine;

namespace Siroshton.Masaya.Effect
{
    [RequireComponent(typeof(ReflectionProbe))]
    public class ReflectionProbeBakeTrigger : MonoBehaviour
    {
        [Tooltip("Bake once every X seconds.")]
        [SerializeField] private float _bakeFrequency = 0.25f;

        private ReflectionProbe _probe;

        private float _timeSinceBake;

        private void Awake()
        {
            _probe = GetComponent<ReflectionProbe>();
        }

        private void Update()
        {
            _timeSinceBake += GameState.deltaTime;
            if( _timeSinceBake >= _bakeFrequency )
            {
                _probe.RenderProbe();
                _timeSinceBake = 0;
            }
        }

    }

}