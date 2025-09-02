using Siroshton.Masaya.Mesh;
using System;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEngine;

namespace Siroshton.Masaya.Editor.Mesh
{
    [CustomEditor(typeof(MeshBorderFill))]
    public class MeshBorderFillEditor : UnityEditor.Editor
    {
        private const string FillTransformName = "Border Fill";
        
        private SerializedProperty _objects;

        private bool _showEdgeDetails;
        private bool _showObjectCounts;
        private bool _showFillProperties;

        private struct Edge
        {
            public Vector3 p1;
            public Vector3 p2;
            public Vector3 insideNormal;
            public Vector3 center { get => p1 * 0.5f + p2 * 0.5f; }

            private float _length;

            public float length { get => _length; }

            public Edge(Vector3 p1, Vector3 p2)
            {
                this.p1 = p1;
                this.p2 = p2;
                this.insideNormal = Vector3.zero;

                _length = (p2 - p1).magnitude;
            }
        }

        static private UnityEngine.Mesh _mesh;
        static private List<Edge> _edges;
        private float _totalLength;

        private void OnEnable()
        {
            _objects = serializedObject.FindProperty("_objects");
            _showEdgeDetails = true;
            _showObjectCounts = true;
            _showFillProperties = true;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            MeshBorderFill meshBorderFill = target as MeshBorderFill;
            MeshFilter filter = meshBorderFill.GetComponent<MeshFilter>();
            if( filter == null ) return;

            if( filter.sharedMesh != _mesh )
            {
                _mesh = filter.sharedMesh;
                CalculateEdges();
            }
            else if (_edges == null) return;

            _showEdgeDetails = EditorGUILayout.Foldout(_showEdgeDetails, "Edge Details", true, EditorStyles.foldoutHeader);
            if (_showEdgeDetails)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel("Edge Count");
                EditorGUILayout.LabelField($"{_edges.Count}");
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel("Total Length");
                EditorGUILayout.LabelField($"{_totalLength}");
                EditorGUILayout.EndHorizontal();
            }

            Transform t = meshBorderFill.transform.Find(FillTransformName);
            if( t != null )
            {
                _showObjectCounts = EditorGUILayout.Foldout(_showObjectCounts, "Object Counts", true, EditorStyles.foldoutHeader);
                if(_showObjectCounts )
                {
                    int total = 0;
                    for (int i = 0; i < t.childCount; i++)
                    {
                        Transform c = t.GetChild(i);
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.PrefixLabel(c.name);
                        EditorGUILayout.LabelField($"{c.childCount}");
                        EditorGUILayout.EndHorizontal();
                        total += c.childCount;
                    }
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PrefixLabel("Total");
                    EditorGUILayout.LabelField($"{total}");
                    EditorGUILayout.EndHorizontal();
                }
            }

            _showFillProperties = EditorGUILayout.Foldout(_showFillProperties, "Border Fill", true, EditorStyles.foldoutHeader);
            if (_showFillProperties)
            {
                EditorGUILayout.PropertyField(_objects);
            }

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Recalculate Edges"))
            {
                _mesh = null;
            }

            if (GUILayout.Button("Fill"))
            {
                FillBorder(meshBorderFill);
            }

            if (GUILayout.Button("Clear"))
            {
                ClearBorder(meshBorderFill);
            }
            EditorGUILayout.EndHorizontal();

            serializedObject.ApplyModifiedProperties();
        }

