using System;
using UnityEditor;
using UnityEngine;

namespace BeauUtil.Editor
{
    [CustomPropertyDrawer(typeof(ConditionalInputFieldAttribute), true)]
    internal sealed class ConditionalInputPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            using(new EditorGUI.DisabledScope(!ShouldEnable(property)))
            {
                label = EditorGUI.BeginProperty(position, label, property);
                EditorGUI.PropertyField(position, property, label, true);
                EditorGUI.EndProperty();
            }
        }

        private bool ShouldEnable(SerializedProperty property)
        {
            ConditionalInputFieldAttribute attr = (ConditionalInputFieldAttribute) attribute;
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

            return conditionalProp.boolValue == attr.DesiredValue;
        }
    }
}