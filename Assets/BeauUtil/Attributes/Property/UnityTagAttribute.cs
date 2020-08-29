/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    26 August 2020
 * 
 * File:    UnityTagAttribute.cs
 * Purpose: Marks a string property as a unity tag id.
 */

using System;
using UnityEngine;

namespace BeauUtil
{
    /// <summary>
    /// Marks a string property as a unity tag id.
    /// </summary>
    public sealed class UnityTagAttribute : PropertyAttribute
    {
        #if UNITY_EDITOR

        [UnityEditor.CustomPropertyDrawer(typeof(UnityTagAttribute))]
        private class Drawer : UnityEditor.PropertyDrawer
        {
            public override void OnGUI(Rect position, UnityEditor.SerializedProperty property, GUIContent label)
            {
                label = UnityEditor.EditorGUI.BeginProperty(position, label, property);
                
                UnityEditor.EditorGUI.BeginChangeCheck();
                string nextVal = UnityEditor.EditorGUI.TagField(position, label, property.stringValue);
                if (UnityEditor.EditorGUI.EndChangeCheck())
                    property.stringValue = nextVal;
                
                UnityEditor.EditorGUI.EndProperty();
            }
        }

        #endif // UNITY_EDITOR
    }
}