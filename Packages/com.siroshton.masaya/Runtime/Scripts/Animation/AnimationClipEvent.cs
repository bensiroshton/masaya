using System;
using UnityEngine;
using UnityEngine.Events;

namespace Siroshton.Masaya.Animation
{
    public class AnimationClipEvent : ScriptableObject
    {
        [SerializeField] private int _clipIndex;
        [SerializeField] private float _time;
        [SerializeField] private UnityEvent<AnimationClipEvent> _event = new UnityEvent<AnimationClipEvent>();

        public int clipIndex { get => _clipIndex; set => _clipIndex = value; }
        public float time { get => _time; set => _time = value; }
        public UnityEvent<AnimationClipEvent> eventHandler { get => _event; set => _event = value; }

        public AnimationClipEvent()
        {

        }

        public AnimationClipEvent(int clipIndex)
        {
            _clipIndex = clipIndex;
        }
    }
}