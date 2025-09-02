using Siroshton.Masaya.Core;
using Siroshton.Masaya.Math;
using UnityEngine;
using UnityEngine.Events;

namespace Siroshton.Masaya.Component
{
    public class PrefabInstanceManager : MonoBehaviour
    {
        [SerializeField] private GameObject _prefab;
        [SerializeField] private bool _recreateOnPlayerDeath = true;
        [SerializeField] private IntervalFloat _yRotation = new IntervalFloat(0, 0);
        [SerializeField] private IntervalFloat _scale = new IntervalFloat(1, 1);

        private GameObject _instance;

        private void Awake()
        {
            CreateInstance();

            if( _recreateOnPlayerDeath )
            {
                GameEvents ge = gameObject.AddComponent<GameEvents>();
                ge.onPlayerRevived = new UnityEvent();
                ge.onPlayerRevived.AddListener(OnPlayerRevived);
            }
        }

        private void OnPlayerRevived()
        {
            if( _instance != null )
            {
                Entity.Entity entity = _instance.GetComponent<Entity.Entity>();
                if( entity != null && entity.isDead )
                {
                    _instance.transform.SetParent(null);
                    Destroy(_instance);
                    _instance = null;
                }
            }

            CreateInstance();
        }

        private void CreateInstance()
        {
            if( transform.childCount > 0 )
            {
                _instance = transform.GetChild(0).gameObject;
            }

            if (_instance == null && _prefab != null)
            {
                _instance = GameObject.Instantiate(_prefab, transform);
            }

            _instance.transform.localPosition = Vector3.zero;
            _instance.transform.localRotation = Quaternion.Euler(0, _yRotation.random, 0);
            float scale = _scale.random;
            _instance.transform.localScale = new Vector3(scale, scale, scale);
        }

    }

}