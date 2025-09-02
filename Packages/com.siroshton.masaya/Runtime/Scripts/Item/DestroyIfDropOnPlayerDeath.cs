using System;
using UnityEngine;

namespace Siroshton.Masaya.Item
{

    public class DestroyIfDropOnPlayerDeath : MonoBehaviour
    {
        [SerializeField] private bool _isDrop;

        public bool isDrop { get => _isDrop; set => _isDrop = value; }

        private void Awake()
        {
            Player.Player.instance.onPlayerRevived.AddListener(OnPlayerRevived);
        }

        private void OnPlayerRevived()
        {
            if( isDrop ) GameObject.Destroy(gameObject);
        }
    }

}