/*
 * Copyright (C) 2023. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    23 June 2023
 * 
 * File:    LLTable.cs
 * Purpose: Doubly-linked list table.
*/

#if (UNITY_EDITOR && !IGNORE_UNITY_EDITOR) || DEVELOPMENT_BUILD
#define DEVELOPMENT
#endif // (UNITY_EDITOR && !IGNORE_UNITY_EDITOR) || DEVELOPMENT_BUILD

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using BeauUtil.Debugger;
using UnityEngine;

namespace BeauUtil
{
    /// <summary>
    /// Linked list table.
    /// Entries can be either free or in use by an external list.
    /// </summary>
    public sealed class LLTable
    {
        private LLNode[] m_Data;
        private LLIndexList m_FreeList;
        private int m_Capacity;

        public LLTable(int inCapacity)
        {
            m_Data = new LLNode[inCapacity];
            m_FreeList = LLIndexList.Empty;
            m_Capacity = inCapacity;

            AddRangeToFree(0, inCapacity);
        }

        /// <summary>
        /// Callback when the capacity gets expanded.
        /// </summary>
        public event Action<int> OnResize;

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

        /// <summary>
        /// Validates the consistency of the given list.
        /// </summary>
        public void Validate(LLIndexList inList)
        {
            int current = inList.Head;
            int remaining = inList.Length;
            while (current >= 0 && remaining-- > 0)
            {
                Assert.True(current < m_Data.Length, "linked list index out of range");
                current = m_Data[current].Next;
            }
            Assert.True(current < 0, "linked list cached length out of sync (too small)");
            Assert.True(remaining == 0, "linked list cached length out of sync (too large)");
        }

        #endregion // Checks

        #region Free List

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int PopFreeNode()
        {
            ReserveFreeNodes(1);
            return PopFrontImpl(ref m_FreeList);
        }

