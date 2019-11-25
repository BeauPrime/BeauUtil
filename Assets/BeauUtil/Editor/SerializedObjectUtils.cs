/*
 * Copyright (C) 2017-2019. Filament Games, LLC. All rights reserved.
 * Author:  Alex Beauchesne
 * Date:    20 Nov 2019
 * 
 * File:    SerializedObjectUtils.cs
 * Purpose: Utility methods for dealing with serialized objects and properties.
 */

using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace BeauUtil.Editor
{
    static public class SerializedObjectUtils
    {
        static public SerializedProperty FindPropertyParent(this SerializedProperty inProperty)
        {
            StringSlice path = inProperty.propertyPath;
            int lastDot = path.LastIndexOf('.');
            if (lastDot < 0)
                return null;

            StringSlice parentPath = path.Substring(0, lastDot);
            return inProperty.serializedObject.FindProperty(parentPath.ToString());
        }

        static public SerializedProperty FindPropertySibling(this SerializedProperty inProperty, string inSiblingPath)
        {
            SerializedProperty parentProp = FindPropertyParent(inProperty);
            if (parentProp == null)
                return inProperty.serializedObject.FindProperty(inSiblingPath);

            return parentProp.FindPropertyRelative(inSiblingPath);
        }
    }
}