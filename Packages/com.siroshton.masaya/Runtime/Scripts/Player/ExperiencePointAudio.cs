using Siroshton.Masaya.Audio;
using System;
using UnityEngine;

namespace Siroshton.Masaya.Player
{
    public class ExperiencePointAudio : MonoBehaviour
    {
        [Serializable]
        public class Sound
        {
            public AudioClip clip;
            [Range(-3, 3)] public float pitch = 1;
            [Range(0, 1)] public float targetVolume = 1;
        }

        private class NoteAudio
        {
            public AudioSource source;
            public AudioSourceController controller;
        }

        [SerializeField] private Sound[] _sounds;
        [SerializeField, Range(0, 2)] private float _smoothVolumeTime = 1.0f;

        private int _index;
        private NoteAudio[] _audio;

        private void Start()
        {
            _audio = new NoteAudio[_sounds.Length];
            for(int i=0;i<_audio.Length;i++)
            {
                _audio[i] = new NoteAudio();
                _audio[i].source = gameObject.AddComponent<AudioSource>();
                _audio[i].controller = gameObject.AddComponent<AudioSourceController>();

                _audio[i].source.clip = _sounds[i].clip;
                _audio[i].source.pitch = _sounds[i].pitch;
                _audio[i].source.loop = true;
                _audio[i].source.volume = 0;

                _audio[i].controller.source = _audio[i].source;
                _audio[i].controller.smoothVolume = true;
                _audio[i].controller.smoothVolumeTime = _smoothVolumeTime;
                _audio[i].controller.volume = 0;
                _audio[i].controller.volumeTarget = 0;

                _audio[i].source.Play();
            }
        }

        public void PlayNotes(int count)
        {      
            count = System.Math.Min(count, _audio.Length);
            for(int i=0;i<count;i++)
            {
                _audio[_index].controller.volume = _sounds[_index].targetVolume;
                _index++;
                if( _index == _audio.Length ) _index = 0;
            }
        }

    }
}