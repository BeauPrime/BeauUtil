/*
 * Copyright (C) 2017-2019. Filament Games, LLC. All rights reserved.
 * Author:  Alex Beauchesne
 * Date:    14 Oct 2019
 * 
 * File:    EnumGUI.cs
 * Purpose: Methods for rendering a more customized enum field.
 */

using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace BeauUtil.Editor
{
    static public class EnumGUI
    {
        public static Enum EnumField(Rect position, Enum selected)
        {
            return EnumField(position, selected, EditorStyles.popup);
        }

        public static Enum EnumField(Rect position, Enum selected, GUIStyle style)
        {
            return EnumField(position, GUIContent.none, selected, style);
        }

        public static Enum EnumField(Rect position, string label, Enum selected)
        {
            return EnumField(position, label, selected, EditorStyles.popup);
        }

        public static Enum EnumField(Rect position, string label, Enum selected, GUIStyle style)
        {
            return EnumField(position, EditorGUIUtility.TrTempContent(label), selected, style);
        }

        public static Enum EnumField(Rect position, GUIContent label, Enum selected)
        {
            return EnumField(position, label, selected, EditorStyles.popup);
        }

        public static Enum EnumField(Rect position, GUIContent label, Enum selected, GUIStyle style)
        {
            Type enumType = selected.GetType();
            EnumInfoCache.Info info = EnumInfoCache.Instance.GetInfo(enumType);
            if ((info.Flags & EnumInfoCache.TypeFlags.Flags) != 0)
            {
                int mask = info.MapToFlagInput(selected);
                int nextMask = EditorGUI.MaskField(position, label, mask, info.FlagNames);
                Enum result = info.MapFromFlagOutput(selected, mask, nextMask);
                return result;
            }
            else if ((info.Flags & EnumInfoCache.TypeFlags.Labeled) != 0)
            {
                return ListGUI.Popup(position, label, selected, info.LabeledList, style);
            }
            else
            {
                return EditorGUI.EnumPopup(position, label, selected, style);
            }
        }

        static internal Type GetSerializedEnumType(FieldInfo inFieldInfo)
        {
            return GetSerializedEnumType(inFieldInfo?.FieldType);
        }

        static internal Type GetSerializedEnumType(Type inType)
        {
            if (inType == null)
                return null;
            
            if (inType.IsEnum)
                return inType;
            if (inType.IsArray)
                return inType.GetElementType();
            if (inType.IsGenericType)
            {
                Type generic = inType.GetGenericTypeDefinition();
                if (typeof(List<>).IsAssignableFrom(generic))
                    return inType.GetGenericArguments() [0];
            }
            return null;
        }
    }
}