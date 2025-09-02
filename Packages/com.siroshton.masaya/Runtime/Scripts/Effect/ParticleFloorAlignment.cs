using UnityEngine;

namespace Siroshton.Masaya.Effect
{
    [RequireComponent(typeof(ParticleSystem))]
    public class ParticleFloorAlignment : MonoBehaviour
    {
        private ParticleSystem _system;
        private ParticleSystem.MainModule _main;

        private void Awake()
        {
            _system = GetComponent<ParticleSystem>();
            _main = _system.main;
        }

        private void LateUpdate()
        {
            _main.startRotationZ = (transform.rotation.eulerAngles.y + 90) * Mathf.Deg2Rad;
        }

    }

}