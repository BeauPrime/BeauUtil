/*
 * Copyright (C) 2022. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    10 June 2022
 * 
 * File:    Unsafe.Array.cs
 * Purpose: Unsafe utility methods.
 */

#if CSHARP_7_3_OR_NEWER
#define UNMANAGED_CONSTRAINT
#endif // CSHARP_7_3_OR_NEWER

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace BeauUtil
{
    /// <summary>
    /// Contains unsafe utility functions.
    /// </summary>
    static public unsafe partial class Unsafe
    {
        #region Copy

        #if UNMANAGED_CONSTRAINT

        // pointer to pointer

        /// <summary>
        /// Copies memory from one buffer to another.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public void CopyArray<T>(T* inSrc, int inSrcCount, T* inDest, int inDestCount)
            where T : unmanaged
        {
            int size = sizeof(T);
            Buffer.MemoryCopy(inSrc, inDest, inDestCount * size, inSrcCount * size);
        }

        /// <summary>
        /// Copies memory from one buffer to another.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public void CopyArray<T>(T* inSrc, long inSrcCount, T* inDest, long inDestCount)
            where T : unmanaged
        {
            int size = sizeof(T);
            Buffer.MemoryCopy(inSrc, inDest, inDestCount * size, inSrcCount * size);
        }

        /// <summary>
        /// Copies memory from one buffer to another.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public void CopyArray<T>(T* inSrc, int inCount, T* inDest)
            where T : unmanaged
        {
            int size = sizeof(T);
            Buffer.MemoryCopy(inSrc, inDest, inCount * size, inCount * size);
        }

        /// <summary>
        /// Copies memory from one buffer to another.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public void CopyArray<T>(T* inSrc, long inCount, T* inDest)
            where T : unmanaged
        {
            int size = sizeof(T);
            Buffer.MemoryCopy(inSrc, inDest, inCount * size, inCount * size);
        }

        /// <summary>
        /// Copies memory from one buffer to another and increments the destination pointer.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public void CopyArrayIncrement<T>(T* inSrc, int inSrcCount, T** inDest, int* inDestCountRemaining)
            where T : unmanaged
        {
            int size = sizeof(T);
            Buffer.MemoryCopy(inSrc, inDest, *inDestCountRemaining * size, inSrcCount * size);
            *inDest += inSrcCount;
            *inDestCountRemaining -= inSrcCount;
        }

        /// <summary>
        /// Copies memory from one buffer to another and increments the destination pointer.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public void CopyArrayIncrement<T>(T* inSrc, long inSrcCount, T** inDest, long* inDestCountRemaining)
            where T : unmanaged
        {
            int size = sizeof(T);
            Buffer.MemoryCopy(inSrc, inDest, *inDestCountRemaining * size, inSrcCount * size);
            *inDest += inSrcCount;
            *inDestCountRemaining -= inSrcCount;
        }

        /// <summary>
        /// Copies memory from one buffer to another and increments the destination pointer.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public void CopyArrayIncrement<T>(T* inSrc, int inCount, T** inDest)
            where T : unmanaged
        {
            int size = sizeof(T);
            Buffer.MemoryCopy(inSrc, inDest, inCount * size, inCount * size);
            *inDest += inCount;
        }

        /// <summary>
        /// Copies memory from one buffer to another and increments the destination pointer.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public void CopyArrayIncrement<T>(T* inSrc, long inCount, T** inDest)
            where T : unmanaged
        {
            int size = sizeof(T);
            Buffer.MemoryCopy(inSrc, inDest, inCount * size, inCount * size);
            *inDest += inCount;
        }

        // pointer to array

        /// <summary>
        /// Copies memory from a buffer to an array.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public void CopyArray<T>(T* inSrc, int inSrcCount, T[] inDest)
            where T : unmanaged
        {
            fixed(T* destPtr = inDest)
            {
                int size = sizeof(T);
                Buffer.MemoryCopy(inSrc, destPtr, inDest.Length * size, inSrcCount * size);
            }
        }

        /// <summary>
        /// Copies memory from a buffer to an array.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public void CopyArray<T>(T* inSrc, long inSrcCount, T[] inDest)
            where T : unmanaged
        {
            fixed(T* destPtr = inDest)
            {
                int size = sizeof(T);
                Buffer.MemoryCopy(inSrc, destPtr, inDest.Length * size, inSrcCount * size);
            }
        }

        /// <summary>
        /// Copies memory from a buffer to an array.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public void CopyArray<T>(T* inSrc, int inSrcCount, T[] inDest, int inDestOffset)
            where T : unmanaged
        {
            fixed(T* destPtr = inDest)
            {
                int size = sizeof(T);
                Buffer.MemoryCopy(inSrc, destPtr + inDestOffset, (inDest.Length - inDestOffset) * size, inSrcCount * size);
            }
        }

        /// <summary>
        /// Copies memory from a buffer to an array.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public void CopyArray<T>(T* inSrc, long inSrcCount, T[] inDest, long inDestOffset)
            where T : unmanaged
        {
            fixed(T* destPtr = inDest)
            {
                int size = sizeof(T);
                Buffer.MemoryCopy(inSrc, destPtr + inDestOffset, (inDest.Length - inDestOffset) * size, inSrcCount * size);
            }
        }

        // array to pointer

        /// <summary>
        /// Copies memory from an array to a buffer.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public void CopyArray<T>(T[] inSrc, T* inDest, int inDestCount)
            where T : unmanaged
        {
            CopyArray<T>(inSrc, 0, inSrc.Length, inDest, inDestCount);
        }

        /// <summary>
        /// Copies memory from an array to a buffer.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public void CopyArray<T>(T[] inSrc, T* inDest, long inDestCount)
            where T : unmanaged
        {
            CopyArray<T>(inSrc, 0, inSrc.Length, inDest, inDestCount);
        }

        /// <summary>
        /// Copies memory from an array to a buffer.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public void CopyArray<T>(T[] inSrc, int inSrcOffset, T* inDest, int inDestCount)
            where T : unmanaged
        {
            CopyArray<T>(inSrc, inSrcOffset, inSrc.Length - inSrcOffset, inDest, inDestCount);
        }

        /// <summary>
        /// Copies memory from an array to a buffer.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public void CopyArray<T>(T[] inSrc, long inSrcOffset, T* inDest, long inDestCount)
            where T : unmanaged
        {
            CopyArray<T>(inSrc, inSrcOffset, inSrc.Length - inSrcOffset, inDest, inDestCount);
        }

        /// <summary>
        /// Copies memory from an array to a buffer.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public void CopyArray<T>(T[] inSrc, int inSrcOffset, int inSrcCount, T* inDest, int inDestCount)
            where T : unmanaged
        {
            fixed(T* srcPtr = inSrc)
            {
                int size = sizeof(T);
                Buffer.MemoryCopy(srcPtr + inSrcOffset, inDest, inDestCount * size, inSrcCount * size);
            }
        }

        /// <summary>
        /// Copies memory from an array to a buffer.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public void CopyArray<T>(T[] inSrc, long inSrcOffset, long inSrcCount, T* inDest, long inDestCount)
            where T : unmanaged
        {
            fixed(T* srcPtr = inSrc)
            {
                int size = sizeof(T);
                Buffer.MemoryCopy(srcPtr + inSrcOffset, inDest, inDestCount * size, inSrcCount * size);
            }
        }

        /// <summary>
        /// Copies memory from an array to a buffer and increments the destination pointer.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public void CopyArrayIncrement<T>(T[] inSrc, T** inDest, int* inDestCountRemaining)
            where T : unmanaged
        {
            CopyArrayIncrement<T>(inSrc, 0, inSrc.Length, inDest, inDestCountRemaining);
        }

        /// <summary>
        /// Copies memory from an array to a buffer and increments the destination pointer.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public void CopyArrayIncrement<T>(T[] inSrc, T** inDest, long* inDestCountRemaining)
            where T : unmanaged
        {
            CopyArrayIncrement<T>(inSrc, 0, inSrc.Length, inDest, inDestCountRemaining);
        }

        /// <summary>
        /// Copies memory from an array to a buffer.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public void CopyArrayIncrement<T>(T[] inSrc, int inSrcOffset, T** inDest, int* inDestCountRemaining)
            where T : unmanaged
        {
            CopyArrayIncrement<T>(inSrc, inSrcOffset, inSrc.Length - inSrcOffset, inDest, inDestCountRemaining);
        }

        /// <summary>
        /// Copies memory from an array to a buffer.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public void CopyArrayIncrement<T>(T[] inSrc, long inSrcOffset, T** inDest, long* inDestCountRemaining)
            where T : unmanaged
        {
            CopyArrayIncrement<T>(inSrc, inSrcOffset, inSrc.Length - inSrcOffset, inDest, inDestCountRemaining);
        }

        /// <summary>
        /// Copies memory from an array to a buffer.
        /// </summary>
        static public void CopyArrayIncrement<T>(T[] inSrc, int inSrcOffset, int inSrcCount, T** inDest, int* inDestCountRemaining)
            where T : unmanaged
        {
            fixed(T* srcPtr = inSrc)
            {
                int size = sizeof(T);
                Buffer.MemoryCopy(srcPtr + inSrcOffset, *inDest, *inDestCountRemaining * size, inSrcCount * size);
                *inDest += inSrcCount;
                *inDestCountRemaining -= inSrcCount;
            }
        }

        /// <summary>
        /// Copies memory from an array to a buffer.
        /// </summary>
        static public void CopyArrayIncrement<T>(T[] inSrc, long inSrcOffset, long inSrcCount, T** inDest, long* inDestCountRemaining)
            where T : unmanaged
        {
            fixed(T* srcPtr = inSrc)
            {
                int size = sizeof(T);
                Buffer.MemoryCopy(srcPtr + inSrcOffset, *inDest, *inDestCountRemaining * size, inSrcCount * size);
                *inDest += inSrcCount;
                *inDestCountRemaining -= inSrcCount;
            }
        }

        #else

        // pointer to pointer

        /// <summary>
        /// Copies memory from one buffer to another.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public void CopyArray<T>(void* inSrc, int inSrcCount, void* inDest, int inDestCount)
            where T : struct
        {
            int size = SizeOf<T>();
            Buffer.MemoryCopy(inSrc, inDest, inDestCount * size, inSrcCount * size);
        }

        /// <summary>
        /// Copies memory from one buffer to another.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public void CopyArray<T>(void* inSrc, long inSrcCount, void* inDest, long inDestCount)
            where T : struct
        {
            int size = SizeOf<T>();
            Buffer.MemoryCopy(inSrc, inDest, inDestCount * size, inSrcCount * size);
        }

        /// <summary>
        /// Copies memory from one buffer to another.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public void CopyArray<T>(void* inSrc, int inCount, void* inDest)
            where T : struct
        {
            int size = SizeOf<T>();
            Buffer.MemoryCopy(inSrc, inDest, inCount * size, inCount * size);
        }

        /// <summary>
        /// Copies memory from one buffer to another.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public void CopyArray<T>(void* inSrc, long inCount, void* inDest)
            where T : struct
        {
            int size = SizeOf<T>();
            Buffer.MemoryCopy(inSrc, inDest, inCount * size, inCount * size);
        }

        /// <summary>
        /// Copies memory from one buffer to another and increments the destination pointer.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public void CopyArrayIncrement<T>(void* inSrc, int inSrcCount, void** inDest, int* inDestCountRemaining)
            where T : struct
        {
            int size = SizeOf<T>();
            Buffer.MemoryCopy(inSrc, inDest, *inDestCountRemaining * size, inSrcCount * size);
            *((byte**) inDest) += inSrcCount * size;
            *inDestCountRemaining -= inSrcCount;
        }

        /// <summary>
        /// Copies memory from one buffer to another and increments the destination pointer.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public void CopyArrayIncrement<T>(void* inSrc, long inSrcCount, void** inDest, long* inDestCountRemaining)
            where T : struct
        {
            int size = SizeOf<T>();
            Buffer.MemoryCopy(inSrc, inDest, *inDestCountRemaining * size, inSrcCount * size);
            *((byte**) inDest) += inSrcCount * size;
            *inDestCountRemaining -= inSrcCount;
        }

        /// <summary>
        /// Copies memory from one buffer to another and increments the destination pointer.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public void CopyArrayIncrement<T>(void* inSrc, int inCount, void** inDest)
            where T : struct
        {
            int size = SizeOf<T>();
            Buffer.MemoryCopy(inSrc, inDest, inCount * size, inCount * size);
            *((byte**) inDest) += inCount * size;
        }

        /// <summary>
        /// Copies memory from one buffer to another and increments the destination pointer.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public void CopyArrayIncrement<T>(void* inSrc, long inCount, void** inDest)
            where T : struct
        {
            int size = SizeOf<T>();
            Buffer.MemoryCopy(inSrc, inDest, inCount * size, inCount * size);
            *((byte**) inDest) += inCount * size;
        }

        // pointer to array

        /// <summary>
        /// Copies memory from a buffer to an array.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public void CopyArray<T>(void* inSrc, int inSrcCount, T[] inDest)
            where T : struct
        {
            CopyArray<T>(inSrc, inSrcCount, inDest, 0);
        }

        /// <summary>
        /// Copies memory from a buffer to an array.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public void CopyArray<T>(void* inSrc, long inSrcCount, T[] inDest)
            where T : struct
        {
            CopyArray<T>(inSrc, inSrcCount, inDest, 0);
        }

        /// <summary>
        /// Copies memory from a buffer to an array.
        /// </summary>
        static public void CopyArray<T>(void* inSrc, int inSrcCount, T[] inDest, int inDestOffset)
            where T : struct
        {
            using(var pin = PinArray(inDest))
            {
                void* destPtr = pin.ElementAddress(inDestOffset);
                int size = pin.ElementSize;
                Buffer.MemoryCopy(inSrc, destPtr, (inDest.Length - inDestOffset) * size, inSrcCount * size);
            }
        }


        /// <summary>
        /// Copies memory from a buffer to an array.
        /// </summary>
        static public void CopyArray<T>(void* inSrc, long inSrcCount, T[] inDest, long inDestOffset)
            where T : struct
        {
            using(var pin = PinArray(inDest))
            {
                void* destPtr = pin.ElementAddress(inDestOffset);
                int size = pin.ElementSize;
                Buffer.MemoryCopy(inSrc, destPtr, (inDest.Length - inDestOffset) * size, inSrcCount * size);
            }
        }

        // array to pointer

        /// <summary>
        /// Copies memory from an array to a buffer.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public void CopyArray<T>(T[] inSrc, void* inDest, int inDestCount)
            where T : struct
        {
            CopyArray<T>(inSrc, 0, inSrc.Length, inDest, inDestCount);
        }

        /// <summary>
        /// Copies memory from an array to a buffer.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public void CopyArray<T>(T[] inSrc, void* inDest, long inDestCount)
            where T : struct
        {
            CopyArray<T>(inSrc, 0, inSrc.Length, inDest, inDestCount);
        }

        /// <summary>
        /// Copies memory from an array to a buffer.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public void CopyArray<T>(T[] inSrc, int inSrcOffset, void* inDest, int inDestCount)
            where T : struct
        {
            CopyArray<T>(inSrc, inSrcOffset, inSrc.Length - inSrcOffset, inDest, inDestCount);
        }

        /// <summary>
        /// Copies memory from an array to a buffer.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public void CopyArray<T>(T[] inSrc, long inSrcOffset, void* inDest, long inDestCount)
            where T : struct
        {
            CopyArray<T>(inSrc, inSrcOffset, inSrc.Length - inSrcOffset, inDest, inDestCount);
        }

        /// <summary>
        /// Copies memory from an array to a buffer.
        /// </summary>
        static public void CopyArray<T>(T[] inSrc, int inSrcOffset, int inSrcCount, void* inDest, int inDestCount)
            where T : struct
        {
            using(var pin = PinArray(inSrc))
            {
                void* srcPtr = pin.ElementAddress(inSrcOffset);
                int size = pin.ElementSize;
                Buffer.MemoryCopy(srcPtr, inDest, inDestCount * size, inSrcCount * size);
            }
        }

        /// <summary>
        /// Copies memory from an array to a buffer.
        /// </summary>
        static public void CopyArray<T>(T[] inSrc, long inSrcOffset, long inSrcCount, void* inDest, long inDestCount)
            where T : struct
        {
            using(var pin = PinArray(inSrc))
            {
                void* srcPtr = pin.ElementAddress(inSrcOffset);
                int size = pin.ElementSize;
                Buffer.MemoryCopy(srcPtr, inDest, inDestCount * size, inSrcCount * size);
            }
        }

        /// <summary>
        /// Copies memory from an array to a buffer.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public void CopyArrayIncrement<T>(T[] inSrc, void** inDest, int* inDestCount)
            where T : struct
        {
            CopyArrayIncrement<T>(inSrc, 0, inSrc.Length, inDest, inDestCount);
        }

        /// <summary>
        /// Copies memory from an array to a buffer.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public void CopyArrayIncrement<T>(T[] inSrc, void** inDest, long* inDestCount)
            where T : struct
        {
            CopyArrayIncrement<T>(inSrc, 0, inSrc.Length, inDest, inDestCount);
        }

        /// <summary>
        /// Copies memory from an array to a buffer.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public void CopyArrayIncrement<T>(T[] inSrc, int inSrcOffset, void** inDest, int* inDestCountRemaining)
            where T : struct
        {
            CopyArrayIncrement<T>(inSrc, inSrcOffset, inSrc.Length - inSrcOffset, inDest, inDestCountRemaining);
        }

        /// <summary>
        /// Copies memory from an array to a buffer.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public void CopyArrayIncrement<T>(T[] inSrc, long inSrcOffset, void** inDest, long* inDestCountRemaining)
            where T : struct
        {
            CopyArrayIncrement<T>(inSrc, inSrcOffset, inSrc.Length - inSrcOffset, inDest, inDestCountRemaining);
        }

        /// <summary>
        /// Copies memory from an array to a buffer.
        /// </summary>
        static public void CopyArrayIncrement<T>(T[] inSrc, int inSrcOffset, int inSrcCount, void** inDest, int* inDestCountRemaining)
            where T : struct
        {
            using(var pin = PinArray(inSrc))
            {
                void* srcPtr = pin.ElementAddress(inSrcOffset);
                int size = pin.ElementSize;
                Buffer.MemoryCopy(srcPtr, inDest, *inDestCountRemaining * size, inSrcCount * size);
                *((byte**) inDest) += inSrcCount * size;
                *inDestCountRemaining -= inSrcCount;
            }
        }

        /// <summary>
        /// Copies memory from an array to a buffer.
        /// </summary>
        static public void CopyArrayIncrement<T>(T[] inSrc, long inSrcOffset, long inSrcCount, void** inDest, long* inDestCountRemaining)
            where T : struct
        {
            using(var pin = PinArray(inSrc))
            {
                void* srcPtr = pin.ElementAddress(inSrcOffset);
                int size = pin.ElementSize;
                Buffer.MemoryCopy(srcPtr, inDest, *inDestCountRemaining * size, inSrcCount * size);
                *((byte**) inDest) += inSrcCount * size;
                *inDestCountRemaining -= inSrcCount;
            }
        }

        #endif // UNMANAGED_CONSTRAINT

        /// <summary>
        /// Copies memory from one buffer to another.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public void Copy(void* inSrc, int inSrcSize, void* inDest, int inDestSize)
        {
            Buffer.MemoryCopy(inSrc, inDest, inDestSize, inSrcSize);
        }

        /// <summary>
        /// Copies memory from one buffer to another.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public void Copy(void* inSrc, long inSrcSize, void* inDest, long inDestSize)
        {
            Buffer.MemoryCopy(inSrc, inDest, inDestSize, inSrcSize);
        }

        /// <summary>
        /// Copies memory from one buffer to another.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public void Copy(void* inSrc, int inSize, void* inDest)
        {
            Buffer.MemoryCopy(inSrc, inDest, inSize, inSize);
        }

        /// <summary>
        /// Copies memory from one buffer to another.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public void Copy(void* inSrc, long inSize, void* inDest)
        {
            Buffer.MemoryCopy(inSrc, inDest, inSize, inSize);
        }

        /// <summary>
        /// Copies memory from one buffer to another and increments the destination pointer.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public void CopyIncrement(void* inSrc, int inSrcSize, void** inDest, int* inDestSizeRemaining)
        {
            Buffer.MemoryCopy(inSrc, *inDest, *inDestSizeRemaining, inSrcSize);
            (*(byte**)inDest) += inSrcSize;
            *inDestSizeRemaining -= inSrcSize;
        }

        /// <summary>
        /// Copies memory from one buffer to another and increments the destination pointer.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public void CopyIncrement(void* inSrc, long inSrcSize, void** inDest, long* inDestSizeRemaining)
        {
            Buffer.MemoryCopy(inSrc, *inDest, *inDestSizeRemaining, inSrcSize);
            (*(byte**)inDest) += inSrcSize;
            *inDestSizeRemaining -= inSrcSize;
        }

        /// <summary>
        /// Copies memory from one buffer to another and increments the destination pointer.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public void CopyIncrement(void* inSrc, int inSize, void** inDest)
        {
            Buffer.MemoryCopy(inSrc, *inDest, inSize, inSize);
            (*(byte**)inDest) += inSize;
        }

        /// <summary>
        /// Copies memory from one buffer to another and increments the destination pointer.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public void CopyIncrement(void* inSrc, long inSize, void** inDest)
        {
            Buffer.MemoryCopy(inSrc, *inDest, inSize, inSize);
            (*(byte**)inDest) += inSize;
        }

        #endregion // Copy

        #region Sort

        #if UNMANAGED_CONSTRAINT

        /// <summary>
        /// Quicksorts a buffer using the default comparer object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public void Quicksort<T>(T* ioBuffer, int inCount)
            where T : unmanaged
        {
            Quicksort<T>(ioBuffer, 0, inCount - 1);
        }

        /// <summary>
        /// Quicksorts a buffer using the given comparer object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public void Quicksort<T>(T* ioBuffer, int inCount, IComparer<T> inComparison)
            where T : unmanaged
        {
            Quicksort<T>(ioBuffer, 0, inCount - 1, inComparison);
        }

        /// <summary>
        /// Quicksorts a buffer using the given comparison function.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public void Quicksort<T>(T* ioBuffer, int inCount, Comparison<T> inComparison)
            where T : unmanaged
        {
            Quicksort<T>(ioBuffer, 0, inCount - 1, inComparison);
        }

        /// <summary>
        /// Quicksorts a buffer using the given comparison function.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public void Quicksort<T>(T* ioBuffer, int inCount, ComparisonPtr<T> inComparison)
            where T : unmanaged
        {
            Quicksort<T>(ioBuffer, 0, inCount - 1, inComparison);
        }

        /// <summary>
        /// Quicksorts a buffer using the given sorting value function.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public void Quicksort<T>(T* ioBuffer, int inCount, ComparisonGetSortingValue<T> inSortingValues)
            where T : unmanaged
        {
            Quicksort<T>(ioBuffer, 0, inCount - 1, inSortingValues);
        }

        /// <summary>
        /// Quicksorts a buffer using the default comparer object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public void Quicksort<T>(T* ioBuffer, int inLowerIndex, int inUpperIndex)
            where T : unmanaged
        {
            Quicksort<T>(ioBuffer, inLowerIndex, inUpperIndex, Comparer<T>.Default);
        }

        /// <summary>
        /// Quicksorts a buffer using the given comparer object.
        /// </summary>
        static public void Quicksort<T>(T* ioBuffer, int inLowerIndex, int inUpperIndex, IComparer<T> inComparison)
            where T : unmanaged
        {
            if (inLowerIndex >= 0 && inLowerIndex < inUpperIndex)
            {
                // lower higher
                int* indexStack = stackalloc int[inUpperIndex - inLowerIndex + 1];
                int indexStackTop = -1;
                int pivotIndex;

                indexStack[++indexStackTop] = inLowerIndex;
                indexStack[++indexStackTop] = inUpperIndex;

                while(indexStackTop >= 1)
                {
                    inUpperIndex = indexStack[indexStackTop--];
                    inLowerIndex = indexStack[indexStackTop--];

                    pivotIndex = Quicksort_Partition(ioBuffer, inLowerIndex, inUpperIndex, inComparison);
                    if (inLowerIndex < pivotIndex)
                    {
                        indexStack[++indexStackTop] = inLowerIndex;
                        indexStack[++indexStackTop] = pivotIndex;
                    }
                    if (inUpperIndex > pivotIndex + 1)
                    {
                        indexStack[++indexStackTop] = pivotIndex + 1;
                        indexStack[++indexStackTop] = inUpperIndex;
                    }
                }
            }
        }

        /// <summary>
        /// Quicksorts a buffer using the given comparison function.
        /// </summary>
        static public void Quicksort<T>(T* ioBuffer, int inLowerIndex, int inUpperIndex, Comparison<T> inComparison)
            where T : unmanaged
        {
            if (inLowerIndex >= 0 && inLowerIndex < inUpperIndex)
            {
                // lower higher
                int* indexStack = stackalloc int[inUpperIndex - inLowerIndex + 1];
                int indexStackTop = -1;
                int pivotIndex;

                indexStack[++indexStackTop] = inLowerIndex;
                indexStack[++indexStackTop] = inUpperIndex;

                while(indexStackTop >= 1)
                {
                    inUpperIndex = indexStack[indexStackTop--];
                    inLowerIndex = indexStack[indexStackTop--];

                    pivotIndex = Quicksort_Partition(ioBuffer, inLowerIndex, inUpperIndex, inComparison);
                    if (inLowerIndex < pivotIndex)
                    {
                        indexStack[++indexStackTop] = inLowerIndex;
                        indexStack[++indexStackTop] = pivotIndex;
                    }
                    if (inUpperIndex > pivotIndex + 1)
                    {
                        indexStack[++indexStackTop] = pivotIndex + 1;
                        indexStack[++indexStackTop] = inUpperIndex;
                    }
                }
            }
        }

        /// <summary>
        /// Quicksorts a buffer using the given comparison function.
        /// </summary>
        static public void Quicksort<T>(T* ioBuffer, int inLowerIndex, int inUpperIndex, ComparisonPtr<T> inComparison)
            where T : unmanaged
        {
            if (inLowerIndex >= 0 && inLowerIndex < inUpperIndex)
            {
                // lower higher
                int* indexStack = stackalloc int[inUpperIndex - inLowerIndex + 1];
                int indexStackTop = -1;
                int pivotIndex;

                indexStack[++indexStackTop] = inLowerIndex;
                indexStack[++indexStackTop] = inUpperIndex;

                while(indexStackTop >= 1)
                {
                    inUpperIndex = indexStack[indexStackTop--];
                    inLowerIndex = indexStack[indexStackTop--];

                    pivotIndex = Quicksort_Partition(ioBuffer, inLowerIndex, inUpperIndex, inComparison);
                    if (inLowerIndex < pivotIndex)
                    {
                        indexStack[++indexStackTop] = inLowerIndex;
                        indexStack[++indexStackTop] = pivotIndex;
                    }
                    if (inUpperIndex > pivotIndex + 1)
                    {
                        indexStack[++indexStackTop] = pivotIndex + 1;
                        indexStack[++indexStackTop] = inUpperIndex;
                    }
                }
            }
        }

        /// <summary>
        /// Quicksorts a buffer using the given sorting value function.
        /// </summary>
        static public void Quicksort<T>(T* ioBuffer, int inLowerIndex, int inUpperIndex, ComparisonGetSortingValue<T> inSortingValues)
            where T : unmanaged
        {
            if (inLowerIndex >= 0 && inLowerIndex < inUpperIndex)
            {
                // lower higher
                int* indexStack = stackalloc int[inUpperIndex - inLowerIndex + 1];
                int indexStackTop = -1;
                int pivotIndex;

                indexStack[++indexStackTop] = inLowerIndex;
                indexStack[++indexStackTop] = inUpperIndex;

                while(indexStackTop >= 1)
                {
                    inUpperIndex = indexStack[indexStackTop--];
                    inLowerIndex = indexStack[indexStackTop--];

                    pivotIndex = Quicksort_Partition(ioBuffer, inLowerIndex, inUpperIndex, inSortingValues);
                    if (inLowerIndex < pivotIndex)
                    {
                        indexStack[++indexStackTop] = inLowerIndex;
                        indexStack[++indexStackTop] = pivotIndex;
                    }
                    if (inUpperIndex > pivotIndex + 1)
                    {
                        indexStack[++indexStackTop] = pivotIndex + 1;
                        indexStack[++indexStackTop] = inUpperIndex;
                    }
                }
            }
        }

        static private int Quicksort_Partition<T>(T* ioBuffer, int inLower, int inHigher, IComparer<T> inComparison)
            where T : unmanaged
        {
            int center = (inLower + inHigher) >> 1;
            T centerVal = ioBuffer[center];

            int i = inLower - 1;
            int j = inHigher + 1;

            while(true)
            {
                do
                {
                    i++;
                }
                while(inComparison.Compare(ioBuffer[i], centerVal) < 0);

                do
                {
                    j--;
                }
                while(inComparison.Compare(ioBuffer[j], centerVal) > 0);
                
                if (i >= j)
                {
                    return j;
                }

                Ref.Swap(ref ioBuffer[i], ref ioBuffer[j]);
            }
        }

        static private int Quicksort_Partition<T>(T* ioBuffer, int inLower, int inHigher, Comparison<T> inComparison)
            where T : unmanaged
        {
            int center = (inLower + inHigher) >> 1;
            T centerVal = ioBuffer[center];

            int i = inLower - 1;
            int j = inHigher + 1;

            while(true)
            {
                do
                {
                    i++;
                }
                while(inComparison(ioBuffer[i], centerVal) < 0);

                do
                {
                    j--;
                }
                while(inComparison(ioBuffer[j], centerVal) > 0);
                
                if (i >= j)
                {
                    return j;
                }

                Ref.Swap(ref ioBuffer[i], ref ioBuffer[j]);
            }
        }

        static private int Quicksort_Partition<T>(T* ioBuffer, int inLower, int inHigher, ComparisonPtr<T> inComparison)
            where T : unmanaged
        {
            int center = (inLower + inHigher) >> 1;
            T* centerPtr = &ioBuffer[center];

            int i = inLower - 1;
            int j = inHigher + 1;

            while(true)
            {
                do
                {
                    i++;
                }
                while(inComparison(&ioBuffer[i], centerPtr) < 0);

                do
                {
                    j--;
                }
                while(inComparison(&ioBuffer[j], centerPtr) > 0);
                
                if (i >= j)
                {
                    return j;
                }

                Ref.Swap(ref ioBuffer[i], ref ioBuffer[j]);
            }
        }

        static private int Quicksort_Partition<T>(T* ioBuffer, int inLower, int inHigher, ComparisonGetSortingValue<T> inSortingValues)
            where T : unmanaged
        {
            int center = (inLower + inHigher) >> 1;
            float centerVal = inSortingValues(&ioBuffer[center]);

            int i = inLower - 1;
            int j = inHigher + 1;

            while(true)
            {
                do
                {
                    i++;
                }
                while(inSortingValues(&ioBuffer[i]) < centerVal);

                do
                {
                    j--;
                }
                while(inSortingValues(&ioBuffer[j]) > centerVal);
                
                if (i >= j)
                {
                    return j;
                }

                Ref.Swap(ref ioBuffer[i], ref ioBuffer[j]);
            }
        }

        /// <summary>
        /// Comparison delegate for comparing the contents of two unmanaged objects.
        /// </summary>
        public delegate int ComparisonPtr<T>(T* x, T* y) where T : unmanaged;
        
        /// <summary>
        /// Comparison delegate that generates the sorting value for the given unmanaged object.
        /// </summary>
        public delegate float ComparisonGetSortingValue<T>(T* x) where T : unmanaged;

        #else

        /// <summary>
        /// Quicksorts a buffer using the default comparer object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public void Quicksort<T>(void* ioBuffer, int inCount)
            where T : struct
        {
            Quicksort<T>(ioBuffer, 0, inCount - 1);
        }

        /// <summary>
        /// Quicksorts a buffer using the given comparer object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public void Quicksort<T>(void* ioBuffer, int inCount, IComparer<T> inComparison)
            where T : struct
        {
            Quicksort<T>(ioBuffer, 0, inCount - 1, inComparison);
        }

        /// <summary>
        /// Quicksorts a buffer using the given comparison function.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public void Quicksort<T>(void* ioBuffer, int inCount, Comparison<T> inComparison)
            where T : struct
        {
            Quicksort<T>(ioBuffer, 0, inCount - 1, inComparison);
        }

        /// <summary>
        /// Quicksorts a buffer using the given comparison function.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public void Quicksort<T>(void* ioBuffer, int inCount, ComparisonPtr<T> inComparison)
            where T : struct
        {
            Quicksort<T>(ioBuffer, 0, inCount - 1, inComparison);
        }

        /// <summary>
        /// Quicksorts a buffer using the given sorting value function.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public void Quicksort<T>(void* ioBuffer, int inCount, ComparisonGetSortingValue<T> inSortingValues)
            where T : struct
        {
            Quicksort<T>(ioBuffer, 0, inCount - 1, inSortingValues);
        }

        /// <summary>
        /// Quicksorts a buffer using the default comparer object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public void Quicksort<T>(void* ioBuffer, int inLowerIndex, int inUpperIndex)
            where T : struct
        {
            Quicksort<T>(ioBuffer, inLowerIndex, inUpperIndex, Comparer<T>.Default);
        }

        /// <summary>
        /// Quicksorts a buffer using the given comparer object.
        /// </summary>
        static public void Quicksort<T>(void* ioBuffer, int inLowerIndex, int inUpperIndex, IComparer<T> inComparison)
            where T : struct
        {
            if (inLowerIndex >= 0 && inLowerIndex < inUpperIndex)
            {
                // lower higher
                int* indexStack = stackalloc int[inUpperIndex - inLowerIndex + 1];
                int indexStackTop = -1;
                int pivotIndex;

                indexStack[++indexStackTop] = inLowerIndex;
                indexStack[++indexStackTop] = inUpperIndex;

                while(indexStackTop >= 1)
                {
                    inUpperIndex = indexStack[indexStackTop--];
                    inLowerIndex = indexStack[indexStackTop--];

                    pivotIndex = Quicksort_Partition((byte*) ioBuffer, inLowerIndex, inUpperIndex, inComparison);
                    if (inLowerIndex < pivotIndex)
                    {
                        indexStack[++indexStackTop] = inLowerIndex;
                        indexStack[++indexStackTop] = pivotIndex;
                    }
                    if (inUpperIndex > pivotIndex + 1)
                    {
                        indexStack[++indexStackTop] = pivotIndex + 1;
                        indexStack[++indexStackTop] = inUpperIndex;
                    }
                }
            }
        }

        /// <summary>
        /// Quicksorts a buffer using the given comparison function.
        /// </summary>
        static public void Quicksort<T>(void* ioBuffer, int inLowerIndex, int inUpperIndex, Comparison<T> inComparison)
            where T : struct
        {
            if (inLowerIndex >= 0 && inLowerIndex < inUpperIndex)
            {
                // lower higher
                int* indexStack = stackalloc int[inUpperIndex - inLowerIndex + 1];
                int indexStackTop = -1;
                int pivotIndex;

                indexStack[++indexStackTop] = inLowerIndex;
                indexStack[++indexStackTop] = inUpperIndex;

                while(indexStackTop >= 1)
                {
                    inUpperIndex = indexStack[indexStackTop--];
                    inLowerIndex = indexStack[indexStackTop--];

                    pivotIndex = Quicksort_Partition((byte*) ioBuffer, inLowerIndex, inUpperIndex, inComparison);
                    if (inLowerIndex < pivotIndex)
                    {
                        indexStack[++indexStackTop] = inLowerIndex;
                        indexStack[++indexStackTop] = pivotIndex;
                    }
                    if (inUpperIndex > pivotIndex + 1)
                    {
                        indexStack[++indexStackTop] = pivotIndex + 1;
                        indexStack[++indexStackTop] = inUpperIndex;
                    }
                }
            }
        }

        /// <summary>
        /// Quicksorts a buffer using the given comparison function.
        /// </summary>
        static public void Quicksort<T>(void* ioBuffer, int inLowerIndex, int inUpperIndex, ComparisonPtr<T> inComparison)
            where T : struct
        {
            if (inLowerIndex >= 0 && inLowerIndex < inUpperIndex)
            {
                // lower higher
                int* indexStack = stackalloc int[inUpperIndex - inLowerIndex + 1];
                int indexStackTop = -1;
                int pivotIndex;

                indexStack[++indexStackTop] = inLowerIndex;
                indexStack[++indexStackTop] = inUpperIndex;

                while(indexStackTop >= 1)
                {
                    inUpperIndex = indexStack[indexStackTop--];
                    inLowerIndex = indexStack[indexStackTop--];

                    pivotIndex = Quicksort_Partition((byte*) ioBuffer, inLowerIndex, inUpperIndex, inComparison);
                    if (inLowerIndex < pivotIndex)
                    {
                        indexStack[++indexStackTop] = inLowerIndex;
                        indexStack[++indexStackTop] = pivotIndex;
                    }
                    if (inUpperIndex > pivotIndex + 1)
                    {
                        indexStack[++indexStackTop] = pivotIndex + 1;
                        indexStack[++indexStackTop] = inUpperIndex;
                    }
                }
            }
        }

        /// <summary>
        /// Quicksorts a buffer using the given sorting value function.
        /// </summary>
        static public void Quicksort<T>(void* ioBuffer, int inLowerIndex, int inUpperIndex, ComparisonGetSortingValue<T> inSortingValues)
            where T : struct
        {
            if (inLowerIndex >= 0 && inLowerIndex < inUpperIndex)
            {
                // lower higher
                int* indexStack = stackalloc int[inUpperIndex - inLowerIndex + 1];
                int indexStackTop = -1;
                int pivotIndex;

                indexStack[++indexStackTop] = inLowerIndex;
                indexStack[++indexStackTop] = inUpperIndex;

                while(indexStackTop >= 1)
                {
                    inUpperIndex = indexStack[indexStackTop--];
                    inLowerIndex = indexStack[indexStackTop--];

                    pivotIndex = Quicksort_Partition((byte*) ioBuffer, inLowerIndex, inUpperIndex, inSortingValues);
                    if (inLowerIndex < pivotIndex)
                    {
                        indexStack[++indexStackTop] = inLowerIndex;
                        indexStack[++indexStackTop] = pivotIndex;
                    }
                    if (inUpperIndex > pivotIndex + 1)
                    {
                        indexStack[++indexStackTop] = pivotIndex + 1;
                        indexStack[++indexStackTop] = inUpperIndex;
                    }
                }
            }
        }

        static private int Quicksort_Partition<T>(byte* ioBuffer, int inLower, int inHigher, IComparer<T> inComparison)
            where T : struct
        {
            int center = (inLower + inHigher) >> 1;
            int size = SizeOf<T>();
            T centerVal = Marshal.PtrToStructure<T>((IntPtr) (ioBuffer + center * size));

            int i = inLower - 1;
            int j = inHigher + 1;

            while(true)
            {
                do
                {
                    i++;
                }
                while(inComparison.Compare(Marshal.PtrToStructure<T>((IntPtr) (ioBuffer + i * size)), centerVal) < 0);

                do
                {
                    j--;
                }
                while(inComparison.Compare(Marshal.PtrToStructure<T>((IntPtr) (ioBuffer + j * size)), centerVal) > 0);
                
                if (i >= j)
                {
                    return j;
                }

                Ref.Swap(ref ioBuffer[i], ref ioBuffer[j]);
            }
        }

        static private int Quicksort_Partition<T>(byte* ioBuffer, int inLower, int inHigher, Comparison<T> inComparison)
            where T : struct
        {
            int center = (inLower + inHigher) >> 1;
            int size = SizeOf<T>();
            T centerVal = Marshal.PtrToStructure<T>((IntPtr) (ioBuffer + center * size));

            int i = inLower - 1;
            int j = inHigher + 1;

            while(true)
            {
                do
                {
                    i++;
                }
                while(inComparison(Marshal.PtrToStructure<T>((IntPtr) (ioBuffer + i * size)), centerVal) < 0);

                do
                {
                    j--;
                }
                while(inComparison(Marshal.PtrToStructure<T>((IntPtr) (ioBuffer + j * size)), centerVal) > 0);
                
                if (i >= j)
                {
                    return j;
                }

                Ref.Swap(ref ioBuffer[i], ref ioBuffer[j]);
            }
        }

        static private int Quicksort_Partition<T>(byte* ioBuffer, int inLower, int inHigher, ComparisonPtr<T> inComparison)
            where T : struct
        {
            int center = (inLower + inHigher) >> 1;
            int size = SizeOf<T>();

            int i = inLower - 1;
            int j = inHigher + 1;

            while(true)
            {
                do
                {
                    i++;
                }
                while(inComparison(&ioBuffer[i * size], &ioBuffer[center * size]) < 0);

                do
                {
                    j--;
                }
                while(inComparison(&ioBuffer[j * size], &ioBuffer[center * size]) > 0);
                
                if (i >= j)
                {
                    return j;
                }

                Ref.Swap(ref ioBuffer[i], ref ioBuffer[j]);
            }
        }

        static private int Quicksort_Partition<T>(byte* ioBuffer, int inLower, int inHigher, ComparisonGetSortingValue<T> inSortingValues)
            where T : struct
        {
            int center = (inLower + inHigher) >> 1;
            int size = SizeOf<T>();
            float centerVal = inSortingValues(&ioBuffer[center * size]);

            int i = inLower - 1;
            int j = inHigher + 1;

            while(true)
            {
                do
                {
                    i++;
                }
                while(inSortingValues(&ioBuffer[i * size]) < centerVal);

                do
                {
                    j--;
                }
                while(inSortingValues(&ioBuffer[j  * size]) > centerVal);
                
                if (i >= j)
                {
                    return j;
                }

                Ref.Swap(ref ioBuffer[i], ref ioBuffer[j]);
            }
        }

        /// <summary>
        /// Comparison delegate for comparing the contents of two unmanaged objects.
        /// </summary>
        public delegate int ComparisonPtr<T>(void* x, void* y) where T : struct;
        
        /// <summary>
        /// Comparison delegate that generates the sorting value for the given unmanaged object.
        /// </summary>
        public delegate float ComparisonGetSortingValue<T>(void* x) where T : struct;

        #endif // UNMANAGED_CONSTRAINT

        #endregion // Sort
    }
}