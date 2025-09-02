
using Siroshton.Masaya.Core;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Siroshton.Masaya.Environment
{
    public class Door : MonoBehaviour
    {
        public enum DoorType
        {
            DirectToOtherDoor,
            LoadScene,
            LoadSubScene,
            ReturnFromSubScene
        }

        [SerializeField] private DoorType _doorType = DoorType.DirectToOtherDoor;
        [Tooltip("This is where the player will be moved to once they have entered the door.")]
        [SerializeField] private Door _otherDoor;
        [SerializeField] private GameScenes.SceneName _scene;
        [SerializeField] private Transform _landingPoint;
        [SerializeField] private UnityEvent _onPlayerArrived;
        [SerializeField] private UnityEvent _onPlayerLeft;

        public Door otherDoor { get => _otherDoor; }
        public Transform landingPoint { get => _landingPoint; }

        public void OnTriggerEnter(Collider other)
        {
            UseDoor(other.gameObject.GetComponent<Player.Player>());
        }

        public void UseDoor(Player.Player player)
        {
            if( player == null ) return;

            PlayerLeft();

            if ( _doorType == DoorType.DirectToOtherDoor && _otherDoor != null )
            {
                Player.Player.instance.Transport(_otherDoor.landingPoint);
                _otherDoor.PlayerArrived();
            }
            else if( _doorType == DoorType.LoadScene )
            {
                GameManager.instance.LoadScene(_scene);
            }
            else if (_doorType == DoorType.LoadSubScene)
            {
                GameManager.instance.LoadSubScene(_scene, new GameLevelStack.Parameters(){
                    onSceneUnloaded = OnSceneUnloaded,
                    data = this
                });
            }
            else if (_doorType == DoorType.ReturnFromSubScene)
            {
                GameManager.instance.UnloadSubScene();
            }
        }

        private void OnSceneUnloaded(GameScenes.SceneName scene, System.Object data)
        {
            Door me = data as Door;
            Player.Player.instance.Transport(me.landingPoint);
        }

#if UNITY_EDITOR
        public void Editor_TransportDirect(Player.Player player)
        {
            player.Transport(landingPoint);
            PlayerArrived();
        }

        public void Editor_PlayerLeft()
        {
            PlayerLeft();
        }
#endif

        protected void PlayerArrived()
        {
            _onPlayerArrived?.Invoke();
        }

        protected void PlayerLeft()
        {
            _onPlayerLeft?.Invoke();
        }

    }

}