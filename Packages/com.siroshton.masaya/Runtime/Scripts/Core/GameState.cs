using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Siroshton.Masaya.Core
{
#if UNITY_EDITOR
    [InitializeOnLoadAttribute]
#endif
    public static class GameState
    {
        private static bool _isPaused;
        private static float _timeScale = 1.0f;

#if UNITY_EDITOR
        static GameState()
        {
            EditorApplication.playModeStateChanged += PlayModeStateChanged;
        }

        private static void PlayModeStateChanged(PlayModeStateChange state)
        {
            if( state == PlayModeStateChange.EnteredPlayMode && _isPaused )
            {
                Debug.LogWarning("Unpausing on Editor PlayModeStateChange, we probably exited play mode while paused.");
                GameManager_SetPaused(false);
            }
        }
#endif

        /// <summary>
        /// This should only be called by the GameManager.
        /// </summary>
        /// <param name="paused"></param>
        internal static void GameManager_SetPaused(bool paused)
        {
            _isPaused = paused;
            _timeScale = _isPaused ? 0 : 1;
        }

        public static bool isPaused { get => _isPaused; }

        public static float deltaTime
        { 
            get
            {
                return Time.deltaTime * _timeScale;
            }
        }

        public static float fixedDeltaTime
        {
            get
            {
                return Time.fixedDeltaTime * _timeScale;
            }
        }

    }

}