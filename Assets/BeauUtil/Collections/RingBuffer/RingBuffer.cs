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
using System.Runtime.CompilerServices;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;

namespace BeauUtil
{
    /// <summary>
    /// Double-ended queue.
    /// </summary>
    [DebuggerDisplay("Count = {Count}")]
    public sealed class RingBuffer<T> : IRingBuffer<T>, IList<T>
    {
        static private readonly T[] s_EmptyArray = new T[0];

        private T[] m_Data;
        private int m_Capacity;
        private RingBufferMode m_BufferMode;
        private IEqualityComparer<T> m_Comparer;

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
            : this(inCapacity, inMode, CompareUtils.DefaultComparer<T>())
        { }

        public RingBuffer(int inCapacity, RingBufferMode inMode, IEqualityComparer<T> inComparer)
        {
            if (inCapacity < 0)
                throw new ArgumentOutOfRangeException("inCapacity");
            
            m_Capacity = inCapacity;
            m_Data = m_Capacity == 0 ? s_EmptyArray : new T[m_Capacity];
            m_BufferMode = inMode;

            m_Head = 0;
            m_Tail = 0;
            m_Count = 0;

            m_Comparer = inComparer;
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

        /// <summary>
        /// Equality comparer. Used for comparing elements during IndexOf operations.
        /// </summary>
        public IEqualityComparer<T> EqualityComparer
        {
            get { return m_Comparer; }
            set { m_Comparer = value ?? CompareUtils.DefaultComparer<T>(); }
        }

        #endregion // Properties

        #region State Checks
        
        /// <summary>
        /// Returns if the buffer is currently full to capacity.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsFull()
        {
            return m_Count == m_Capacity;
        }

        /// <summary>
        /// Returns if the given element is present in the buffer.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
        [Il2CppSetOption(Option.NullChecks, false)]
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
#if EXPANDED_REFS
        public int IndexOf(in T inItem)
#else
        public int IndexOf(T inItem)
#endif // EXPANDED_REFS
        {
            if (m_Count <= 0)
                return -1;

            var eq = m_Comparer;
            int ptr = m_Head;
            int idx = 0;
            int count = m_Count;
            while(idx < count)
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
            m_Tail = m_Count % m_Capacity;
        }

        static private int GetDesiredCapacity(int inCurrentCapacity)
        {
            int newCapacity = Mathf.NextPowerOfTwo(inCurrentCapacity);
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
            [Il2CppSetOption(Option.NullChecks, false)]
            [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
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
            [Il2CppSetOption(Option.NullChecks, false)]
            [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
            get
            {
                if (inIndex < 0 || inIndex >= m_Count)
                    throw new ArgumentOutOfRangeException("inIndex");
                return m_Data[(m_Head + inIndex) % m_Capacity];
            }
            
            [Il2CppSetOption(Option.NullChecks, false)]
            [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
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
        [Il2CppSetOption(Option.NullChecks, false)]
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T PeekFront()
        {
            if (m_Count <= 0)
                throw new InvalidOperationException("Buffer is empty");
            
            return m_Data[m_Head];
        }

        /// <summary>
        /// Peeks at the value at the back of the buffer.
        /// </summary>
        [Il2CppSetOption(Option.NullChecks, false)]
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
        [Il2CppSetOption(Option.NullChecks, false)]
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
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
        [Il2CppSetOption(Option.NullChecks, false)]
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
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
        [Il2CppSetOption(Option.NullChecks, false)]
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
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
        [Il2CppSetOption(Option.NullChecks, false)]
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
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
        /// Removes the given element from the buffer.
        /// Preserves element order at the cost of speed.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#if EXPANDED_REFS
        public bool Remove(in T inValue)
#else
        public bool Remove(T inValue)
#endif // EXPANDED_REFS
        {
            int idx = IndexOf(inValue);
            if (idx < 0)
                return false;

            RemoveAt(idx);
            return true;
        }

        /// <summary>
        /// Removes the given element from the buffer by swapping.
        /// Does not preserve element order.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
        /// Removes the entry at the given index.
        /// Preserves element order at the cost of speed.
        /// </summary>
        public void RemoveAt(int inIndex)
        {
            if (inIndex < 0 || inIndex >= m_Count)
                throw new ArgumentOutOfRangeException("inIndex");

            int entryIdx = (m_Head + inIndex) % m_Capacity;
            int tailIdx = (m_Tail + m_Capacity - 1) % m_Capacity;

            if (entryIdx != tailIdx)
            {
                int remainder = m_Count - inIndex - 1;
            
                if (m_Head < m_Tail)
                {
                    Array.Copy(m_Data, entryIdx + 1, m_Data, entryIdx, remainder);
                }
                else
                {
                    // TODO: Do this in blocks with Array.Copy
                    for(int i = 0; i < remainder; i++)
                    {
                        m_Data[(entryIdx + i) % m_Capacity] = m_Data[(entryIdx + i + 1) % m_Capacity];
                    }
                }
            }

            m_Data[tailIdx] = default(T);
            m_Tail = tailIdx;
            --m_Count;
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

        /// <summary>
        /// Removes elements who pass the given predicate.
        /// </summary>
        public int RemoveWhere(Predicate<T> inPredicate)
        {
            int removed = 0;
            if (inPredicate != null)
            {
                for(int i = m_Count - 1; i >= 0; i--)
                {
                    if (inPredicate(m_Data[(m_Head + i) % m_Capacity]))
                    {
                        FastRemoveAt(i);
                        removed++;
                    }
                }
            }
            return removed;
        }

        /// <summary>
        /// Removes elements who pass the given predicate.
        /// </summary>
        public int RemoveWhere<TArg>(Predicate<T, TArg> inPredicate, TArg inArg)
        {
            int removed = 0;
            if (inPredicate != null)
            {
                for(int i = m_Count - 1; i >= 0; i--)
                {
                    if (inPredicate(m_Data[(m_Head + i) % m_Capacity], inArg))
                    {
                        FastRemoveAt(i);
                        removed++;
                    }
                }
            }
            return removed;
        }

        #endregion // Remove

        #region Cycling

        /// <summary>
        /// Moves the element at the front of the buffer to the back.
        /// </summary>
        public void MoveFrontToBack()
        {
            if (m_Count <= 1)
                return;

            T value = m_Data[m_Head];
            m_Data[m_Head] = default(T);
            m_Head = (m_Head + 1) % m_Capacity;

            m_Data[m_Tail] = value;
            m_Tail = (m_Tail + 1) % m_Capacity;
        }

        /// <summary>
        /// Moves the element at the back of the buffer to the front.
        /// </summary>
        public void MoveBackToFront()
        {
            if (m_Count <= 1)
                return;

            m_Tail = (m_Tail + m_Capacity - 1) % m_Capacity;
            T val = m_Data[m_Tail];
            m_Data[m_Tail] = default(T);

            m_Head = (m_Head + m_Capacity - 1) % m_Capacity;
            m_Data[m_Head] = val;
        }

        /// <summary>
        /// Moves elements from the front of the buffer to the back until they fail to satisfy the given predicate.
        /// Returns the number of elements moved.
        /// </summary>
        public int MoveFrontToBackWhere(Predicate<T> inPredicate)
        {
            if (m_Count <= 1)
                return 0;

            int moved = 0;
            int count = m_Count;
            while(moved < count)
            {
                T val = m_Data[m_Head];
                if (!inPredicate(val))
                {
                    break;
                }

                m_Data[m_Head] = default(T);
                m_Head = (m_Head + 1) % m_Capacity;

                m_Data[m_Tail] = val;
                m_Tail = (m_Tail + 1) % m_Capacity;
                moved++;
            }

            return moved;
        }

        /// <summary>
        /// Moves elements from the front of the buffer to the back until they fail to satisfy the given predicate.
        /// Returns the number of elements moved.
        /// </summary>
        public int MoveFrontToBackWhere<TArg>(Predicate<T, TArg> inPredicate, TArg inArg)
        {
            if (m_Count <= 1)
                return 0;

            int moved = 0;
            int count = m_Count;
            while (moved < count)
            {
                T val = m_Data[m_Head];
                if (!inPredicate(val, inArg))
                {
                    break;
                }

                m_Data[m_Head] = default(T);
                m_Head = (m_Head + 1) % m_Capacity;

                m_Data[m_Tail] = val;
                m_Tail = (m_Tail + 1) % m_Capacity;
                moved++;
            }

            return moved;
        }

        /// <summary>
        /// Moves elements from the back of the buffer to the front until they fail to satisfy the given predicate.
        /// Returns the number of elements moved.
        /// </summary>
        public int MoveBackToFrontWhere(Predicate<T> inPredicate)
        {
            if (m_Count <= 1)
                return 0;

            int moved = 0;
            int count = m_Count;
            while (moved < count)
            {
                int nextTail = (m_Tail + m_Capacity - 1) % m_Capacity;
                T val = m_Data[nextTail];

                if (!inPredicate(val))
                {
                    break;
                }

                m_Tail = nextTail;
                m_Data[m_Tail] = default(T);

                m_Head = (m_Head + m_Capacity - 1) % m_Capacity;
                m_Data[m_Head] = val;

                moved++;
            }

            return moved;
        }

        /// <summary>
        /// Moves elements from the back of the buffer to the front until they fail to satisfy the given predicate.
        /// Returns the number of elements moved.
        /// </summary>
        public int MoveBackToFrontWhere<TArg>(Predicate<T, TArg> inPredicate, TArg inArg)
        {
            if (m_Count <= 1)
                return 0;

            int moved = 0;
            int count = m_Count;
            while (moved < count)
            {
                int nextTail = (m_Tail + m_Capacity - 1) % m_Capacity;
                T val = m_Data[nextTail];

                if (!inPredicate(val, inArg))
                {
                    break;
                }

                m_Tail = nextTail;
                m_Data[m_Tail] = default(T);

                m_Head = (m_Head + m_Capacity - 1) % m_Capacity;
                m_Data[m_Head] = val;

                moved++;
            }

            return moved;
        }

        #endregion // Cycling

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
        /// Sorts the buffer using the given comparer.
        /// </summary>
        public void Sort(Comparison<T> inComparer)
        {
            if (m_Count <= 1)
                return;

            Compress(false);
            using(var noAllocCompare = ArrayUtils.WrapComparison(inComparer))
            {
                Array.Sort(m_Data, m_Head, m_Count, noAllocCompare);
            }
        }

        /// <summary>
        /// Reverses element order in the buffer.
        /// </summary>
        public void Reverse()
        {
            if (m_Count <= 1)
                return;

            Compress(false);
            ArrayUtils.Reverse(m_Data, m_Head, m_Count);
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
                    m_Tail = m_Count % m_Capacity;
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
            m_Tail = m_Count % m_Capacity;

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
            if (ioDest == null)
                throw new ArgumentNullException("ioDest");

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

        /// <summary>
        /// Copies the contents of the ring buffer to another ring buffer.
        /// </summary>
        public int CopyTo(RingBuffer<T> ioDest)
        {
            if (ioDest == null)
                throw new ArgumentNullException("ioDest");

            if (m_Count <= 0)
            {
                ioDest.Clear();
                return 0;
            }

            if (ioDest.m_Capacity == m_Capacity)
            {
                Array.Copy(m_Data, 0, ioDest.m_Data, 0, m_Capacity);
                ioDest.m_Head = m_Head;
                ioDest.m_Tail = m_Tail;
                ioDest.m_Count = m_Count;
                return m_Count;
            }
            else if (ioDest.m_Capacity > m_Capacity || ioDest.m_BufferMode != RingBufferMode.Expand)
            {
                Array.Clear(ioDest.m_Data, 0, ioDest.m_Capacity);
                int copied = CopyTo(0, ioDest.m_Data, 0, m_Count);
                ioDest.m_Head = 0;
                ioDest.m_Tail = copied;
                ioDest.m_Count = copied;
                return copied;
            }
            else
            {
                Array.Clear(ioDest.m_Data, 0, ioDest.m_Capacity);
                Array.Resize(ref ioDest.m_Data, m_Capacity);
                int copied = CopyTo(0, ioDest.m_Data, 0, m_Count);
                ioDest.m_Head = 0;
                ioDest.m_Tail = m_Count % m_Capacity;
                ioDest.m_Count = m_Count;
                ioDest.m_Capacity = m_Capacity;
                return copied;
            }
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

            if (m_Head < m_Tail && inFlags == 0)
            {
                return Array.BinarySearch(m_Data, m_Head, m_Count, inValue, inComparer);
            }

            CompareWrapper<T> comparer = CompareWrapper<T>.Wrap(inComparer, inFlags);

            int low = 0;
            int high = m_Count - 1;

            while(low <= high)
            {
                int med = low + ((high - low) >> 1);
                int comp = comparer.Compare(m_Data[(m_Head + med) % m_Capacity], inValue);
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

        #region Predicates

        /// <summary>
        /// Returns if an element passing the given predicate exists.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Exists(Predicate<T> inPredicate)
        {
            return FindIndex(inPredicate) >= 0;
        }

        /// <summary>
        /// Returns the index of the first element that passes the given predicate.
        /// </summary>
        public int FindIndex(Predicate<T> inPredicate)
        {
            if (m_Count <= 0)
                return -1;

            int ptr = m_Head;
            int idx = 0;
            int count = m_Count;
            while(idx < count)
            {
                if (inPredicate(m_Data[ptr]))
                    return idx;
                
                ptr = (ptr + 1) % m_Capacity;
                ++idx;
            }

            return -1;
        }

        /// <summary>
        /// Returns the the first element that passes the given predicate.
        /// </summary>
        public T Find(Predicate<T> inPredicate)
        {
            int idx = FindIndex(inPredicate);
            if (idx >= 0)
                return m_Data[(m_Head + idx) % m_Capacity];
            return default(T);
        }

        /// <summary>
        /// Returns if an element passing the given predicate exists.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Exists<U>(Predicate<T, U> inPredicate, U inArg)
        {
            return FindIndex<U>(inPredicate, inArg) >= 0;
        }

        /// <summary>
        /// Returns the index of the first element that passes the given predicate.
        /// </summary>
        public int FindIndex<U>(Predicate<T, U> inPredicate, U inArg)
        {
            if (m_Count <= 0)
                return -1;

            int ptr = m_Head;
            int idx = 0;
            int count = m_Count;
            while(idx < count)
            {
                if (inPredicate(m_Data[ptr], inArg))
                    return idx;
                
                ptr = (ptr + 1) % m_Capacity;
                ++idx;
            }

            return -1;
        }

        /// <summary>
        /// Returns the the first element that passes the given predicate.
        /// </summary>
        public T Find<U>(Predicate<T, U> inPredicate, U inArg)
        {
            int idx = FindIndex<U>(inPredicate, inArg);
            if (idx >= 0)
                return m_Data[(m_Head + idx) % m_Capacity];
            return default(T);
        }

        #endregion // Predicates

        #region Iteration

        /// <summary>
        /// Executes an action for each element in the buffer.
        /// </summary>
        public void ForEach(Action<T> inAction)
        {
            if (m_Count <= 0)
                return;

            int ptr = m_Head;
            int idx = 0;
            int count = m_Count;
            while(idx < count)
            {
                inAction(m_Data[ptr]);
                ptr = (ptr + 1) % m_Capacity;
                ++idx;
            }
        }

        /// <summary>
        /// Executes an action for each element in the buffer.
        /// </summary>
        public void ForEach(RefAction<T> inAction)
        {
            if (m_Count <= 0)
                return;

            int ptr = m_Head;
            int idx = 0;
            int count = m_Count;
            while(idx < count)
            {
                inAction(ref m_Data[ptr]);
                ptr = (ptr + 1) % m_Capacity;
                ++idx;
            }
        }

        #endregion // Iteration

        #region Interfaces

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        bool ICollection.IsSynchronized { get { return false; } }

        object ICollection.SyncRoot { get { return null; } }

        int ICollection<T>.Count { get { return m_Count; } }

        bool ICollection<T>.IsReadOnly { get { return false; } }

        T IList<T>.this[int index] { get { return this[index]; } set { this[index] = value; } }

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

        int IList<T>.IndexOf(T item) {
            return IndexOf(item);
        }

        void IList<T>.Insert(int index, T item) {
            throw new NotSupportedException();
        }

        void IList<T>.RemoveAt(int index) {
            RemoveAt(index);
        }

        void ICollection<T>.Add(T item) {
            PushBack(item);
        }

        void ICollection<T>.Clear() {
            Clear();
        }

        bool ICollection<T>.Contains(T item) {
            return Contains(item);
        }

        void ICollection<T>.CopyTo(T[] array, int arrayIndex) {
            CopyTo(0, array, arrayIndex, array.Length - arrayIndex);
        }

        bool ICollection<T>.Remove(T item) {
            return Remove(item);
        }

        #endregion // Interfaces

        #region Overrides

        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }

        #endregion // Overrides

        #region Enumerator

        public struct Enumerator : IEnumerator<T>, IDisposable
        {
            private RingBuffer<T> m_Buffer;
            private int m_Head;
            private int m_Index;
            private int m_Count;
            private int m_Capacity;

            public Enumerator(RingBuffer<T> inBuffer)
            {
                m_Buffer = inBuffer;
                m_Head = inBuffer.m_Head;
                m_Index = -1;
                m_Count = inBuffer.m_Count;
                m_Capacity = inBuffer.m_Capacity;
            }

            #region IEnumerator

            public T Current { get { return m_Buffer.m_Data[(m_Head + m_Index) % m_Capacity]; } }

            object IEnumerator.Current { get { return Current; } }

            public void Dispose()
            {
                m_Buffer = null;
            }

            public bool MoveNext()
            {
                return ++m_Index < m_Count;
            }

            public void Reset()
            {
                m_Index = -1;
            }

            #endregion // IEnumerator
        }

        #endregion // Enumerator
    }
}