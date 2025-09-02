using UnityEngine;
using UnityEngine.Events;

namespace Siroshton.Masaya.Event
{

    public class SpendMana : MonoBehaviour
    {
        [SerializeField] private int _cost = 1;
        [SerializeField] private UnityEvent _onSpent;
        [SerializeField] private UnityEvent _onNotEnoughMana;

        public void TryToSpend()
        {
            if(Player.Player.instance.characterSheet.UseBottles(_cost))
            {
                _onSpent?.Invoke();
            }
            else
            {
                _onNotEnoughMana?.Invoke();
            }

        }
    }

}