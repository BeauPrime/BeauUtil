/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    14 Oct 2019
 * 
 * File:    AutoEnumPropertyDrawer.cs
 * Purpose: Implements the AutoEnumAttribute property drawer.
 */

using System;
using UnityEditor;
using UnityEngine;

namespace BeauUtil.Editor
{
    [CustomPropertyDrawer(typeof(AutoEnumAttribute))]
    internal sealed class AutoEnumPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, UnityEditor.SerializedProperty property, GUIContent label)
        {
            Type enumType = EnumGUI.GetSerializedEnumType(fieldInfo);
            if (enumType == null)
            {
                EditorGUI.HelpBox(position, "Unable to locate enum type", MessageType.Error);
                return;
            }

            Enum enumVal = (Enum) Enum.ToObject(enumType, property.intValue);

            label = UnityEditor.EditorGUI.BeginProperty(position, label, property);
            enumVal = EnumGUI.EnumField(position, label, enumVal);
            property.intValue = Convert.ToInt32(enumVal);
            UnityEditor.EditorGUI.EndProperty();
        }
    }
}