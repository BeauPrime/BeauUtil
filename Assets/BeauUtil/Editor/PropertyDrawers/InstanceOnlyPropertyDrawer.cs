using System;
using UnityEditor;
using UnityEngine;

namespace BeauUtil.Editor
{
    [CustomPropertyDrawer(typeof(InstanceOnlyAttribute), true)]
    internal sealed class InstanceOnlyPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            InstanceOnlyAttribute attr = (InstanceOnlyAttribute) attribute;
            if (attr.Hide)
            {
                if (!IsInstance(property))
                    return;
                
                label = EditorGUI.BeginProperty(position, label, property);
                EditorGUI.PropertyField(position, property, label, true);
                EditorGUI.EndProperty();
            }
            else
            {
                using(new EditorGUI.DisabledScope(!IsInstance(property)))
                {
                    label = EditorGUI.BeginProperty(position, label, property);
                    EditorGUI.PropertyField(position, property, label, true);
                    EditorGUI.EndProperty();
                }
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            InstanceOnlyAttribute attr = (InstanceOnlyAttribute) attribute;
            if (attr.Hide && !IsInstance(property))
            {
                return -EditorGUIUtility.standardVerticalSpacing;
            }

            return EditorGUI.GetPropertyHeight(property, label, true);
        }

        static private bool IsInstance(SerializedProperty property)
        {
            return !SerializedObjectUtils.IsEditingPrefab(property);
        }
    }
}