/*
 * Copyright (C) 2021. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    26 July 2020
 * 
 * File:    Unsafe.cs
 * Purpose: Unsafe utility methods.
 */

#if CSHARP_7_3_OR_NEWER
#define UNMANAGED_CONSTRAINT
#endif // CSHARP_7_3_OR_NEWER

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace BeauUtil
{
    /// <summary>
    /// Contains unsafe utility functions.
    /// </summary>
    static public unsafe partial class Unsafe
    {
        /// <summary>
        /// Size of an unmanaged pointer.
        /// </summary>
        static public readonly uint PointerSize = (uint) IntPtr.Size;

        /// <summary>
        /// Attempts to load the given address into the cache.
        /// </summary>
        static public void Prefetch(void* inData)
        {
            byte b = *((byte*) inData);
        }

        #region Alignment

        private struct AlignHelper<T>
            where T : struct
        {
            private byte _;
            private T i;

            static internal readonly uint Alignment = (uint) Marshal.OffsetOf<AlignHelper<T>>("i");
        }

        /// <summary>
        /// Returns the alignment of the given type.
        /// </summary>
        [MethodImpl(256)]
        static public uint AlignOf<T>()
            where T : struct
        {
            return AlignHelper<T>.Alignment;
        }

        [MethodImpl(256)]
        static public uint AlignUp4(uint val)
        {
            return (val + 4u - 1) & ~(4u - 1);
        }

        [MethodImpl(256)]
        static public ulong AlignUp4(ulong val)
        {
            return (val + 4u - 1) & ~(ulong) (4u - 1);
        }

        [MethodImpl(256)]
        static public uint AlignDown4(uint val)
        {
            return (val) & ~(4u - 1);
        }

        [MethodImpl(256)]
        static public ulong AlignDown4(ulong val)
        {
            return (val) & ~(ulong) (4u - 1);
        }

        [MethodImpl(256)]
        static public uint AlignUp8(uint val)
        {
            return (val + 8u - 1) & ~(8u - 1);
        }

        [MethodImpl(256)]
        static public ulong AlignUp8(ulong val)
        {
            return (val + 8u - 1) & ~(ulong) (8u - 1);
        }

        [MethodImpl(256)]
        static public uint AlignDown8(uint val)
        {
            return (val) & ~(8u - 1);
        }

        [MethodImpl(256)]
        static public ulong AlignDown8(ulong val)
        {
            return (val) & ~(ulong) (8u - 1);
        }

        [MethodImpl(256)]
        static public uint AlignUp16(uint val)
        {
            return (val + 16u - 1) & ~(16u - 1);
        }

        [MethodImpl(256)]
        static public ulong AlignUp16(ulong val)
        {
            return (val + 16u - 1) & ~(ulong) (16u - 1);
        }

        [MethodImpl(256)]
        static public uint AlignDown16(uint val)
        {
            return (val) & ~(16u - 1);
        }

        [MethodImpl(256)]
        static public ulong AlignDown16(ulong val)
        {
            return (val) & ~(ulong) (16u - 1);
        }

        [MethodImpl(256)]
        static public uint AlignUp32(uint val)
        {
            return (val + 32u - 1) & ~(32u - 1);
        }

        [MethodImpl(256)]
        static public ulong AlignUp32(ulong val)
        {
            return (val + 32u - 1) & ~(ulong) (32u - 1);
        }

        [MethodImpl(256)]
        static public uint AlignDown32(uint val)
        {
            return (val) & ~(32u - 1);
        }

        [MethodImpl(256)]
        static public ulong AlignDown32(ulong val)
        {
            return (val) & ~(ulong) (32u - 1);
        }

        [MethodImpl(256)]
        static public uint AlignUp64(uint val)
        {
            return (val + 64u - 1) & ~(64u - 1);
        }

        [MethodImpl(256)]
        static public ulong AlignUp64(ulong val)
        {
            return (val + 64u - 1) & ~(ulong) (64u - 1);
        }

        [MethodImpl(256)]
        static public uint AlignDown64(uint val)
        {
            return (val) & ~(64u - 1);
        }

        [MethodImpl(256)]
        static public ulong AlignDown64(ulong val)
        {
            return (val) & ~(ulong) (64u - 1);
        }

        [MethodImpl(256)]
        static public uint AlignUpN(uint val, uint n)
        {
            return (val + n - 1) & ~(n - 1);
        }

        [MethodImpl(256)]
        static public ulong AlignUpN(ulong val, uint n)
        {
            return (val + n - 1) & ~(ulong) (n - 1);
        }

        [MethodImpl(256)]
        static public uint AlignDown(uint val, uint n)
        {
            return (val) & ~(n - 1);
        }

        [MethodImpl(256)]
        static public ulong AlignDownN(ulong val, uint n)
        {
            return (val) & ~(ulong) (n - 1); 
        }

        #endregion // Alignment

        #region Size

        #if UNMANAGED_CONSTRAINT

        /// <summary>
        /// Returns the size of the unmanaged type.
        /// </summary>
        [MethodImpl(256)]
        static public int SizeOf<T>()
            where T : unmanaged
        {
            return sizeof(T);
        }

        #else

        /// <summary>
        /// Returns the size of the unmanaged type.
        /// </summary>
        [MethodImpl(256)]
        static public int SizeOf<T>()
            where T : struct
        {
            return Marshal.SizeOf(typeof(T));
        }

        #endif // UNMANAGED_CONSTRAINT

        #endregion // Size

        #region Copy

        #if UNMANAGED_CONSTRAINT

        /// <summary>
        /// Copies memory from one buffer to another.
        /// </summary>
        static public void CopyArray<T>(T* inSrc, int inSrcCount, T* inDest, int inDestCount)
            where T : unmanaged
        {
            int size = sizeof(T);
            Buffer.MemoryCopy(inSrc, inDest, inDestCount * size, inSrcCount * size);
        }

        /// <summary>
        /// Copies memory from one buffer to another.
        /// </summary>
        static public void CopyArray<T>(T* inSrc, int inCount, T* inDest)
            where T : unmanaged
        {
            int size = sizeof(T);
            Buffer.MemoryCopy(inSrc, inDest, inCount * size, inCount * size);
        }

        /// <summary>
        /// Copies memory from a buffer to an array.
        /// </summary>
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
        /// Copies memory from an array to a buffer.
        /// </summary>
        static public void CopyArray<T>(T[] inSrc, T* inDest, int inDestCount)
            where T : unmanaged
        {
            fixed(T* srcPtr = inSrc)
            {
                int size = sizeof(T);
                Buffer.MemoryCopy(srcPtr, inDest, inDestCount * size, inSrc.Length * size);
            }
        }

        /// <summary>
        /// Copies memory from an array to a buffer.
        /// </summary>
        static public void CopyArray<T>(T[] inSrc, int inSrcOffset, T* inDest, int inDestCount)
            where T : unmanaged
        {
            fixed(T* srcPtr = inSrc)
            {
                int size = sizeof(T);
                Buffer.MemoryCopy(srcPtr + inSrcOffset, inDest, inDestCount * size, (inSrc.Length - inSrcOffset) * size);
            }
        }

        /// <summary>
        /// Copies memory from an array to a buffer.
        /// </summary>
        static public void CopyArray<T>(T[] inSrc, int inSrcOffset, int inSrcCount, T* inDest, int inDestCount)
            where T : unmanaged
        {
            fixed(T* srcPtr = inSrc)
            {
                int size = sizeof(T);
                Buffer.MemoryCopy(srcPtr + inSrcOffset, inDest, inDestCount * size, inSrcCount * size);
            }
        }

        #else

        /// <summary>
        /// Copies memory from one buffer to another.
        /// </summary>
        static public void CopyArray<T>(void* inSrc, int inSrcCount, void* inDest, int inDestCount)
            where T : struct
        {
            int size = SizeOf<T>();
            Buffer.MemoryCopy(inSrc, inDest, inDestCount * size, inSrcCount * size);
        }

        /// <summary>
        /// Copies memory from one buffer to another.
        /// </summary>
        static public void CopyArray<T>(void* inSrc, int inCount, void* inDest)
            where T : struct
        {
            int size = SizeOf<T>();
            Buffer.MemoryCopy(inSrc, inDest, inCount * size, inCount * size);
        }

        /// <summary>
        /// Copies memory from a buffer to an array.
        /// </summary>
        [MethodImpl(256)]
        static public void CopyArray<T>(void* inSrc, int inSrcCount, T[] inDest)
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
        /// Copies memory from an array to a buffer.
        /// </summary>
        [MethodImpl(256)]
        static public void CopyArray<T>(T[] inSrc, void* inDest, int inDestCount)
            where T : struct
        {
            CopyArray<T>(inSrc, 0, inSrc.Length, inDest, inDestCount);
        }

        /// <summary>
        /// Copies memory from an array to a buffer.
        /// </summary>
        [MethodImpl(256)]
        static public void CopyArray<T>(T[] inSrc, int inSrcOffset, void* inDest, int inDestCount)
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

        #endif // UNMANAGED_CONSTRAINT

        /// <summary>
        /// Copies memory from one buffer to another.
        /// </summary>
        [MethodImpl(256)]
        static public void Copy(void* inSrc, int inSrcSize, void* inDest, int inDestSize)
        {
            Buffer.MemoryCopy(inSrc, inDest, inDestSize, inSrcSize);
        }

        /// <summary>
        /// Copies memory from one buffer to another.
        /// </summary>
        [MethodImpl(256)]
        static public void Copy(void* inSrc, int inSize, void* inDest)
        {
            Buffer.MemoryCopy(inSrc, inDest, inSize, inSize);
        }

        #endregion // Copy

        #region Pinning

        /// <summary>
        /// Handle and address for a pinned array.
        /// </summary>
        public struct PinnedArrayHandle<T> : IDisposable
#if UNMANAGED_CONSTRAINT
            where T : unmanaged
#else
            where T : struct
#endif // UNMANAGED_CONSTRAINT
        {
            #if UNMANAGED_CONSTRAINT
            public T* Address;
            #else
            public void* Address;
            internal int ElementSize;
            #endif // UNMANAGED_CONSTRAINT

            internal GCHandle Handle;

            internal PinnedArrayHandle(GCHandle inHandle, T[] inSource)
            {
                Handle = inHandle;

                #if UNMANAGED_CONSTRAINT
                Address = (T*) Marshal.UnsafeAddrOfPinnedArrayElement(inSource, 0);
                #else
                Address = (void*) Marshal.UnsafeAddrOfPinnedArrayElement(inSource, 0);
                ElementSize = SizeOf<T>();
                #endif // UNMANAGED_CONSTRAINT
            }

            internal PinnedArrayHandle(T[] inSource)
            {
                Handle = GCHandle.Alloc(inSource, GCHandleType.Pinned);

                #if UNMANAGED_CONSTRAINT
                Address = (T*) Marshal.UnsafeAddrOfPinnedArrayElement(inSource, 0);
                #else
                Address = (void*) Marshal.UnsafeAddrOfPinnedArrayElement(inSource, 0);
                ElementSize = SizeOf<T>();
                #endif // UNMANAGED_CONSTRAINT
            }

            #if UNMANAGED_CONSTRAINT
            public T* ElementAddress(int inIndex)
            {
                return Address + inIndex;
            }
            #else
            public void* ElementAddress(int inIndex)
            {
                return (byte*) Address + ElementSize * inIndex;
            }
            #endif // UNMANAGED_CONSTRAINT

            public void Dispose()
            {
                if (Handle.IsAllocated)
                {
                    Address = null;
                    Handle.Free();
                }
            }

            #if UNMANAGED_CONSTRAINT
            static public implicit operator T*(PinnedArrayHandle<T> inHandle)
            {
                return inHandle.Address;
            }
            #else
            static public implicit operator void*(PinnedArrayHandle<T> inHandle)
            {
                return inHandle.Address;
            }
            #endif // UNMANAGED_CONSTRAINT
        }

        /// <summary>
        /// Pins the given array in memory.
        /// </summary>
        static public PinnedArrayHandle<T> PinArray<T>(T[] inArray)
#if UNMANAGED_CONSTRAINT
            where T : unmanaged
#else
            where T : struct
#endif // UNMANAGED_CONSTRAINT
        {
            if (inArray == null)
                throw new ArgumentNullException("inArray", "Cannot pin null array");

            return new PinnedArrayHandle<T>(inArray);
        }

        #endregion // Pinning
    }
}