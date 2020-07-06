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
#endif // CSHARP_7_3_OR_NEWER

using System;
using System.Collections;
using System.Collections.Generic;

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
            inBuffer.Sort(Comparer<T>.Default);
        }

        #endregion // Sort
    
        #region CopyTo


        
        #endregion // CopyTo
    }
}