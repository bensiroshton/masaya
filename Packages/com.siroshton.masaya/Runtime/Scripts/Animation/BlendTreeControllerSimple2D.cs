using Siroshton.Masaya.Math;
using UnityEngine;

namespace Siroshton.Masaya.Animation
{
    [RequireComponent(typeof(Entity.Entity))]
    public class BlendTreeControllerSimple2D : MonoBehaviour
    {
        [Tooltip("If not set then GetComponent<Animator>() will be used.")]
        [SerializeField] private Animator _animator;
        [SerializeField] private string _propertyX;
        [SerializeField] private string _propertyY;
        [SerializeField] private IntervalFloat _xRange = new IntervalFloat(-1, 1);
        [SerializeField] private IntervalFloat _yRange = new IntervalFloat(-1, 1);

        private Entity.Entity _entity;
        private int _xId;
        private int _yId;

        private void Awake()
        {
            if( _animator == null ) _animator = GetComponent<Animator>();
            _entity = GetComponent<Entity.Entity>();
            _xId = Animator.StringToHash(_propertyX);
            _yId = Animator.StringToHash(_propertyY);
        }

        private void Update()
        {
            if( _entity.speed == 0 )
            {
                _animator.SetFloat(_xId, 0);
                _animator.SetFloat(_yId, 0);
            }
            else
            {
                // make sure these are relative to our forward direction.
                Vector3 velocity = _entity.velocity;
                velocity = Quaternion.Inverse(_entity.transform.rotation) * velocity;
                _animator.SetFloat(_xId, velocity.x / _entity.speed);
                _animator.SetFloat(_yId, velocity.z / _entity.speed);
            }
        }


    }
}