        private void CalculateEdges()
        {
            Debug.Log("Calculating edges (can be slow)...");

            // Weld Vertices
            //int merged = 0;
            //float minSqrDist = 0.000001f * 0.000001f;

            int[] triangles = new int[_mesh.triangles.Length];
            Array.Copy(_mesh.triangles, triangles, triangles.Length);

            /*
            for (int i = 0; i < triangles.Length; i++)
            {
                Vector3 v1 = _mesh.vertices[triangles[i]];

                for(int vi = 0; vi < _mesh.vertices.Length; vi++)
                {
                    if( vi == triangles[i] ) continue;

                    Vector3 v2 = _mesh.vertices[vi];
                    if( (v2 - v1).sqrMagnitude <= minSqrDist ) 
                    {
                        triangles[i] = vi;
                        merged++;
                        break;
                    }
                }
            }

            Debug.Log($"Merged {merged} vertices.");
            */

            // Find edges that only appear in a single triangle
            _edges = new List<Edge>();
            for (int i = 0; i < triangles.Length; i += 3)
            {
                // store the triangle edges (first two elements, the 3rd is the non edge vertice used to calculate the edge normal)
                int[] ab = new int[] { triangles[i], triangles[i + 1], triangles[i + 2] };
                int[] bc = new int[] { triangles[i + 1], triangles[i + 2], triangles[i] };

                for (int i2 = 0; i2 < triangles.Length; i2 += 3)
                {
                    if( i == i2 ) continue;

                    int[] vi2 = new int[]{
                        triangles[i2],
                        triangles[i2 + 1],
                        triangles[i2 + 2],
                    };

                    int count;

                    if( ab != null )
                    {
                        count = 0;
                        if ( ab[0] == vi2[0] || ab[0] == vi2[1] || ab[0] == vi2[2] ) count++;
                        if ( ab[1] == vi2[0] || ab[1] == vi2[1] || ab[1] == vi2[2] ) count++;
                        if ( count == 2 ) ab = null;
                    }

                    if( bc != null )
                    {
                        count = 0;
                        if ( bc[0] == vi2[0] || bc[0] == vi2[1] || bc[0] == vi2[2] ) count++;
                        if ( bc[1] == vi2[0] || bc[1] == vi2[1] || bc[1] == vi2[2] ) count++;
                        if ( count == 2 ) bc = null;
                    }

                    if( ab == null && bc == null ) break;
                }

                if( ab != null )
                {
                    Edge e = new Edge(_mesh.vertices[ab[0]], _mesh.vertices[ab[1]]);
                    
                    Plane facePlane = new Plane(e.p1, e.p2, _mesh.vertices[ab[2]]);
                    Plane edgePlane = new Plane(e.p1, e.p2, e.center + facePlane.normal);
                    if (edgePlane.GetSide(_mesh.vertices[ab[2]])) e.insideNormal = edgePlane.normal;
                    else e.insideNormal = -edgePlane.normal;
                    
                    _edges.Add(e);
                }

                if (bc != null)
                {
                    Edge e = new Edge(_mesh.vertices[bc[0]], _mesh.vertices[bc[1]]);

                    Plane facePlane = new Plane(e.p1, e.p2, _mesh.vertices[bc[2]]);
                    Plane edgePlane = new Plane(e.p1, e.p2, e.center + facePlane.normal);
                    if (edgePlane.GetSide(_mesh.vertices[bc[2]])) e.insideNormal = edgePlane.normal;
                    else e.insideNormal = -edgePlane.normal;

                    _edges.Add(e);
                }
            }

            Debug.Log($"Found {_edges.Count} edges.");
        }

        private void ClearBorder(MeshBorderFill meshBorderFill)
        {
            Transform t = meshBorderFill.transform.Find(FillTransformName);
            if( t != null ) DestroyImmediate( t.gameObject );
        }

