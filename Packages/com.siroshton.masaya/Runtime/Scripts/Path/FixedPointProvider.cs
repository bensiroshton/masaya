using Siroshton.Masaya.Math;
using UnityEngine;

namespace Siroshton.Masaya.Path
{
    public class FixedPointProvider : TargetProvider
    {
        public enum PointType
        {
            Manual,
            PositionOnStart,
        }

        [SerializeField] private PointType _type;
        [SerializeField] private Vector3 _point;
        [SerializeField] private IntervalFloat _range = new IntervalFloat(0, 0);

        public PointType type { get => _type; set => _type = value; }
        public Vector3 point { get => _point; set => _point = value; }

        private void Start()
        {
            if( _type == PointType.PositionOnStart ) _point = transform.position;
        }

        public override bool GetTarget(Transform current, out Vector3 target, float timeSinceLastCall)
        {
            Vector2 r = Random.insideUnitCircle;
            target = _point + new Vector3(r.x, 0, r.y) * _range.random;

            return true;
        }

        public override string ToString()
        {
            return $"FixedPointProvider({_type})";
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if( _type == PointType.PositionOnStart )
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(transform.position, 0.1f);
            }

            UnityEditor.Handles.color = Color.red;
            if (_range.a > 0) UnityEditor.Handles.DrawWireDisc(point, Vector3.up, _range.a);
            if (_range.b > 0) UnityEditor.Handles.DrawWireDisc(point, Vector3.up, _range.b);
        }
#endif
    }
}
