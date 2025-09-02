using Siroshton.Masaya.Util;
using System.Collections.Generic;
using UnityEngine;

namespace Siroshton.Masaya.Environment
{
    public class CircleZone : MonoBehaviour
    {
        [Tooltip("If you leave the surface null, this mask will be used to try and detect a surface.")]
        [SerializeField] private LayerMask _surfaceMask;
        [SerializeField] private Collider _surface;
        [SerializeField] private float _radius = 10;
        [SerializeField] private float _spacing = 0.5f;

        public Collider surface { get => _surface; set => _surface = value; }
        public float radius { get => _radius; set => _radius = value; }
        public float spacing { get => _spacing; set => _spacing = value; }

        public void Start()
        {
            if( _surface == null ) UpdateSurface();
        }

        private void UpdateSurface()
        {
            RaycastHit hit;
            Ray ray = new Ray(transform.position + Vector3.up, Vector3.down);
            if (UnityEngine.Physics.Raycast(ray, out hit, 5, _surfaceMask.value))
            {
                _surface = hit.collider;
            }
        }

        public List<Vector3> GetContactPoints()
        {
            return GetContactPoints(_radius, _spacing);
        }

        public List<Vector3> GetContactPoints(float radius, float spacing)
        {
            if (_surface == null) UpdateSurface();

            return PhysicsUtil.GetCircleContactPointsXZ(_surface, transform.position, radius, spacing, transform.rotation.eulerAngles.y);
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            bool nullSurface = _surface == null;

            UnityEditor.Handles.DrawWireDisc(transform.position, Vector3.up, _radius);

            List<Vector3> points = GetContactPoints();
            if (points != null)
            {
                foreach (Vector3 point in points)
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawSphere(point, 0.1f);
                }
            }

            GUIStyle style = GUI.skin.label;
            style.alignment = TextAnchor.MiddleCenter;
            UnityEditor.Handles.Label(transform.position + Vector3.up * 1, points.Count.ToString(), style);

            if(nullSurface) _surface = null;
        }
#endif

    }
}