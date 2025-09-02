using Siroshton.Masaya.Core;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace Siroshton.Masaya.Event
{
    public class EventProxy : MonoBehaviour
    {
        [SerializeField] private UnityEvent _onTrigger;

        public void TriggerEvent()
        {
            if( _onTrigger != null ) _onTrigger.Invoke();
        }
    }

}