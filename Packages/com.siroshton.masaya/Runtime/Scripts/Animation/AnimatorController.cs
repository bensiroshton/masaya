using UnityEngine;
using static Siroshton.Masaya.Core.Types;

namespace Siroshton.Masaya.Animation
{

    public class AnimatorController : MonoBehaviour
    {
        [SerializeField] private Animator _animator;
        [SerializeField] private BooleanAction _keepAnimatorStateOnDisable = BooleanAction.True;
        [SerializeField] private bool _playWithRandomizeTimeOnStartup = false;

        private void Awake()
        {
            if (_animator == null) _animator = GetComponentInChildren<Animator>();
            if (_keepAnimatorStateOnDisable != BooleanAction.Unchanged) _animator.keepAnimatorStateOnDisable = _keepAnimatorStateOnDisable == BooleanAction.True;
        }

        private void Start()
        {
            if(_playWithRandomizeTimeOnStartup) RandomizeCurrentStateTime();
        }

        public void RandomizeCurrentStateTime()
        {
            _animator.Play(0, -1, Random.value);
        }

        public void PlayAtRandomizedTime(string stateName)
        {        
            _animator.Play(stateName, -1, Random.value);
        }

    }

}