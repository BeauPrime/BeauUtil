/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    14 Oct 2019
 * 
 * File:    NamedItemList.cs
 * Purpose: Named item list. For use with ListGUI and ListGUILayout.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace BeauUtil.Editor
{
    /// <summary>
    /// List of named items for the editor.
    /// </summary>
    public sealed class NamedItemList<T> : IReadOnlyList<T>
    {
        #region Types

        private sealed class Entry : IComparable<Entry>
        {
            public readonly T Value;
            public readonly string Name;
            public readonly int Order;

            public Entry(T inValue, string inName, int inOrder)
            {
                Value = inValue;
                Name = inName;
                Order = inOrder;
            }

            int IComparable<Entry>.CompareTo(Entry other)
            {
                if (Order == other.Order)
                    return EditorUtility.NaturalCompare(Name, other.Name);
                if (Order < other.Order)
                    return -1;
                return 1;
            }
        }

        #endregion // Types

        private readonly List<Entry> m_Entries;
        private readonly IEqualityComparer<T> m_Comparer;

        private GUIContent[] m_GUIContent = null;
        private string[] m_StringContent = null;
        private bool m_RequireRefresh = true;

        public GUIContent[] SortedContent()
        {
            RefreshList();
            return m_GUIContent;
        }

        public string[] SortedStrings()
        {
            RefreshList();
            return m_StringContent;
        }

        public NamedItemList()
        {
           m_Entries = new List<Entry>();
           m_Comparer = CompareUtils.DefaultEquals<T>();
        }

        public NamedItemList(NamedItemList<T> inSource)
        {
            m_Entries = new List<Entry>(inSource.m_Entries);
            m_Comparer = inSource.m_Comparer;
        }

        public NamedItemList(int inCapacity)
        {
            m_Entries = new List<Entry>(inCapacity);
            m_Comparer = CompareUtils.DefaultEquals<T>();
        }

        #region Modifications

        public void Add(T inValue, string inName, int inOrder = 0)
        {
            m_Entries.Add(new Entry(inValue, inName, inOrder));
            m_RequireRefresh = true;
        }

        public void Clear()
        {
            if (m_Entries.Count > 0)
            {
                m_Entries.Clear();
                m_RequireRefresh = true;
            }
        }

        #endregion // Modifications

        #region List

        public int Count { get { return m_Entries.Count; } }

        public T this[int index]
        {
            get { return Get(index); }
        }

        public T Get(int inIndex)
        {
            return m_Entries[inIndex].Value;
        }

        public T Get(int inIndex, T inDefault)
        {
            RefreshList();

            if (inIndex < 0 || inIndex >= m_Entries.Count)
                return inDefault;

            return m_Entries[inIndex].Value;
        }

        public int IndexOf(T inElement)
        {
            RefreshList();

            for (int i = 0; i < m_Entries.Count; ++i)
            {
                if (m_Comparer.Equals(m_Entries[i].Value, inElement))
                    return i;
            }

            return -1;
        }

        public bool Contains(T inElement)
        {
            return IndexOf(inElement) >= 0;
        }

        #endregion // List

        #region Content Population

        private void RefreshList()
        {
            if (!m_RequireRefresh)
                return;

            m_Entries.Sort();
            Array.Resize(ref m_GUIContent, m_Entries.Count);
            Array.Resize(ref m_StringContent, m_Entries.Count);
            for (int i = 0; i < m_Entries.Count; ++i)
            {
                PopulateContent(ref m_GUIContent[i], m_Entries[i]);
                m_StringContent[i] = m_Entries[i].Name;
            }

            m_RequireRefresh = false;
        }

        static private void PopulateContent(ref GUIContent ioContent, Entry inEntry)
        {
            if (ioContent == null)
                ioContent = new GUIContent();

            ioContent.text = inEntry.Name;
        }

        #endregion // Content Population

        #region IReadOnlyList

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            foreach (var element in m_Entries)
                yield return element.Value;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<T>) this).GetEnumerator();
        }

        #endregion // IReadOnlyList
    }
}