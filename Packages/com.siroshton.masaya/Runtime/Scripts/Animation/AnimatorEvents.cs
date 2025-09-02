using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

namespace Siroshton.Masaya.Animation
{
	public class AnimatorEvents : MonoBehaviour
	{
		[SerializeField] private Animator _animator;
        [SerializeField] private List<AnimationClipEvent> _clipEvents;

		public List<AnimationClipEvent> clipEvents { get => _clipEvents; set => _clipEvents = value; }

        private void Start()
		{
			if( _animator == null || _clipEvents == null || _clipEvents.Count == 0 )
			{
				enabled = false;
				return;
			}

			for(int i=0;i<_clipEvents.Count;i++)
			{
				AnimationClipEvent clipEvent = _clipEvents[i];
                AnimationClip clip = _animator.runtimeAnimatorController.animationClips[clipEvent.clipIndex];

				AnimationEvent evt = new AnimationEvent();
				evt.time = clipEvent.time;
				evt.intParameter = i;
				evt.functionName = "OnAnimationEvent";
                clip.AddEvent(evt);
            }
		}

        public void OnAnimationEvent()
		{
            //clipEvents[index].eventHandler.Invoke(clipEvents[index]);
            clipEvents[0].eventHandler.Invoke(clipEvents[0]);
        }


    }

}