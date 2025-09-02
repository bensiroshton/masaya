using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Siroshton.Masaya.Component
{
    [RequireComponent(typeof(Collider))]
    public class SetActiveStateOnTrigger : EnableDisable
    {
        [Flags]
        public enum TriggerEvent
        {
            None = 0,
            Enter = 1,
            Exit = 2,
        }

        [SerializeField] private TriggerEvent _triggers = TriggerEvent.Enter;
        [SerializeField] private bool _stateOnTriggerEnter;
        [SerializeField] private bool _stateOnTriggerExit;

        private void OnTriggerEnter(Collider other)
        {
            if (!_triggers.HasFlag(TriggerEvent.Enter)) return;

            SetObjectState(_stateOnTriggerEnter);
        }

        private void OnTriggerExit(Collider other)
        {
            if (!_triggers.HasFlag(TriggerEvent.Exit)) return;

            SetObjectState(_stateOnTriggerExit);
        }

        private void SetObjectState(bool enabled)
        {
            SetEnabled(enabled);
        }

    }
}
