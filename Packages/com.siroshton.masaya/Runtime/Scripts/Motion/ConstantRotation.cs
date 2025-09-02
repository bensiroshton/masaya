using Siroshton.Masaya.Core;
using UnityEngine;

namespace Siroshton.Masaya.Motion
{
    public class ConstantRotation : MonoBehaviour
    {
        [SerializeField] private float _xDegreesPerSecond;
        [SerializeField] private float _yDegreesPerSecond;
        [SerializeField] private float _zDegreesPerSecond;
        [SerializeField] private bool _useGameStateTime = true;

        void Update()
        {
            float time = _useGameStateTime ? GameState.deltaTime : Time.deltaTime;

            Vector3 rotation = transform.localRotation.eulerAngles;
            rotation.x += _xDegreesPerSecond * time;
            rotation.y += _yDegreesPerSecond * time;
            rotation.z += _zDegreesPerSecond * time;
            transform.localRotation = Quaternion.Euler(rotation);
        }
    }
}