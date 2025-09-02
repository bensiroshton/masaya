using Siroshton.Masaya.Core;
using Siroshton.Masaya.Math;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace Siroshton.Masaya.Event
{

    public class ActionEvents : MonoBehaviour
    {
        [Serializable]
        public class Action
        {
            public string name;
            [Tooltip("When Auto Trigger is enabled the onTriggered event will be called immediately after onInterval, otherwise it is up to the user to trigger this action which in turn will fire the onTrigger event; this effectivly pauses the action until it is triggered.")]
            public bool autoTrigger;
            public bool onlyPlayWhenEngaged;
            public IntervalFloat interval;
            public UnityEvent onInterval;
            public UnityEvent onTriggerd;

            [HideInInspector] public float timeToInterval;
            [HideInInspector] public float nextInterval;
            [HideInInspector] public bool update;
            [HideInInspector] public bool isWaitingForTrigger;
        }

        [SerializeField] private Action[] _actions;

        private Creature.Creature _creature;

        private void Awake()
        {
            _creature = GetComponentInParent<Creature.Creature>();

            if (_actions != null)
            {
                for(int i=0;i<_actions.Length;i++)
                {
                    Action a = _actions[i];
                    a.nextInterval = a.interval.random;
                    a.update = true;
                    a.isWaitingForTrigger = false;
                }
            }
        }

        public void TriggerAction(string name)
        {
            for (int i = 0; i < _actions.Length; i++)
            {
                if( _actions[i].name == name )
                {
                    TriggerAction(i);
                    return;
                }
            }
        }

        public void TriggerAction(int index)
        {
            Action a = _actions[index];
            if( !a.isWaitingForTrigger ) return;

            a.onTriggerd?.Invoke();
            a.update = true;
            a.timeToInterval = 0;
            a.nextInterval = a.interval.random;
            a.isWaitingForTrigger = false;
        }

        private void Update()
        {
            if( _actions == null ) return;

            for (int i = 0; i < _actions.Length; i++)
            {
                Action a = _actions[i];
                if( a.update )
                {
                    a.timeToInterval += GameState.deltaTime;
                    if( a.timeToInterval >= a.nextInterval && !a.isWaitingForTrigger)
                    {
                        if( a.onlyPlayWhenEngaged && _creature != null && !_creature.isEngaged) continue;

                        a.isWaitingForTrigger = true;
                        a.update = false;
                        a.onInterval?.Invoke();

                        if( a.autoTrigger )
                        {
                            TriggerAction(i);
                        }
                    }
                }
            }

        }
    }

}