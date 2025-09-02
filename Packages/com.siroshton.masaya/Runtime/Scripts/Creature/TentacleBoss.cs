using JetBrains.Annotations;
using Siroshton.Masaya.Core;
using Siroshton.Masaya.Entity;
using Siroshton.Masaya.Environment;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Siroshton.Masaya.Creature
{

    public class TentacleBoss : Entity.Entity
    {
        [Tooltip("Each prefab represents a single phase of tentacles.")]
        [SerializeField] private GameObject[] _phaseTentaclePrefab;
        [SerializeField] private Emitter[] _tentacleEmitters;
        [SerializeField] private UnityEvent _onActivated;
        [SerializeField] private UnityEvent _onWakeTheBeast;
        [SerializeField] private UnityEvent _onPhase2Started;

        private int _tentaclePhase = 0;
        private EntityDestroyedTracker _deathTracker;

        private enum State
        {
            Sleeping,
            TentaclePhase,
            WakingTheBeast,
            Dead
        }

        private struct WakingTheBeast
        {
            public int step;
            public float stepTime;
        }

        private State _state = State.Sleeping;
        private WakingTheBeast _waking;

        protected new void Awake()
        {
            base.Awake();

            _deathTracker = gameObject.AddComponent<EntityDestroyedTracker>();
            _deathTracker.onAllEntitiesDestroyed = new UnityEvent();
            _deathTracker.onAllEntitiesDestroyed.AddListener(OnAllTentaclesDestroyed);
        }

        protected override void OnKilled()
        {
            base.OnKilled();
            _state = State.Dead;
        }

        public void Reset()
        {
            _state = State.Sleeping;
        }

        public void Activate()
        {
            if( _state != State.Sleeping ) return;

            StartTentaclePhase(0);
            _onActivated?.Invoke();
        }

        private bool StartTentaclePhase(int phase)
        {
            if( phase >= _phaseTentaclePrefab.Length ) return false;

            _tentaclePhase = phase;
            _state = State.TentaclePhase;

            _deathTracker.Reset();
            if ( phase == 0 ) _deathTracker.deathEvent = EntityDestroyedTracker.DeathEvent.OnDestroyed;
            else _deathTracker.deathEvent = EntityDestroyedTracker.DeathEvent.OnKilled;

            for (int i = 0; i < _tentacleEmitters.Length; i++)
            {
                _tentacleEmitters[i].prefab = _phaseTentaclePrefab[_tentaclePhase];
                GameObject o = _tentacleEmitters[i].EmitObject();
                _deathTracker.AddEntity(o);
            }

            if( phase == 1 )
            {
                _onPhase2Started?.Invoke();
            }

            return true;
        }

        private void OnAllTentaclesDestroyed()
        {
            StartNextPhase();
        }

        private void StartNextPhase()
        {
            if( _state == State.TentaclePhase )
            {
                if( !StartTentaclePhase(_tentaclePhase + 1) )
                {
                    WakeTheBeast();
                }
            }
        }

        // this was originally named for "phase 3" the final "beast" wave.
        private void WakeTheBeast()
        {
            if( _state != State.TentaclePhase ) return;

            _state = State.WakingTheBeast;
            if (_onWakeTheBeast != null) _onWakeTheBeast.Invoke();
        }

        protected new void Update()
        {
            base.Update();

        }

    }

}