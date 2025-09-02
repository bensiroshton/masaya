using Siroshton.Masaya.Effect;
using Siroshton.Masaya.Util;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Siroshton.Masaya.Core
{
    [Serializable]
    public class GameLevelStack
    {
        public delegate void LevelDelegate(GameLevel level, System.Object data);
        public delegate void SceneDelegate(GameScenes.SceneName scene, System.Object data);

        [Serializable]
        public struct Parameters
        {
            public SceneDelegate onSceneLoading;
            public LevelDelegate onLevelLoaded;
            public LevelDelegate onLevelActive;
            public LevelDelegate onLevelUnloading;
            public SceneDelegate onSceneUnloaded;
            public System.Object data;

            public static Parameters GetDefault()
            {
                return new Parameters() {
                };
            }
        }

        private struct SceneLevel
        {
            public Scene scene;
            public GameLevel level;
            public GameObject[] activeRoots;
            public Parameters parameters;
        }

        [SerializeField] private Parameters _defaults;
        [SerializeField] private SceneFade _sceneFader;

        private Stack<SceneLevel> _levels = new Stack<SceneLevel>();
        private SceneLevel _active;
        private SceneLevel _loading;

        # region Public Properties
        public GameLevel activeLevel => _active.level;
        public Scene activeScene => _active.scene;
        public int count => _levels.Count;

        public SceneDelegate onSceneLoading { get => _defaults.onSceneLoading; set => _defaults.onSceneLoading = value; }
        public SceneDelegate onSceneUnloaded { get => _defaults.onSceneUnloaded; set => _defaults.onSceneUnloaded = value; }

        public LevelDelegate onLevelLoaded { get => _defaults.onLevelLoaded; set => _defaults.onLevelLoaded = value; }
        public LevelDelegate onLevelActive { get => _defaults.onLevelActive; set => _defaults.onLevelActive = value; }
        public LevelDelegate onLevelUnloading { get => _defaults.onLevelUnloading; set => _defaults.onLevelUnloading = value; }

        public SceneFade sceneFader => _sceneFader;
        # endregion

        public GameLevelStack()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.sceneUnloaded += OnSceneUnloaded;
        }

#if UNITY_EDITOR
        public bool Editor_PushActive()
        {
            Scene scene = SceneManager.GetActiveScene();
            if( !scene.isLoaded ) return false;

            if( scene.name == "GameScene" )
            {
                Debug.LogError("GameScene should not be the active scene.");
                Log.Println($"GameLevelStack.Editor_PushActive - GameScene should not be the active scene.");
                return false;
            }

            Log.Println($"GameLevelStack.Editor_PushActive, calling OnSceneLoaded {scene.name}");

            OnSceneLoaded(scene, LoadSceneMode.Additive);
            return true;
        }
#endif

        public bool PushLevel(GameScenes.SceneName sceneName)
        {
            return PushLevel(sceneName, Parameters.GetDefault());
        }

        public bool PushLevel(GameScenes.SceneName sceneName, Parameters parameters)
        {
            string strname = GameScenes.GetSceneName(sceneName);

            Scene scene = SceneManager.GetSceneByName(strname);
            if (scene.isLoaded)
            {
                Log.Println($"{scene.name} is already loaded");
                return false;
            }

            _loading.scene = scene;
            _loading.parameters = parameters;

            if (parameters.onSceneLoading != null) parameters.onSceneLoading(sceneName, parameters.data);
            if (_defaults.onSceneLoading != null) _defaults.onSceneLoading(sceneName, _defaults.data);

            Log.Println($"Loading {strname}");
            SceneManager.LoadSceneAsync(strname, LoadSceneMode.Additive);

            return true;
        }

        public void PopLevel()
        {
            if( _levels.Count == 0 ) return;

            Log.Println($"GameLevelStack.PopLevel {_levels.Peek().scene.name}");

            if (_active.parameters.onLevelUnloading != null) _active.parameters.onLevelUnloading(_active.level, _active.parameters.data);
            if (_defaults.onLevelUnloading != null) _defaults.onLevelUnloading(_active.level, _defaults.data);

            SceneManager.UnloadSceneAsync(_active.scene);
        }

        private void SetRootsActive(SceneLevel sceneLevel, bool active)
        {
            for (int i = 0; i < sceneLevel.activeRoots.Length; i++)
            {
                if( sceneLevel.activeRoots[i] != null )
                {
                    sceneLevel.activeRoots[i].SetActive(active);
                }
            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Log.Println($"GameLevelStack.OnSceneLoaded {scene.name}");

            if( scene.name == GameScenes.GetSceneName(GameScenes.SceneName.GameScene) )
            {
                Log.Println($"GameLevelStack.OnSceneLoaded ignoring GameScene loaded.");
                return;
            }

            // deactivate last scene
            if ( _levels.Count > 0 )
            {
                SetRootsActive(_levels.Peek(), false);
            }

            // activate newly loaded scene
            SceneManager.SetActiveScene(scene);

            GameObject[] roots = scene.GetRootGameObjects();

            _active = _loading;
            _active.scene = scene;
            _active.level = null;
            _active.activeRoots = new GameObject[roots.Length];

            for (int i = 0; i < roots.Length; i++)
            {
                if (_active.level == null) _active.level = roots[i].GetComponent<GameLevel>();
                if (roots[i].activeSelf)
                {
                    _active.activeRoots[i] = roots[i];
                }
            }

            _levels.Push(_active);

            if (_active.parameters.onLevelLoaded != null) _active.parameters.onLevelLoaded(_active.level, _active.parameters.data);
            if (_defaults.onLevelLoaded != null) _defaults.onLevelLoaded(_active.level, _defaults.data);

            if (_active.parameters.onLevelActive != null) _active.parameters.onLevelActive(_active.level, _active.parameters.data);
            if (_defaults.onLevelActive != null) _defaults.onLevelActive(_active.level, _defaults.data);

            if(_sceneFader != null)
            {
                _sceneFader.sceneViewValue = 0;
                _sceneFader.FadeSceneIn();
            }
        }

        private void OnSceneUnloaded(Scene scene)
        {
            if( string.IsNullOrEmpty(scene.name) )
            {
                Log.Println($"GameLevelStack.OnSceneUnloaded scene name is empty.  ignoring.");
                return;
            }

            Log.Println($"GameLevelStack.OnSceneUnloaded {scene.name}");

            SceneLevel sceneLevel = _levels.Pop();
            GameScenes.SceneName sceneName = GameScenes.StringToEnum(sceneLevel.scene.name);

            if (sceneLevel.parameters.onSceneUnloaded != null) sceneLevel.parameters.onSceneUnloaded(sceneName, sceneLevel.parameters.data);
            if (_defaults.onSceneUnloaded != null) _defaults.onSceneUnloaded(sceneName, _defaults.data);

            if ( _levels.Count > 0 )
            {
                _active = _levels.Peek();
                SceneManager.SetActiveScene(_active.scene);
                SetRootsActive(_active, true);

                if (_active.parameters.onLevelActive != null) _active.parameters.onLevelActive(_active.level, _active.parameters.data);
                if (_defaults.onLevelActive != null) _defaults.onLevelActive(_active.level, _defaults.data);
            }
            else
            {
                _active = new SceneLevel();
            }
        }

    }

}