/*
 * Copyright (C) 2023. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    5 July 2023
 * 
 * File:    UnsafeSpan.cs
 * Purpose: Unsafe range wrapper.
 */

#if CSHARP_7_3_OR_NEWER
#define UNMANAGED_CONSTRAINT
#endif // CSHARP_7_3_OR_NEWER

#if (UNITY_EDITOR && !IGNORE_UNITY_EDITOR) || DEVELOPMENT_BUILD || DEVELOPMENT
#define HAS_DEBUGGER
#endif // UNITY_EDITOR

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace BeauUtil
{
    #if UNMANAGED_CONSTRAINT

    /// <summary>
    /// Memory span enumerator.
    /// </summary>
    public struct UnsafeEnumerator<T> : IEnumerator<T>
        where T : unmanaged
    {
        public unsafe T* Ptr;
        public unsafe T* End;

        public unsafe UnsafeEnumerator(T* inPtr, uint inLength)
        {
            Ptr = inPtr - 1;
            End = inPtr + inLength;
        }

        public unsafe UnsafeEnumerator(T* inPtr, T* inEnd) {
            Ptr = inPtr - 1;
            End = inEnd < inPtr ? inPtr : inEnd;
        }

        public unsafe T Current { get { return *Ptr; } }

        unsafe object IEnumerator.Current { get { return *Ptr; } }

        public unsafe void Dispose() {
            Ptr = End = null;
        }

        public unsafe bool MoveNext() {
            return ++Ptr < End;
        }

        public void Reset() {
            throw new NotSupportedException();
        }
    }

    #endif // UNMANAGED_CONSTRAINT
}