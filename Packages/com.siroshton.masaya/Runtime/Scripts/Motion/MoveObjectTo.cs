using Siroshton.Masaya.Core;
using Siroshton.Masaya.Math;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Siroshton.Masaya.Motion
{
    public class MoveObjectTo : MonoBehaviour
    {
        [SerializeField] private Transform _targetPosition;
        [SerializeField] private float _duration = 1;
        [SerializeField] private bool _easeInOut = false;
        [SerializeField] private UnityEvent<GameObject> _onFinished;

        Dictionary<GameObject, MoveTo> _moves = new Dictionary<GameObject, MoveTo>();

        public void MoveObject(GameObject o)
        {
            MoveTo mt = o.AddComponent<MoveTo>();
            mt.duration = _duration;
            mt.easeInOut = _easeInOut;
            mt.isLocal = false;
            mt.targetPosition = _targetPosition.position;

            if(_onFinished != null)
            {
                mt.onFinishedWithObject = new UnityEvent<GameObject>();
                mt.onFinishedWithObject.AddListener(OnFinished);
                _moves[o] = mt;
            }

            mt.StartMoving();
        }

        private void OnFinished(GameObject o)
        {
            _moves.Remove(o);
            _onFinished?.Invoke(o);
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawCube(transform.position, new Vector3(0.1f, 0.1f, 0.1f));

            if ( _targetPosition != null )
            {
                Gizmos.DrawLine(transform.position, _targetPosition.position);
                Gizmos.color = Color.green;
                Gizmos.DrawCube(_targetPosition.position, new Vector3(0.1f, 0.1f, 0.1f));
            }
        }
#endif

    }

}