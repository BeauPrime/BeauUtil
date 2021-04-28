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

            return -EditorGUIUtility.standardVerticalSpacing;
        }

        private bool ShouldDisplay(SerializedProperty property)
        {
            ConditionalVisibleFieldAttribute attr = (ConditionalVisibleFieldAttribute) attribute;
            string propName = attr.PropertyName;
            return property.EvaluateSiblingCondition(propName) == attr.DesiredValue;
        }
    }
}