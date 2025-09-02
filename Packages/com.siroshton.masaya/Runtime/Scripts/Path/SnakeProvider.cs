using Siroshton.Masaya.Math;
using UnityEngine;

namespace Siroshton.Masaya.Path
{
    public class SnakeProvider : TargetProvider
    {
        [Tooltip("The target point will sweep between +/- this angle.")]
        [SerializeField] private float _angleRange = 45;
        [Tooltip("Number of waves per second.")]
        [SerializeField] private float _speed = 2;
        [Tooltip("Distance from current position to project next position.")]
        [SerializeField] private float _step = 1;

        private float _time;

        public override bool GetTarget(Transform current, out Vector3 target, float timeSinceLastCall)
        {
            _time += timeSinceLastCall;

            float angle = Mathf.Sin(_time * _speed) * _angleRange;
            Vector3 forward = Quaternion.AngleAxis(angle, Vector3.up) * current.forward;
            target = current.position + forward * _step;

            return true;
        }

        public override string ToString()
        {
            return $"SnakeProvider(range: +/- {_angleRange}, speed: {_speed}, step: {_step})";
        }

    }
}
