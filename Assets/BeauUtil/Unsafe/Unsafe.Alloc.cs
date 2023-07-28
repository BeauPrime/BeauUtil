/*
 * Copyright (C) 2021. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    26 July 2020
 * 
 * File:    Unsafe.Alloc.cs
 * Purpose: Unsafe utility methods.
 */

#if CSHARP_7_3_OR_NEWER
#define UNMANAGED_CONSTRAINT
#endif // CSHARP_7_3_OR_NEWER

#if (UNITY_EDITOR && !IGNORE_UNITY_EDITOR) || DEVELOPMENT_BUILD || DEVELOPMENT
#define HAS_DEBUGGER
#define VALIDATE_ARENA_MEMORY
#endif // (UNITY_EDITOR && !IGNORE_UNITY_EDITOR) || DEVELOPMENT_BUILD || DEVELOPMENT

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using BeauUtil.Debugger;

namespace BeauUtil
{
    /// <summary>
    /// Contains unsafe utility functions.
    /// </summary>
    static public unsafe partial class Unsafe
    {
        /// <summary>
        /// Exception indicating some type of memory corruption has occurred.
        /// </summary>
        public class MemoryCorruptionException : Exception
        {
            public unsafe MemoryCorruptionException(void* inAddress)
                : base(string.Format("Memory corruption at address '0x{0:X}'", (ulong) inAddress))
            {
            }

            public unsafe MemoryCorruptionException(void* inAddress, string inFormat, params object[] inArgs)
                : base(string.Format("Memory corruption at address '0x{0:X}': {1}", (ulong) inAddress, string.Format(inFormat, inArgs)))
            {
            }

            public unsafe MemoryCorruptionException(IntPtr inAddress)
                : this((void*) inAddress)
            {
            }

            public unsafe MemoryCorruptionException(IntPtr inAddress, string inFormat, params object[] inArgs)
                : this((void*) inAddress, inFormat, inArgs)
            {
            }
        }
        
        /// <summary>
        /// Interface for an allocator.
        /// </summary>
        public interface IAllocator
        {
            /// <summary>
            /// Returns if the given memory address is within the allocator's memory range.
            /// </summary>
            bool Owns(void* inPtr);

            /// <summary>
            /// Returns if the given memory address is within the allocator's currrently allocated memory range.
            /// </summary>
            bool IsValid(void* inPtr);

            /// <summary>
            /// Returns the total size of the allocator.
            /// </summary>
            int Size();

            /// <summary>
            /// Returns the number of free bytes in the allocator.
            /// </summary>
            int FreeBytes();

            /// <summary>
            /// Returns the number of used bytes in the allocator.
            /// </summary>
            int UsedBytes();

            /// <summary>
            /// Allocates a block of memory with the given size.
            /// </summary>
            void* Alloc(int inSize);

            /// <summary>
            /// Allocates an aligned block of memory with the given size.
            /// </summary>
            void* AllocAligned(int inSize, uint inAlign);

            /// <summary>
            /// Frees the given block of memory.
            /// </summary>
            void Free(void* inPtr);
            
            /// <summary>
            /// Releases all memory owned by the allocator.
            /// </summary>
            void Release();

            /// <summary>
            /// Returns if the allocator is actually allocated and its internal state is valid.
            /// </summary>
            bool Validate();
        }

        #region Default Allocator

        #if UNMANAGED_CONSTRAINT

        /// <summary>
        /// Allocates an instance of an unmanaged type.
        /// </summary>
        static public T* Alloc<T>()
            where T : unmanaged
        {
            return (T*) Alloc(sizeof(T));
        }

        /// <summary>
        /// Allocates an array of the given unmanaged type.
        /// </summary>
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

        /// <summary>
        /// Allocates an instance of an unmanaged type.
        /// </summary>
        static public void* Alloc<T>()
            where T : struct
        {
            return Alloc(SizeOf<T>());
        }

        /// <summary>
        /// Allocates an array of the given unmanaged type.
        /// </summary>
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

        #endif // UNMANAGED_CONSTRAINT

        /// <summary>
        /// Allocates unmanaged memory from the application's memory.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public void* Alloc(int inLength)
        {
            return (void*) Marshal.AllocHGlobal(inLength);
        }

        /// <summary>
        /// Reallocates unmanaged memory from the application's memory.
        /// </summary>
        static public void* Realloc(void* inPtr, int inLength)
        {
            return (void*) Marshal.ReAllocHGlobal((IntPtr) inPtr, (IntPtr) inLength);
        }

        /// <summary>
        /// Frees unmanaged memory back to the application's memory.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public void Free(void* inPtr)
        {
            Marshal.FreeHGlobal((IntPtr) inPtr);
        }

        /// <summary>
        /// Attempts to free unmanaged memory back to the application's memory.
        /// </summary>
        static public bool TryFree(ref void* ioPtr)
        {
            if (ioPtr != null)
            {
                Marshal.FreeHGlobal((IntPtr) ioPtr);
                ioPtr = null;
                return true;
            }

            return false;
        }

        #endregion // Default Allocator
    }
}