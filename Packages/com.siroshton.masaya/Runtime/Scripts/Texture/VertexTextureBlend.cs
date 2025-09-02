using System;
using UnityEngine;

namespace Siroshton.Masaya.Texture
{
    [RequireComponent(typeof(MeshFilter))]
    public class VertexTextureBlend : MonoBehaviour
    {
        [SerializeField, Range(0, 2)] private int _layer = 0;

        public UnityEngine.Mesh mesh => GetComponent<MeshFilter>().sharedMesh;

        public int currentLayer => _layer;
    }

}