
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Siroshton.Masaya.Component
{
    public class DestroyAfterTime : MonoBehaviour
    {
        [SerializeField] private float _countdownTime = 1;
        [SerializeField] private bool _countdownImmediately = false;
        [SerializeField] UnityEvent<GameObject> _beforeDestroy = new UnityEvent<GameObject>();

        public float countdownTime { get => _countdownTime; set => _countdownTime = value; }
        public bool countdownImmediately { get => _countdownImmediately; set => _countdownImmediately = value; }
        public UnityEvent<GameObject> beforeDestroy { get => _beforeDestroy; set => _beforeDestroy = value; }

        public void Start()
        {
            if( _countdownImmediately ) StartCountdown();
        }

        public void StartCountdown()
        {
            if( _countdownTime <= 0 ) DestroyNow();
            else StartCoroutine(DeathCountdown());
        }

        IEnumerator DeathCountdown()
        {
            yield return new WaitForSeconds(_countdownTime);
            DestroyNow();
        }

        public void DestroyNow()
        {
            _beforeDestroy.Invoke(gameObject);
            Destroy(gameObject);
        }

    }
}
