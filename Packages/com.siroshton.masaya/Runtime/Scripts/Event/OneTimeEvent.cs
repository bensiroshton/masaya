using Siroshton.Masaya.Core;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace Siroshton.Masaya.Event
{

    public class OneTimeEvent : MonoBehaviour
    {
        [SerializeField] private string _id = System.Guid.NewGuid().ToString();
        [SerializeField] private UnityEvent _onEvent;

        public void TriggerEvent()
        {
            
            if( GameManager.instance.gameData.GetBool(_id) ) return;

            if ( _onEvent != null )
            {
                GameManager.instance.gameData.SetBool(_id, true);
                _onEvent.Invoke();
            }
        }

    }

}