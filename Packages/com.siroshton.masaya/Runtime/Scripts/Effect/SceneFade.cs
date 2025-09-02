using Siroshton.Masaya.Core;
using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Siroshton.Masaya.Effect
{
    [RequireComponent(typeof(Volume))]
    public class SceneFade : MonoBehaviour
    {
        [SerializeField, Range(0, 1)] private float _sceneViewValue = 1;
        [SerializeField, Range(0, 5)] private float _defaultDuration = 3.0f;
        [SerializeField] private bool _autoEnableDisable = true;

        [Serializable]
        public class FadeEvents
        {
            public UnityEvent onFinished;
        }

        private struct Fade
        {
            public bool isFading;
            public float start;
            public float target;
            public float time;
            public float duration;
        }

        private ColorAdjustments _colorAdj;
        private Color _baseColor;
        private Fade _fade;
        private FadeEvents _fadeEvents;

        public float defaultDuration { get => _defaultDuration; set => _defaultDuration = value; }

        /// <summary>
        /// Range [0..1], 1 has the scene in full view, 0 is black.
        /// </summary>
        public float sceneViewValue
        {
            get => _sceneViewValue;
            set
            {
                _sceneViewValue = value;
                _colorAdj.colorFilter.value = Color.Lerp(Color.black, _baseColor, value);
            }
        }

        private void Awake()
        {
            Volume v = GetComponent<Volume>();
            v.profile.TryGet<ColorAdjustments>(out _colorAdj);
            _baseColor = _colorAdj.colorFilter.value;
            sceneViewValue = _sceneViewValue;
            if(_autoEnableDisable) enabled = _fade.isFading;
        }

        public void FadeSceneIn()
        {
            FadeSceneIn(_defaultDuration);
        }

        public void FadeSceneIn(float duration, FadeEvents fadeEvents = null)
        {
            FadeSceneTo(1, duration);
        }

        public void FadeSceneOut()
        {
            FadeSceneOut(_defaultDuration);
        }

        public void FadeSceneOut(float duration, FadeEvents fadeEvents = null)
        {
            FadeSceneTo(0, duration);
        }

        public void FadeSceneTo(float targetSceneView, float duration, FadeEvents fadeEvents = null)
        {
            _fadeEvents = fadeEvents;

            if (duration <= 0)
            {
                _fade.isFading = false;
                sceneViewValue = targetSceneView;
                if (_autoEnableDisable) enabled = false;
                _fadeEvents?.onFinished?.Invoke();
            }
            else
            {
                _fade.isFading = true;
                _fade.start = sceneViewValue;
                _fade.target = targetSceneView;
                _fade.time = 0;
                _fade.duration = duration;
                if (_autoEnableDisable) enabled = true;
            }
        }

        private void Update()
        {
            if (_fade.isFading)
            {
                _fade.time += GameState.deltaTime;
                if (_fade.time >= _fade.duration)
                {
                    _fade.time = _fade.duration;
                    _fade.isFading = false;
                    if (_autoEnableDisable) enabled = false;
                    _fadeEvents?.onFinished?.Invoke();
                    _fadeEvents = null;
                }
                sceneViewValue = Mathf.Lerp(_fade.start, _fade.target, _fade.time / _fade.duration);
            }
        }

    }

}