using Siroshton.Masaya.Core;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace Siroshton.Masaya.Animation
{
    public class AnimationSequence : MonoBehaviour
    {
        public enum State
        {
            NotRunning,
            DoingDelay,
            PlayingClip
        }

        public enum SequenceState
        {
            WaitingToStart,
            Started,
        }

        [Serializable]
        public class Sequence
        {
#if UNITY_EDITOR
            [HideInInspector] public string name;
#endif
            public AnimationClip clip;
            public string triggerName;
            public float startDelay;
            [Tooltip("When set no animation is played, this step will simply wait and then call the sequenceEnded event (sequenceStarted event will never be called.)")]
            public bool delayOnly;
            public UnityEvent sequenceStarted;
            public UnityEvent sequenceEnded;

            [HideInInspector] public int triggerId;
            [HideInInspector] public float delayTime;
            [HideInInspector] public SequenceState state;
            [HideInInspector] public bool hasStartTime;
        }

        private class EventParams : ScriptableObject
        {
            public AnimationSequence instance;
            public Sequence sequence;
        }

        [SerializeField] private Animator _animator;
        [SerializeField] private string _sequenceName;
        [SerializeField] private bool _allowTriggerBeforeFinished;
        [SerializeField] private Sequence[] _sequence;
        
        private int _sequenceIndex;
        private State _state;
        private float[] _startClipTime;

        private void Awake()
        {
            if( _animator == null ) _animator = GetComponentInChildren<Animator>();

            if( _sequence != null )
            {
                for(int i=0;i<_sequence.Length;i++)
                {
                    Sequence seq = _sequence[i];
                    seq.state = SequenceState.WaitingToStart;
                    seq.triggerId = Animator.StringToHash(seq.triggerName);
                }
            }
        }

        public void StartSequence()
        {
            if( _state != State.NotRunning && !_allowTriggerBeforeFinished ) return;

            StartSequence(0);
        }

        private void StartSequence(int index)
        {
            Sequence seq = _sequence[index];
            seq.delayTime = 0;
            seq.state = SequenceState.WaitingToStart;
            _sequenceIndex = index;
            _state = State.DoingDelay;
        }

        private void TriggerSequenceStarted()
        {
            //Debug.Log($"Sequence Started: {_sequence[_sequenceIndex].name}, time: {Time.realtimeSinceStartup}");

            Sequence seq = _sequence[_sequenceIndex];
            seq.sequenceStarted?.Invoke();
            seq.state = SequenceState.Started;
            seq.hasStartTime = false;
        }

        private void TriggerSequenceEnded()
        {
            //Debug.Log($"Sequence Ended: {_sequence[_sequenceIndex].name}, time: {Time.realtimeSinceStartup}");

            _sequence[_sequenceIndex].sequenceEnded?.Invoke();

            if (_sequenceIndex < _sequence.Length - 1)
            {
                StartSequence(_sequenceIndex + 1);
            }
            else
            {
                _state = State.NotRunning;
            }
        }

        public void Update()
        {
            if( _sequence == null || _state == State.NotRunning ) return;

            Sequence seq = _sequence[_sequenceIndex];

            if ( _state == State.PlayingClip )
            {
                AnimatorClipInfo[] clipInfo = _animator.GetCurrentAnimatorClipInfo(0);
                AnimatorStateInfo info = _animator.GetCurrentAnimatorStateInfo(0);

                if( clipInfo[0].clip == seq.clip )
                {
                    if ( !seq.hasStartTime )
                    {
                        _startClipTime = AnimUtil.SplitNormalizedTime(info.normalizedTime);
                        seq.hasStartTime = true;
                    }
                    else
                    {
                        float[] normTime = AnimUtil.SplitNormalizedTime(info.normalizedTime);
                        if( normTime[0] > _startClipTime[0] )
                        {
                            TriggerSequenceEnded();
                        }
                    }
                }
                else if(seq.hasStartTime && seq.state == SequenceState.Started )
                {
                    TriggerSequenceEnded();
                }
            }
            else if( _state == State.DoingDelay )
            {
                seq.delayTime += GameState.deltaTime;
                if(seq.delayTime >= seq.startDelay )
                {
                    if( seq.delayOnly )
                    {
                        TriggerSequenceEnded();
                    }
                    else
                    {
                        _animator.SetTrigger(seq.triggerId);
                        TriggerSequenceStarted();
                        _state = State.PlayingClip;
                    }
                }
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if( _sequence == null ) return;

            for(int i=0;i<_sequence.Length;i++)
            {
                Sequence seq = _sequence[i];
                seq.name = seq.clip != null ? $"[{i}] {seq.clip.name}" : $"[{i}]";
            }
        }
#endif
    }
}