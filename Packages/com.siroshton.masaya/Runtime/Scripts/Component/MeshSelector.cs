#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

namespace Siroshton.Masaya.Component
{
    [RequireComponent(typeof(MeshFilter))]
    public class MeshSelector : MonoBehaviour
    {
        [SerializeField] private string _assetFolder;

        public string meshPath
        {
            get
            {
                MeshFilter filter = GetComponent<MeshFilter>();
                return AssetDatabase.GetAssetPath(filter.sharedMesh);
            }
        }

        public UnityEngine.Mesh mesh
        {
            set
            {
                MeshFilter filter = GetComponent<MeshFilter>();
                Undo.RecordObject(filter, "Changing mesh.");

                MeshCollider collider = GetComponent<MeshCollider>();
                if( collider != null ) Undo.RecordObject(collider, "Changing mesh.");

                NavMeshObstacle obstacle = GetComponent<NavMeshObstacle>();
                if( obstacle != null) Undo.RecordObject(obstacle, "Changing mesh.");

                filter.sharedMesh = value;
                if( collider != null ) collider.sharedMesh = value;
                
                if( obstacle != null )
                {
                    // TODO: handle other shapes, this is just handling Box right now.
                    obstacle.size = value.bounds.size;
                    obstacle.center = value.bounds.center;
                }
            }
        }
    }

}
#endif
