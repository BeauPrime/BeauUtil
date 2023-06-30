/*
 * Copyright (C) 2017-2021. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    5 March 2021
 * 
 * File:    BufferedCollection.cs
 * Purpose: Collection with buffered remove.
 */

#if CSHARP_7_3_OR_NEWER
#define EXPANDED_REFS
#endif // CSHARP_7_3_OR_NEWER

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BeauUtil
{
    public class BufferedCollection<T> : ICollection<T>
    {
        private struct Entry
        {
            public T Item;
            public bool Remove;

            public Entry(T inItem)
            {
                Item = inItem;
                Remove = false;
            }
        }

        private RingBuffer<Entry> m_Entries;
        private int m_InternalCount;
        private int m_EnumeratorCount;
        private int m_MinChangeIndex = int.MaxValue;
        private int m_MaxChangeIndex = int.MinValue;
        private IEqualityComparer<T> m_Comparer;

        public BufferedCollection()
        {
            m_Entries = new RingBuffer<Entry>();
            m_Comparer = CompareUtils.DefaultEquals<T>();
        }

        public BufferedCollection(int inCapacity)
        {
            m_Entries = new RingBuffer<Entry>(inCapacity, RingBufferMode.Expand);
            m_Comparer = CompareUtils.DefaultEquals<T>();
        }

        public BufferedCollection(int inCapacity, IEqualityComparer<T> inComparer)
        {
            m_Entries = new RingBuffer<Entry>(inCapacity, RingBufferMode.Expand);
            m_Comparer = inComparer;
        }

        /// <summary>
        /// Number of items in the collection.
        /// </summary>
        public int Count { get { return m_InternalCount; } }

        /// <summary>
        /// Adds an item to the collection.
        /// </summary>
        public void Add(T inItem)
        {
            m_Entries.PushBack(new Entry(inItem));
            ++m_InternalCount;
        }

        /// <summary>
        /// Clears all items from the collection.
        /// </summary>
        public void Clear()
        {
            m_InternalCount = 0;

            if (m_EnumeratorCount > 0)
            {
                for(int i = m_Entries.Count - 1; i >= 0; --i)
                {
                    #if EXPANDED_REFS
                    m_Entries[i].Remove = true;
                    #else
                    Entry entry = m_Entries[i];
                    entry.Remove = true;
                    m_Entries[i] = entry;
                    #endif // EXPANDED_REFS
                }

                m_MinChangeIndex = 0;
                m_MaxChangeIndex = m_Entries.Count - 1;
                return;
            }

            m_Entries.Clear();
        }

        /// <summary>
        /// Returns if the collection contains the given item.
        /// </summary>
        public bool Contains(T inItem)
        {
            return IndexOf(inItem) >= 0;
        }

        /// <summary>
        /// Removes the given item from the collection.
        /// </summary>
        public bool Remove(T inItem)
        {
            int itemIndex = IndexOf(inItem);
            if (itemIndex < 0)
                return false;

            --m_InternalCount;

            if (m_EnumeratorCount > 0)
            {
                #if EXPANDED_REFS
                m_Entries[itemIndex].Remove = true;
                #else
                Entry entry = m_Entries[itemIndex];
                entry.Remove = true;
                m_Entries[itemIndex] = entry;
                #endif // EXPANDED_REFS

                m_MinChangeIndex = Math.Min(itemIndex, m_MinChangeIndex);
                m_MaxChangeIndex = Math.Max(itemIndex, m_MaxChangeIndex);
                return true;
            }

            m_Entries.FastRemoveAt(itemIndex);
            return true;
        }

        /// <summary>
        /// Removes all items that pass the given predicate.
        /// </summary>
        public int RemoveAll(Predicate<T> inPredicate)
        {
            int removedCount = 0;

            for(int i = m_Entries.Count - 1; i >= 0; --i)
            {
                #if EXPANDED_REFS
                ref Entry entry = ref m_Entries[i];
                #else
                Entry entry = m_Entries[i];
                #endif // EXPANDED_REFS

                if (entry.Remove)
                    continue;

                if (inPredicate(entry.Item))
                {
                    --m_InternalCount;
                    ++removedCount;

                    if (m_EnumeratorCount > 0)
                    {
                        entry.Remove = true;
                        #if !EXPANDED_REFS
                        m_Entries[i] = entry;
                        #endif // !EXPANDED_REFS

                        m_MinChangeIndex = Math.Min(i, m_MinChangeIndex);
                        m_MaxChangeIndex = Math.Max(i, m_MaxChangeIndex);
                    }
                    else
                    {
                        m_Entries.FastRemoveAt(i);
                    }
                }
            }

            return removedCount;
        }

        /// <summary>
        /// Begins enumerating.
        /// This locks all buffered operations from completing until enumeration is complete.
        /// </summary>
        public void BeginEnumerate()
        {
            ++m_EnumeratorCount;
        }

        /// <summary>
        /// Ends enumeration.
        /// If all enumerations are complete, any buffered operatiosn will be completed.
        /// </summary>
        public void EndEnumerate()
        {
            --m_EnumeratorCount;
            if (m_EnumeratorCount < 0)
                throw new InvalidOperationException("Mismatched Begin/End Enumerate calls");
            if (m_EnumeratorCount == 0)
                Clean();
        }

        /// <summary>
        /// Enumerates over all current items.
        /// </summary>
        public void ForEach(Action<T> inAction)
        {
            BeginEnumerate();
            try
            {
                for(int i = 0, len = m_Entries.Count; i < len; ++i)
                {
                    #if EXPANDED_REFS
                    ref Entry entry = ref m_Entries[i];
                    #else
                    Entry entry = m_Entries[i];
                    #endif // EXPANDED_REFS

                    if (entry.Remove)
                        continue;

                    inAction(entry.Item);
                }
            }
            finally
            {
                EndEnumerate();
            }
        }

        /// <summary>
        /// Enumerates over all current items.
        /// </summary>
        public void ForEach<U>(Action<T, U> inAction, U inArg)
        {
            BeginEnumerate();
            try
            {
                for(int i = 0, len = m_Entries.Count; i < len; ++i)
                {
                    #if EXPANDED_REFS
                    ref Entry entry = ref m_Entries[i];
                    #else
                    Entry entry = m_Entries[i];
                    #endif // EXPANDED_REFS

                    if (entry.Remove)
                        continue;

                    inAction(entry.Item, inArg);
                }
            }
            finally
            {
                EndEnumerate();
            }
        }

        private int IndexOf(T inItem)
        {
            var eq = m_Comparer;
            for(int i = m_Entries.Count - 1; i >= 0; --i)
            {
                if (eq.Equals(m_Entries[i].Item, inItem))
                {
                    return m_Entries[i].Remove ? -1 : i;
                }
            }

            return -1;
        }

        private void Clean()
        {
            if (m_MaxChangeIndex < m_MinChangeIndex)
                return;

            for(int i = m_MaxChangeIndex; i >= m_MinChangeIndex; --i)
            {
                 #if EXPANDED_REFS
                ref Entry entry = ref m_Entries[i];
                #else
                Entry entry = m_Entries[i];
                #endif // EXPANDED_REFS

                if (entry.Remove)
                    m_Entries.FastRemoveAt(i);
            }

            m_MaxChangeIndex = int.MinValue;
            m_MinChangeIndex = int.MaxValue;
        }

        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }

        /// <summary>
        /// Copies the contents of the buffer to the given array.
        /// </summary>
        public void CopyTo(T[] array, int arrayIndex)
        {
            for(int i = 0; i < m_Entries.Count; ++i)
            {
                Entry e = m_Entries[i];
                if (!e.Remove)
                    array[arrayIndex++] = e.Item;
            }
        }

        #region ICollection

        bool ICollection<T>.IsReadOnly { get { return false; } }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion // ICollection
    
        #region Enumerator

        public struct Enumerator : IEnumerator<T>, IDisposable
        {
            private BufferedCollection<T> m_Parent;
            private int m_Index;
            private int m_Count;

            public Enumerator(BufferedCollection<T> inCollection)
            {
                m_Parent = inCollection;
                m_Index = -1;
                m_Count = inCollection.m_InternalCount;
            }

            #region IEnumerator

            public T Current { get { return m_Parent.m_Entries[m_Index].Item; } }

            object IEnumerator.Current { get { return Current; } }

            public void Dispose()
            {
                m_Parent = null;
            }

            public bool MoveNext()
            {
                bool bExceeded;
                do
                {
                    ++m_Index;
                    bExceeded = m_Index >= m_Count || m_Index >= m_Parent.m_Entries.Count;
                }
                while(!bExceeded && m_Parent.m_Entries[m_Index].Remove);

                return !bExceeded;
            }

            public void Reset()
            {
                m_Index = -1;
                m_Count = m_Parent.m_InternalCount;
            }

            #endregion // IEnumerator
        }

        #endregion // Enumerator
    }
}