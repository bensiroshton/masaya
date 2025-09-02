using Siroshton.Masaya.Core;
using Siroshton.Masaya.Math;
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace Siroshton.Masaya.Environment
{
    [RequireComponent(typeof(SphereCollider))]
    public class Area : MonoBehaviour
    {
        public enum ActiveState
        {
            Unchanged,
            Active,
            Disabled
        }

        [Serializable]
        public class LevelLights
        {
            [Tooltip("Set the level lights when the player enters this area.")]
            public ActiveState activeState = ActiveState.Active;
            public bool fadeWithDistance = false;
            public float innerFadeRadius = 0;
        }

#pragma warning disable 414
        [SerializeField] private float _radiusExtension = 0;
#pragma warning restore 414
        [SerializeField] private bool _deactivateChildrenOnStartup = true;
        [SerializeField] private LevelLights _levelLights = new LevelLights();
        [Tooltip("Set the level audio when the player enters this area.")]
        [SerializeField] private ActiveState _levelAudioActiveState = ActiveState.Active;
        [Tooltip("Additional objects to disable/enable anytime the Area decides to do so.")]
        [SerializeField] private GameObject[] _additionalObjects;
        [SerializeField] private UnityEvent _onPlayerEnter;
        [SerializeField] private UnityEvent _onPlayerExit;

#if UNITY_EDITOR
        [Tooltip("This is used for moving the player during design, it is not used in release builds.")]
        [SerializeField] private Vector3 _playerSpawnPoint;
#endif

        private SphereCollider _sphereCollider;

#if UNITY_EDITOR
        public Vector3 playerSpawnPoint { get => _playerSpawnPoint; set => _playerSpawnPoint = value; }
        public Vector3 worldPlayerSpawnPoint { get => transform.position + (Vector3)(transform.localToWorldMatrix * _playerSpawnPoint); }
#endif

        public float radius { get => GetComponent<SphereCollider>().radius; }

        public static Area FindNearest(Vector3 point)
        {
            Collider[] areas = UnityEngine.Physics.OverlapSphere(point, 1, GameLayers.areaMask);
            if (areas.Length > 0)
            {
                return areas[0].GetComponent<Area>();
            }

            return null;
        }

        private void Awake()
        {
            _sphereCollider = GetComponent<SphereCollider>();
            if (_deactivateChildrenOnStartup) Deactivate();
        }

        public void Activate()
        {
            SetChildrenActive(true);

            if (_levelLights.activeState != ActiveState.Unchanged)
            {
                GameManager.instance.gameLevel.EnableLevelLights(_levelLights.activeState == ActiveState.Active);
            }

            if( !_levelLights.fadeWithDistance )
            {
                // set default intensity levels if we are not fading the lights.
                GameManager.instance.gameLevel.SetLightIntensity(1.0f);
            }

            if (_levelAudioActiveState != ActiveState.Unchanged)
            {
                GameManager.instance.gameLevel.EnableLevelAudio(_levelAudioActiveState == ActiveState.Active);
            }
        }

        public void Deactivate()
        {
            SetChildrenActive(false);
        }

        public void SetChildrenActive(bool active)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                transform.GetChild(i).gameObject.SetActive(active);
            }

            if(_additionalObjects != null)
            {
                for(int i = 0; i< _additionalObjects.Length; i++)
                {
                    _additionalObjects[i].SetActive(active);
                }
            }
        }

        private void Update()
        {
            if( _levelLights.fadeWithDistance )
            {
                Vector3 sphereCenter = transform.TransformPoint(_sphereCollider.center);
                float amount = MathUtil.GetNormalizedPosition(sphereCenter, _levelLights.innerFadeRadius, _sphereCollider.radius, Player.Player.instance.transform.position);
                GameManager.instance.gameLevel.SetLightIntensity(amount);
            }
        }

#if UNITY_EDITOR
        private static Door _lastDoor;

        public void Editor_TransportPlayer(Player.Player player)
        {
            if(_lastDoor != null) _lastDoor.Editor_PlayerLeft();
            _lastDoor = null;

            // use check point?
            Checkpoint checkPoint = GetComponentInChildren<Checkpoint>();
            if( checkPoint != null ) 
            {
                player.Transport(checkPoint.spawnPoint);
                return;
            }

            // use door?
            Door door = GetComponentInChildren<Door>();
            if ( door != null )
            {
                _lastDoor = door;
                door.Editor_TransportDirect(player);
                return;
            }

            // go to area spawn point
            Quaternion rotation = Quaternion.LookRotation(transform.position - worldPlayerSpawnPoint);
            player.Transport(worldPlayerSpawnPoint, rotation);
            
        }
#endif

        private void OnTriggerEnter(Collider other)
        {
            if( other.gameObject == Player.Player.instance.gameObject )
            {
#if UNITY_EDITOR
                _lastDoor = GetComponentInChildren<Door>();
#endif
                _onPlayerEnter?.Invoke();
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject == Player.Player.instance.gameObject)
            {
                _onPlayerExit?.Invoke();
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            GUIStyle style = GUI.skin.label;
            style.alignment = TextAnchor.MiddleCenter;
            UnityEditor.Handles.Label(transform.position + Vector3.up * 3, name, style);
        }

        private void OnDrawGizmosSelected()
        {
            if (_levelLights.fadeWithDistance && _levelLights.innerFadeRadius > 0)
            {
                SphereCollider sc = GetComponent<SphereCollider>();

                UnityEditor.Handles.color = Color.yellow;
                UnityEditor.Handles.DrawWireDisc(transform.TransformPoint(sc.center), Vector3.up, _levelLights.innerFadeRadius);
            }
        }
#endif

    }
}