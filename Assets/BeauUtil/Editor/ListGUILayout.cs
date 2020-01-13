/*
 * Copyright (C) 2017-2020. Filament Games, LLC. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    14 Oct 2019
 * 
 * File:    ListGUILayout.cs
 * Purpose: Methods for rendering a customized list popup field in GUILayout.
 */

using System;
using UnityEditor;
using UnityEngine;

namespace BeauUtil.Editor
{
    /// <summary>
    /// List renderers for layout GUI.
    /// </summary>
    static public class ListGUILayout
    {
        #region NamedItemList

        static public T Popup<T>(T inCurrent, NamedItemList<T> inElementList, params GUILayoutOption[] inOptions)
        {
            int currentIdx = inElementList.IndexOf(inCurrent);
            int nextIdx = EditorGUILayout.Popup(currentIdx, inElementList.SortedContent(), inOptions);

            return inElementList.Get(nextIdx, inCurrent);
        }

        static public T Popup<T>(T inCurrent, NamedItemList<T> inElementList, GUIStyle inStyle, params GUILayoutOption[] inOptions)
        {
            int currentIdx = inElementList.IndexOf(inCurrent);
            int nextIdx = EditorGUILayout.Popup(currentIdx, inElementList.SortedContent(), inStyle, inOptions);

            return inElementList.Get(nextIdx, inCurrent);
        }

        static public T Popup<T>(GUIContent inLabel, T inCurrent, NamedItemList<T> inElementList, params GUILayoutOption[] inOptions)
        {
            int currentIdx = inElementList.IndexOf(inCurrent);
            int nextIdx = EditorGUILayout.Popup(inLabel, currentIdx, inElementList.SortedContent(), inOptions);

            return inElementList.Get(nextIdx, inCurrent);
        }

        static public T Popup<T>(GUIContent inLabel, T inCurrent, NamedItemList<T> inElementList, GUIStyle inStyle, params GUILayoutOption[] inOptions)
        {
            int currentIdx = inElementList.IndexOf(inCurrent);
            int nextIdx = EditorGUILayout.Popup(inLabel, currentIdx, inElementList.SortedContent(), inStyle, inOptions);

            return inElementList.Get(nextIdx, inCurrent);
        }

        #endregion // NamedItemList

        #region String Array

        static public string Popup(string inCurrent, string[] inElementList, params GUILayoutOption[] inOptions)
        {
            int currentIdx = Array.IndexOf(inElementList, inCurrent);
            int nextIdx = EditorGUILayout.Popup(currentIdx, inElementList, inOptions);

            return nextIdx < 0 ? inCurrent : inElementList[nextIdx];
        }

        static public string Popup(string inCurrent, string[] inElementList, GUIStyle inStyle, params GUILayoutOption[] inOptions)
        {
            int currentIdx = Array.IndexOf(inElementList, inCurrent);
            int nextIdx = EditorGUILayout.Popup(currentIdx, inElementList, inStyle, inOptions);

            return nextIdx < 0 ? inCurrent : inElementList[nextIdx];
        }

        static public string Popup(GUIContent inLabel, string inCurrent, string[] inElementList, params GUILayoutOption[] inOptions)
        {
            int currentIdx = Array.IndexOf(inElementList, inCurrent);
            int nextIdx = EditorGUILayout.Popup(inLabel, currentIdx, inElementList, inOptions);

            return nextIdx < 0 ? inCurrent : inElementList[nextIdx];
        }

        static public string Popup(GUIContent inLabel, string inCurrent, string[] inElementList, GUIStyle inStyle, params GUILayoutOption[] inOptions)
        {
            EditorGUILayout.PrefixLabel(inLabel);

            int currentIdx = Array.IndexOf(inElementList, inCurrent);
            int nextIdx = EditorGUILayout.Popup(currentIdx, inElementList, inStyle, inOptions);

            return nextIdx < 0 ? inCurrent : inElementList[nextIdx];
        }

        #endregion // String Array
    }
}