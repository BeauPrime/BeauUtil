using System;
using UnityEditor;
using UnityEngine;

namespace BeauUtil.Editor
{
    [CustomPropertyDrawer(typeof(ConditionalVisibleFieldAttribute), true)]
    internal sealed class ConditionalVisiblePropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (ShouldDisplay(property))
            {
                label = EditorGUI.BeginProperty(position, label, property);
                EditorGUI.PropertyField(position, property, label, true);
                EditorGUI.EndProperty();
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (ShouldDisplay(property))
            {
                return EditorGUI.GetPropertyHeight(property);
            }

            return 0;
        }

        private bool ShouldDisplay(SerializedProperty property)
        {
            ConditionalVisibleFieldAttribute attr = (ConditionalVisibleFieldAttribute) attribute;
            string propName = attr.PropertyName;
            SerializedProperty conditionalProp = property.FindPropertySibling(propName);
            if (conditionalProp == null)
            {
                Debug.LogError("No property with name '" + propName + " located");
                return false;
            }

            if (conditionalProp.hasMultipleDifferentValues)
            {
                return false;
            }

            bool bBool;

            switch(conditionalProp.propertyType)
            {
                case SerializedPropertyType.Boolean:
                default:
                    bBool = conditionalProp.boolValue;
                    break;

                case SerializedPropertyType.ObjectReference:
                    bBool = conditionalProp.objectReferenceValue != null;
                    break;
            }

            return bBool == attr.DesiredValue;
        }
    }
}