using System;
using BeauUtil.IO;
using UnityEditor;
using UnityEngine;

namespace BeauUtil.Editor
{
    [CustomPropertyDrawer(typeof(AssetOnlyAttribute))]
    internal sealed class AssetOnlyPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedObjectUtils.GetFieldInfoFromProperty(property, out Type type);
            EditorGUI.BeginChangeCheck();
            UnityEngine.Object objectRefValue = property.objectReferenceValue;
            position = EditorGUI.PrefixLabel(position, label);
            objectRefValue = EditorGUI.ObjectField(position, objectRefValue, type, false);
            if (EditorGUI.EndChangeCheck())
            {
                property.objectReferenceValue = objectRefValue;
            }
        }
    }
}