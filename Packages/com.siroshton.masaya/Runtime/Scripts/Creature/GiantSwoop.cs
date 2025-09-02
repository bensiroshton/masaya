using Siroshton.Masaya.Audio;
using Siroshton.Masaya.Core;
using Siroshton.Masaya.Environment;
using System;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Splines;

namespace Siroshton.Masaya.Creature
{

    public class GiantSwoop : Entity.Entity, IEmitterObjectReference
    {

        [SerializeField] private UnityEvent _onPathEndReached;
        [SerializeField] private AudioClip _wingSound;

        private enum State
        {
            PickingUp,
            HoldingPlayer,
        }

        private struct Pickup
        {
            public Vector3 moveVelocity;
        }

        private State _state;
        private Player.Player _player;
        private Pickup _pickup;
        private SplineContainer _flightPath;
        private SplineAnimate _splineAnim;
        private AudioSourceController _audioController;

        protected new void Awake()
        {
            base.Awake();

            _player = Player.Player.instance;
            _splineAnim = GetComponent<SplineAnimate>();
            _splineAnim.Completed += OnSplineCompleted;

            _audioController = GetComponent<AudioSourceController>();
        }

        protected new void Start()
        {
            base.Start();

            if(_flightPath != null )
            {
                IEnumerator<BezierKnot> knots = _flightPath.Spline.Knots.GetEnumerator();
                knots.MoveNext();

                BezierKnot knot = knots.Current;
                knot.Position = _flightPath.transform.InverseTransformPoint(_player.transform.position);

                _flightPath.Spline.SetKnot(0, knot);
            }

            _state = State.PickingUp;
        }

        public void Anim_PlayWingSound()
        {
            _audioController.PlayClip(_wingSound);
        }

        private void OnSplineCompleted()
        {
            if (_onPathEndReached != null) _onPathEndReached.Invoke();
        }

        public void AddReference(GameObject o)
        {
            _flightPath = o.GetComponent<SplineContainer>();
        }

        protected new void Update()
        {
            base.Update();

            float deltaTime = GameState.deltaTime;

            if( _state == State.PickingUp )
            {
                transform.position = Vector3.SmoothDamp(transform.position, _player.transform.position, ref _pickup.moveVelocity, 3.0f, speed, deltaTime);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, _player.transform.rotation, 20.0f * deltaTime);

                if( (transform.position - _player.transform.position).sqrMagnitude < 0.1f * 0.1f )
                {
                    // we're there.
                    transform.position = _player.transform.position;
                    _player.transform.SetParent(transform);
                    _state = State.HoldingPlayer;
                    _player.SetBeingCarried(true);
                    
                    _splineAnim.Container = _flightPath;
                    _splineAnim.MaxSpeed = speed;
                    _splineAnim.Play();
                }
            }
            else if( _state == State.HoldingPlayer )
            {

            }

        }

    }

}