using Siroshton.Masaya.Core;
using UnityEngine;

namespace Siroshton.Masaya.Audio
{

    public class GameAudioDelegate : MonoBehaviour
    {

        public void FadeOut(float duration)
        {
            GameManager.instance.gameAudio.FadeOut(duration);
        }

        public void FadeIn(float duration)
        {
            GameManager.instance.gameAudio.FadeIn(duration);
        }

    }
}