/*
 * Copyright (C) 2023. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    23 June 2023
 * 
 * File:    LLTable.cs
 * Purpose: Doubly-linked list table.
*/

using System;
using System.Collections;
using System.Collections.Generic;
using BeauUtil.Debugger;

namespace BeauUtil
{
    /// <summary>
    /// Linked list table.
    /// Entries can be either free or in use by an external list.
    /// </summary>
    public sealed class LLTable
    {
        private LLIndices[] m_Data;
        private LLIndexList m_FreeList;
        private int m_Capacity;

        public LLTable(int inCapacity)
        {
            m_Data = new LLIndices[inCapacity];
            m_FreeList = LLIndexList.Empty;
            m_Capacity = inCapacity;
        }

        #region Properties

        /// <summary>
        /// Total number of entries in the table.
        /// </summary>
        public int Capacity
        {
            get { return m_Capacity; }
        }

        /// <summary>
        /// Returns the number of free table entries.
        /// </summary>
        public int FreeCount
        {
            get { return m_FreeList.Length; }
        }

        /// <summary>
        /// Returns the number of in use table entries.
        /// </summary>
        public int InUseCount
        {
            get { return m_Capacity - m_FreeList.Length; }
        }

        #endregion // Properties

        #region Checks

        /// <summary>
        /// Returns if the given table index is free.
        /// </summary>
        public bool IsFree(int inIndex)
        {
            return Contains(m_FreeList, inIndex);
        }

        /// <summary>
        /// Returns if the given list contains the given index.
        /// </summary>
        public bool Contains(LLIndexList inList, int inIndex)
        {
            int current = inList.Head;
            int remaining = inList.Length;
            while (current >= 0 && remaining-- > 0)
            {
                if (current == inIndex)
                    return true;
                Assert.True(current < m_Data.Length, "linked list index out of range");
                current = m_Data[current].Next;
            }
            Assert.True(current < 0, "linked list cached length out of sync (too small)");
            Assert.True(remaining == 0, "linked list cached length out of sync (too large)");
            return false;
        }

        #endregion // Checks

        #region Enumerators

        /// <summary>
        /// Returns an enumerator for the given list.
        /// </summary>
        public Enumerator GetEnumerator(LLIndexList inList)
        {
            return new Enumerator(this, inList);
        }

        /// <summary>
        /// Enumerator over all indices for a given list.
        /// </summary>
        public struct Enumerator : IEnumerator<int>
        {
            private LLIndices[] m_Data;
            private LLIndexList m_List;
            private int m_Current;

            internal Enumerator(LLTable inTable, LLIndexList inList)
            {
                m_Data = inTable.m_Data;
                m_List = inList;
                m_Current = -1;
            }

            #region IEnumerator

            public int Current
            {
                get { return m_Current; }
            }

            object IEnumerator.Current
            {
                get { return (object) Current; }
            }

            public void Dispose()
            {
                m_Data = null;
            }

            public bool MoveNext()
            {
                m_Current = m_List.Head;
                if (m_Current >= 0)
                {
                    Assert.True(m_Current < m_Data.Length, "linked list index out of range");
                    m_List.Head = m_Data[m_Current].Next;
                    m_List.Length--;
                    return true;
                }
                else
                {
                    Assert.True(m_Current < 0, "linked list cached length out of sync (too small)");
                    Assert.True(m_List.Length == 0, "linked list cached length out of sync (too large)");
                    return false;
                }
            }

            public void Reset()
            {
                throw new NotSupportedException();
            }

            #endregion // IEnumerator
        }

        #endregion // Enumerators
    }

