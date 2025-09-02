using Siroshton.Masaya.Math;
using Siroshton.Masaya.Mesh;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEngine;

namespace Siroshton.Masaya.Editor.Mesh
{
    [EditorTool("Prefab Painter", typeof(PrefabPainter))]
    public class PrefabPainterTool : EditorTool, IDrawSelectedHandles
    {
        /*
        class ToolOverlay : Overlay
        {
            public override VisualElement CreatePanelContent()
            {
                var root = new VisualElement();
                
                return root;
            }
        }
        */

        struct PaintTool
        {
            public GameObject selection;
            public UnityEngine.Mesh mesh;
            public bool hasHit;
            public RaycastHit hitInfo;
            public Ray brushRay;
            public Vector3 lastPaintPosition;
            public float flowSqr;
        }

        struct EraserTool
        {
            public Vector2 center;
            public Rect screenArea;
            public GUIStyle boxStyle;
        }

        struct UI
        {
            public GUIContent icon;
            public GUIContent[] brushes;
            public byte[][] brushIcons;
            public Rect windowRect;
        }


        private const string ObjectNodeName = "PaintedObjects";
        private Transform _objectNode;

        private PaintTool _paintTool = new PaintTool();
        private bool _isPainting = false;
        private EraserTool _eraserTool = new EraserTool();
        private bool _isErasing = false;

        private UI _ui;

        public override GUIContent toolbarIcon
        {
            get
            {
                if(_ui.icon == null)
                {
                    Texture2D tex = new Texture2D(1, 1);
                    ImageConversion.LoadImage(tex, PrefabPainterIcons.paintBrush, true);
                    _ui.icon = new GUIContent("Prefab Painter", tex, "Prefab Painter Tool");
                }

                return _ui.icon;
            }
        }

        private void OnEnable()
        {
            _ui.windowRect.Set(50, 10, 200, 60); // it will auto expand as we build out the controls

            _ui.brushIcons = new byte[2][];
            _ui.brushIcons[0] = PrefabPainterIcons.paintBrush;
            _ui.brushIcons[1] = PrefabPainterIcons.eraser;
            _ui.brushes = new GUIContent[2];
            _ui.brushes[0] = new GUIContent();
            _ui.brushes[1] = new GUIContent();

            _paintTool.selection = null;
            _paintTool.mesh = null;
            _paintTool.hasHit = false;
        }

        private void OnDisable()
        {
        }

        private Transform objectNode {
            get
            {
                if( _objectNode == null )
                {
                    PrefabPainter painter = target as PrefabPainter;
                    GameObject o = GameObject.Find(ObjectNodeName);
                    if( o != null ) _objectNode = o.transform;

                    if (_objectNode == null)
                    {
                        _objectNode = new GameObject(ObjectNodeName).transform;
                        _objectNode.localPosition = Vector3.zero;
                        _objectNode.localRotation = Quaternion.identity;
                        _objectNode.localScale = Vector3.one;
                    }
                }
                return _objectNode;
            }
        }

        public override void OnToolGUI(EditorWindow window)
        {
            if (window is not SceneView sceneView) return;

            PrefabPainter painter = target as PrefabPainter;

            // Draw Scene GUI
            Handles.BeginGUI();
            
            if (painter.brushMode == PrefabPainter.BrushMode.Erase)
            {
                if (_eraserTool.boxStyle == null || _eraserTool.boxStyle.normal.background == null)
                {
                    _eraserTool.boxStyle = new GUIStyle(GUIStyle.none);
                    _eraserTool.boxStyle.normal.background = new Texture2D(1, 1);
                    TextureUtil.ClearTexture(_eraserTool.boxStyle.normal.background, new Color32(255, 255, 255, 150));
                }

                GUI.Box(_eraserTool.screenArea, GUIContent.none, _eraserTool.boxStyle);
            }

            DrawGUI();
            Handles.EndGUI();

            // Prevent Unity from auto selecting a different object on mouse down
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));

