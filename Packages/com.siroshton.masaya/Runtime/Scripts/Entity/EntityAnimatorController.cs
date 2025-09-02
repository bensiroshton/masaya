using Siroshton.Masaya.Math;
using System;
using UnityEngine;

namespace Siroshton.Masaya.Entity
{
    [RequireComponent(typeof(Entity))]
    public class EntityAnimatorController : MonoBehaviour
    {
        [Serializable]
        public class Property
        {
            public bool enabled;
            public string propertyName;
            public bool normalize;
            public IntervalFloat normalizedRange;
            public float multiplier;

            [HideInInspector] public int propertyId;

            public Property(string name, bool norm, IntervalFloat range)
            {
                enabled = false;
                propertyName = name;
                normalize = norm;
                normalizedRange = range;
                multiplier = 1;
            }
        }

        public enum TurnAngleMode
        {
            RelativeTurnAngle,
            AngleToPlayer
        }

        [SerializeField] private Animator _animator;
        [SerializeField] private Property _speed = new Property("movementSpeed", false, new IntervalFloat(0, 1));
        [SerializeField] private TurnAngleMode _turnAngleMode = TurnAngleMode.RelativeTurnAngle;
        [SerializeField] private Property _turnAngle = new Property("turnAngle", true, new IntervalFloat(-1, 1));
        [SerializeField, Range(0, 90)] private float _turnAngleExtent = 50;
        
        private Entity _entity;

        public Property speed => _speed;
        public bool speedEnabled { get => _speed.enabled; set => _speed.enabled = value; }
        public Property turnAngle => _turnAngle;
        public bool turnAngleEnabled { get => _turnAngle.enabled; set => _turnAngle.enabled = value; }
        public TurnAngleMode turnAngleMode { get => _turnAngleMode; set => _turnAngleMode = value; }
        public float turnAngleExtent { get => _turnAngleExtent; set => _turnAngleExtent = value; }
        public float turnAngleMultiplier { get => _turnAngle.multiplier; set => _turnAngle.multiplier = value; }

        private void Awake()
        {
            _entity = GetComponent<Entity>();
            if ( _animator == null ) _animator = GetComponentInChildren<Animator>();

            _speed.propertyId = Animator.StringToHash(_speed.propertyName);
            _turnAngle.propertyId = Animator.StringToHash(_turnAngle.propertyName);
        }

        public void EnableAngleToPlayerMode()
        {
            _turnAngleMode = TurnAngleMode.AngleToPlayer;
        }

        public void EnableRelativeTurnAngleMode()
        {
            _turnAngleMode = TurnAngleMode.RelativeTurnAngle;
        }

        private void Update()
        {

            if( _speed.enabled )
            {
                if(_speed.normalize)
                {
                    if (_entity.speed == 0 )
                    {
                        _animator.SetFloat(_speed.propertyId, 0);
                    }
                    else
                    {
                        _animator.SetFloat(_speed.propertyId, _speed.normalizedRange.Lerp(_entity.currentSpeed / _entity.speed) * _speed.multiplier);
                    }
                }
                else
                {
                    _animator.SetFloat(_speed.propertyId, _entity.currentSpeed * _speed.multiplier);
                }
            }

            if( _turnAngle.enabled )
            {
                float angle;

                if(_turnAngleMode == TurnAngleMode.RelativeTurnAngle)
                {
                    angle = _entity.relativeTurnAngle;
                }
                else
                {
                    angle = Vector3.SignedAngle(transform.forward, (Player.Player.instance.transform.position - transform.position).normalized, Vector3.up);
                }

                if ( _turnAngle.normalize )
                {
                    _animator.SetFloat(_turnAngle.propertyId, MathUtil.Normalize(-_turnAngleExtent, _turnAngleExtent, angle * _turnAngle.multiplier));
                }
                else
                {
                    _animator.SetFloat(_turnAngle.propertyId, angle * _turnAngle.multiplier);
                }

            }
        }
    }

}