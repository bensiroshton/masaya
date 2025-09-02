#if UNITY_EDITOR
using UnityEngine;

namespace Siroshton.Masaya.Audio
{
    // this is simply used to not touch the audio source using the copy audio menu command. (see Menu.cs)
    public class DontCopyAudioSettings : MonoBehaviour
    {
    }
}
#endif
