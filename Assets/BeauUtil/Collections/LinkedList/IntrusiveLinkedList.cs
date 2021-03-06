/*
 * Copyright (C) 2017-2021. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    26 Feb 20201
 * 
 * File:    IntrusiveLinkedList.cs
 * Purpose: An intrusively tracked linked list.
 */

#if UNITY_EDITOR || DEVELOPMENT_BUILD
#define DEVELOPMENT
#endif

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace BeauUtil
{
    /// <summary>
    /// Intrusively tracked linked list.
    /// </summary>
    [DebuggerDisplay("Count = {Count}")]
    public class IntrusiveLinkedList<T> : ICollection<T>
        where T : class, IIntrusiveLLNode<T>
    {
        private int m_Count;
        private T m_Head;
        private T m_Tail;

        /// <summary>
        /// Number of elements in the linked list.
        /// </summary>
        public int Count { get { return m_Count; } }

        #region Queries

        /// <summary>
        /// Returns whether or not the given item is contained in this list.
        /// </summary>
        public bool Contains(T inItem)
        {
            if (inItem == null)
                return false;

            T current = m_Head;
            while(current != null)
            {
                if (current == inItem)
                    return true;
                current = current.Next;
            }

            return false;
        }

        /// <summary>
        /// Creates a new array with the contents of the linked list.
        /// </summary>
        public T[] ToArray()
        {
            if (m_Count == 0)
                return Array.Empty<T>();

            T[] arr = new T[m_Count];
            
            T current = m_Head;
            int idx = 0;
            while(current != null)
            {
                arr[idx++] = current;
                current = current.Next;
            }

            return arr;
        }

        /// <summary>
        /// Returns an enumerator to iterate over the list.
        /// </summary>
        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }

        #endregion // Queries

        #region Operations

        /// <summary>
        /// Clears all elements.
        /// </summary>
        public void Clear()
        {
            T current = m_Head;
            T next;

            while(current != null)
            {
                next = current.Next;

                current.Previous = null;
                current.Next = null;

                current = next;
            }

            m_Head = null;
            m_Tail = null;
            m_Count = 0;
        }

        /// <summary>
        /// Adds an element to the start of the list.
        /// </summary>
        public void AddFirst(T inItem)
        {
            #if DEVELOPMENT
            if (inItem == null)
                throw new ArgumentNullException("inItem");
            if (Contains(inItem))
                throw new InvalidOperationException("Item already present in list");
            #endif // DEVELOPMENT

            inItem.Previous = null;
            inItem.Next = m_Head;
            if (m_Head != null)
                m_Head.Previous = inItem;
            m_Head = inItem;
            if (m_Tail == null)
                m_Tail = inItem;
            ++m_Count;
        }

        /// <summary>
        /// Removes the first element and returns it.
        /// </summary>
        public T RemoveFirst()
        {
            T head = m_Head;
            if (head != null)
            {
                T next = head.Next;
                m_Head = next;

                if (m_Tail == head)
                    m_Tail = null;

                head.Next = null;
                head.Previous = null;
                --m_Count;
            }
            return head;
        }

        /// <summary>
        /// Adds an element to the end of the list.
        /// </summary>
        public void AddLast(T inItem)
        {
            #if DEVELOPMENT
            if (inItem == null)
                throw new ArgumentNullException("inItem");
            if (Contains(inItem))
                throw new InvalidOperationException("Item already present in list");
            #endif // DEVELOPMENT
            
            inItem.Previous = m_Tail;
            inItem.Next = null;
            if (m_Tail != null)
                m_Tail.Next = inItem;
            m_Tail = inItem;
            if (m_Head == null)
                m_Head = inItem;
            ++m_Count;
        }

        /// <summary>
        /// Removes the last element and returns it.
        /// </summary>
        public T RemoveLast()
        {
            T tail = m_Tail;
            if (tail != null)
            {
                T prev = tail.Previous;
                m_Tail = prev;

                if (m_Head == tail)
                    m_Head = null;

                tail.Next = null;
                tail.Previous = null;
                --m_Count;
            }
            return tail;
        }

        /// <summary>
        /// Inserts an item before the given item.
        /// </summary>
        public void InsertBefore(T inItem, T inBeforeTarget)
        {
            #if DEVELOPMENT
            if (inItem == null)
                throw new ArgumentNullException("inItem");
            if (inBeforeTarget == null)
                throw new ArgumentNullException("inBeforeTarget");
            if (Contains(inItem))
                throw new InvalidOperationException("Item already present in list");
            if (!Contains(inBeforeTarget))
                throw new InvalidOperationException("Target item is not present in list");
            #endif // DEVELOPMENT

            if (m_Head == inBeforeTarget)
                m_Head = inItem;
            
            T prev = inBeforeTarget.Previous;
            inBeforeTarget.Previous = inItem;
            inItem.Previous = prev;
            inItem.Next = inBeforeTarget;
            if (prev != null)
                prev.Next = inItem;
            
            ++m_Count;
        }

        /// <summary>
        /// Inserts an item after the given item.
        /// </summary>
        public void InsertAfter(T inItem, T inAfterTarget)
        {
            #if DEVELOPMENT
            if (inItem == null)
                throw new ArgumentNullException("inItem");
            if (inAfterTarget == null)
                throw new ArgumentNullException("inAfterTarget");
            if (Contains(inItem))
                throw new InvalidOperationException("Item already present in list");
            if (!Contains(inAfterTarget))
                throw new InvalidOperationException("Target item is not present in list");
            #endif // DEVELOPMENT

            if (m_Tail == inAfterTarget)
                m_Tail = inItem;
            
            T next = inAfterTarget.Next;
            inAfterTarget.Next = inItem;
            inItem.Next = next;
            inItem.Previous = inAfterTarget;
            if (next != null)
                next.Previous = inItem;
            
            ++m_Count;
        }

        /// <summary>
        /// Removes the given item from the linked list.
        /// </summary>
        public bool Remove(T inItem)
        {
            if (inItem == null || !Contains(inItem))
                return false;

            T prev = inItem.Previous;
            T next = inItem.Next;

            if (m_Head == inItem)
                m_Head = next;
            if (m_Tail == inItem)
                m_Tail = prev;
            
            if (prev != null)
                prev.Next = next;
            if (next != null)
                next.Previous = prev;

            inItem.Next = null;
            inItem.Previous = null;

            --m_Count;
            return true;
        }

        #endregion // Operations

        #region ICollection

        bool ICollection<T>.IsReadOnly { get { return false; } }

        void ICollection<T>.Add(T item)
        {
            AddLast(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            if (arrayIndex + m_Count > array.Length)
                throw new IndexOutOfRangeException("Array is not long enough");

            T current = m_Head;
            while(current != null)
            {
                array[arrayIndex++] = current;
                current = current.Next;
            }
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion // ICollection

        #region Custom Enumerator

        /// <summary>
        /// Custom enumerator.
        /// </summary>
        public struct Enumerator : IEnumerator<T>, IDisposable
        {
            private IntrusiveLinkedList<T> m_Parent;
            private T m_Current;
            private T m_Next;

            public Enumerator(IntrusiveLinkedList<T> inList)
            {
                m_Parent = inList;
                m_Current = null;
                m_Next = inList.m_Head;
            }

            #region IEnumerator

            public T Current { get { return m_Next; } }

            object IEnumerator.Current { get { return Current; } }

            public void Dispose()
            {
                m_Parent = null;
                m_Current = null;
                m_Next = null;
            }

            public bool MoveNext()
            {
                m_Current = m_Next;
                m_Next = m_Current != null ? m_Current.Next : null;
                return m_Current != null;
            }

            public void Reset()
            {
                m_Current = null;
                m_Next = m_Parent.m_Head;
            }

            #endregion // IEnumerator
        }

        #endregion // Custom Enumerator
    }
}