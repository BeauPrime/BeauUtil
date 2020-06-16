/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    14 Oct 2019
 * 
 * File:    EnumGUILayout.cs
 * Purpose: Methods for rendering a more customized enum field in GUILayout.
 */

using System;
using UnityEditor;
using UnityEngine;

namespace BeauUtil.Editor
{
    static public class EnumGUILayout
    {
        public static Enum EnumField(Enum selected, params GUILayoutOption[] options)
        {
            return EnumField(selected, EditorStyles.popup, options);
        }

        public static Enum EnumField(Enum selected, GUIStyle style, params GUILayoutOption[] options)
        {
            return EnumField(GUIContent.none, selected, style, options);
        }

        public static Enum EnumField(string label, Enum selected, params GUILayoutOption[] options)
        {
            return EnumField(label, selected, EditorStyles.popup, options);
        }

        public static Enum EnumField(string label, Enum selected, GUIStyle style, params GUILayoutOption[] options)
        {
            return EnumField(EditorGUIUtility.TrTempContent(label), selected, style, options);
        }

        public static Enum EnumField(GUIContent label, Enum selected, params GUILayoutOption[] options)
        {
            return EnumField(label, selected, EditorStyles.popup, options);
        }

        public static Enum EnumField(GUIContent label, Enum selected, GUIStyle style, params GUILayoutOption[] options)
        {
            Type enumType = selected.GetType();
            EnumInfoCache.Info info = EnumInfoCache.Instance.GetInfo(enumType);
            if ((info.Flags & EnumInfoCache.TypeFlags.Flags) != 0)
            {
                int mask = info.MapToFlagInput(selected);
                int nextMask = EditorGUILayout.MaskField(label, mask, info.FlagNames, options);
                Enum result = info.MapFromFlagOutput(selected, mask, nextMask);
                return result;
            }
            else if ((info.Flags & EnumInfoCache.TypeFlags.Labeled) != 0)
            {
                return ListGUILayout.Popup(label, selected, info.LabeledList, style, options);
            }
            else
            {
                return EditorGUILayout.EnumPopup(label, selected, style, options);
            }
        }
    }
}