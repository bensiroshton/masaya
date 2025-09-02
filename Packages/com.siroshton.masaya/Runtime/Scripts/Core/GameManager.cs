using Siroshton.Masaya.Effect;
using Siroshton.Masaya.Environment;
using Siroshton.Masaya.Item;
using Siroshton.Masaya.UI;
using Siroshton.Masaya.Util;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Siroshton.Masaya.Core
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private GameUIController _ui;
        [SerializeField] private GameObject _npcMessagePrefab;
        [SerializeField] private UnityEvent<bool> _onPauseStateChange = new UnityEvent<bool>();
        [SerializeField] private GameData _gameData = new GameData();
        [SerializeField, Range(0, 5)] private float _interactibleSearchRadius = 2.0f;
        [SerializeField] private LayerMask _interactibleLayerMask;
#pragma warning disable CS0414
        [SerializeField] private GameScenes.SceneName _startScene = GameScenes.SceneName.Floor_01;
#pragma warning restore CS0414
        [SerializeField] private GameLevelStack _levelStack = new GameLevelStack();

        private DamageTextParticles _damageText;
        private GameAudio _gameAudio;
        private GameLevelStack.SceneDelegate _onSceneLoaded;
        private GameLevelStack.SceneDelegate _onSceneUnloaded;

        static private GameManager _instance;
    
        static public GameManager instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject o = GameObject.Find("GameManager");
                    if (o == null) Debug.LogError("Unable to find object `GameManager`.");
                    _instance = o.GetComponent<GameManager>();
                    if (_instance == null) Debug.LogError("`GameManager` GameObject is missing 'GameManager' Component.");
                }
                return _instance;
            }
        }

        public UnityEvent<bool> onPauseStateChange { get => _onPauseStateChange; set => _onPauseStateChange = value; }
        public DamageTextParticles damageText => _damageText;
        public GameData gameData => _gameData;
        public GameLevel gameLevel => _levelStack.activeLevel;
        public SceneFade sceneFader => _levelStack.sceneFader;
        public GameAudio gameAudio => _gameAudio;
        public GameUIController ui => _ui;
        public GameLevelStack.SceneDelegate onSceneLoaded { get => _onSceneLoaded; set => _onSceneLoaded = value; }
        public GameLevelStack.SceneDelegate onSceneUnloaded { get => _onSceneUnloaded; set => _onSceneUnloaded = value; }

        private void Awake()
        {
            if (_instance == null) _instance = this;

#if !UNITY_EDITOR
            Cursor.visible = false;
#endif
            _gameAudio = GetComponent<GameAudio>();
            _damageText = GetComponentInChildren<DamageTextParticles>();
            _ui.gameObject.SetActive(true);
            _ui.ShowPauseUI(false);

            _levelStack.onLevelActive += OnLevelActive;
            _levelStack.onLevelLoaded += OnLevelLoaded;
            _levelStack.onLevelUnloading += OnLevelUnloading;
            _levelStack.onSceneLoading += OnSceneLoading;
            _levelStack.onSceneUnloaded += OnSceneUnloaded;
        }

        private void Start()
        {
#if UNITY_EDITOR
            if(SceneManager.loadedSceneCount > 1)
            {
                _levelStack.Editor_PushActive();
            }
            else
            {
                LoadScene(_startScene);
            }
#else
            LoadScene(_startScene);
#endif
        }

        public void LoadScene(GameScenes.SceneName sceneName)
        {
            LoadScene(sceneName, GameLevelStack.Parameters.GetDefault());
        }

        public void LoadScene(GameScenes.SceneName sceneName, GameLevelStack.Parameters parameters)
        {
            _levelStack.PopLevel();
            LoadSubScene(sceneName, parameters);
        }

        public void LoadSubScene(GameScenes.SceneName sceneName)
        {
            LoadSubScene(sceneName, GameLevelStack.Parameters.GetDefault());
        }

        public void LoadSubScene(GameScenes.SceneName sceneName, GameLevelStack.Parameters parameters)
        {
            _levelStack.PushLevel(sceneName, parameters);
        }

        public void UnloadSubScene()
        {
            if( _levelStack.count > 1 )
            {
                _levelStack.PopLevel();
            }
        }

        private void OnSceneLoading(GameScenes.SceneName scene, System.Object data)
        {
            Log.Println($"GameManager.OnSceneLoading {scene}");

            Player.Player.instance.gameObject.SetActive(false);
        }

        private void OnLevelLoaded(GameLevel level, System.Object data)
        {
            Log.Println($"GameManager.OnLevelLoaded {level}");

            InitLevel();
            if (_onSceneLoaded != null) _onSceneLoaded(GameScenes.GetActiveSceneName(), data);
        }

        private void OnLevelActive(GameLevel level, System.Object data)
        {
            Log.Println($"GameManager.OnLevelActive {level}");

            Player.Player.instance.gameObject.SetActive(true);
        }

        private void OnLevelUnloading(GameLevel level, System.Object data)
        {
            Log.Println($"GameManager.OnLevelUnloading {level}");

            Player.Player.instance.gameObject.SetActive(false);
            if (_onSceneLoaded != null) _onSceneLoaded(GameScenes.GetActiveSceneName(), data);
        }

        private void OnSceneUnloaded(GameScenes.SceneName scene, System.Object data)
        {
            Log.Println($"GameManager.OnSceneUnloaded {scene}");

            if (_onSceneUnloaded != null) _onSceneUnloaded(scene, data);
        }

        private void InitLevel()
        {
            Log.Println($"GameManager.InitLevel");

            Player.Player.instance.gameObject.SetActive(true);
            Player.Player.instance.SetPlaying();
            Player.Player.instance.Transport(gameLevel.playerStartPoint);
            _instance._gameAudio.volume = 1;
        }

        public List<GameObject> TryDropItems(Vector3 atPosition, ItemDropOptions p)
        {
            if( gameLevel != null ) return gameLevel.itemDrops.TryDropItems(atPosition, p);
            else return null;
        }

        public void OnPauseButton(InputAction.CallbackContext context)
        {
            if( !context.started ) return;

            SetPaused(!GameState.isPaused);
        }

        public void OnInteractButton(InputAction.CallbackContext context)
        {
            if (!context.started) return;

            Vector3 playerPos = Player.Player.instance.transform.position;
            Collider[] colliders = UnityEngine.Physics.OverlapSphere(playerPos, _interactibleSearchRadius, _interactibleLayerMask);
            
            if( colliders.Length == 0 ) return;
            else if( colliders.Length > 1 ) Array.Sort<Collider>(colliders, (a, b) => { return (a.transform.position - playerPos).sqrMagnitude.CompareTo((b.transform.position - playerPos).sqrMagnitude); });

            for(int i=0;i<colliders.Length;i++)
            {
                IInteractible inter = colliders[i].GetComponentInChildren<IInteractible>();
                if( inter != null && inter.isReadyForInteraction )
                {
                    inter.TriggerInteraction();
                    break;
                }
            }
        }

        public void SetPaused(bool paused)
        {
            if( GameState.isPaused == paused ) return;
            else if( Player.Player.instance.isDead ) return;
            else if( Player.Player.instance.isWaiting ) return;
            
            GameState.GameManager_SetPaused(paused);

            _onPauseStateChange.Invoke(paused);
            _ui.ShowPauseUI(paused);
        }

        public MessageOverlayUI ShowNPCMessage(MessageOverlay overlay)
        {
            return _ui.ShowNPCMessage(overlay);
        }

        public MessageOverlayUI ShowNPCMessage(Transform attachTo, string message, bool showButton)
        {
            return _ui.ShowNPCMessage(attachTo, message, showButton);
        }
    }
}
