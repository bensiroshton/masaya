using Siroshton.Masaya.Core;
using Siroshton.Masaya.Math;
using UnityEngine;

namespace Siroshton.Masaya.Audio
{
    public class AudioSourceController : MonoBehaviour
    {
        [SerializeField] private AudioSource _source;
        [SerializeField] private bool _smoothVolume;
        [SerializeField] private bool _randomizePitchOnStartup;
        [SerializeField, Range(0, 1)] private float _volume = 1.0f;
        [SerializeField, Range(0, 1)] private float _volumeTarget = 1.0f;
        [SerializeField] private float _smoothVolumeTime = 1.0f;
        [SerializeField] private IntervalFloat _pitchRange = new IntervalFloat(1.0f, 1.0f);

        public AudioSource source { get => _source; set => _source = value; }
        public float volume { get => _volume; set => _volume = value; }
        public bool smoothVolume { get => _smoothVolume; set => _smoothVolume = value; }
        public float volumeTarget { get => _volumeTarget; set => _volumeTarget = value; }
        public float smoothVolumeTime { get => _smoothVolumeTime; set => _smoothVolumeTime = value; }
        public IntervalFloat pitchRange { get => _pitchRange; set => _pitchRange = value; }

        private float _volumeVelocity;

        private void Awake()
        {
            if (_source == null) _source = GetComponent<AudioSource>();
            _source.volume = _volume;
            if (_randomizePitchOnStartup) _source.pitch = _pitchRange.random;
        }

        public void PlayClip(AudioClip clip)
        {
            _source.pitch = _pitchRange.random;
            _source.PlayOneShot(clip);
        }

        public void PlayClip(AudioClip clip, float volumeScale)
        {
            _source.pitch = _pitchRange.random;
            _source.PlayOneShot(clip, volumeScale);
        }

        private void LateUpdate()
        {
            if( _source == null ) return;

            if( _smoothVolume )
            {
                _volume = Mathf.SmoothDamp(_volume, _volumeTarget, ref _volumeVelocity, _smoothVolumeTime);
            }

            _source.volume = _volume;
        }

    }


}
