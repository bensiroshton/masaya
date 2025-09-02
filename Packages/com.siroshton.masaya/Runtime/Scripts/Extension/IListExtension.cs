using System.Collections.Generic;

namespace Siroshton.Masaya.Extension
{
    public static class IListExtension
    {
        /// <summary>
        /// Shuffles the element order of the specified list.
        /// </summary>
        // Original from now broken url: https://www.smooth-games.com/downloads/unity/smooth-foundations/smooth-foundations.unitypackage
        public static void Shuffle<T>(this IList<T> ts)
        {
            var count = ts.Count;
            var last = count - 1;
            for (var i = 0; i < last; ++i)
            {
                var r = UnityEngine.Random.Range(i, count);
                var tmp = ts[i];
                ts[i] = ts[r];
                ts[r] = tmp;
            }
        }
    }
}