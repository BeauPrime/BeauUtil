/*
 * Copyright (C) 2021. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    26 July 2020
 * 
 * File:    Unsafe.cs
 * Purpose: Unsafe utility methods.
 */

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

        static public int SizeOf<T>()
            where T : struct
        {
            return Marshal.SizeOf<T>();
        }

        static public void* Alloc(int inLength)
        {
            return (void*) Marshal.AllocHGlobal(inLength);
        }
        
        static public void* AllocArray<T>(int inLength)
            where T : struct
        {
            return (void*) Marshal.AllocHGlobal(inLength * SizeOf<T>());
        }

        static public void* Realloc(void* inPtr, int inLength)
        {
            return (void*) Marshal.ReAllocHGlobal((IntPtr) inPtr, (IntPtr) inLength);
        }

        static public void* ReallocArray<T>(void* inPtr, int inLength)
            where T : struct
        {
            return (void*) Marshal.ReAllocHGlobal((IntPtr) inPtr, (IntPtr) (inLength * SizeOf<T>()));
        }

        static public void Free(void* inPtr)
        {
            Marshal.FreeHGlobal((IntPtr) inPtr);
        }
    }
}