        private void FillBorder(MeshBorderFill meshBorderFill)
        {
            if( _edges == null ) return;

            MeshBorderFill.ObjectDetails[] objects = meshBorderFill.objects;
            if ( objects.Length == 0 ) return;

            Transform t = meshBorderFill.transform.Find(FillTransformName);
            if (t == null)
            {
                GameObject o = new GameObject(FillTransformName);
                o.transform.SetParent(meshBorderFill.transform);
                o.transform.localPosition = Vector3.zero;
                o.transform.localRotation = Quaternion.identity;
                t = o.transform;

                NavMeshModifier mod = o.AddComponent<NavMeshModifier>();
                mod.applyToChildren = true;
                mod.ignoreFromBuild = true;
            }

            for (int di = 0; di < objects.Length; di++)
            {
                MeshBorderFill.ObjectDetails od = objects[di];
                if (!od.include) continue;
                else if (od.density <= 0) continue;

                //GameObject obj = GameObject.Instantiate(od.prefab);
                GameObject obj = PrefabUtility.InstantiatePrefab(od.prefab) as GameObject;
                MeshFilter filter = obj.GetComponentInChildren<MeshFilter>();
                float objArea = filter.sharedMesh.bounds.size.x * filter.sharedMesh.bounds.size.z;
                Vector3 avgScale = new Vector3(od.scale.center, od.scale.center, od.scale.center);
                objArea *= (avgScale.x + avgScale.z) / 2.0f;
                if( objArea == 0 )
                {
                    Debug.LogWarning($"Not adding any {od.prefab.name} objects, its calculated area is zero (check your scale range).");
                    continue;
                }

                GameObject group = new GameObject(od.prefab.name);
                group.transform.SetParent(t);
                group.transform.localPosition = Vector3.zero;
                group.transform.localRotation = Quaternion.identity;

                for (int ei = 0; ei < _edges.Count; ei++)
                {
                    Edge edge = _edges[ei];
                    float edgeArea = edge.length * od.width;
                    float edgeRotation = Vector3.SignedAngle(Vector3.forward, edge.insideNormal, Vector3.up);
                    int count = (int)(edgeArea / objArea * od.density);

                    if( count <= 0 )
                    {
                        if( obj != null ) DestroyImmediate(obj);
                        continue;
                    }

                    for(int oi = 0; oi < count; oi++)
                    {
                        if( oi > 0 || obj == null )
                        {
                            //obj = GameObject.Instantiate(od.prefab);
                            obj = PrefabUtility.InstantiatePrefab(od.prefab) as GameObject;
                        }

                        Vector3 p = Vector3.Lerp(edge.p1, edge.p2, UnityEngine.Random.Range(0.0f, 1.0f));
                        p += edge.insideNormal * od.offset;

                        float distanceFromEdge = UnityEngine.Random.Range(0, od.width);
                        p += edge.insideNormal * distanceFromEdge;
                 
                        Vector3 rotation = od.rotation.random;

                        obj.isStatic = od.isStatic;
                        obj.transform.SetParent(group.transform);
                        obj.transform.localPosition = p;
                        obj.transform.localRotation = Quaternion.Euler(rotation.x, edgeRotation + rotation.y, rotation.z);                        
                        if ( od.scaleByEdgeDistance )
                        {
                            Vector3 scaleA = new Vector3(od.scale.a, od.scale.a, od.scale.a);
                            Vector3 scaleB = new Vector3(od.scale.b, od.scale.b, od.scale.b);

                            if ( od.width > 0 ) obj.transform.localScale = Vector3.Lerp(scaleA, scaleB, 1.0f - distanceFromEdge / od.width);
                            else obj.transform.localScale = scaleA;
                        }
                        else
                        {
                            float scale = od.scale.random;
                            obj.transform.localScale = new Vector3(scale, scale, scale);
                        }
                        
                        if( obj.transform.localScale.magnitude == 0 )
                        {
                            Debug.LogWarning($"Not adding {obj.name}, created with zero scale.");
                            DestroyImmediate(obj);
                        }
                    }
                }
            }
        }

        [DrawGizmo(GizmoType.Selected | GizmoType.Active)]
        private static void DrawGizmos(MeshBorderFill meshBorderFill, GizmoType gizmoType)
        {
            if( _edges == null ) return;
            int objLenth = meshBorderFill.objects != null ? meshBorderFill.objects.Length : 0;

            Handles.matrix = meshBorderFill.transform.localToWorldMatrix;
            Vector3 p1, p2, center;

            Color c1 = Color.cyan;
            Color c2 = Color.red;
            float colorPos;
            float colorStep = 1.0f / ((float)objLenth + 1);
            float normPos;

            for (int ei=0; ei<_edges.Count; ei++)
            {
                colorPos = 0;
                Handles.color = Color.Lerp(c1, c2, colorPos);
                colorPos += colorStep;

                Edge edge = _edges[ei];
                p1 = edge.p1;
                p2 = edge.p2;
                Handles.DrawLine(p1, p2, 2);

                for(int oi=0;oi<meshBorderFill.objects?.Length;oi++)
                {
                    Handles.color = Color.Lerp(c1, c2, colorPos);
                    colorPos += colorStep;

                    MeshBorderFill.ObjectDetails od = meshBorderFill.objects[oi];

                    p1 = edge.p1 + edge.insideNormal * od.offset;
                    p2 = edge.p2 + edge.insideNormal * od.offset;
                    Handles.DrawLine(p1, p2, 2);

                    normPos = (float)oi / (float)objLenth;
                    normPos = Mathf.Lerp(0.25f, 0.75f, normPos);
                    center = Vector3.Lerp(p1, p2, normPos);
                    Handles.DrawLine(center, center + edge.insideNormal * od.width, 2);
                }

            }
        }

    }
}