        private void ReserveFreeNodes(int inNodeCount)
        {
            if (m_FreeList.Length < inNodeCount)
            {
                int oldLength = m_Data.Length;
                int newLength = Mathf.NextPowerOfTwo(m_Capacity + inNodeCount);
                Array.Resize(ref m_Data, newLength);
                m_Capacity = newLength;
                AddRangeToFree(oldLength, newLength - oldLength);
                OnResize?.Invoke(newLength);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AppendToFreeList(int inIndex)
        {
            PushBackImpl(ref m_FreeList, inIndex);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void PushToFreeList(int inIndex)
        {
            PushFrontImpl(ref m_FreeList, inIndex);
        }

        private void AddRangeToFree(int inIndex, int inLength)
        {
            if (inLength <= 0)
                return;

            if (m_FreeList.Head < 0)
            {
                m_FreeList.Head = inIndex;
                m_Data[m_FreeList.Head].Prev = -1;
            }

            m_FreeList.Tail = inIndex + inLength - 1;
            m_Data[m_FreeList.Tail].Next = -1;

            for(int i = inIndex + 1, end = inIndex + inLength; i < end; i++)
            {
                m_Data[i - 1].Next = i;
                m_Data[i].Prev = i - 1;
            }

            m_FreeList.Length += inLength;
        }

        #endregion // Free List

        #region Add

        /// <summary>
        /// Pushes a node to the head of the given list.
        /// </summary>
        public int PushFront(ref LLIndexList ioList)
        {
            int newIndex = PopFreeNode();
            PushFrontImpl(ref ioList, newIndex);
            return newIndex;
        }

        /// <summary>
        /// Pushes a node to the tail of the given list.
        /// </summary>
        public int PushBack(ref LLIndexList ioList)
        {
            int newIndex = PopFreeNode();
            PushBackImpl(ref ioList, newIndex);
            return newIndex;
        }

        /// <summary>
        /// Inserts a node before the given index in the given list.
        /// </summary>
        public int InsertBefore(ref LLIndexList ioList, int inTarget)
        {
#if DEVELOPMENT
            if (!Contains(ioList, inTarget))
                throw new ArgumentException("Index not present in provided list", "inTarget");
#endif // DEVELOPMENT

            int newIndex = PopFreeNode();
            ref LLNode targetEntry = ref m_Data[inTarget];
            ref LLNode newEntry = ref m_Data[newIndex];

            newEntry.Prev = targetEntry.Prev;
            newEntry.Next = inTarget;
            targetEntry.Prev = newIndex;

            if (newEntry.IsHead())
                ioList.Head = newIndex;
            ioList.Length++;

            return newIndex;
        }

        /// <summary>
        /// Inserts a node after the given index in the given list.
        /// </summary>
        public int InsertAfter(ref LLIndexList ioList, int inTarget)
        {
#if DEVELOPMENT
            if (!Contains(ioList, inTarget))
                throw new ArgumentException("Index not present in provided list", "inTarget");
#endif // DEVELOPMENT

            int newIndex = PopFreeNode();
            ref LLNode targetEntry = ref m_Data[inTarget];
            ref LLNode newEntry = ref m_Data[newIndex];

            newEntry.Next = targetEntry.Next;
            newEntry.Prev = inTarget;
            targetEntry.Next = newIndex;

            if (newEntry.IsTail())
                ioList.Tail = newIndex;
            ioList.Length++;

            return newIndex;
        }

        private void PushFrontImpl(ref LLIndexList ioList, int inIndex)
        {
            LLIndexList.Fixup(ref ioList);

            ref LLNode node = ref m_Data[inIndex];
            node.Prev = -1;
            node.Next = ioList.Head;
            if (ioList.Head >= 0)
            {
                m_Data[ioList.Head].Prev = inIndex;
            }
            ioList.Head = inIndex;
            if (node.IsTail())
                ioList.Tail = inIndex;

            ioList.Length++;
        }

        private void PushBackImpl(ref LLIndexList ioList, int inIndex)
        {
            LLIndexList.Fixup(ref ioList);

            ref LLNode node = ref m_Data[inIndex];
            node.Next = -1;
            node.Prev = ioList.Tail;
            if (ioList.Tail >= 0)
            {
                m_Data[ioList.Tail].Next = inIndex;
            }
            ioList.Tail = inIndex;
            if (node.IsHead())
                ioList.Head = inIndex;

            ioList.Length++;
        }

        #endregion // Add

        #region Move

        /// <summary>
        /// Moves the given index to the head of the given list.
        /// </summary>
        public void MoveToFront(ref LLIndexList ioList, int inIndex)
        {
#if DEVELOPMENT
            if (!Contains(ioList, inIndex))
                throw new ArgumentException("Index not present in provided list", "inIndex");
#endif // DEVELOPMENT

            if (ioList.Length < 2)
                return;

            RemoveImpl(ref ioList, inIndex);
            PushFrontImpl(ref ioList, inIndex);
        }

        /// <summary>
        /// Moves the given index to the tail of the given list.
        /// </summary>
        public void MoveToBack(ref LLIndexList ioList, int inIndex)
        {
#if DEVELOPMENT
            if (!Contains(ioList, inIndex))
                throw new ArgumentException("Index not present in provided list", "inIndex");
#endif // DEVELOPMENT

            if (ioList.Length < 2)
                return;

            RemoveImpl(ref ioList, inIndex);
            PushBackImpl(ref ioList, inIndex);
        }

        #endregion // Move

        #region Remove

        /// <summary>
        /// Removes the index from the head of the given list.
        /// </summary>
        public int PopFront(ref LLIndexList ioList)
        {
            int index = PopFrontImpl(ref ioList);
            if (index >= 0)
            {
                PushToFreeList(index);
            }
            return index;
        }

        /// <summary>
        /// Removes the index from the tail of the given list.
        /// </summary>
        public int PopBack(ref LLIndexList ioList)
        {
            int index = PopBackImpl(ref ioList);
            if (index >= 0)
            {
                PushToFreeList(index);
            }
            return index;
        }

        /// <summary>
        /// Removes the given index from the given list.
        /// </summary>
        public void Remove(ref LLIndexList ioList, int inIndex)
        {
#if DEVELOPMENT
            if (!Contains(ioList, inIndex))
                throw new ArgumentException("Index not present in provided list", "inIndex");
#endif // DEVELOPMENT

            RemoveImpl(ref ioList, inIndex);
            PushToFreeList(inIndex);
        }

        /// <summary>
        /// Clears the given list.
        /// </summary>
        public void Clear(ref LLIndexList ioList)
        {
            int current = ioList.Head;
            int next;
            int remaining = ioList.Length;
            while (current >= 0 && remaining-- > 0)
            {
                Assert.True(current < m_Data.Length, "linked list index out of range");
                next = m_Data[current].Next;
                m_Data[current].Clear();
                AppendToFreeList(current);
                current = next;
            }
            Assert.True(current < 0, "linked list cached length out of sync (too small)");
            Assert.True(remaining == 0, "linked list cached length out of sync (too large)");

            ioList.Head = -1;
            ioList.Tail = -1;
            ioList.Length = 0;
        }

        private int PopFrontImpl(ref LLIndexList ioList)
        {
            if (ioList.Length > 0)
            {
                int headIndex = ioList.Head;
                ref LLNode headNode = ref m_Data[headIndex];

                ioList.Head = headNode.Next;
                if (ioList.Head >= 0)
                {
                    m_Data[ioList.Head].Prev = headNode.Prev;
                }

                headNode.Clear();
                ioList.Length--;

                return headIndex;
            }

            return -1;
        }

        private int PopBackImpl(ref LLIndexList ioList)
        {
            if (ioList.Length > 0)
            {
                int tailIndex = ioList.Tail;
                ref LLNode tailNode = ref m_Data[tailIndex];

                ioList.Tail = tailNode.Prev;
                if (ioList.Tail >= 0)
                {
                    m_Data[ioList.Tail].Next = tailNode.Next;
                }

                tailNode.Clear();
                ioList.Length--;
                return tailIndex;
            }

            return -1;
        }

        private void RemoveImpl(ref LLIndexList ioList, int inIndex)
        {
            ref LLNode entry = ref m_Data[inIndex];

            if (entry.Prev >= 0)
            {
                m_Data[entry.Prev].Next = entry.Next;
            }
            else
            {
                // no prev pointer - head
                ioList.Head = entry.Next;
            }

            if (entry.Next >= 0)
            {
                m_Data[entry.Next].Prev = entry.Prev;
            }
            else
            {
                // no next pointer - tail
                ioList.Tail = entry.Prev;
            }

            entry.Clear();
            ioList.Length--;
        }

        #endregion // Remove

        #region Linearize

        /// <summary>
        /// Reorders list pointers into a linear order.
        /// </summary>
        public void Linearize(ref LLIndexList ioList)
        {
            if (ioList.Length < 2)
                return;

            unsafe
            {
                int* indices = stackalloc int[ioList.Length];
                int indicesIndex = 0;
                int current = ioList.Head;
                int remaining = ioList.Length;
                bool outOfOrder = false;
                int next;
                while (current >= 0 && remaining-- > 0)
                {
                    Assert.True(current < m_Data.Length, "linked list index out of range");
                    indices[indicesIndex++] = current;
                    next = m_Data[current].Next;
                    outOfOrder |= current > next;
                    current = next;
                }
                Assert.True(current < 0, "linked list cached length out of sync (too small)");
                Assert.True(remaining == 0, "linked list cached length out of sync (too large)");

                if (outOfOrder)
                {
                    Unsafe.Quicksort(indices, ioList.Length);

                    ioList.Head = indices[0];
                    m_Data[ioList.Head].Prev = -1;
                    ioList.Tail = indices[ioList.Length - 1];
                    m_Data[ioList.Tail].Next = -1;

                    int prev = indices[0];
                    int now;
                    for (int i = 1; i < ioList.Length; i++)
                    {
                        now = indices[i];
                        m_Data[prev].Next = now;
                        m_Data[now].Prev = prev;
                        prev = now;
                    }
                }
            }
        }

        /// <summary>
        /// Linearizes the free list.
        /// </summary>
        public void OptimizeFreeList()
        {
            Validate(m_FreeList);
            Linearize(ref m_FreeList);
        }

        #endregion // Linearize

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
            private LLNode[] m_Data;
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
        private LLNode<TTag>[] m_Data;
        private LLIndexList m_FreeList;
        private int m_Capacity;
        private readonly IEqualityComparer<TTag> m_TagComparer;

        public LLTable(int inCapacity)
        {
            m_Data = new LLNode<TTag>[inCapacity];
            m_FreeList = LLIndexList.Empty;
            m_Capacity = inCapacity;
            m_TagComparer = CompareUtils.DefaultEquals<TTag>();

            AddRangeToFree(0, inCapacity);
        }

        /// <summary>
        /// Returns a reference to the tag at the given table index.
        /// </summary>
        public ref TTag this[int inIndex]
        {
            get { return ref m_Data[inIndex].Tag; }
        }

        /// <summary>
        /// Callback when the capacity gets expanded.
        /// </summary>
        public event Action<int> OnResize;

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

        /// <summary>
        /// Validates the consistency of the given list.
        /// </summary>
        public void Validate(LLIndexList inList)
        {
            int current = inList.Head;
            int remaining = inList.Length;
            while (current >= 0 && remaining-- > 0)
            {
                Assert.True(current < m_Data.Length, "linked list index out of range");
                current = m_Data[current].Next;
            }
            Assert.True(current < 0, "linked list cached length out of sync (too small)");
            Assert.True(remaining == 0, "linked list cached length out of sync (too large)");
        }

        #endregion // Checks

        #region Free List

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int PopFreeNode()
        {
            ReserveFreeNodes(1);
            return PopFrontImpl(ref m_FreeList, out var _);
        }

        private void ReserveFreeNodes(int inNodeCount)
        {
            if (m_FreeList.Length < inNodeCount)
            {
                int oldLength = m_Data.Length;
                int newLength = Mathf.NextPowerOfTwo(m_Capacity + inNodeCount);
                Array.Resize(ref m_Data, newLength);
                m_Capacity = newLength;
                AddRangeToFree(oldLength, newLength - oldLength);
                OnResize?.Invoke(newLength);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AppendToFreeList(int inIndex)
        {
            PushBackImpl(ref m_FreeList, inIndex);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void PushToFreeList(int inIndex)
        {
            PushFrontImpl(ref m_FreeList, inIndex);
        }

        private void AddRangeToFree(int inIndex, int inLength)
        {
            if (inLength <= 0)
                return;

            if (m_FreeList.Head < 0)
            {
                m_FreeList.Head = inIndex;
                m_Data[m_FreeList.Head].Prev = -1;
            }

            m_FreeList.Tail = inIndex + inLength - 1;
            m_Data[m_FreeList.Tail].Next = -1;

            for (int i = inIndex + 1, end = inIndex + inLength; i < end; i++)
            {
                m_Data[i - 1].Next = i;
                m_Data[i].Prev = i - 1;
            }

            m_FreeList.Length += inLength;
        }

        #endregion // Free List

        #region Add

        /// <summary>
        /// Pushes a node to the head of the given list.
        /// </summary>
        public int PushFront(ref LLIndexList ioList, TTag inTag)
        {
            int newIndex = PopFreeNode();
            PushFrontImpl(ref ioList, newIndex);
            m_Data[newIndex].Tag = inTag;
            return newIndex;
        }

        /// <summary>
        /// Pushes a node to the tail of the given list.
        /// </summary>
        public int PushBack(ref LLIndexList ioList, TTag inTag)
        {
            int newIndex = PopFreeNode();
            PushBackImpl(ref ioList, newIndex);
            m_Data[newIndex].Tag = inTag;
            return newIndex;
        }

        private void PushFrontImpl(ref LLIndexList ioList, int inIndex)
        {
            LLIndexList.Fixup(ref ioList);

            ref LLNode<TTag> node = ref m_Data[inIndex];
            node.Prev = -1;
            node.Next = ioList.Head;
            if (ioList.Head >= 0)
            {
                m_Data[ioList.Head].Prev = inIndex;
            }
            ioList.Head = inIndex;
            if (node.IsTail())
                ioList.Tail = inIndex;

            ioList.Length++;
        }

        private void PushBackImpl(ref LLIndexList ioList, int inIndex)
        {
            LLIndexList.Fixup(ref ioList);

            ref LLNode<TTag> node = ref m_Data[inIndex];
            node.Next = -1;
            node.Prev = ioList.Tail;
            if (ioList.Tail >= 0)
            {
                m_Data[ioList.Tail].Next = inIndex;
            }
            ioList.Tail = inIndex;
            if (node.IsHead())
                ioList.Head = inIndex;

            ioList.Length++;
        }

        #endregion // Add

        #region Move

        /// <summary>
        /// Moves the given index to the head of the given list.
        /// </summary>
        public void MoveToFront(ref LLIndexList ioList, int inIndex)
        {
#if DEVELOPMENT
            if (!Contains(ioList, inIndex))
                throw new ArgumentException("Index not present in provided list", "inIndex");
#endif // DEVELOPMENT

            RemoveImpl(ref ioList, inIndex);
            PushFrontImpl(ref ioList, inIndex);
        }

        /// <summary>
        /// Moves the given index to the tail of the given list.
        /// </summary>
        public void MoveToBack(ref LLIndexList ioList, int inIndex)
        {
#if DEVELOPMENT
            if (!Contains(ioList, inIndex))
                throw new ArgumentException("Index not present in provided list", "inIndex");
#endif // DEVELOPMENT

            RemoveImpl(ref ioList, inIndex);
            PushBackImpl(ref ioList, inIndex);
        }

        #endregion // Move

        #region Remove

        /// <summary>
        /// Removes the index from the head of the given list.
        /// </summary>
        public LLTaggedIndex<TTag> PopFront(ref LLIndexList ioList)
        {
            int index = PopFrontImpl(ref ioList, out TTag tag);
            if (index >= 0)
            {
                PushToFreeList(index);
            }
            return new LLTaggedIndex<TTag>(index, tag);
        }

        /// <summary>
        /// Removes the index from the tail of the given list.
        /// </summary>
        public LLTaggedIndex<TTag> PopBack(ref LLIndexList ioList)
        {
            int index = PopBackImpl(ref ioList, out TTag tag);
            if (index >= 0)
            {
                PushToFreeList(index);
            }
            return new LLTaggedIndex<TTag>(index, tag);
        }

        /// <summary>
        /// Removes the given index from the given list.
        /// </summary>
        public void Remove(ref LLIndexList ioList, int inIndex)
        {
#if DEVELOPMENT
            if (!Contains(ioList, inIndex))
                throw new ArgumentException("Index not present in provided list", "inIndex");
#endif // DEVELOPMENT

            RemoveImpl(ref ioList, inIndex);
            PushToFreeList(inIndex);
        }

        /// <summary>
        /// Removes the given index from the given list.
        /// </summary>
        public bool RemoveTag(ref LLIndexList ioList, TTag inTag)
        {
            int index = IndexOfTag(ioList, inTag);
            if (index >= 0)
            {
                Remove(ref ioList, index);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Clears the given list.
        /// </summary>
        public void Clear(ref LLIndexList ioList)
        {
            int current = ioList.Head;
            int next;
            int remaining = ioList.Length;
            while (current >= 0 && remaining-- > 0)
            {
                Assert.True(current < m_Data.Length, "linked list index out of range");
                next = m_Data[current].Next;
                m_Data[current].Clear();
                AppendToFreeList(current);
                current = next;
            }
            Assert.True(current < 0, "linked list cached length out of sync (too small)");
            Assert.True(remaining == 0, "linked list cached length out of sync (too large)");

            ioList.Head = -1;
            ioList.Tail = -1;
            ioList.Length = 0;
        }

        private int PopFrontImpl(ref LLIndexList ioList, out TTag outTag)
        {
            if (ioList.Length > 0)
            {
                int headIndex = ioList.Head;
                ref LLNode<TTag> headNode = ref m_Data[headIndex];

                ioList.Head = headNode.Next;
                if (ioList.Head >= 0)
                {
                    m_Data[ioList.Head].Prev = headNode.Prev;
                }

                outTag = headNode.Tag;
                headNode.Clear();
                ioList.Length--;

                return headIndex;
            }

            outTag = default(TTag);
            return -1;
        }

        private int PopBackImpl(ref LLIndexList ioList, out TTag outTag)
        {
            if (ioList.Length > 0)
            {
                int tailIndex = ioList.Tail;
                ref LLNode<TTag> tailNode = ref m_Data[tailIndex];

                ioList.Tail = tailNode.Prev;
                if (ioList.Tail >= 0)
                {
                    m_Data[ioList.Tail].Next = tailNode.Next;
                }

                outTag = tailNode.Tag;
                tailNode.Clear();
                ioList.Length--;
                return tailIndex;
            }

            outTag = default(TTag);
            return -1;
        }

        private void RemoveImpl(ref LLIndexList ioList, int inIndex)
        {
            ref LLNode<TTag> entry = ref m_Data[inIndex];

            if (entry.Prev >= 0)
            {
                m_Data[entry.Prev].Next = entry.Next;
            }
            else
            {
                // no prev pointer - head
                ioList.Head = entry.Next;
            }

            if (entry.Next >= 0)
            {
                m_Data[entry.Next].Prev = entry.Prev;
            }
            else
            {
                // no next pointer - tail
                ioList.Tail = entry.Prev;
            }

            entry.Clear();
            ioList.Length--;
        }

        #endregion // Remove

        #region Linearize

        /// <summary>
        /// Reorders list pointers into a linear order.
        /// </summary>
        public void Linearize(ref LLIndexList ioList)
        {
            if (ioList.Length < 2)
                return;

            unsafe
            {
                int* indices = stackalloc int[ioList.Length];
                int indicesIndex = 0;
                int current = ioList.Head;
                int remaining = ioList.Length;
                bool outOfOrder = false;
                int next;
                while (current >= 0 && remaining-- > 0)
                {
                    Assert.True(current < m_Data.Length, "linked list index out of range");
                    indices[indicesIndex++] = current;
                    next = m_Data[current].Next;
                    outOfOrder |= (current > next);
                    current = next;
                }
                Assert.True(current < 0, "linked list cached length out of sync (too small)");
                Assert.True(remaining == 0, "linked list cached length out of sync (too large)");

                if (outOfOrder)
                {
                    Unsafe.Quicksort(indices, ioList.Length);

                    ioList.Head = indices[0];
                    m_Data[ioList.Head].Prev = -1;
                    ioList.Tail = indices[ioList.Length - 1];
                    m_Data[ioList.Tail].Next = -1;

                    int prev = indices[0];
                    int now;
                    for (int i = 1; i < ioList.Length; i++)
                    {
                        now = indices[i];
                        m_Data[prev].Next = now;
                        m_Data[now].Prev = prev;
                        prev = now;
                    }
                }
            }
        }

        /// <summary>
        /// Linearizes the free list.
        /// </summary>
        public void OptimizeFreeList()
        {
            Validate(m_FreeList);
            Linearize(ref m_FreeList);
        }

        #endregion // Linearize

        #region Enumerator

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
        public struct Enumerator : IEnumerator<LLTaggedIndex<TTag>>
        {
            private LLNode<TTag>[] m_Data;
            private LLIndexList m_List;
            private LLTaggedIndex<TTag> m_Current;

            internal Enumerator(LLTable<TTag> inTable, LLIndexList inList)
            {
                m_Data = inTable.m_Data;
                m_List = inList;
                m_Current = LLTaggedIndex<TTag>.Invalid;
            }

            #region IEnumerator

            public LLTaggedIndex<TTag> Current
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
                    LLNode<TTag> node = m_Data[m_Current.Index];
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
}