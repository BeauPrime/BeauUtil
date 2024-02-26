/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    10 June 2020
 * 
 * File:    IRingBuffer.cs
 * Purpose: Shared ring buffer interface.
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
    /// Shared interface for a double-ended queue.
    /// </summary>
    public interface IRingBuffer : ICollection
    {
        int Capacity { get; }
        RingBufferMode BufferMode { get; }

        bool IsFull();
        void Clear();
        void SetCapacity(int inCapacity);

        void RemoveFromFront(int inCount);
        void RemoveFromBack(int inCount);

        void Reverse();
    }

    /// <summary>
    /// Interface for a double-ended queue and ring buffer.
    /// </summary>
    public interface IRingBuffer<T> : IRingBuffer, IReadOnlyList<T>
    {
        new int Count { get; }

        #if EXPANDED_REFS
        bool Contains(in T inValue);
        int IndexOf(in T inValue);
        bool Contains(T inValue);
        int IndexOf(T inValue);
        new ref T this[int inIndex] { get; }
        
        void PushFront(in T inValue);
        void PushBack(in T inValue);
        void PushFront(T inValue);
        void PushBack(T inValue);
#else
        bool Contains(T inValue);
        int IndexOf(T inValue);
        T this[int inIndex] { get; set; }

        void PushFront(T inValue);
        void PushBack(T inValue);
#endif // // EXPANDED_REFS

        T PopFront();
        T PeekFront();

        T PopBack();
        T PeekBack();

        int CopyTo(int inSrcIndex, T[] ioDest, int inDestIndex, int inLength);

        void Sort(IComparer<T> inComparer);
    }
}