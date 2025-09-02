
using System;
using UnityEngine;

namespace Siroshton.Masaya.Core
{

    static public class Types
    {
        public enum Direction1D
        {
            Forward,
            Backward
        }

        public enum BooleanAction
        {
            False,
            True,
            Unchanged
        }

        public enum Rarity
        {
            NotSet,
            Common,
            Uncommon,
            Rare,
            Legendary
        }

        public enum NextSelectionType
        {
            Incremental,
            Random,
            RandomButNotLast,
            PingPong
        }

        public enum Axis
        {
            X,
            Y,
            Z
        }

        [Flags]
        public enum AxisFlag
        {
            X = 1,
            Y = 2,
            Z = 4,
            XYZ = X | Y | Z
        }

        public enum OptionalAxis
        {
            None,
            X,
            Y,
            Z
        }

        public struct SimpleTransform
        {
            public Vector3 position;
            public Quaternion rotation;

            public SimpleTransform(Vector3 position, Quaternion rotation)
            {
                this.position = position;
                this.rotation = rotation;
            }

            public static SimpleTransform Slerp(SimpleTransform from, SimpleTransform to, float pos)
            {
                return new SimpleTransform(
                    Vector3.Lerp(from.position, to.position, pos),
                    Quaternion.Slerp(from.rotation, to.rotation, pos)
                );
            }
        }

        public struct TimeClip
        {
            public float start;     // time to start the clip
            public float end;       // time to end the clip
            public float duration;  // duration of the source that start and end are clipping

            public float normalizedStartTime => duration > 0 ? start / duration : 0;
            public float normalizedEndTime => duration > 0 ? end / duration : 0;
            public float clipDuration => end - start;

            /// <summary>
            /// Get the normalized position defined by start and end for a time value within the full source time range.
            /// </summary>
            /// <param name="time">time within the source</param>
            /// <returns>[0, 1] of where time falls between the start and end positions.</returns>
            public float GetClipPositionFromTime(float time)
            {
                if( time < start ) return 0;
                else if( time > end ) return 1;

                return (time - start) / clipDuration;
            }

            /// <summary>
            /// Get the normalized position defined by start and end for a normalized time value within the full source time range.
            /// </summary>
            /// <param name="normalizedTime">normalized time within the source</param>
            /// <returns>[0, 1] of where normalizedTime falls between the start and end positions.</returns>
            public float GetClipPositionFromNormalizedTime(float normalizedTime)
            {
                return GetClipPositionFromTime(normalizedTime * duration);
            }
        }

    }
}