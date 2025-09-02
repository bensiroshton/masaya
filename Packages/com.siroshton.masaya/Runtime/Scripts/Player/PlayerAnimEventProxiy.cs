using UnityEngine;
using UnityEngine.Events;

namespace Siroshton.Masaya.Player
{

    public class PlayerAnimEventProxy : MonoBehaviour
    {
        private Player _player;

        private void Start()
        {
            _player = Player.instance;
        }

        public void Anim_OnDeathFinished()
        {
            _player.Anim_OnDeathFinished();
        }

        public void Anim_OnReviveFinished()
        {
            _player.Anim_OnReviveFinished();
        }

    }

}