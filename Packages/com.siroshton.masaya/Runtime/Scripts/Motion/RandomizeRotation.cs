using Siroshton.Masaya.Math;
using UnityEngine;
using UnityEngine.Events;

namespace Siroshton.Masaya.Motion
{

    public class RandomizeRotation : MonoBehaviour
    {
        [SerializeField] private IntervalVec3 _range;
        [SerializeField] private bool _randomizeOnStart = true;
        [SerializeField] private UnityEvent _onRandomized;

        private void Start()
        {
            if( _randomizeOnStart ) Randomize();
        }

        public void Randomize()
        {
            transform.localRotation = Quaternion.Euler(_range.random);

            if(_onRandomized != null) _onRandomized.Invoke();
        }

    }

}