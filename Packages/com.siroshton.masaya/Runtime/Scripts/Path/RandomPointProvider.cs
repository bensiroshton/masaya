using Siroshton.Masaya.Math;
using UnityEngine;

namespace Siroshton.Masaya.Path
{
    public class RandomPointProvider : TargetProvider
    {
        [SerializeField] private IntervalFloat _radius = new IntervalFloat(1, 5);

        public override bool GetTarget(Transform current, out Vector3 target, float timeSinceLastCall)
        {
            Vector2 point = Random.insideUnitCircle * _radius.random;
            target = current.position + new Vector3(point.x, 0, point.y);

            return true;
        }

        public override string ToString()
        {
            return $"RandomPointProvider(r: {_radius})";
        }

    }
}
