using Siroshton.Masaya.Core;
using Siroshton.Masaya.Math;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Siroshton.Masaya.Audio
{

    public class DynamicAudio :  MonoBehaviour
    {
        public enum AudioCommand
        {
            PlayOneShot,
            PlayLoop,
            Stop
        }

        [Serializable]
        public struct RangeSettings
        {
            public IntervalFloat range;
            [Tooltip("Only valid with PlayLoop command.")]
            public bool adjustFromSpeed;
            public float maxSpeed;
        }

        [Serializable]
        public struct AudioSettings
        {
            public LayerMask layer;
            public AudioClip clip;
            public AudioCommand audioCommand;
            public RangeSettings pitch;
            public RangeSettings volume;
        }

        [SerializeField] private AudioSource _audioSource;

        [SerializeField] private AudioSettings[] _onCollisionEnter;
        [SerializeField] private AudioSettings[] _onCollisionExit;

        [SerializeField] private AudioSettings[] _onTriggerEnter;
        [SerializeField] private AudioSettings[] _onTriggerExit;

        private class LoopingAudio
        {
            public RangeSettings pitch;
            public RangeSettings volume;
            public AudioSource source;
        }

        private Dictionary<AudioClip, LoopingAudio> _loops = new Dictionary<AudioClip, LoopingAudio>();

        private void Awake()
        {
            if( _audioSource == null )
            {
                _audioSource = gameObject.AddComponent<AudioSource>();
                GameAudio.ConfigureWithDefaultSettings(_audioSource);
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            HandleInteraction(_onCollisionEnter, collision.collider);
        }

        private void OnCollisionExit(Collision collision)
        {
            HandleInteraction(_onCollisionExit, collision.collider);
        }

        private void OnTriggerEnter(Collider collider)
        {
            HandleInteraction(_onTriggerEnter, collider);
        }

        private void OnTriggerExit(Collider collider)
        {
            HandleInteraction(_onTriggerExit, collider);
        }

        private void HandleInteraction(AudioSettings[] audio, Collider collider)
        {
            if (audio == null || audio.Length == 0) return;
            
            LoopingAudio loop;

            for(int i=0;i<audio.Length;i++)
            {
                AudioSettings settings = audio[i];
                if( (settings.layer.value & 1 << collider.gameObject.layer) == 0 ) continue;

                if( settings.audioCommand == AudioCommand.PlayOneShot )
                {
                    _audioSource.pitch = settings.pitch.range.random;
                    _audioSource.PlayOneShot(settings.clip);
                }
                else
                {
                    _loops.TryGetValue(settings.clip, out loop);

                    if ( settings.audioCommand == AudioCommand.Stop )
                    {
                        if( loop == null ) continue;
                        else loop.source.Stop();
                    }
                    else if( settings.audioCommand == AudioCommand.PlayLoop )
                    {
                        if( loop != null && loop.source.isPlaying ) continue;

                        if( loop == null )
                        {
                            loop = new LoopingAudio();
                            loop.source = gameObject.AddComponent<AudioSource>();
                            GameAudio.ConfigureWithDefaultSettings(loop.source);
                            loop.source.clip = settings.clip;
                            loop.source.loop = true;
                            loop.pitch = settings.pitch;
                            loop.volume = settings.volume;
                            _loops[settings.clip] = loop;
                        }

                        if( !loop.pitch.adjustFromSpeed )
                        {
                            loop.source.pitch = loop.pitch.range.random;
                        }

                        if (!loop.volume.adjustFromSpeed)
                        {
                            loop.source.volume = loop.volume.range.random;
                        }

                        loop.source.Play();
                    }
                }
            }
        }

    }

}