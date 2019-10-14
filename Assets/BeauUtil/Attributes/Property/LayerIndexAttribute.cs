/*
 * Copyright (C) 2017-2019. Filament Games, LLC. All rights reserved.
 * Author:  Alex Beauchesne
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
                property.intValue = UnityEditor.EditorGUI.LayerField(position, label, property.intValue);
                UnityEditor.EditorGUI.EndProperty();
            }
        }

        #endif // UNITY_EDITOR
    }
}