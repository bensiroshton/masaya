using UnityEngine;

namespace Siroshton.Masaya.Motion
{
    public interface IPath
    {
        public int pathPointCount { get; }
        public PathPoint GetPathPoint(int index);
    }
}