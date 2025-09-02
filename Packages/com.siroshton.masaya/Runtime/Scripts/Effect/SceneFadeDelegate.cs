using Siroshton.Masaya.Core;
using UnityEngine;
using UnityEngine.Events;

namespace Siroshton.Masaya.Effect
{

    public class SceneFadeDelegate : MonoBehaviour
    {
        [SerializeField] private SceneFade.FadeEvents _events;

        public void FadeSceneOut()
        {
            GameManager.instance.sceneFader.FadeSceneOut(GameManager.instance.sceneFader.defaultDuration, _events);
        }

        public void FadeSceneOut(float duration)
        {
            GameManager.instance.sceneFader.FadeSceneOut(duration, _events);
        }

        public void FadeSceneIn()
        {
            GameManager.instance.sceneFader.FadeSceneIn(GameManager.instance.sceneFader.defaultDuration, _events);
        }

        public void FadeSceneIn(float duration)
        {
            GameManager.instance.sceneFader.FadeSceneIn(duration, _events);
        }

    }

}