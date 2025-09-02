using Siroshton.Masaya.Math;
using System;
using UnityEngine;
using static Siroshton.Masaya.Core.Types;

namespace Siroshton.Masaya.Animation
{

    public static class AnimUtil
    {
        /// <summary>
        /// Returns the "events" property for a clipo if it is found.
        /// </summary>
        /// <param name="animator">Animator to search.</param>
        /// <param name="clipName">Name of the clip, a non case sensitive comparison is performed.</param>
        /// <returns>AnimationEvent[] or null if the clip was not found.</returns>
        public static AnimationEvent[] GetEvents(Animator animator, string clipName)
        {
            AnimationClip clip = null;
            return GetEvents(animator, clipName, ref clip);
        }
        /// <summary>
        /// Returns the "events" property for a clipo if it is found.
        /// </summary>
        /// <param name="animator">Animator to search.</param>
        /// <param name="clipName">Name of the clip, a non case sensitive comparison is performed.</param>
        /// <returns>AnimationEvent[] or null if the clip was not found.</returns>
        public static AnimationEvent[] GetEvents(Animator animator, string clipName, ref AnimationClip clip)
        {
            foreach (AnimationClip c in animator.runtimeAnimatorController.animationClips)
            {
                if (c.name.Equals(clipName, System.StringComparison.CurrentCultureIgnoreCase))
                {
                    clip = c;
                    return clip.events;
                }
            }

            return null;
        }

        /// <summary>
        /// Noramlized time is Unity's AnimatorStateInfo.normalizedTime pramter where the whole number is the times the
        /// animation has looped and the decimal is the position within the animation [0, 1].  This function splits the
        /// two values and returns them in an array.  The first element is the loop count, the second is the position.
        /// </summary>
        /// <param name="normalizedTime">AnimatorStateInfo.normalizedTime</param>
        /// <returns>float[2]{ loopCount, position }</returns>
        public static float[] SplitNormalizedTime(float normalizedTime)
        {
            float[] parts = new float[2];
            parts[0] = Mathf.Floor(normalizedTime);
            parts[1] = normalizedTime - parts[0];
            return parts;
        }

        /// <summary>
        /// Returns the normalized time without the loop component. (ie., the decimals only with any whole numbers removed.)
        /// </summary>
        /// <param name="normalizedTime"></param>
        /// <returns>Normazlied Time.</returns>
        public static float GetNormalizedTime(float normalizedTime)
        {
            return normalizedTime - Mathf.Floor(normalizedTime);
        }

        /// <summary>
        /// Returns animation clip timing information.
        /// </summary>
        /// <param name="animator"></param>
        /// <param name="clipName"></param>
        /// <param name="interval"></param>
        /// <param name="startFunction"></param>
        /// <param name="endFunction"></param>
        /// <returns></returns>
        public static bool GetClipRange(Animator animator, string clipName, out TimeClip clipTime, Action startFunction = null, Action endFunction = null)
        {
            clipTime = new TimeClip();
            clipTime.start = clipTime.end = -1;

            AnimationClip clip = null;
            AnimationEvent[] events = AnimUtil.GetEvents(animator, clipName, ref clip);

            if (events == null)
            {
                Debug.LogError($"Unable to find animation clip: {clipName}");
                return false;
            }

            clipTime.duration = clip.length;

            foreach (AnimationEvent e in events)
            {
                if (startFunction != null && e.functionName == startFunction.Method.Name) clipTime.start = e.time;
                else if (endFunction != null && e.functionName == endFunction.Method.Name) clipTime.end = e.time;
            }

            return clipTime.start >= 0 && clipTime.end >= 0;
        }
    }

}