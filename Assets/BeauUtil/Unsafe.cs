/*
 * Copyright (C) 2021. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    26 July 2020
 * 
 * File:    Unsafe.cs
 * Purpose: Unsafe utility methods.
 */

#if CSHARP_7_3_OR_NEWER
#define UNMANAGED
#endif // CSHARP_7_3_OR_NEWER

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace BeauUtil
{
    /// <summary>
    /// Contains unsafe utility functions.
    /// </summary>
    static public unsafe class Unsafe
    {
        static public void Prefetch(void* inData)
        {
            char c = *((char*) inData);
        }

        #region Alignment

        [MethodImpl(256)]
        static public uint AlignUp4(uint val)
        {
            return (val + 4u - 1) & ~(4u - 1);
        }

        [MethodImpl(256)]
        static public ulong AlignUp4(ulong val)
        {
            return (val + 4u - 1) & ~(4u - 1);
        }

        [MethodImpl(256)]
        static public uint AlignDown4(uint val)
        {
            return (val) & ~(4u - 1);
        }

        [MethodImpl(256)]
        static public ulong AlignDown4(ulong val)
        {
            return (val) & ~(4u - 1);
        }

        [MethodImpl(256)]
        static public uint AlignUp8(uint val)
        {
            return (val + 8u - 1) & ~(8u - 1);
        }

        [MethodImpl(256)]
        static public ulong AlignUp8(ulong val)
        {
            return (val + 8u - 1) & ~(8u - 1);
        }

        [MethodImpl(256)]
        static public uint AlignDown8(uint val)
        {
            return (val) & ~(8u - 1);
        }

        [MethodImpl(256)]
        static public ulong AlignDown8(ulong val)
        {
            return (val) & ~(8u - 1);
        }

        [MethodImpl(256)]
        static public uint AlignUp16(uint val)
        {
            return (val + 16u - 1) & ~(16u - 1);
        }

        [MethodImpl(256)]
        static public ulong AlignUp16(ulong val)
        {
            return (val + 16u - 1) & ~(16u - 1);
        }

        [MethodImpl(256)]
        static public uint AlignDown16(uint val)
        {
            return (val) & ~(16u - 1);
        }

        [MethodImpl(256)]
        static public ulong AlignDown16(ulong val)
        {
            return (val) & ~(16u - 1);
        }

        [MethodImpl(256)]
        static public uint AlignUp32(uint val)
        {
            return (val + 32u - 1) & ~(32u - 1);
        }

        [MethodImpl(256)]
        static public ulong AlignUp32(ulong val)
        {
            return (val + 32u - 1) & ~(32u - 1);
        }

        [MethodImpl(256)]
        static public uint AlignDown32(uint val)
        {
            return (val) & ~(32u - 1);
        }

        [MethodImpl(256)]
        static public ulong AlignDown32(ulong val)
        {
            return (val) & ~(32u - 1);
        }

        [MethodImpl(256)]
        static public uint AlignUp64(uint val)
        {
            return (val + 64u - 1) & ~(64u - 1);
        }

        [MethodImpl(256)]
        static public ulong AlignUp64(ulong val)
        {
            return (val + 64u - 1) & ~(64u - 1);
        }

        [MethodImpl(256)]
        static public uint AlignDown64(uint val)
        {
            return (val) & ~(64u - 1);
        }

        [MethodImpl(256)]
        static public ulong AlignDown64(ulong val)
        {
            return (val) & ~(64u - 1);
        }

        #endregion // Alignment

        #if UNMANAGED

        [MethodImpl(256)]
        static public int SizeOf<T>()
            where T : unmanaged
        {
            return sizeof(T);
        }

        static public T* AllocArray<T>(int inLength)
            where T : unmanaged
        {
            return (T*) Marshal.AllocHGlobal(inLength * sizeof(T));
        }

        static public T* ReallocArray<T>(void* inPtr, int inLength)
            where T : unmanaged
        {
            return (T*) Marshal.ReAllocHGlobal((IntPtr) inPtr, (IntPtr) (inLength * sizeof(T)));
        }

        #else

        [MethodImpl(256)]
        static public int SizeOf<T>()
            where T : struct
        {
            return Marshal.SizeOf<T>();
        }

        static public void* AllocArray<T>(int inLength)
            where T : struct
        {
            return (void*) Marshal.AllocHGlobal(inLength * SizeOf<T>());
        }

        static public void* ReallocArray<T>(void* inPtr, int inLength)
            where T : struct
        {
            return (void*) Marshal.ReAllocHGlobal((IntPtr) inPtr, (IntPtr) (inLength * SizeOf<T>()));
        }

        #endif // UNMANAGED

        static public void* Alloc(int inLength)
        {
            return (void*) Marshal.AllocHGlobal(inLength);
        }

        static public void* Realloc(void* inPtr, int inLength)
        {
            return (void*) Marshal.ReAllocHGlobal((IntPtr) inPtr, (IntPtr) inLength);
        }

        static public void Free(void* inPtr)
        {
            Marshal.FreeHGlobal((IntPtr) inPtr);
        }
    }
}