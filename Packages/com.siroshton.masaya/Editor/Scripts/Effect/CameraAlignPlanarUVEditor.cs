using Siroshton.Masaya.Core;
using Siroshton.Masaya.Effect;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Siroshton.Masaya.Editor.Effects
{
    [CustomEditor(typeof(CameraAlignPlanarUV))]
    public class CameraAlignPlanarUVEditor : UnityEditor.Editor
    {
        private SerializedProperty _renderer;

        private void OnEnable()
        {
            _renderer = serializedObject.FindProperty("_renderer");
        }
    
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            CameraAlignPlanarUV cap = target as CameraAlignPlanarUV;

            EditorGUILayout.PropertyField(_renderer);

            if ( GUILayout.Button("Fit Camera") )
            {
                cap.FitCamera();
            }

            serializedObject.ApplyModifiedProperties();

        }
    }
}

