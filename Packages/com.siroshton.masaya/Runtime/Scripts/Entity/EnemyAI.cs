using Siroshton.Masaya.Core;
using Siroshton.Masaya.Weapon;
using System;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using Random = UnityEngine.Random;

namespace Siroshton.Masaya.Entity
{

    [RequireComponent(typeof(Entity))]
    [RequireComponent(typeof(WorldPerceptor))]
    public class EnemyAI : MonoBehaviour
    {
        public enum DodgeState
        {
            Started,
            Updating,
            Ended
        }

        public struct DodgeInfo
        {
            public DodgeState state;
            public float pos;
        }

        [Flags]
        public enum DodgeMode
        {
            Never = 0,
            OnBulletInProximity = 1,
            OnBulletMightHit = 2,
            Random = 4
        }

        [SerializeField] private DodgeMode _dodgeMode;
        [SerializeField] private float _dodgeSpeedModifier = 3;
        [SerializeField] private float _dodgeDistance = 2;
        [SerializeField] private float _dodgeCooldown = 2;
        [SerializeField] private float _randomDodgeCooldownMin = 1;
        [SerializeField] private float _randomDodgeCooldownMax = 4;
        [SerializeField] private float _pathUpdateInterval = 0.5f;
        [SerializeField] private Gun _gun;
        [SerializeField] private float _shootInterval = 2;

        private Entity _entity;
        private WorldPerceptor _perceptor;
        private NavMeshAgent _agent;

        /*
        private enum ActionType
        {
            PathFinding,
            Dodging,
            Shooting
        }

        private class ActionInfo
        {
            public bool isDoingAction;
            public float timeSinceAction;
            public float actionCooldown;
        }

        ActionInfo[] _actionInfo = new ActionInfo[3];
        */

        // dodge support
        private bool _isDodging;
        private float _timeSinceDodge;
        private Vector3 _evadeDirection;
        private float _dodgeTime; 
        private float _randomDodgeCooldown;

        [SerializeField] private UnityEvent _onDodgeStarted = new UnityEvent();
        [SerializeField] private UnityEvent _onDodgeEnded = new UnityEvent();
        private UnityEvent<DodgeInfo> _onDodgeStateChange = new UnityEvent<DodgeInfo>();
        
        // shooting support
        private float _timeSinceShoot;

        // path suport
        private float _timeSincePathUpdate;

        // utility
        private float speed { get => _entity.attributes.GetSpeed(_entity.attributeModifiers); }
        private float dodgeSpeed { get => _entity.attributes.GetSpeed(_entity.attributeModifiers) * _dodgeSpeedModifier; }

        // public
        public UnityEvent<DodgeInfo> onDodgeStateChange { get => onDodgeStateChange; set => onDodgeStateChange = value; }

        private void Awake()
        {
            _entity = GetComponent<Entity>();
            _perceptor = GetComponent<WorldPerceptor>();
            _perceptor.onBulletEnteredPerception.AddListener(onBulletEnteredPerception);
            _agent = GetComponent<NavMeshAgent>();


            _timeSincePathUpdate = _pathUpdateInterval;
            _timeSinceDodge = 0;
            _randomDodgeCooldown = Random.Range(_randomDodgeCooldownMin, _randomDodgeCooldownMax);
        }

        private void onBulletEnteredPerception(WorldPerceptor.BulletInfo info)
        {            
            if ( !_isDodging && _timeSinceDodge > _dodgeCooldown )
            {
                if( !_dodgeMode.HasFlag(DodgeMode.OnBulletMightHit) && !info.mightHit ) return;
                else if (!_dodgeMode.HasFlag(DodgeMode.OnBulletInProximity) ) return;

                _evadeDirection = info.evadeDirection;
                _dodgeTime = _dodgeDistance / dodgeSpeed;
                _timeSinceDodge = 0;
                _isDodging = true;

                SendDodgeEvent(DodgeState.Started, 0);
                _onDodgeStarted.Invoke();
            }
        }

        private void SendDodgeEvent(DodgeState state, float pos)
        {
            DodgeInfo info = new DodgeInfo();
            info.state = state;
            info.pos = pos;
            _onDodgeStateChange.Invoke(info);
        }

        private void Update()
        {
            _timeSincePathUpdate += GameState.deltaTime;
            _timeSinceDodge += GameState.deltaTime;
            _timeSinceShoot += GameState.deltaTime;

            if ( _isDodging )
            {
                if ( _agent != null )
                {
                    _agent.Move(_evadeDirection * dodgeSpeed * GameState.deltaTime);
                }
                else
                {
                    transform.position += _evadeDirection * dodgeSpeed * GameState.deltaTime;
                }

                if( _timeSinceDodge >= _dodgeTime )
                {
                    _isDodging = false;
                    _timeSinceDodge = 0;

                    SendDodgeEvent(DodgeState.Ended, 1);
                    _onDodgeEnded.Invoke();
                }
                else
                {
                    SendDodgeEvent(DodgeState.Updating, _timeSinceDodge / _dodgeTime);
                }
            }
            else if( _dodgeMode.HasFlag(DodgeMode.Random) && _timeSinceDodge > _randomDodgeCooldown)
            {
                _evadeDirection = Vector3.Cross(transform.forward, Vector3.up);
                if( Random.Range(1, 100) > 50 ) _evadeDirection = -_evadeDirection;
                _dodgeTime = _dodgeDistance / dodgeSpeed;
                _timeSinceDodge = 0;
                _isDodging = true;

                _randomDodgeCooldown = Random.Range(_randomDodgeCooldownMin, _randomDodgeCooldownMax);

                SendDodgeEvent(DodgeState.Started, 0);
                _onDodgeStarted.Invoke();
            }
            else if( _gun != null && _timeSinceShoot > _shootInterval )
            {
                _gun.Trigger();
                _timeSinceShoot = 0;
            }
            else if (_timeSincePathUpdate >= _pathUpdateInterval)
            {
                // Update Path
                if( _agent != null )
                {
                    _agent.speed = speed;
                    _agent.SetDestination(_perceptor.playerPosition);
                }

                _timeSincePathUpdate = 0;
            }

        }

    }

}