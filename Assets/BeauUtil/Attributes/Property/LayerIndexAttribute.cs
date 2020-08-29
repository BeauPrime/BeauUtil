/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    4 April 2019
 * 
 * File:    LayerIndexAttribute.cs
 * Purpose: Marks an integer property as a GameObject layer index.
 */

using UnityEngine;

namespace BeauUtil
{
    /// <summary>
    /// Marks an integer property as a GameObject layer index.
    /// </summary>
    public sealed class LayerIndexAttribute : PropertyAttribute
    {
        #if UNITY_EDITOR

        [UnityEditor.CustomPropertyDrawer(typeof(LayerIndexAttribute))]
        private class Drawer : UnityEditor.PropertyDrawer
        {
            public override void OnGUI(Rect position, UnityEditor.SerializedProperty property, GUIContent label)
            {
                label = UnityEditor.EditorGUI.BeginProperty(position, label, property);
                UnityEditor.EditorGUI.BeginChangeCheck();
                int nextVal = UnityEditor.EditorGUI.LayerField(position, label, property.intValue);
                if (UnityEditor.EditorGUI.EndChangeCheck())
                    property.intValue = nextVal;
                UnityEditor.EditorGUI.EndProperty();
            }
        }

        #endif // UNITY_EDITOR
    }
}