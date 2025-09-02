using Siroshton.Masaya.Math;
using UnityEngine;
using static Siroshton.Masaya.Core.Types;

namespace Siroshton.Masaya.Environment
{
    public class BoxEmitter : Emitter
    {
        [SerializeField] private Bounds _bounds = new Bounds(Vector3.zero, Vector3.one);
        [SerializeField] private OptionalAxis _flipSideEachEmission = OptionalAxis.None;

        private bool _flip;

        public Vector3 worldCenter => transform.position + _bounds.center;

        protected override void OnObjectEmitted(GameObject o)
        {
            Bounds bounds = _bounds;
            if(_flipSideEachEmission == OptionalAxis.X)
            {
                float x = _bounds.extents.x * 0.5f;
                bounds.extents = new Vector3(x, _bounds.extents.y, _bounds.extents.z);
                if ( _flip ) bounds.center = _bounds.center + new Vector3(-x, 0, 0);
                else bounds.center = _bounds.center + new Vector3(x, 0, 0);

            }
            else if( _flipSideEachEmission == OptionalAxis.Y )
            {
                float y = _bounds.extents.y * 0.5f;
                bounds.extents = new Vector3(_bounds.extents.x, y, _bounds.extents.z);
                if (_flip) bounds.center = _bounds.center + new Vector3(0, -y, 0);
                else bounds.center = _bounds.center + new Vector3(0, y, 0);
            }
            else if (_flipSideEachEmission == OptionalAxis.Z)
            {
                float z = _bounds.extents.z * 0.5f;
                bounds.extents = new Vector3(_bounds.extents.x, _bounds.extents.y, z);
                if (_flip) bounds.center = _bounds.center + new Vector3(0, 0, -z);
                else bounds.center = _bounds.center + new Vector3(0, 0, z);
            }

            o.transform.position = transform.TransformPoint(bounds.center + MathUtil.GetRandomExtent(bounds.extents));
            o.transform.rotation = transform.rotation;
            _flip = !_flip;
        }

#if UNITY_EDITOR
        private new void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(_bounds.center, _bounds.size);
        }
#endif
    }
}
