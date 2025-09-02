using Siroshton.Masaya.Attribute;
using UnityEditor;
using UnityEngine;

namespace Siroshton.Masaya.Editor.Attribute
{
    [CustomPropertyDrawer(typeof(InterfaceAttribute))]
    public class InterfaceAttributePropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.serializedObject.isEditingMultipleObjects) return;
            if (property.propertyType != SerializedPropertyType.ObjectReference) return;

            InterfaceAttribute interfaceAttribute = this.attribute as InterfaceAttribute;
            System.Type requiredType = interfaceAttribute.RequiredType;

            EditorGUI.BeginProperty(position, label, property);

            Object objRef = EditorGUI.ObjectField(position, label, property.objectReferenceValue, typeof(UnityEngine.Object), true);

            if( objRef != null )
            {
                if (objRef is GameObject o)
                {
                    objRef = o.GetComponent(requiredType);
                }
                else if ( !requiredType.IsAssignableFrom(objRef.GetType()) ) 
                {
                    objRef = null;
                }
            }

            property.objectReferenceValue = objRef;

            EditorGUI.EndProperty();
        }
    }

}