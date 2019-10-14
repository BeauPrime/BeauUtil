/*
 * Copyright (C) 2017-2019. Filament Games, LLC. All rights reserved.
 * Author:  Alex Beauchesne
 * Date:    14 Oct 2019
 * 
 * File:    ListGUI.cs
 * Purpose: Methods for rendering a customized list popup field.
 */

using UnityEditor;
using UnityEngine;

namespace BeauUtil.Editor
{
    /// <summary>
    /// List renderers.
    /// </summary>
    static public class ListGUI
    {
        static public T Popup<T>(Rect inPosition, T inCurrent, NamedItemList<T> inElementList)
        {
            int currentIdx = inElementList.IndexOf(inCurrent);
            int nextIdx = EditorGUI.Popup(inPosition, currentIdx, inElementList.SortedContent());

            return inElementList.Get(nextIdx, inCurrent);
        }

        static public T Popup<T>(Rect inPosition, T inCurrent, NamedItemList<T> inElementList, GUIStyle inStyle)
        {
            int currentIdx = inElementList.IndexOf(inCurrent);
            int nextIdx = EditorGUI.Popup(inPosition, currentIdx, inElementList.SortedContent(), inStyle);

            return inElementList.Get(nextIdx, inCurrent);
        }

        static public T Popup<T>(Rect inPosition, GUIContent inLabel, T inCurrent, NamedItemList<T> inElementList)
        {
            int currentIdx = inElementList.IndexOf(inCurrent);
            int nextIdx = EditorGUI.Popup(inPosition, inLabel, currentIdx, inElementList.SortedContent());

            return inElementList.Get(nextIdx, inCurrent);
        }

        static public T Popup<T>(Rect inPosition, GUIContent inLabel, T inCurrent, NamedItemList<T> inElementList, GUIStyle inStyle)
        {
            int currentIdx = inElementList.IndexOf(inCurrent);
            int nextIdx = EditorGUI.Popup(inPosition, inLabel, currentIdx, inElementList.SortedContent(), inStyle);

            return inElementList.Get(nextIdx, inCurrent);
        }
    }
}