/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    14 Oct 2019
 * 
 * File:    ListGUI.cs
 * Purpose: Methods for rendering a customized list popup field.
 */

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace BeauUtil.Editor
{
    /// <summary>
    /// List renderers.
    /// </summary>
    static public class ListGUI
    {
        #region NamedItemList

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

        #endregion // NamedItemList

        #region String Array

        static public string Popup(Rect inPosition, string inCurrent, string[] inElementList)
        {
            int currentIdx = Array.IndexOf(inElementList, inCurrent);
            int nextIdx = EditorGUI.Popup(inPosition, currentIdx, inElementList);

            return nextIdx < 0 ? inCurrent : inElementList[nextIdx];
        }

        static public string Popup(Rect inPosition, string inCurrent, string[] inElementList, GUIStyle inStyle)
        {
            int currentIdx = Array.IndexOf(inElementList, inCurrent);
            int nextIdx = EditorGUI.Popup(inPosition, currentIdx, inElementList, inStyle);

            return nextIdx < 0 ? inCurrent : inElementList[nextIdx];
        }

        static public string Popup(Rect inPosition, GUIContent inLabel, string inCurrent, string[] inElementList)
        {
            inPosition = EditorGUI.PrefixLabel(inPosition, inLabel);

            int currentIdx = Array.IndexOf(inElementList, inCurrent);
            int nextIdx = EditorGUI.Popup(inPosition, currentIdx, inElementList);

            return nextIdx < 0 ? inCurrent : inElementList[nextIdx];
        }

        static public string Popup<T>(Rect inPosition, GUIContent inLabel, string inCurrent, string[] inElementList, GUIStyle inStyle)
        {
            inPosition = EditorGUI.PrefixLabel(inPosition, inLabel);

            int currentIdx = Array.IndexOf(inElementList, inCurrent);
            int nextIdx = EditorGUI.Popup(inPosition, currentIdx, inElementList, inStyle);

            return nextIdx < 0 ? inCurrent : inElementList[nextIdx];
        }

        #endregion // String Array

        #region List Utils

        /// <summary>
        /// Delegate for determining a mapping from item to name and order for a named item list.
        /// </summary>
        public delegate void NamedListMapperDelegate<T>(int inIndex, T inItem, out string outName, out int outOrder);

        /// <summary>
        /// Returns the default item mapper for the given item type.
        /// </summary>
        static public NamedListMapperDelegate<T> DefaultNamedListMapper<T>(bool inbSorted = false)
        {
            if (!inbSorted)
                return DefaultNamedListMapperUnsortedImpl<T>;

            Type t = typeof(T);
            if (t.IsEnum || t.IsPrimitive)
                return DefaultNamedListMapperPrimitiveImpl<T>;

            return DefaultNamedListMapperSortedImpl<T>;
        }

        static private void DefaultNamedListMapperUnsortedImpl<T>(int inIndex, T inItem, out string outName, out int outOrder)
        {
            outName = inItem != null ? inItem.ToString() : "null";
            outOrder = inIndex;
        }

        static private void DefaultNamedListMapperSortedImpl<T>(int inIndex, T inItem, out string outName, out int outOrder)
        {
            outName = inItem != null ? inItem.ToString() : "null";
            outOrder = 0;
        }

        static private void DefaultNamedListMapperPrimitiveImpl<T>(int inIndex, T inItem, out string outName, out int outOrder)
        {
            outName = inItem != null ? inItem.ToString() : "null";
            outOrder = Convert.ToInt32(inItem);
        }

        static public NamedItemList<T> MakeList<T>(IEnumerable<T> inItems, NamedListMapperDelegate<T> inMapper = null, NamedItemList<T> inTarget = null)
        {
            inMapper = inMapper ?? DefaultNamedListMapper<T>(true);

            NamedItemList<T> itemList;
            if (inTarget != null)
            {
                itemList = inTarget;
                itemList.Clear();
            }
            else
            {
                itemList = new NamedItemList<T>();
            }

            int idx = 0;
            foreach (var item in inItems)
            {
                string name;
                int order;
                inMapper(idx++, item, out name, out order);
                itemList.Add(item, name, order);
            }

            return itemList;
        }

        static public NamedItemList<T> MakeListUnsorted<T>(IEnumerable<T> inItems, NamedListMapperDelegate<T> inMapper = null, NamedItemList<T> inTarget = null)
        {
            inMapper = inMapper ?? DefaultNamedListMapper<T>(false);

            NamedItemList<T> itemList;
            if (inTarget != null)
            {
                itemList = inTarget;
                itemList.Clear();
            }
            else
            {
                itemList = new NamedItemList<T>();
            }

            int idx = 0;
            foreach (var item in inItems)
            {
                string name;
                int order;
                inMapper(idx++, item, out name, out order);
                itemList.Add(item, name, order);
            }

            return itemList;
        }

        #endregion // List Utils
    }
}