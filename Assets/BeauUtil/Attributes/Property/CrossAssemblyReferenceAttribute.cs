/*
 * Copyright (C) 2017-2020. Filament Games, LLC. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    15 July 2019
 * 
 * File:    CrossAssemblyReferenceAttribute.cs
 * Purpose: Filters object references by type, resolving type by name at runtime.
 */

using System;
using UnityEngine;

namespace BeauUtil
{
    /// <summary>
    /// Reference to a UnityEngine.Object type whose type isn't completely known at compile time.
    /// Useful only if you need to reference an otherwise inaccessible type (possibly in another assembly).
    /// </summary>
    public sealed class CrossAssemblyReferenceAttribute : PropertyAttribute
    {
        public string TypeName { get; private set; }

        public CrossAssemblyReferenceAttribute(string inTypeName)
        {
            TypeName = inTypeName;
        }

        #if UNITY_EDITOR

        [UnityEditor.CustomPropertyDrawer(typeof(CrossAssemblyReferenceAttribute))]
        private class Drawer : UnityEditor.PropertyDrawer
        {
            public override void OnGUI(Rect position, UnityEditor.SerializedProperty property, GUIContent label)
            {
                CrossAssemblyReferenceAttribute attr = (CrossAssemblyReferenceAttribute) attribute;
                Type type = Type.GetType(attr.TypeName);
                label = UnityEditor.EditorGUI.BeginProperty(position, label, property);
                property.objectReferenceValue = UnityEditor.EditorGUI.ObjectField(position, label, property.objectReferenceValue, type, true);
                UnityEditor.EditorGUI.EndProperty();
            }
        }

        #endif // UNITY_EDITOR
    }
}