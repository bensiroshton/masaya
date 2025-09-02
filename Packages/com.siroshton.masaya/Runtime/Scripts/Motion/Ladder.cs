using UnityEngine;
using Siroshton.Masaya.Core;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Siroshton.Masaya.Motion
{
    public class Ladder : MonoBehaviour, IPath
    {
        [SerializeField] private float _height;
        [SerializeField] private float _offset = 0;
        [SerializeField] private float _overhang = 0;

        private List<PathPoint> _pathPoints = new List<PathPoint>();

        public float height 
        { 
            get => _height; 
            set
            {
                _height = value; 
                UpdatePoints();
            }
        }

        public float offset 
        {
            get => _offset; 
            set
            {
                _offset = value; 
                UpdatePoints();
            }
        }

        public float overhang 
        { 
            get => _overhang; 
            set 
            {
                _overhang = value; 
            }
        }

        public Vector3 bottom { get => transform.position - Vector3.forward * _offset; }
        public Vector3 top { get => transform.position + new Vector3(0, _height, 0) - Vector3.forward * _offset; }

        public void Awake()
        {
            UpdatePoints();
        }

        private void UpdatePoints()
        {
            int count = 2;
            if( _overhang != 0 ) count++;

            while( _pathPoints.Count < count ) _pathPoints.Add(new PathPoint(1));
            if (_pathPoints.Count > count) _pathPoints.RemoveRange(count, _pathPoints.Count - count);

            Matrix4x4 t = transform.localToWorldMatrix;
            Vector3 local = transform.position;
            Vector3 localBottom = t * (bottom - local);
            Vector3 localTop = t * (top - local);

            _pathPoints[0] = new PathPoint {
                position = localBottom + transform.position,
                rotation = transform.rotation,
                speedMultiplier = _pathPoints[0].speedMultiplier
            };

            _pathPoints[1] = new PathPoint
            {
                position = localTop + transform.position,
                rotation = transform.rotation,
                speedMultiplier = _pathPoints[1].speedMultiplier
            };

            if( _overhang != 0 )
            {
                Vector3 localOverhang = t * (localTop + Vector3.forward * _overhang);

                _pathPoints[2] = new PathPoint
                {
                    position = localOverhang + transform.position,
                    rotation = transform.rotation,
                    speedMultiplier = _pathPoints[2].speedMultiplier
                };
            }
        }

        public int pathPointCount
        {
            get => _pathPoints.Count;
        }

        public PathPoint GetPathPoint(int index)
        {
            return _pathPoints[index];
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Vector3 local = transform.position;
            Vector3 localBottom = bottom - local;
            Vector3 localTop = top - local;

            Handles.matrix = transform.localToWorldMatrix;
            Handles.color = Color.yellow;
            Handles.zTest = UnityEngine.Rendering.CompareFunction.LessEqual;
            Handles.DrawLine(localBottom, localTop, 2);

            if( _overhang != 0 ) Handles.DrawLine(localTop, localTop + Vector3.forward * _overhang, 2);

            if ( _offset != 0 )
            {
                Handles.color = Color.cyan;
                Vector3 mid = localBottom * 0.1f + localTop * 0.9f;
                Handles.DrawLine(mid, mid + Vector3.forward * _offset);
            }
        }
#endif
    }
}
