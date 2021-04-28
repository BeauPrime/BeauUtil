using System;
using UnityEditor;
using UnityEngine;

namespace BeauUtil.Editor
{
    [CustomPropertyDrawer(typeof(EditModeOnlyAttribute), true)]
    internal sealed class EditModeOnlyPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditModeOnlyAttribute attr = (EditModeOnlyAttribute) attribute;
            if (attr.Hide)
            {
                if (EditorApplication.isPlayingOrWillChangePlaymode)
                    return;
                    
                label = EditorGUI.BeginProperty(position, label, property);
                EditorGUI.PropertyField(position, property, label, true);
                EditorGUI.EndProperty();
            }
            else
            {
                using(new EditorGUI.DisabledScope(EditorApplication.isPlayingOrWillChangePlaymode))
                {
                    label = EditorGUI.BeginProperty(position, label, property);
                    EditorGUI.PropertyField(position, property, label, true);
                    EditorGUI.EndProperty();
                }
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            EditModeOnlyAttribute attr = (EditModeOnlyAttribute) attribute;
            if (attr.Hide && !EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return -EditorGUIUtility.standardVerticalSpacing;
            }

            return EditorGUI.GetPropertyHeight(property, label, true);
        }
    }
}