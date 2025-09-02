using Siroshton.Masaya.Texture;
using UnityEditor;
using UnityEngine;

namespace Siroshton.Masaya.Editor.Texture
{

    [CustomEditor(typeof(VertexTextureBlend))]
    public class VertexTextureBlendrEditor : UnityEditor.Editor
    {
        //private SerializedProperty _layer;

        private void OnEnable()
        {
            //_layer = serializedObject.FindProperty("_layer");
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();


            //serializedObject.ApplyModifiedProperties();
        }
    }
}