using System;
using UnityEditor;
using UnityEngine;

namespace BeauUtil.Editor
{
    [CustomPropertyDrawer(typeof(PrefabModeOnlyAttribute), true)]
    internal sealed class PrefabModeOnlyPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            PrefabModeOnlyAttribute attr = (PrefabModeOnlyAttribute) attribute;
            if (attr.Hide)
            {
                if (!IsPrefab(property))
                    return;
                
                label = EditorGUI.BeginProperty(position, label, property);
                EditorGUI.PropertyField(position, property, label, true);
                EditorGUI.EndProperty();
            }
            else
            {
                using(new EditorGUI.DisabledScope(!IsPrefab(property)))
                {
                    label = EditorGUI.BeginProperty(position, label, property);
                    EditorGUI.PropertyField(position, property, label, true);
                    EditorGUI.EndProperty();
                }
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            PrefabModeOnlyAttribute attr = (PrefabModeOnlyAttribute) attribute;
            if (attr.Hide && !IsPrefab(property))
            {
                return 0;
            }

            return EditorGUI.GetPropertyHeight(property, label, true);
        }

        static private bool IsPrefab(SerializedProperty property)
        {
            return SerializedObjectUtils.IsEditingPrefab(property);
        }
    }
}