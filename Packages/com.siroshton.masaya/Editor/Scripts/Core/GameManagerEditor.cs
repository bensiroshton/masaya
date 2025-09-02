using Siroshton.Masaya.Core;
using System.Collections.Generic;
using UnityEditor;

namespace Siroshton.Masaya.Editor.Player
{
    [CustomEditor(typeof(GameManager))]
    public class GameManagerEditor : UnityEditor.Editor
    {
        private SerializedProperty _gameData;

        private void OnEnable()
        {
            _gameData = serializedObject.FindProperty("_gameData");
        }

        public override void OnInspectorGUI()
        {
            GameManager gm = target as GameManager;
            DrawDefaultInspector();

            List<string> keys = gm.gameData.GetBoolKeys();
            foreach(string key in keys)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel(key);
                bool value = gm.gameData.GetBool(key);
                bool newValue = EditorGUILayout.Toggle(value);
                if( value != newValue ) gm.gameData.SetBool(key, value);
                EditorGUILayout.EndHorizontal();
            }
        }
    }
}

