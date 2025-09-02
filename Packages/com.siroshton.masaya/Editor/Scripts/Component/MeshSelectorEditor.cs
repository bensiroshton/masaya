using UnityEngine;
using UnityEditor;
using Siroshton.Masaya.Component;
using Siroshton.Masaya.Util;

namespace Siroshton.Masaya.Editor.Component
{
    [CustomEditor(typeof(MeshSelector))]
    public class MeshSelectorEditor : UnityEditor.Editor
    {
        private const int _thumbSize = 128;

        private SerializedProperty _assetFolder;
        private string _selectedAssetFolder = null;

        private void OnEnable()
        {
            _assetFolder = serializedObject.FindProperty("_assetFolder");
        }

        public override void OnInspectorGUI()
        {
            MeshSelector selector = serializedObject.targetObject as MeshSelector;

            serializedObject.Update();

            if(_selectedAssetFolder != null)
            {
                _assetFolder.stringValue = _selectedAssetFolder;
                _selectedAssetFolder = null;
            }

            // Selected Mesh Folder
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Mesh Folder", GUILayout.Width(EditorGUIUtility.labelWidth));
            if ( EditorGUILayout.DropdownButton(new GUIContent(_assetFolder.stringValue), FocusType.Keyboard) )
            {
                string[] paths = AssetDatabase.FindAssets("t:Mesh");
                for(int i=0;i<paths.Length;i++)
                {
                    paths[i] = AssetDatabase.GUIDToAssetPath(paths[i]);
                    if( !paths[i].StartsWith("Assets/") ) paths[i] = null;
                }

                paths = PathUtil.GetUniquePathsFromFiles(paths);

                GenericMenu menu = new GenericMenu();
                for (int i = 0; i < paths.Length; i++)
                {
                    string path = paths[i];
                    menu.AddItem(new GUIContent(path), path == _assetFolder.stringValue, OnAssetPathSelected, path);
                }

                menu.ShowAsContext();
            }
            EditorGUILayout.EndHorizontal();

            // Draw Meshes
            UnityEngine.Mesh randomMesh = null;
            if(_assetFolder.stringValue != "")
            {
                string[] guids = AssetDatabase.FindAssets("t:Mesh", new string[] { _assetFolder.stringValue });
                if( guids.Length > 0 )
                {
                    int cols = System.Math.Max(1, Mathf.FloorToInt(EditorGUIUtility.currentViewWidth / (float)_thumbSize));
                    EditorGUILayout.BeginVertical();
                    int iRandomMeshIndex = UnityEngine.Random.Range(0, guids.Length);
                    for (int i=0;i<guids.Length;i++)
                    {
                        if( i % cols == 0 )
                        {
                            if( i > 0 ) EditorGUILayout.EndHorizontal();
                            EditorGUILayout.BeginHorizontal();
                        }

                        string meshPath = AssetDatabase.GUIDToAssetPath(guids[i]);
                        UnityEngine.Mesh mesh = AssetDatabase.LoadAssetAtPath<UnityEngine.Mesh>(meshPath);
                        if( i == iRandomMeshIndex ) randomMesh = mesh;
                        Texture2D tex = AssetPreview.GetAssetPreview(mesh);
                        GUIStyle style = GUI.skin.button;
                        if (meshPath == selector.meshPath)
                        {
                            // TODO: change style to make it look selected
                        }

                        if (GUILayout.Button(tex, style, new GUILayoutOption[] { GUILayout.Width(_thumbSize), GUILayout.Height(_thumbSize) }))
                        {
                            selector.mesh = mesh;
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndVertical();
                }
            }

            // Randomize Buttons
            EditorGUILayout.LabelField("Randomize");
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Everything"))
            {
                selector.mesh = randomMesh;

                float scale = UnityEngine.Random.Range(0.5f, 1.5f);
                selector.gameObject.transform.localScale = new Vector3(scale, scale, scale);
                selector.gameObject.transform.localRotation = Quaternion.Euler(new Vector3(0, UnityEngine.Random.Range(0.0f, 360.0f), 0));

            }
            if ( GUILayout.Button("Scale + Rotation") )
            {
                float scale = UnityEngine.Random.Range(0.5f, 1.5f);
                selector.gameObject.transform.localScale = new Vector3(scale, scale, scale);
                selector.gameObject.transform.localRotation = Quaternion.Euler(new Vector3(0, UnityEngine.Random.Range(0.0f, 360.0f), 0));

            }
            if (GUILayout.Button("Scale"))
            {
                float scale = UnityEngine.Random.Range(0.5f, 1.5f);
                selector.gameObject.transform.localScale = new Vector3(scale, scale, scale);

            }
            if (GUILayout.Button("Rotation"))
            {
                selector.gameObject.transform.localRotation = Quaternion.Euler(new Vector3(0, UnityEngine.Random.Range(0.0f, 360.0f), 0));

            }
            EditorGUILayout.EndHorizontal();

            serializedObject.ApplyModifiedProperties();
        }

        private void OnAssetPathSelected(object path)
        {
            _selectedAssetFolder = (string)path;
        }
    }
}
