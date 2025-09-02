#if UNITY_EDITOR
using Siroshton.Masaya.Math;
using System;
using UnityEngine;

namespace Siroshton.Masaya.Mesh
{
    [RequireComponent(typeof(MeshFilter))]
    public class MeshBorderFill : MonoBehaviour
    {
        [Serializable]
        public struct ObjectDetails
        {
            public bool include;
            [Range(0, 2)] public float offset;
            [Range(0, 2)] public float width;
            public GameObject prefab;
            public IntervalFloat scale;
            public IntervalVec3 rotation;
            public bool scaleByEdgeDistance;
            [Range(0, 1)] public float density;
            public bool isStatic;
        }

        [SerializeField] private ObjectDetails[] _objects;

        public ObjectDetails[] objects { get => _objects; }
    }
}
#endif
