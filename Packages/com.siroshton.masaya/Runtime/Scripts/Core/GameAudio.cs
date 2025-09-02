using System.Collections;
using UnityEngine;

namespace Siroshton.Masaya.Core
{

    public class GameAudio : MonoBehaviour
    {
        [SerializeField] private AudioListener _audioListener;
        [SerializeField, Range(0, 1)] private float _maxVolume = 1.0f;

        private float _volume;

        private struct Fade
        {
            public bool isFading;
            public float start;
            public float target;
            public float time;
            public float duration;
        }

        private Fade _fade;

        public static void ConfigureWithDefaultSettings(AudioSource source)
        {
            source.spatialBlend = 1;
            source.minDistance = 2;
            source.maxDistance = 12;
            source.rolloffMode = AudioRolloffMode.Linear;
        }

        public float maxVolume
        { 
            get => _maxVolume; 
            set
            {
                _maxVolume = value;
                AudioListener.volume = _maxVolume * _volume;
            }
        }
        
        public float volume
        { 
            get => AudioListener.volume; 
            set
            {
                _volume = value;
                AudioListener.volume = _maxVolume * _volume;
            }
        }

        private void Awake()
        {
            volume = _maxVolume;
        }

        public void FadeIn(float duration)
        {
            FadeAudio(1, duration);
        }

        public void FadeOut(float duration)
        {
            FadeAudio(0, duration);
        }

        public void FadeAudio(float target, float duration)
        {
            if( duration <= 0 )
            {
                _fade.isFading = false;
                volume = target;
            }
            else
            {
                _fade.isFading = true;
                _fade.start = volume;
                _fade.target = target;
                _fade.time = 0;
                _fade.duration = duration;
            }
        }

        private void Update()
        {
            if( _fade.isFading )
            {
                _fade.time += GameState.deltaTime;
                if( _fade.time >= _fade.duration)
                {
                    _fade.time = _fade.duration;
                    _fade.isFading = false;
                }
                volume = Mathf.Lerp(_fade.start, _fade.target, _fade.time / _fade.duration);
            }
        }

    }
}