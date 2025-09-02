
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Siroshton.Masaya.Core
{
    [DisallowMultipleComponent]
    public class GameStateHandler : MonoBehaviour
    {
        private Behaviour[] _behaviours;
        private bool[] _behavioursEnabled;

        private ParticleSystem[] _particles;
        private bool[] _particleActive;

        protected void Awake()
        {
            GameManager.instance.onPauseStateChange.AddListener(OnPauseStateChange);
        }

        private void OnPauseStateChange(bool paused)
        {
            if( !gameObject.activeSelf ) return;

            if( paused )
            {
                _behaviours = GetComponentsInChildren<Behaviour>(false);
                if( _behaviours.Length > 0 )
                {
                    _behavioursEnabled = new bool[_behaviours.Length];
                    for (int i = 0; i < _behaviours.Length; i++)
                    {
                        if( _behaviours[i] == null ) continue;

                        if (_behaviours[i] is AudioSource audio)
                        {
                            _behavioursEnabled[i] = audio.isPlaying;
                        }
                        else
                        {
                            _behavioursEnabled[i] = _behaviours[i].enabled;
                        }
                    }
                }
                else
                {
                    _behavioursEnabled = null;
                }

                _particles = GetComponentsInChildren<ParticleSystem>(false);
                if( _particles.Length > 0 )
                {
                    _particleActive = new bool[_particles.Length];
                    for (int i=0;i<_particles.Length;i++) _particleActive[i] = _particles[i].isPlaying;
                } 
                else
                {
                    _particleActive = null;
                }
            }

            if (_behaviours != null)
            {
                for(int i=0;i<_behaviours.Length;i++)
                {
                    if (_behaviours[i] == null) continue;

                    if (_behaviours[i] is AudioSource audio)
                    {
                        if (paused) audio.Pause();
                        else if (_behavioursEnabled[i]) audio.Play();
                    }
                    else
                    {
                        _behaviours[i].enabled = !paused && _behavioursEnabled[i];
                    }
                }
            }

            if (_particles != null)
            {
                for (int i = 0; i < _particles.Length; i++)
                {
                    if( paused ) _particles[i].Pause(true);
                    else if(_particleActive[i]) _particles[i].Play(true);
                }
            }
        }
    }

}