            // Handle mouse events
            UnityEngine.Event evt = UnityEngine.Event.current;
            EditorGUI.BeginChangeCheck();
            bool hasUpdate = false;
            if( evt.isMouse )
            {
                if( painter.brushMode == PrefabPainter.BrushMode.Draw )
                {
                    UpdatePaintTool(evt.mousePosition);
                }
                else if( painter.brushMode == PrefabPainter.BrushMode.Erase )
                {
                    UpdateEraserTool(evt.mousePosition);
                }

                if( evt.button == 0 )
                {
                    if( evt.type == EventType.MouseDown)
                    {
                        if( painter.brushMode == PrefabPainter.BrushMode.Draw )
                        {
                            // start painting
                            _isPainting = PickPaintObject(evt.mousePosition);
                            if( _isPainting )
                            {
                                _paintTool.flowSqr = (1.0f - painter.paintTool.flow) * (1.0f - painter.paintTool.flow);
                                UpdatePaintTool(evt.mousePosition);
                                PaintObjects(true);
                            }
                            evt.Use();
                        }
                        else if( painter.brushMode == PrefabPainter.BrushMode.Erase )
                        {
                            _isErasing = true;
                            EraseObjects(evt.mousePosition);
                            evt.Use();
                        }
                    }
                    else if(evt.type == EventType.MouseDrag)
                    {
                        if(_isPainting)
                        {
                            // paint
                            PaintObjects(false);
                            hasUpdate = true;
                            evt.Use();
                        }
                        else if(_isErasing)
                        {
                            EraseObjects(evt.mousePosition);
                            hasUpdate = true;
                            evt.Use();
                        }
                    }
                    else if( evt.type == EventType.MouseUp )
                    {
                        // stop painting/erasing
                        _isPainting = false;
                        _isErasing = false;
                        evt.Use();
                    }
                    else if( evt.type == EventType.MouseMove )
                    {
                        // moving mouse without any button down.
                        hasUpdate = true;
                        evt.Use();
                    }
                }
            }
            EditorGUI.EndChangeCheck();

