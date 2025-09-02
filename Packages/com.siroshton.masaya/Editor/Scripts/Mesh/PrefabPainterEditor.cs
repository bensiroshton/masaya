using Siroshton.Masaya.Mesh;
using UnityEditor;
using UnityEngine;

namespace Siroshton.Masaya.Editor.Mesh
{

    [CustomEditor(typeof(PrefabPainter))]
    public class PrefabPainterEditor : UnityEditor.Editor
    {
        private SerializedProperty _brushes;

        private void OnEnable()
        {
            _brushes = serializedObject.FindProperty("_brushes");

        }

        public override void OnInspectorGUI()
        {
            PrefabPainter painter = target as PrefabPainter;

            if(painter.brushes != null && painter.brushes.Length > 0)
            {
                Texture2D[] images = new Texture2D[painter.brushes.Length];
                for(int i=0;i<painter.brushes.Length;i++)
                {
                    if(painter.brushes[i].prefab != null )
                    {
                        images[i] = AssetPreview.GetAssetPreview(painter.brushes[i].prefab);
                    }
                }

                painter.selectedIndex = GUILayout.SelectionGrid(painter.selectedIndex, images, 4);
            }

            DrawDefaultInspector();

            serializedObject.ApplyModifiedProperties();

        }
    }
}