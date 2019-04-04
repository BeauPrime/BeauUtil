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
    public sealed class LayerIndexAttribute : PropertyAttribute
    {
        #if UNITY_EDITOR

        [UnityEditor.CustomPropertyDrawer(typeof(LayerIndexAttribute))]
        private class Drawer : UnityEditor.PropertyDrawer
        {
            public override void OnGUI(Rect position, UnityEditor.SerializedProperty property, GUIContent label)
            {
                property.intValue = UnityEditor.EditorGUI.LayerField(position, label, property.intValue);
            }
        }

        #endif // UNITY_EDITOR
    }
}