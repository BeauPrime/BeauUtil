/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    9 June 2020
 * 
 * File:    RingBuffer.cs
 * Purpose: Ring buffer structure.
 */

#if CSHARP_7_3_OR_NEWER
#define EXPANDED_REFS
#endif // CSHARP_7_3_OR_NEWER

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace BeauUtil
{
    /// <summary>
    /// Double-ended queue.
    /// </summary>
    [DebuggerDisplay("Count = {Count}")]
    public class RingBuffer<T> : IRingBuffer<T>
    {
        static private readonly T[] s_EmptyArray = new T[0];

        private T[] m_Data;
        private int m_Capacity;
        private RingBufferMode m_BufferMode;

        private int m_Head;
        private int m_Tail;
        private int m_Count;

        public RingBuffer()
            : this(0, RingBufferMode.Expand)
        { }

        public RingBuffer(int inCapacity)
            : this(inCapacity, RingBufferMode.Fixed)
        { }

        public RingBuffer(int inCapacity, RingBufferMode inMode)
        {
            if (inCapacity < 0)
                throw new ArgumentOutOfRangeException("inCapacity");
            
            m_Capacity = inCapacity;
            m_Data = m_Capacity == 0 ? s_EmptyArray : new T[m_Capacity];
            m_BufferMode = inMode;

            m_Head = 0;
            m_Tail = 0;
            m_Count = 0;
        }

        #region Properties

        /// <summary>
        /// Returns the capacity of the buffer.
        /// </summary>
        public int Capacity
        {
            get { return m_Capacity; }
        }

        /// <summary>
        /// Returns the current size of the buffer.
        /// </summary>
        public int Count
        {
            get { return m_Count; }
        }

        /// <summary>
        /// Buffer mode. Controls behavior when out of space.
        /// </summary>
        public RingBufferMode BufferMode
        {
            get { return m_BufferMode; }
            set { m_BufferMode = value; }
        }

        #endregion // Properties

        #region State Checks
        
        /// <summary>
        /// Returns if the buffer is currently full to capacity.
        /// </summary>
        public bool IsFull()
        {
            return m_Count == m_Capacity;
        }

        /// <summary>
        /// Returns if the given element is present in the buffer.
        /// </summary>
#if EXPANDED_REFS
        public bool Contains(in T inItem)
#else
        public bool Contains(T inItem)
#endif // EXPANDED_REFS
        {
            return IndexOf(inItem) >= 0;
        }

        /// <summary>
        /// Returns the index of the given element in the buffer.
        /// </summary>
#if EXPANDED_REFS
        public int IndexOf(in T inItem)
#else
        public int IndexOf(T inItem)
#endif // EXPANDED_REFS
        {
            if (m_Count <= 0)
                return -1;

            var eq = EqualityComparer<T>.Default;
            int ptr = m_Head;
            int idx = 0;
            while(ptr != m_Tail)
            {
                if (eq.Equals(m_Data[ptr], inItem))
                    return idx;
                
                ptr = (ptr + 1) % m_Capacity;
                ++idx;
            }

            return -1;
        }

        #endregion // State Checks

        #region Clear and Capacity

        /// <summary>
        /// Clears the contents of the buffer.
        /// </summary>
        public void Clear()
        {
            if (m_Count <= 0)
                return;

            if (m_Head < m_Tail)
            {
                Array.Clear(m_Data, m_Head, m_Count);
            }
            else
            {
                Array.Clear(m_Data, m_Head, m_Capacity - m_Head);
                Array.Clear(m_Data, 0, m_Tail);
            }
            
            m_Head = 0;
            m_Tail = 0;
            m_Count = 0;
        }

        /// <summary>
        /// Resizes the buffer to a new capacity.
        /// </summary>
        public void SetCapacity(int inCapacity)
        {
            if (inCapacity < 0)
                throw new ArgumentOutOfRangeException("inCapacity");

            if (m_Capacity == inCapacity)
                return;
            
            if (inCapacity < m_Count)
                throw new InvalidOperationException("Cannot resize RingBuffer to less than current number of elements");

            if (inCapacity == 0)
            {
                m_Data = s_EmptyArray;
                m_Capacity = 0;
                m_Head = m_Tail = 0;
                return;
            }

            T[] newData = new T[inCapacity];
            if (m_Count > 0)
            {
                if (m_Head < m_Tail)
                {
                    Array.Copy(m_Data, m_Head, newData, 0, m_Count);
                }
                else
                {
                    Array.Copy(m_Data, m_Head, newData, 0, m_Capacity - m_Head);
                    Array.Copy(m_Data, 0, newData, m_Capacity - m_Head, m_Tail);
                }
            }

            m_Data = newData;
            m_Capacity = inCapacity;
            m_Head = 0;
            m_Tail = m_Count;
        }

        static private int GetDesiredCapacity(int inCurrentCapacity)
        {
            int newCapacity = inCurrentCapacity * 2;
            if (newCapacity > 0 && newCapacity < 4)
                newCapacity = 4;
            return newCapacity;
        }

        #endregion // Clear and Capacity

        #region Accessors

        #if EXPANDED_REFS

        /// <summary>
        /// Returns the element contained in the buffer at the given index.
        /// </summary>
        public ref T this[int inIndex]
        {
            get
            {
                if (inIndex < 0 || inIndex >= m_Count)
                    throw new ArgumentOutOfRangeException("inIndex");
                return ref m_Data[(m_Head + inIndex) % m_Capacity];
            }
        }

        #else

        /// <summary>
        /// Returns the element contained in the buffer at the given index.
        /// </summary>
        public T this[int inIndex]
        {
            get
            {
                if (inIndex < 0 || inIndex >= m_Count)
                    throw new ArgumentOutOfRangeException("inIndex");
                return m_Data[(m_Head + inIndex) % m_Capacity];
            }
            set
            {
                if (inIndex < 0 || inIndex >= m_Count)
                    throw new ArgumentOutOfRangeException("inIndex");
                m_Data[(m_Head + inIndex) % m_Capacity] = value;
            }
        }

        #endif // EXPANDED_REFS

        /// <summary>
        /// Peeks at the value at the front of the buffer.
        /// </summary>
        public T PeekFront()
        {
            if (m_Count <= 0)
                throw new InvalidOperationException("Buffer is empty");
            
            return m_Data[m_Head];
        }

        /// <summary>
        /// Peeks at the value at the back of the buffer.
        /// </summary>
        public T PeekBack()
        {
            if (m_Count <= 0)
                throw new InvalidOperationException("Buffer is empty");

            return m_Data[(m_Tail + m_Capacity - 1) % m_Capacity];
        }

        #endregion // Accessors

        #region Insert

        /// <summary>
        /// Pushes a value to the front of the buffer.
        /// </summary>
#if EXPANDED_REFS
        public void PushFront(in T inValue)
#else
        public void PushFront(T inValue)
#endif // EXPANDED_REFS
        {
            if (m_Count >= m_Capacity)
            {
                switch(m_BufferMode)
                {
                    case RingBufferMode.Fixed:
                        throw new InvalidOperationException("Ring buffer is full");
                    case RingBufferMode.Overwrite:
                        if (m_Capacity <= 0)
                            throw new InvalidOperationException("Ring buffer has no capacity");
                        m_Tail = (m_Tail + m_Capacity - 1) % m_Capacity;
                        --m_Count;
                        break;
                    case RingBufferMode.Expand:
                        SetCapacity(GetDesiredCapacity(m_Capacity + 1));
                        break;
                    default:
                        throw new InvalidOperationException("Unknown RingBufferMode");
                }
            }
            
            m_Head = (m_Head + m_Capacity - 1) % m_Capacity;
            m_Data[m_Head] = inValue;
            ++m_Count;
        }

        /// <summary>
        /// Pushes a value to the back of the buffer.
        /// </summary>
#if EXPANDED_REFS
        public void PushBack(in T inValue)
#else
        public void PushBack(T inValue)
#endif // EXPANDED_REFS
        {
            if (m_Count >= m_Capacity)
            {
                switch(m_BufferMode)
                {
                    case RingBufferMode.Fixed:
                        throw new InvalidOperationException("Ring buffer is full");
                    case RingBufferMode.Overwrite:
                        if (m_Capacity <= 0)
                            throw new InvalidOperationException("Ring buffer has no capacity");
                        m_Head = (m_Head + 1) % m_Capacity;
                        --m_Count;
                        break;
                    case RingBufferMode.Expand:
                        SetCapacity(GetDesiredCapacity(m_Capacity + 1));
                        break;

                    default:
                        throw new InvalidOperationException("Unknown RingBufferMode");
                }
            }

            m_Data[m_Tail] = inValue;
            m_Tail = (m_Tail + 1) % m_Capacity;
            ++m_Count;
        }

        #endregion // Insert
        
        #region Remove

        /// <summary>
        /// Dequeues the next available value from the front of the buffer.
        /// </summary>
        public T PopFront()
        {
            if (m_Count <= 0)
                throw new InvalidOperationException("Ring buffer is empty");

            T val = m_Data[m_Head];
            m_Data[m_Head] = default(T);

            --m_Count;
            m_Head = (m_Head + 1) % m_Capacity;

            return val;
        }

        /// <summary>
        /// Pops the next available value from the back of the buffer.
        /// </summary>
        public T PopBack()
        {
            if (m_Count <= 0)
                throw new InvalidOperationException("Ring buffer is empty");

            --m_Count;
            m_Tail = (m_Tail + m_Capacity - 1) % m_Capacity;

            T val = m_Data[m_Tail];
            m_Data[m_Tail] = default(T);

            return val;
        }

        /// <summary>
        /// Removes the given element from the buffer by swapping.
        /// Does not preserve element order.
        /// </summary>
#if EXPANDED_REFS
        public bool FastRemove(in T inValue)
#else
        public bool FastRemove(T inValue)
#endif // EXPANDED_REFS
        {
            int idx = IndexOf(inValue);
            if (idx < 0)
                return false;

            FastRemoveAt(idx);
            return true;
        }

        /// <summary>
        /// Removes the entry at the given index by swapping.
        /// Does not preserve element order.
        /// </summary>
        public void FastRemoveAt(int inIndex)
        {
            if (inIndex < 0 || inIndex >= m_Count)
                throw new ArgumentOutOfRangeException("inIndex");

            int entryIdx = (m_Head + inIndex) % m_Capacity;
            
            int tailIdx = (m_Tail + m_Capacity - 1) % m_Capacity;
            if (tailIdx != entryIdx)
            {
                m_Data[entryIdx] = m_Data[tailIdx];
            }
            m_Data[tailIdx] = default(T);
            m_Tail = tailIdx;

            --m_Count;
        }

        /// <summary>
        /// Removes a number of elements from the front of the buffer.
        /// </summary>
        public void RemoveFromFront(int inCount)
        {
            if (inCount == 0)
                return;

            if (inCount < 0 || inCount > m_Count)
                throw new ArgumentOutOfRangeException("inCount");

            if (inCount == m_Count)
            {
                Clear();
                return;
            }

            int endIdx = m_Head + inCount - 1;
            if (endIdx < m_Capacity)
            {
                Array.Clear(m_Data, m_Head, inCount);
            }
            else
            {
                int firstCount = m_Capacity - m_Head;
                int overflow = inCount - firstCount;
                Array.Clear(m_Data, m_Head, firstCount);
                Array.Clear(m_Data, 0, overflow);
            }

            m_Head = (m_Head + inCount) % m_Capacity;
            m_Count -= inCount;
        }

        /// <summary>
        /// Removes a number of elements from the back of the buffer.
        /// </summary>
        public void RemoveFromBack(int inCount)
        {
            if (inCount == 0)
                return;

            if (inCount < 0 || inCount > m_Count)
                throw new ArgumentOutOfRangeException("inCount");

            if (inCount == m_Count)
            {
                Clear();
                return;
            }

            int endIdx = m_Tail - inCount + 1;

            if (endIdx >= 0)
            {
                Array.Clear(m_Data, endIdx, inCount);
            }
            else
            {
                int firstCount = m_Tail + 1;
                int overflow = inCount - firstCount;
                Array.Clear(m_Data, 0, firstCount);
                Array.Clear(m_Data, m_Capacity - overflow, overflow);
            }

            m_Tail = (m_Tail + m_Capacity - inCount) % m_Capacity;
            m_Count -= inCount;
        }

        #endregion // Remove

        #region Sorting

        /// <summary>
        /// Sorts the buffer using the given comparer.
        /// </summary>
        public void Sort(IComparer<T> inComparer)
        {
            if (m_Count <= 1)
                return;

            Compress(false);
            Array.Sort(m_Data, m_Head, m_Count, inComparer);
        }

        /// <summary>
        /// Reverses element order in the buffer.
        /// </summary>
        public void Reverse()
        {
            if (m_Count <= 1)
                return;

            Compress(false);
            Array.Reverse(m_Data, m_Head, m_Count);
        }

        /// <summary>
        /// Compresses the buffer to ensure elements are contiguous in memory.
        /// </summary>
        public void Compress()
        {
            Compress(true);
        }

        private void Compress(bool inbForceToZero)
        {
            if (m_Count == 0)
                return;

            if (inbForceToZero && m_Head == 0)
            {
                return;
            }

            if (m_Head < m_Tail)
            {
                if (inbForceToZero)
                {
                    Array.Copy(m_Data, m_Head, m_Data, 0, m_Count);
                    m_Head = 0;
                    Array.Clear(m_Data, m_Count, m_Data.Length - m_Count);
                }
                return;
            }

            // TODO(Beau): Implement this inline, without extra alloc

            T[] newData = new T[m_Capacity];
            int headLength = m_Capacity - m_Head;
            int tailLength = m_Count - headLength;

            Array.Copy(m_Data, m_Head, newData, 0, headLength);
            Array.Copy(m_Data, 0, newData, headLength, tailLength);
            m_Head = 0;
            m_Tail = m_Count;

            m_Data = newData;
        }

        #endregion // Sorting

        #region Copy

        /// <summary>
        /// Copies the contents of the ring buffer into an array.
        /// Returns how many elements were able to be copied.
        /// </summary>
        public int CopyTo(int inSrcIndex, T[] ioDest, int inDestIndex, int inLength)
        {
            int desiredCopy = inLength - inDestIndex;
            int available = m_Count - inSrcIndex;
            if (desiredCopy > m_Count)
                desiredCopy = m_Count;
            if (desiredCopy > available)
                desiredCopy = available;
            if (desiredCopy > ioDest.Length)
                desiredCopy = ioDest.Length;
            
            if (desiredCopy <= 0)
                return 0;

            if (m_Head + desiredCopy <= m_Capacity)
            {
                Array.Copy(m_Data, m_Head, ioDest, inDestIndex, desiredCopy);
            }
            else
            {
                int headLength = m_Capacity - m_Head;
                int tailLength = desiredCopy - headLength;

                Array.Copy(m_Data, m_Head, ioDest, inDestIndex, headLength);
                Array.Copy(m_Data, 0, ioDest, inDestIndex + headLength, tailLength);
            }

            return desiredCopy;
        }

        #endregion // Copy

        #region Search

        /// <summary>
        /// Performs a binary search for the given element in a sorted buffer.
        /// </summary>
#if EXPANDED_REFS
        public int BinarySearch(in T inValue, SearchFlags inFlags = 0)
#else
        public int BinarySearch(T inValue, SearchFlags inFlags = 0)
#endif // EXPANDED_REFS
        {
            return BinarySearch(inValue, Comparer<T>.Default, inFlags);
        }

        /// <summary>
        /// Performs a binary search for the given element in a sorted buffer with the given comparer.
        /// </summary>
#if EXPANDED_REFS
        public int BinarySearch(in T inValue, IComparer<T> inComparer, SearchFlags inFlags = 0)
#else
        public int BinarySearch(T inValue, IComparer<T> inComparer, SearchFlags inFlags = 0)
#endif // EXPANDED_REFS
        {
            if (inComparer == null)
                throw new ArgumentNullException("inComparer");

            if (m_Count <= 0)
                return -1;

            inComparer = CompareWrapper<T>.Wrap(inComparer, inFlags);

            if (m_Head < m_Tail)
            {
                return Array.BinarySearch(m_Data, m_Head, m_Count, inValue, inComparer);
            }

            int low = 0;
            int high = m_Count - 1;

            while(low <= high)
            {
                int med = low + ((high - low) >> 1);
                int comp = inComparer.Compare(m_Data[(m_Head + med) % m_Capacity], inValue);
                if (comp == 0)
                    return med;
                if (comp == -1)
                    low = med + 1;
                else
                    high = med - 1;
            }

            return ~low;
        }

        /// <summary>
        /// Performs a binary search for the given element in a sorted buffer.
        /// </summary>
#if EXPANDED_REFS
        public int BinarySearch<U>(in U inValue, ComparePredicate<T, U> inComparer, SearchFlags inFlags = 0)
#else
        public int BinarySearch<U>(U inValue, ComparePredicate<T, U> inComparer, SearchFlags inFlags = 0)
#endif // EXPANDED_REFS
        {
            if (inComparer == null)
                throw new ArgumentNullException("inComparer");

            if (m_Count <= 0)
                return -1;

            int low = 0;
            int high = m_Count - 1;

            int sign = (inFlags & SearchFlags.IsReversed) != 0 ? -1 : 1;

            while(low <= high)
            {
                int med = low + ((high - low) >> 1);
                int comp = inComparer(m_Data[(m_Head + med) % m_Capacity], inValue) * sign;
                if (comp == 0)
                    return med;
                if (comp == -1)
                    low = med + 1;
                else
                    high = med - 1;
            }

            return ~low;
        }

        #endregion // Search

        #region Interfaces

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<T>) this).GetEnumerator();
        }

        bool ICollection.IsSynchronized { get { return false; } }

        object ICollection.SyncRoot { get { return null; } }

        void ICollection.CopyTo(Array array, int index)
        {
            CopyTo(0, (T[]) array, index, m_Count);
        }

        T IReadOnlyList<T>.this[int inIndex]
        {
            get
            {
                if (inIndex < 0 || inIndex >= m_Count)
                    throw new ArgumentOutOfRangeException("inIndex");
                return m_Data[(m_Head + inIndex) % m_Capacity];
            }
        }

        #endregion // Interfaces

        #region Overrides

        public IEnumerator<T> GetEnumerator()
        {
            int head = m_Head;
            for(int i = 0, length = m_Count; i < length; ++i)
                yield return m_Data[(head + i) % m_Capacity];
        }

        #endregion // Overrides
    }
}