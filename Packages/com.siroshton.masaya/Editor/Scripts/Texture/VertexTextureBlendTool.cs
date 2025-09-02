using Siroshton.Masaya.Texture;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEngine;

namespace Siroshton.Masaya.Editor.Texture
{
    [EditorTool("Vertex Texture Blend", typeof(VertexTextureBlend))]
    public class VertexTextureBlendTool : EditorTool, IDrawSelectedHandles
    {
        private static string TempNodeName = "EditorNode";

        private float _brushRadius = 1;
        private float _brushWeight = 1;
        private bool _isPainting = false;

        struct HitInfo
        {
            public RaycastHit hitInfo;
            public bool hasHit;
        }

        private MeshCollider _collider;
        private GameObject _tempObject;
        private HitInfo _hit = new HitInfo();

        private void OnEnable()
        {
            VertexTextureBlend tb = target as VertexTextureBlend;
            Transform t = tb.transform.Find(TempNodeName);
            if( t == null )
            {
                _tempObject = new GameObject(TempNodeName);
                _tempObject.transform.SetParent(tb.transform);
                _tempObject.transform.localPosition = Vector3.zero;
                _tempObject.transform.localRotation = Quaternion.identity;
                _tempObject.transform.localScale = Vector3.one;

                _collider = _tempObject.AddComponent<MeshCollider>();
                _collider.cookingOptions = MeshColliderCookingOptions.None;
                _collider.sharedMesh = tb.mesh;
            }
            else
            {
                _tempObject = t.gameObject;
                _collider = _tempObject.GetComponent<MeshCollider>();
            }
        }

        private void OnDisable()
        {
            //DestroyImmediate(_tempObject);
            //_tempObject = null;
        }

        public override void OnToolGUI(EditorWindow window)
        {
            if (window is not SceneView sceneView) return;

            VertexTextureBlend tb = target as VertexTextureBlend;
            UnityEngine.Mesh mesh = tb.mesh;
            if( mesh.vertexCount == 0 ) return;


            Handles.BeginGUI();
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();

                GUILayout.Label("Radius");
                _brushRadius = EditorGUILayout.Slider(_brushRadius, 0.1f, 3.0f);
                GUILayout.Label("Weight");
                _brushWeight = EditorGUILayout.Slider(_brushWeight, 0, 1);

                GUILayout.FlexibleSpace();
            }
            Handles.EndGUI();

            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));

            UnityEngine.Event evt = UnityEngine.Event.current;
            EditorGUI.BeginChangeCheck();
            bool hasUpdate = false;
            if( evt.isMouse )
            {
                if( evt.button == 0 )
                {
                    if( evt.type == EventType.MouseDown)
                    {
                        // start painting
                        _isPainting = true;
                        evt.Use();
                    }
                    else if(evt.type == EventType.MouseDrag && _isPainting)
                    {
                        // paint
                        //Debug.Log(evt.mousePosition);
                        //ProcessVertices(sceneView.camera.ScreenPointToRay(evt.mousePosition), mesh);
                        ProcessVertices(HandleUtility.GUIPointToWorldRay(evt.mousePosition), mesh);
                        hasUpdate = true;
                        evt.Use();
                    }
                    else if( evt.type == EventType.MouseUp )
                    {
                        // stop painting
                        _isPainting = false;
                        evt.Use();
                    }
                    else if( evt.type == EventType.MouseMove )
                    {
                        // moving mouse without any button down.
                        ProcessVertices(HandleUtility.GUIPointToWorldRay(evt.mousePosition), mesh);
                        hasUpdate = true;
                        evt.Use();
                    }
                }
            }
            EditorGUI.EndChangeCheck();

            if( hasUpdate && !sceneView.sceneViewState.fxEnabled )
            {
                sceneView.sceneViewState.fxEnabled = true;
            }


        }

        public void OnDrawHandles()
        {
            if(_hit.hasHit)
            {
                Handles.color = Color.white;
                Handles.DrawWireDisc(_hit.hitInfo.point, _hit.hitInfo.normal, _brushRadius);
                Handles.color = Color.red;
                Handles.DrawLine(_hit.hitInfo.point, _hit.hitInfo.point + _hit.hitInfo.normal * _brushRadius);
            }
        }

        private void ProcessVertices(Ray brushRay, UnityEngine.Mesh mesh)
        {
            VertexTextureBlend tb = target as VertexTextureBlend;

            int layer = 3; //tb.currentLayer;
            _hit.hasHit = _collider.Raycast(brushRay, out _hit.hitInfo, 1000);
            if( !_hit.hasHit ) return;

            if( _isPainting )
            {
                Vector3 p;
                float d2;
                float r2 = _brushRadius * _brushRadius;
                float strength;
                Color c;

                for(int i=0;i<mesh.vertices.Length;i++)
                {
                    p = tb.transform.TransformPoint(mesh.vertices[i]);
                    d2 = (p - _hit.hitInfo.point).sqrMagnitude;
                    if ( d2 >= r2 )
                    {
                        // paint this vertice
                        strength = d2 / r2 * _brushWeight;
                        c = mesh.colors[i];
                        c[layer] += strength;
                        if( c[layer] > 1 ) c[layer] = 1;
                        // todo: balance other layers
                        mesh.colors[i] = c;
                    }
                }

                mesh.UploadMeshData(false);
            }

        }
    }

}