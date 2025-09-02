#if UNITY_EDITOR
using UnityEngine;

namespace Siroshton.Masaya.Player
{
    [RequireComponent(typeof(Player))]
    public class PlayerDebug : MonoBehaviour
    {
        [SerializeField] GameObject[] _equipment;

        private Player _player;

        public GameObject[] equipment => _equipment;

        public Player player
        {
            get
            {
                if( _player == null ) _player = GetComponent<Player>();
                return _player;
            }
        }

    }

}

#endif