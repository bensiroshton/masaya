using Siroshton.Masaya.Environment;
using Siroshton.Masaya.Item;
using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Siroshton.Masaya.Core
{
    [RequireComponent(typeof(ItemDrops))]
    public class GameLevel : MonoBehaviour
    {
        [Serializable]
        public class RenderFeature
        {
            public ScriptableRendererFeature feature;
            public bool enabled;
        }

        [SerializeField] private string _levelName;
        [SerializeField] private bool _showLevelName = true;
        [SerializeField] private Checkpoint _initialCheckpoint;
        [SerializeField] private Transform _playerStartPoint;
        [SerializeField] private UnityEngine.Light[] _levelLights;
        [SerializeField] private UnityEngine.AudioSource[] _levelAudio;
        [SerializeField] private RenderFeature[] _renderFeatures;

        private class LightProperties
        {
            public UnityEngine.Light light;
            public float defaultIntensity;
        }

        private ItemDrops _itemDrops;
        private LightProperties[] _lights;

        public Checkpoint initialCheckpoint => _initialCheckpoint;

        public Transform playerStartPoint
        {
            get
            {
                if( _playerStartPoint != null ) return _playerStartPoint;
                else if( _initialCheckpoint != null ) return _initialCheckpoint.spawnPoint;
                else return null;
            }
        }

        public ItemDrops itemDrops => _itemDrops;

        private void Awake()
        {
            _itemDrops = GetComponent<ItemDrops>();

            if( Checkpoint.activeCheckpoint == null && _initialCheckpoint != null ) _initialCheckpoint.SetAsActive();

            _lights = new LightProperties[_levelLights.Length];
            for(int i=0;i<_levelLights.Length;i++)
            {
                _lights[i] = new LightProperties(){
                    light = _levelLights[i],
                    defaultIntensity = _levelLights[i].intensity,
                };
            }

            if( _renderFeatures != null )
            {
                foreach(RenderFeature rf in _renderFeatures)
                {
                    rf.feature.SetActive(rf.enabled);
                }
            }

        }

        private void Start()
        {
            if(_showLevelName)
            {
                GameManager.instance.ui.ShowLevelName(_levelName);
            }
        }

        public static GameLevel FindLevel()
        {
            return GameObject.FindAnyObjectByType<GameLevel>();
        }

        public void SetLightIntensity(float percentage)
        {
            if (_levelLights == null) return;

            LightProperties lp;
            for (int i = 0; i < _lights.Length; i++)
            {
                lp = _lights[i];
                lp.light.intensity = lp.defaultIntensity * percentage;
            }
        }

        public void EnableLevelLights(bool enabled)
        {
            if( _levelLights == null ) return;

            LightProperties lp;
            for (int i = 0; i < _lights.Length; i++)
            {
                lp = _lights[i];
                lp.light.enabled = enabled;
            }
        }

        public void EnableLevelAudio(bool enabled)
        {
            if (_levelAudio == null) return;

            for (int i = 0; i < _levelAudio.Length; i++)
            {
                _levelAudio[i].enabled = enabled;
            }
        }

    }

}