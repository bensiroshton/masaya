using Siroshton.Masaya.Math;
using UnityEngine;

namespace Siroshton.Masaya.Audio
{
    public class SetAudioFromSpeed : MonoBehaviour
    {
        [SerializeField] private Entity.Entity _entity;
        [SerializeField] private AudioSource _audioSource;
        [SerializeField] private bool _adjustVolume = true;
        [SerializeField] private IntervalFloat _volumeRange = new IntervalFloat(0, 1);
        [SerializeField] private bool _adjustPitch = true;
        [SerializeField] private IntervalFloat _pitchRange = new IntervalFloat(0.5f, 1);

        private void Awake()
        {
            if( _entity == null ) _entity = GetComponent<Entity.Entity>();
            if( _audioSource == null ) _audioSource = GetComponent<AudioSource>();
        }

        private void Update()
        {
            if(_entity.speed == 0)
            {
                if (_adjustVolume) _audioSource.volume = _volumeRange.a;
                if (_adjustPitch) _audioSource.pitch = _pitchRange.a;
                return;
            }

            float normalizedSpeed = _entity.currentSpeed / _entity.speed;
            if (_adjustVolume) _audioSource.volume = _volumeRange.Lerp(normalizedSpeed);
            if (_adjustPitch) _audioSource.pitch = _pitchRange.Lerp(normalizedSpeed);
        }

    }

}