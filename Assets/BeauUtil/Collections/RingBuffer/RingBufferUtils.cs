/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    10 June 2020
 * 
 * File:    RingBufferUtils.cs
 * Purpose: Utilities for dealing with ring buffers.
 */

#if CSHARP_7_3_OR_NEWER
#define EXPANDED_REFS
#define UNMANAGED_CONSTRAINT
#endif // CSHARP_7_3_OR_NEWER

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace BeauUtil
{
    /// <summary>
    /// Ring buffer utilities.
    /// </summary>
    static public class RingBufferUtils
    {
        #region Pop

        /// <summary>
        /// Attempts to pop the next available value from the front of the buffer.
        /// </summary>
        static public bool TryPopFront<T>(this IRingBuffer<T> ioBuffer, out T outValue)
        {
            if (ioBuffer.Count <= 0)
            {
                outValue = default(T);
                return false;
            }

            outValue = ioBuffer.PopFront();
            return true;
        }

        /// <summary>
        /// Attempts to pop the next available value from the back of the buffer.
        /// </summary>
        static public bool TryPopBack<T>(this IRingBuffer<T> ioBuffer, out T outValue)
        {
            if (ioBuffer.Count <= 0)
            {
                outValue = default(T);
                return false;
            }

            outValue = ioBuffer.PopBack();
            return true;
        }

        #endregion // Pop

        #region Peek

        /// <summary>
        /// Attempts to peek the next available value from the front of the buffer.
        /// </summary>
        static public bool TryPeekFront<T>(this IRingBuffer<T> inBuffer, out T outValue)
        {
            if (inBuffer.Count <= 0)
            {
                outValue = default(T);
                return false;
            }

            outValue = inBuffer.PeekFront();
            return true;
        }

        /// <summary>
        /// Attempts to peek the next available value from the back of the buffer.
        /// </summary>
        static public bool TryPeekBack<T>(this IRingBuffer<T> inBuffer, out T outValue)
        {
            if (inBuffer.Count <= 0)
            {
                outValue = default(T);
                return false;
            }

            outValue = inBuffer.PeekBack();
            return true;
        }

        #endregion // Peek

        #region Sort

        /// <summary>
        /// Sorts the given buffer with the default comparer.
        /// </summary>
        static public void Sort<T>(this IRingBuffer<T> inBuffer)
        {
            inBuffer.Sort(CompareUtils.DefaultSort<T>());
        }

#if UNMANAGED_CONSTRAINT

        /// <summary>
        /// Sorts the given unsafe buffer with the default comparer.
        /// </summary>
        static public unsafe void Quicksort<T>(this RingBuffer<T> inBuffer)
            where T : unmanaged
        {
            if (inBuffer.Count <= 1)
                return;

            inBuffer.Unpack(out T[] arr, out int offset, out int length);
            
            fixed (T* ptr = arr)
            {
                Unsafe.Quicksort(ptr + offset, length, CompareUtils.DefaultSort<T>());
            }
        }

        /// <summary>
        /// Sorts the given unsafe buffer with the given comparer.
        /// </summary>
        static public unsafe void Quicksort<T>(this RingBuffer<T> inBuffer, Comparison<T> inComparison)
            where T : unmanaged
        {
            if (inBuffer.Count <= 1)
                return;

            inBuffer.Unpack(out T[] arr, out int offset, out int length);
            fixed (T* ptr = arr)
            {
                Unsafe.Quicksort(ptr + offset, length, inComparison);
            }
        }

        /// <summary>
        /// Sorts the given unsafe buffer with the given comparer.
        /// </summary>
        static public unsafe void Quicksort<T>(this RingBuffer<T> inBuffer, IComparer<T> inComparer)
            where T : unmanaged
        {
            if (inBuffer.Count <= 1)
                return;

            inBuffer.Unpack(out T[] arr, out int offset, out int length);
            fixed (T* ptr = arr)
            {
                Unsafe.Quicksort(ptr + offset, length, inComparer);
            }
        }

        /// <summary>
        /// Sorts the given unsafe buffer with the given comparer.
        /// </summary>
        static public unsafe void Quicksort<T>(this RingBuffer<T> inBuffer, Unsafe.ComparisonPtr<T> inComparer)
            where T : unmanaged
        {
            if (inBuffer.Count <= 1)
                return;

            inBuffer.Unpack(out T[] arr, out int offset, out int length);
            fixed (T* ptr = arr)
            {
                Unsafe.Quicksort(ptr + offset, length, inComparer);
            }
        }

        /// <summary>
        /// Sorts the given unsafe buffer with the given comparer.
        /// </summary>
        static public unsafe void Quicksort<T>(this RingBuffer<T> inBuffer, Unsafe.ComparisonGetSortingValue<T> inSortingValue)
            where T : unmanaged
        {
            if (inBuffer.Count <= 1)
                return;

            inBuffer.Unpack(out T[] arr, out int offset, out int length);
            fixed (T* ptr = arr)
            {
                Unsafe.Quicksort(ptr + offset, length, inSortingValue);
            }
        }

#endif // UNMANAGED_CONSTRAINT

        #endregion // Sort

        #region CopyTo

        /// <summary>
        /// Copies the given buffer into a destination array.
        /// </summary>
        static public void CopyTo<T>(this IRingBuffer<T> inBuffer, T[] ioDest)
        {
            inBuffer.CopyTo(0, ioDest, 0, inBuffer.Count);
        }

        /// <summary>
        /// Copies the given buffer into a destination array.
        /// </summary>
        static public void CopyTo<T>(this IRingBuffer<T> inBuffer, int inIndex, T[] ioDest)
        {
            inBuffer.CopyTo(inIndex, ioDest, 0, inBuffer.Count - inIndex);
        }

        /// <summary>
        /// Copies the given buffer into a new array.
        /// </summary>
        static public T[] ToArray<T>(this IRingBuffer<T> inBuffer)
        {
            T[] array = new T[inBuffer.Count];
            CopyTo(inBuffer, array);
            return array;
        }
        
        #endregion // CopyTo
    }
}