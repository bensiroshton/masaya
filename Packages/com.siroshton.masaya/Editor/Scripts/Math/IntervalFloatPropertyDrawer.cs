using Siroshton.Masaya.Math;
using UnityEditor;
using UnityEngine;

namespace Siroshton.Masaya.Editor.Attribute
{
    [CustomPropertyDrawer(typeof(IntervalFloat))]
    public class IntervalFloatPropertyDrawer : PropertyDrawer
    {

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            int indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            float labelW = 30;
            float x = position.x;
            float w = (position.width - labelW) / 2;

            GUIStyle labelStyle = GUI.skin.label;
            labelStyle.alignment = TextAnchor.MiddleCenter;

            EditorGUI.PropertyField(new Rect(x, position.y, w, position.height), property.FindPropertyRelative("a"), GUIContent.none);
            x += w;

            EditorGUI.LabelField(new Rect(x, position.y, labelW, position.height), "to", labelStyle);
            x += labelW;

            EditorGUI.PropertyField(new Rect(x, position.y, w, position.height), property.FindPropertyRelative("b"), GUIContent.none);

            EditorGUI.EndProperty();

            EditorGUI.indentLevel = indent;
        }
 
    }

}