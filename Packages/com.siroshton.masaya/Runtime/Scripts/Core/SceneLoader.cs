using UnityEngine;

namespace Siroshton.Masaya.Core
{
    public class SceneLoader : MonoBehaviour
    {
        public enum LoadType
        {
            Scene,
            SubScene
        }

        [SerializeField] private LoadType _loadType = LoadType.Scene;
        [SerializeField] private GameScenes.SceneName _scene;

        public void LoadScene()
        {
            if( _loadType == LoadType.Scene )
            {
                GameManager.instance.LoadScene(_scene);
            }
            else if (_loadType == LoadType.SubScene)
            {
                GameManager.instance.LoadSubScene(_scene);
            }
        }
    }
}