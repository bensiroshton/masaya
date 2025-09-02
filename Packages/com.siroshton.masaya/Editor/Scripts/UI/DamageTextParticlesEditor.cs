using Siroshton.Masaya.UI;
using System.ComponentModel;
using UnityEditor;
using UnityEngine;

namespace Siroshton.Masaya.Editor.UI
{
    [CustomEditor(typeof(DamageTextParticles))]
    public class DamageTextParticlesEditor : UnityEditor.Editor
    {
        int _testDamage;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            DamageTextParticles dt = target as DamageTextParticles;

            serializedObject.Update();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Test Number");
            _testDamage = EditorGUILayout.IntField(_testDamage);
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Test"))
            {
                dt.Editor_SpawnTestDamage(_testDamage);
            }

            serializedObject.ApplyModifiedProperties();
        }



    }
}

