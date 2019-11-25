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
            using(new EditorGUI.DisabledScope(EditorApplication.isPlayingOrWillChangePlaymode))
            {
                label = EditorGUI.BeginProperty(position, label, property);
                EditorGUI.PropertyField(position, property, label, true);
                EditorGUI.EndProperty();
            }
        }
    }
}