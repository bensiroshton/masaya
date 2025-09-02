using UnityEngine.SceneManagement;

namespace Siroshton.Masaya.Core
{
    public static class GameScenes
    {
        public enum SceneName
        {
            Unknown = -1,
            GameScene,
            Floor_01,
            Floor_02,
            DemonRoom
        }

        public static readonly string[] SceneNames = { 
            "GameScene",
            "Floor-01",
            "Floor-02",
            "DemonRoom"
        };

        public static string GetSceneName(SceneName scene)
        {
            if( scene == SceneName.Unknown ) return null;

            return SceneNames[(int)scene];
        }

        public static SceneName GetActiveSceneName()
        {
            Scene scene = SceneManager.GetActiveScene();
            return StringToEnum(scene.name);
        }

        public static SceneName StringToEnum(string name)
        {
            for (int i = 0; i < SceneNames.Length; i++)
            {
                if (SceneNames[i] == name) return (SceneName)i;
            }

            return SceneName.Unknown;
        }
    }
}