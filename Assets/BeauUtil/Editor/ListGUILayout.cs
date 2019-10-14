/*
 * Copyright (C) 2017-2019. Filament Games, LLC. All rights reserved.
 * Author:  Alex Beauchesne
 * Date:    14 Oct 2019
 * 
 * File:    ListGUILayout.cs
 * Purpose: Methods for rendering a customized list popup field in GUILayout.
 */

using UnityEditor;
using UnityEngine;

namespace BeauUtil.Editor
{
    /// <summary>
    /// List renderers for layout GUI.
    /// </summary>
    static public class ListGUILayout
    {
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
    }
}