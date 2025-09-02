using Siroshton.Masaya.Core;
using Siroshton.Masaya.Entity;
using Siroshton.Masaya.Environment;
using Siroshton.Masaya.Math;
using Siroshton.Masaya.Motion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Siroshton.Masaya.Environment
{
    public class CircleZoneEmitter : Emitter
    {
        public enum StartDirection
        {
            Inward,
            CounterClockwise,
            Clockwise,
            Outward,
            Random
        }

        [SerializeField] private CircleZone _zone;
        [SerializeField] private StartDirection _startDirection;
        [SerializeField] private int _waveCount = 1;
        [SerializeField] private float _timeBetweenWaves = 2;

        private int _wave;
        private float _timeSinceWave;

        public CircleZone zone { get => _zone; set => _zone = value; }
        public StartDirection startDirection { get => _startDirection; set => _startDirection = value; }
        public int waveCount { get => _waveCount; set => _waveCount = value; }
        public float timeBetweenWaves { get => _timeBetweenWaves; set => _timeBetweenWaves = value; }

        override public void ResetEmitter()
        {
            base.ResetEmitter();

            _wave = 0;
            _timeSinceWave = _timeBetweenWaves;
        }

        protected new void Awake()
        {
            base.Awake();

            if( _zone == null ) _zone = GetComponent<CircleZone>();
        }

        protected new void Start()
        {
            base.Start();

            mode = EmitterMode.Manual;
            _wave = 0;
            _timeSinceWave = _timeBetweenWaves;
        }

        protected new void Update()
        {
            if (_wave >= _waveCount) return;

            _timeSinceWave += GameState.deltaTime;
            if (_timeSinceWave > _timeBetweenWaves)
            {                
                List<Vector3> points = _zone.GetContactPoints();
                if (points != null)
                {
                    for(int i=0;i<points.Count;i++)
                    {
                        Vector3 point = points[i];
                        Vector3 dir;

                        if(_startDirection == StartDirection.Inward ) dir = (_zone.transform.position - point).normalized;
                        else if (_startDirection == StartDirection.Outward) dir = (point - _zone.transform.position).normalized;
                        else if (_startDirection == StartDirection.CounterClockwise)
                        {
                            int next = i - 1;
                            if (next < 0) next = points.Count - 1;
                            dir = (points[next] - point).normalized;
                        }
                        else if (_startDirection == StartDirection.Clockwise)
                        {
                            int next = i + 1;
                            if (next == points.Count) next = 0;
                            dir = (points[next] - point).normalized;
                        }
                        else
                        {
                            do
                            {
                                dir = new Vector3(Random.Range(-1, 1), 0, Random.Range(-1, 1)).normalized;
                            } while(dir.sqrMagnitude == 0);
                        }

                        if( EmitObject(point, Quaternion.LookRotation(dir, Vector3.up)) == null)
                        {
                            // probably reached the emitters max emission count
                            break;
                        }
                    }
                }

                _timeSinceWave = 0;
                _wave++;
                ResetEmissionCount();
            }
        }

    }
}