            // auto refresh if we made changes
            if( hasUpdate && !sceneView.sceneViewState.fxEnabled )
            {
                sceneView.sceneViewState.fxEnabled = true;
            }
        }

        public void DrawGUI()
        {
            PrefabPainter painter = target as PrefabPainter;

            for(int i=0;i<_ui.brushes.Length;i++)
            {
                if( _ui.brushes[i].image == null )
                {
                    _ui.brushes[i].image = new Texture2D(1, 1);
                    ImageConversion.LoadImage(_ui.brushes[i].image as Texture2D, _ui.brushIcons[i], true);
                }
            }

            GUILayout.BeginArea(_ui.windowRect, GUIContent.none, GUI.skin.box);
            EditorGUILayout.BeginVertical();

            EditorGUI.BeginChangeCheck();
            int mode = GUILayout.SelectionGrid((int)painter.brushMode, _ui.brushes, 2);
            if (EditorGUI.EndChangeCheck()) painter.brushMode = (PrefabPainter.BrushMode)mode;

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if (painter.brushMode == PrefabPainter.BrushMode.Draw)
            {
                GUILayout.Label("Radius");
                painter.paintTool.radius = EditorGUILayout.Slider(painter.paintTool.radius, 0, PrefabPainter.MaxBrushRadius);

            }
            else if (painter.brushMode == PrefabPainter.BrushMode.Erase)
            {
                GUILayout.Label("Eraser Size");
                EditorGUI.BeginChangeCheck();
                painter.eraserTool.size = EditorGUILayout.Slider(painter.eraserTool.size, 1, PrefabPainter.MaxEraserSize);
                if( EditorGUI.EndChangeCheck() ) UpdateEraserSize();
            }

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
            GUILayout.EndArea();
        }

        public void OnDrawHandles()
        {
            if(_paintTool.hasHit)
            {
                PrefabPainter painter = target as PrefabPainter;

                if(painter.paintTool.radius > 0)
                {
                    Handles.color = Color.white;
                    Handles.DrawWireDisc(_paintTool.hitInfo.point, _paintTool.hitInfo.normal, painter.paintTool.radius);
                }
                Handles.color = Color.red;
                Handles.DrawLine(_paintTool.hitInfo.point, _paintTool.hitInfo.point + _paintTool.hitInfo.normal.normalized * 0.5f);
            }
        }

        private bool PickPaintObject(Vector2 mousePosition)
        {
            PrefabPainter painter = target as PrefabPainter;
            if (painter.selectedBrush == null) return false;

            List<GameObject> ignoreList = new List<GameObject>(painter.ignoreList);
            bool tryAgain;

            GameObject picked;

            do
            {
                picked = HandleUtility.PickGameObject(mousePosition, false, ignoreList.ToArray());

                if (picked == null) return false;
                else if (picked == _paintTool.selection) return true;

                tryAgain = false;

                for (int i = 0; i < painter.brushes.Length; i++)
                {
                    // There has to be a better way to check instead of a string comparison.
                    PrefabPainter.Brush brush = painter.brushes[i];
                    string path = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(picked);
                    string assetPath = AssetDatabase.GetAssetPath(brush.prefab);
                    if( path == assetPath )
                    {
                        // ignore any object that is also one of the brushes
                        ignoreList.Add(picked);
                        tryAgain = true;
                        break;
                    }
                }
            }
            while(tryAgain);

            MeshFilter meshFilter = picked.GetComponentInChildren<MeshFilter>();
            if( meshFilter != null ) _paintTool.mesh = meshFilter.sharedMesh;
            else
            {
                SkinnedMeshRenderer skinnedMesh = picked.GetComponentInChildren<SkinnedMeshRenderer>();
                if( skinnedMesh != null ) _paintTool.mesh = skinnedMesh.sharedMesh;
            }
            
            if(_paintTool.mesh == null) return false;

            _paintTool.selection = picked;

            return true;
        }

        private void UpdatePaintTool(Vector2 mousePosition)
        {
            PrefabPainter painter = target as PrefabPainter;
            _paintTool.hasHit = false;

            if (!_isPainting || painter.paintTool.autoSwitchMesh || _paintTool.selection == null)
            {
                if (!PickPaintObject(mousePosition)) return;
            }

            _paintTool.brushRay = HandleUtility.GUIPointToWorldRay(mousePosition);
            RaycastHit? hit = HandleUtilityPrivate.IntersectRayMesh(_paintTool.mesh, _paintTool.brushRay, _paintTool.selection.transform.localToWorldMatrix);
            if (hit == null) return;

            _paintTool.hasHit = true;
            _paintTool.hitInfo = hit.Value;
        }

        private void PaintObjects(bool force)
        {
            if( _paintTool.selection == null ) return;

            // only place objects if we moved some amount or we are forcing it
            if(force || (_paintTool.lastPaintPosition - _paintTool.hitInfo.point).sqrMagnitude >= _paintTool.flowSqr)
            {
                PrefabPainter painter = target as PrefabPainter;

                Vector3 point = _paintTool.hitInfo.point;
                Vector3 normal = _paintTool.hitInfo.normal;

                if( painter.paintTool.radius > 0 )
                {
                    // find a random point within the radius.
                    Vector3 randomPoint = MathUtil.GetRandomPointOnDisc(point, normal, painter.paintTool.radius);
                    Ray newRay = new Ray(_paintTool.brushRay.origin, randomPoint - _paintTool.brushRay.origin);
                    RaycastHit? hit = HandleUtilityPrivate.IntersectRayMesh(_paintTool.mesh, newRay, _paintTool.selection.transform.localToWorldMatrix);
                    if (hit != null)
                    {
                        point = hit.Value.point;
                        normal = hit.Value.normal;
                    }
                }

                GameObject obj = PrefabUtility.InstantiatePrefab(painter.selectedBrush.prefab) as GameObject;
                Undo.RegisterCreatedObjectUndo(obj, $"Placed {obj.name}");

                switch( painter.paintTool.parentMode )
                {
                    case PrefabPainter.ParentMode.ParentToObject: obj.transform.SetParent(_paintTool.selection.transform); break;
                    default: obj.transform.SetParent(objectNode); break;
                }

                obj.transform.position = point;
                float scale = painter.selectedBrush.scale.random * painter.paintTool.scaleModifier;
                obj.transform.localScale = new Vector3(scale, scale, scale);

                if( painter.selectedBrush.maxUprightAdjustment > 0 )
                {
                    float angle = Vector3.Angle(normal, Vector3.up);
                    float adjustment = Mathf.Min(angle, painter.selectedBrush.maxUprightAdjustment);
                    normal = Vector3.RotateTowards(normal, Vector3.up, Mathf.Deg2Rad * adjustment, 1);
                }

                obj.transform.rotation 
                    = Quaternion.FromToRotation(Vector3.up, normal) 
                    * Quaternion.Euler(painter.selectedBrush.tiltAngle.random, painter.selectedBrush.yRotation.random, painter.selectedBrush.tiltAngle.random);

                PrefabUtility.RecordPrefabInstancePropertyModifications(obj.transform);

                _paintTool.lastPaintPosition = _paintTool.hitInfo.point;
            }

        }

        private void UpdateEraserTool(Vector2 mousePosition)
        {
            PrefabPainter painter = target as PrefabPainter;

            _eraserTool.center = mousePosition;
            UpdateEraserSize();
        }

        private void UpdateEraserSize()
        {
            PrefabPainter painter = target as PrefabPainter;

            float halfSize = painter.eraserTool.size / 2.0f;
            _eraserTool.screenArea.Set(_eraserTool.center.x - halfSize, _eraserTool.center.y - halfSize, painter.eraserTool.size, painter.eraserTool.size);
        }

        private void EraseObjects(Vector2 mousePosition)
        {
            PrefabPainter painter = target as PrefabPainter;

            GameObject[] picked = HandleUtility.PickRectObjects(_eraserTool.screenArea);

            // check each object if its one of our brush prefabs and if so destroy it.
            GameObject selectedObject;
            for(int oi = 0; oi < picked.Length; oi++)
            {
                selectedObject = picked[oi];

                // check against our brushes for a match
                for (int i = 0; i < painter.brushes.Length; i++)
                {
                    PrefabPainter.Brush brush = painter.brushes[i];
                    string path = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(selectedObject);
                    string assetPath = AssetDatabase.GetAssetPath(brush.prefab);
                    if (path == assetPath)
                    {
                        selectedObject = PrefabUtility.GetOutermostPrefabInstanceRoot(selectedObject);
                        Undo.DestroyObjectImmediate(selectedObject);
                        break;
                    }
                }
            }

        }
    }

}