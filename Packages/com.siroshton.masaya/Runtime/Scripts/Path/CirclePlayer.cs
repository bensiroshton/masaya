using Siroshton.Masaya.Math;
using UnityEngine;

namespace Siroshton.Masaya.Path
{
    public class CirclePlayerProvider : TargetProvider
    {
        public enum Direction
        {
            Clockwise,
            CounterClockwise
        }

        [SerializeField] private Direction _direction;
        [SerializeField] private IntervalFloat _radius = new IntervalFloat(2, 2);
        [SerializeField, Range(0, 5)] private float _stepSize = 1.0f;

        public override bool GetTarget(Transform current, out Vector3 target, float timeSinceLastCall)
        {
            Vector3 center = Player.Player.instance.transform.position;
            float radius = _radius.random;
            if( radius <= 0 )
            {
                target = center;
                return true;
            }

            Vector3 dial = (current.position - center).normalized * radius;
            float angle = _stepSize / radius; // using Arc Length Formula, Angle = Arc Length / Radius
            if( _direction == Direction.CounterClockwise ) angle *= -1.0f;

            Quaternion rotation = Quaternion.Euler(new Vector3(0, angle * Mathf.Rad2Deg, 0));
            dial = rotation * dial;

            target = center + dial;

            return true;
        }

        public override string ToString()
        {
            return $"CirclePlayerProvider(dir: {_direction}, r: {_radius}, step: {_stepSize})";
        }

    }
}
