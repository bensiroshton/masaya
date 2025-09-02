using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Siroshton.Masaya.Core
{
    public class GameEvents : MonoBehaviour
    {
        [SerializeField] private UnityEvent _onPlayerDeath;
        [SerializeField] private UnityEvent _onPlayerRevived;

        public UnityEvent onPlayerDeath { get => _onPlayerDeath; set => _onPlayerDeath = value; }
        public UnityEvent onPlayerRevived { get => _onPlayerRevived; set => _onPlayerRevived = value; }

        internal static void TriggerPlayerDeathEvents() // Triggered in Player.cs
        {
            GameEvents[] gameEvents = GameObject.FindObjectsByType<GameEvents>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            if (gameEvents != null)
            {
                for (int i = 0; i < gameEvents.Length; i++)
                {
                    GameEvents e = gameEvents[i];
                    if( e._onPlayerDeath != null ) e._onPlayerDeath.Invoke();
                }
            }
        }

        internal static void TriggerPlayerRevivedEvents() // Triggered in Player.cs
        {
            GameEvents[] gameEvents = GameObject.FindObjectsByType<GameEvents>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            if (gameEvents != null)
            {
                for (int i = 0; i < gameEvents.Length; i++)
                {
                    GameEvents e = gameEvents[i];
                    if (e._onPlayerRevived != null) e._onPlayerRevived.Invoke();
                }
            }
        }

    }
}