using Siroshton.Masaya.Core;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Siroshton.Masaya.Motion
{
    public class GameObjectPath : MonoBehaviour
    {
        [Tooltip("Must contain an IPath based component.")]
        [SerializeField] private GameObject _objectPath;

        [Tooltip("Meters per second.")]
        [SerializeField] private float objectSpeed = 1;
        
        [Tooltip("Degrees per second.")]
        [SerializeField] private float rotationSpeed = 20;

        [SerializeField] private UnityEvent<GameObject> _onObjectAdded;
        [SerializeField] private UnityEvent<GameObject> _onObjectRemoved;

        private class TrackedObject
        {
            public GameObject obj;
            public int currentPoint;
        }

        private List<TrackedObject> _objects = new List<TrackedObject>();
        private IPath _path;

        public int objectCount { get => _objects.Count; }

        public IPath path { get => _path; set => _path = value; }

        private void Awake()
        {
            if( _objectPath != null ) _path = _objectPath.GetComponent<IPath>();
        }

        public void AddGameObject(GameObject obj)
        {
            if( _path == null || _path.pathPointCount == 0 ) return;

            PathPoint p = _path.GetPathPoint(0);
            obj.transform.position = p.position;
            obj.transform.rotation = p.rotation;

            if( _path.pathPointCount > 1 )
            {
                TrackedObject t = new TrackedObject
                {
                    obj = obj,
                    currentPoint = 0,
                };

                _objects.Add(t);
                if(_onObjectAdded != null) _onObjectAdded.Invoke(t.obj);
            }
        }

        private void Update()
        {
            TrackedObject t;
            PathPoint thisPoint;
            PathPoint nextPoint;

            for (int i = _objects.Count - 1; i >= 0; i--)
            {
                t = _objects[i];

                if( t.obj == null )
                {
                    // object was destroyed
                    _objects.RemoveAt(i);
                    continue;
                }

                if( t.currentPoint >= _path.pathPointCount - 1 )
                {
                    // object has reached the end, stop tracking it.
                    _objects.RemoveAt(i);
                    if (_onObjectRemoved != null) _onObjectRemoved.Invoke(t.obj);
                    continue;
                }

                thisPoint = _path.GetPathPoint(t.currentPoint);
                nextPoint = _path.GetPathPoint(t.currentPoint + 1);
                t.obj.transform.position = Vector3.MoveTowards(t.obj.transform.position, nextPoint.position, objectSpeed * thisPoint.speedMultiplier * GameState.deltaTime);
                t.obj.transform.rotation = Quaternion.RotateTowards(t.obj.transform.rotation, nextPoint.rotation, rotationSpeed * GameState.deltaTime);
                if( (t.obj.transform.position - nextPoint.position).sqrMagnitude < 0.00000001 )
                {
                    // We reached the next point.  Remove it on the next update cycle if needed.
                    t.currentPoint = t.currentPoint + 1;
                }
            }
        }
    }
}