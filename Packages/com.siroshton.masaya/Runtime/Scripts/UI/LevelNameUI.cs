using Siroshton.Masaya.Core;
using TMPro;
using UnityEngine;

namespace Siroshton.Masaya.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class LevelNameUI : MonoBehaviour
    {
        [SerializeField] private float _duration = 3;
        [SerializeField] private float _fadeDuration = 2;
        [SerializeField] private TMP_Text _text;

        private enum Phase
        {
            Hidden,
            Showing,
            Fading
        }

        private CanvasGroup _canvasGroup;
        private Phase _phase = Phase.Hidden;

        private float _time;

        public void ShowName(string name)
        {
            if( _canvasGroup == null ) _canvasGroup = GetComponent<CanvasGroup>();

            gameObject.SetActive(true);
            _canvasGroup.alpha = 1;
            _phase = Phase.Showing;
            _time = 0;
            _text.text = name;
        }

        private void Update()
        {
            _time += GameState.deltaTime;

            if( _phase == Phase.Showing )
            {
                if( _time >= _duration )
                {
                    _phase = Phase.Fading;
                    _time = 0;
                }
            }
            else if( _phase == Phase.Fading )
            {
                if (_time >= _fadeDuration)
                {
                    _phase = Phase.Hidden;
                    gameObject.SetActive(false);
                    _time = _fadeDuration;
                }

                if(_fadeDuration > 0)
                {
                    _canvasGroup.alpha = 1.0f - _time / _fadeDuration;
                }
            }
        }
    }

}