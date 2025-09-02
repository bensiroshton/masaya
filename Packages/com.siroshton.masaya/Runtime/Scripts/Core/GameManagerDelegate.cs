using System;
using UnityEngine;

namespace Siroshton.Masaya.Core
{

    public class GameManagerDelegate : MonoBehaviour
    {

        [Obsolete("Use SceneLoader component.")]
        public void LoadScene(GameScenes.SceneName sceneName)
        {
            GameManager.instance.LoadScene(sceneName);
        }

        [Obsolete("Use SceneLoader component.")]
        public void LoadSubScene(GameScenes.SceneName sceneName)
        {
            GameManager.instance.LoadSubScene(sceneName);
        }

        [Obsolete("Use SceneLoader component.")]
        public void UnloadSubScene()
        {
            GameManager.instance.UnloadSubScene();
        }

    }

}