    /// <summary>
    /// Linked list table with tag data.
    /// Entries can be either free or in use by an external list.
    /// </summary>
    public sealed class LLTable<TTag>
        where TTag : struct
    {
        private LLIndices<TTag>[] m_Data;
        private LLIndexList m_FreeList;
        private int m_Capacity;
        private readonly IEqualityComparer<TTag> m_TagComparer;

        public LLTable(int inCapacity)
        {
            m_Data = new LLIndices<TTag>[inCapacity];
            m_FreeList = LLIndexList.Empty;
            m_Capacity = inCapacity;
            m_TagComparer = CompareUtils.DefaultComparer<TTag>();
        }

        /// <summary>
        /// Returns a reference to the tag at the given table index.
        /// </summary>
        public ref TTag this[int inIndex]
        {
            get { return ref m_Data[inIndex].Tag; }
        }

        #region Properties

        /// <summary>
        /// Total number of entries in the table.
        /// </summary>
        public int Capacity
        {
            get { return m_Capacity; }
        }

        /// <summary>
        /// Returns the number of free table entries.
        /// </summary>
        public int FreeCount
        {
            get { return m_FreeList.Length; }
        }

        /// <summary>
        /// Returns the number of in use table entries.
        /// </summary>
        public int InUseCount
        {
            get { return m_Capacity - m_FreeList.Length; }
        }

        #endregion // Properties

        #region Checks

        /// <summary>
        /// Returns if the given table index is free.
        /// </summary>
        public bool IsFree(int inIndex)
        {
            return Contains(m_FreeList, inIndex);
        }

        /// <summary>
        /// Returns if the given list contains the given table index.
        /// </summary>
        public bool Contains(LLIndexList inList, int inIndex)
        {
            int current = inList.Head;
            int remaining = inList.Length;
            while (current >= 0 && remaining-- > 0)
            {
                if (current == inIndex)
                    return true;
                Assert.True(current < m_Data.Length, "linked list index out of range");
                current = m_Data[current].Next;
            }
            Assert.True(current < 0, "linked list cached length out of sync (too small)");
            Assert.True(remaining == 0, "linked list cached length out of sync (too large)");
            return false;
        }

        /// <summary>
        /// Returns the table index of the given tag.
        /// </summary>
        public int IndexOfTag(LLIndexList inList, in TTag inTag)
        {
            int current = inList.Head;
            int remaining = inList.Length;
            while (current >= 0 && remaining-- > 0)
            {
                Assert.True(current < m_Data.Length, "linked list index out of range");
                if (m_TagComparer.Equals(m_Data[current].Tag, inTag))
                    return current;
                current = m_Data[current].Next;
            }
            Assert.True(current < 0, "linked list cached length out of sync (too small)");
            Assert.True(remaining == 0, "linked list cached length out of sync (too large)");
            return -1;
        }

        #endregion // Checks

        #region Enumerator

        public struct TaggedIndex
        {
            public TTag Tag;
            public int Index;

            public TaggedIndex(TTag inTag, int inIndex)
            {
                Tag = inTag;
                Index = inIndex;
            }
        }

        /// <summary>
        /// Returns an enumerator for the given list.
        /// </summary>
        public Enumerator GetEnumerator(LLIndexList inList)
        {
            return new Enumerator(this, inList);
        }

        /// <summary>
        /// Enumerator over all indices for a given list.
        /// </summary>
        public struct Enumerator : IEnumerator<TaggedIndex>
        {
            private LLIndices<TTag>[] m_Data;
            private LLIndexList m_List;
            private TaggedIndex m_Current;

            internal Enumerator(LLTable<TTag> inTable, LLIndexList inList)
            {
                m_Data = inTable.m_Data;
                m_List = inList;
                m_Current = new TaggedIndex(default(TTag), -1);
            }

            #region IEnumerator

            public TaggedIndex Current
            {
                get { return m_Current; }
            }

            object IEnumerator.Current
            {
                get { return (object) Current; }
            }

            public void Dispose()
            {
                m_Data = null;
                m_Current.Tag = default(TTag);
            }

            public bool MoveNext()
            {
                m_Current.Index = m_List.Head;
                if (m_Current.Index >= 0)
                {
                    Assert.True(m_Current.Index < m_Data.Length, "linked list index out of range");
                    LLIndices<TTag> node = m_Data[m_Current.Index];
                    m_Current.Tag = node.Tag;
                    m_List.Head = node.Next;
                    m_List.Length--;
                    return true;
                }
                else
                {
                    m_Current.Tag = default(TTag);
                    Assert.True(m_Current.Index < 0, "linked list cached length out of sync (too small)");
                    Assert.True(m_List.Length == 0, "linked list cached length out of sync (too large)");
                    return false;
                }
            }

            public void Reset()
            {
                throw new NotSupportedException();
            }

            #endregion // IEnumerator
        }

        #endregion // Enumerator
    }

    /// <summary>
    /// Linked list utility methods.
    /// </summary>
    static public class LLUtility
    {
        
    }
}