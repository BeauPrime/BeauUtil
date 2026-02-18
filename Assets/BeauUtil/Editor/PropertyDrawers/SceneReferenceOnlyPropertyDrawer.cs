using System;
using BeauUtil.IO;
using UnityEditor;
using UnityEngine;

namespace BeauUtil.Editor
{
    [CustomPropertyDrawer(typeof(SceneReferenceOnlyAttribute))]
    internal sealed class SceneReferenceOnlyPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedObjectUtils.GetFieldInfoFromProperty(property, out Type type);
            EditorGUI.BeginChangeCheck();
            UnityEngine.Object objectRefValue = property.objectReferenceValue;
            position = EditorGUI.PrefixLabel(position, label);
            objectRefValue = EditorGUI.ObjectField(position, objectRefValue, type, true);
            if (EditorGUI.EndChangeCheck())
            {
                if (objectRefValue != null && EditorUtility.IsPersistent(objectRefValue))
                {
                    Debug.LogWarningFormat(objectRefValue, "Object {0} is not a scene reference", objectRefValue.ToString());
                }
                else
                {
                    property.objectReferenceValue = objectRefValue;
                }
            }
        }
    }
}