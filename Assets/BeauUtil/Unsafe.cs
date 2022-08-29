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

#if UNITY_2018_1_OR_NEWER
#define NATIVE_ARRAY_EXT
#endif // UNITY_2018_1_OR_NEWER

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

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

        #if UNMANAGED_CONSTRAINT

        /// <summary>
        /// Reinterprets a value as a value of another type.
        /// </summary>
        [MethodImpl(256)]
        static public TTo Reinterpret<TFrom, TTo>(TFrom inValue)
            where TFrom : unmanaged
            where TTo : unmanaged
        {
            return *(TTo*)(&inValue);
        }

        /// <summary>
        /// Reinterprets a value as a value of another type.
        /// </summary>
        [MethodImpl(256)]
        static public TTo Reinterpret<TFrom, TTo>(TFrom* inValuePtr)
            where TFrom : unmanaged
            where TTo : unmanaged
        {
            return *(TTo*)(inValuePtr);
        }

        #endif // UNMANAGED_CONSTRAINT

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

        #region Unity Native

        #if NATIVE_ARRAY_EXT

        #if UNMANAGED_CONSTRAINT

        /// <summary>
        /// Returns a pointer to the start of the given native buffer.
        /// </summary>
        [MethodImpl(256)]
        static public T* NativePointer<T>(Unity.Collections.NativeArray<T> inArray)
            where T : unmanaged
        {
            return (T*) Unity.Collections.LowLevel.Unsafe.NativeArrayUnsafeUtility.GetUnsafePtr(inArray);
        }

        /// <summary>
        /// Returns a read-only pointer to the start of the given native buffer.
        /// </summary>
        [MethodImpl(256)]
        static public T* NativePointerReadOnly<T>(Unity.Collections.NativeArray<T> inArray)
            where T : unmanaged
        {
            return (T*) Unity.Collections.LowLevel.Unsafe.NativeArrayUnsafeUtility.GetUnsafeReadOnlyPtr(inArray);
        }

        /// <summary>
        /// Returns a pointer to the start of the given native slice.
        /// </summary>
        [MethodImpl(256)]
        static public T* NativePointer<T>(Unity.Collections.NativeSlice<T> inSlice)
            where T : unmanaged
        {
            return (T*) Unity.Collections.LowLevel.Unsafe.NativeSliceUnsafeUtility.GetUnsafePtr(inSlice);
        }

        #else

        /// <summary>
        /// Returns a pointer to the start of the given native buffer.
        /// </summary>
        [MethodImpl(256)]
        static public void* NativePointer<T>(Unity.Collections.NativeArray<T> inArray)
            where T : struct
        {
            return Unity.Collections.LowLevel.Unsafe.NativeArrayUnsafeUtility.GetUnsafePtr(inArray);
        }

        /// <summary>
        /// Returns a read-only pointer to the start of the given native buffer.
        /// </summary>
        [MethodImpl(256)]
        static public void* NativePointerReadOnly<T>(Unity.Collections.NativeArray<T> inArray)
            where T : struct
        {
            return Unity.Collections.LowLevel.Unsafe.NativeArrayUnsafeUtility.GetUnsafeReadOnlyPtr(inArray);
        }

        /// <summary>
        /// Returns a pointer to the start of the given native slice.
        /// </summary>
        [MethodImpl(256)]
        static public void* NativePointer<T>(Unity.Collections.NativeSlice<T> inSlice)
            where T : struct
        {
            return Unity.Collections.LowLevel.Unsafe.NativeSliceUnsafeUtility.GetUnsafePtr(inSlice);
        }

        #endif // UNMANAGED_CONSTRAINT

        #endif // NATIVE_ARRAY_EXT

        #endregion // Unity Native

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

            internal PinnedArrayHandle(string inSource)
            {
                Handle = GCHandle.Alloc(inSource, GCHandleType.Pinned);

                #if UNMANAGED_CONSTRAINT
                Address = (T*) Handle.AddrOfPinnedObject();
                #else
                Address = (void*) Handle.AddrOfPinnedObject();
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

        /// <summary>
        /// Pins the given string in memory.
        /// </summary>
        static public PinnedArrayHandle<char> PinString(string inString)
        {
            if (inString == null)
                throw new ArgumentNullException("inArray", "Cannot pin null array");

            return new PinnedArrayHandle<char>(inString);
        }

        #endregion // Pinning
    
        #region Debug

        private const string HexChars = "0123456789ABCDEF";

        /// <summary>
        /// Dumps the given memory contents to string.
        /// </summary>
        static public string DumpMemory(void* inBuffer, int inSize, char inSeparator = (char) 0, int inSeparatorInterval = 0)
        {
            int stringLength = inSize * 2;
            
            int separatorCount = 0;
            if (inSeparatorInterval > 0)
            {
                separatorCount = (inSize - 1) / inSeparatorInterval;
                stringLength += separatorCount;
            }

            if (stringLength <= 256)
            {
                byte* bytes = (byte*) inBuffer;
                byte val;
                char* charBuffer = stackalloc char[stringLength];
                char* writeHead = charBuffer;

                for(int i = 0; i < inSize; i++)
                {
                    if (i > 0 && inSeparatorInterval != 0 && (i % inSeparatorInterval) == 0)
                    {
                        *writeHead++ = inSeparator;
                    }
                    val = *bytes++;
                    *writeHead++ = HexChars[val / 16];
                    *writeHead++ = HexChars[val % 16];
                }

                return new string(charBuffer, 0, stringLength);
            }
            else
            {
                StringBuilder sb = new StringBuilder(stringLength);
                DumpMemory(inBuffer, inSize, sb, inSeparator, inSeparatorInterval);
                return sb.Flush();
            }
        }

        /// <summary>
        /// Dumps the given memory contents to string.
        /// </summary>
        static public string DumpMemory(void* inBuffer, long inSize, char inSeparator = (char) 0, int inSeparatorInterval = 0)
        {
            long stringLength = inSize * 2;
            
            long separatorCount = 0;
            if (inSeparatorInterval > 0)
            {
                separatorCount = (inSize - 1) / inSeparatorInterval;
                stringLength += separatorCount;
            }

            if (stringLength <= 256)
            {
                byte* bytes = (byte*) inBuffer;
                byte val;
                char* charBuffer = stackalloc char[(int) stringLength];
                char* writeHead = charBuffer;

                for(int i = 0; i < inSize; i++)
                {
                    if (i > 0 && inSeparatorInterval != 0 && (i % inSeparatorInterval) == 0)
                    {
                        *writeHead++ = inSeparator;
                    }
                    val = *bytes++;
                    *writeHead++ = HexChars[val / 16];
                    *writeHead++ = HexChars[val % 16];
                }

                return new string(charBuffer, 0, (int) stringLength);
            }
            else
            {
                StringBuilder sb = new StringBuilder((int) stringLength);
                DumpMemory(inBuffer, inSize, sb, inSeparator, inSeparatorInterval);
                return sb.Flush();
            }
        }

        /// <summary>
        /// Dumps the given memory contents to a StringBuilder.
        /// </summary>
        static public void DumpMemory(void* inBuffer, int inSize, StringBuilder ioStringBuilder, char inSeparator = (char) 0, int inSeparatorInterval = 0)
        {
            byte* bytes = (byte*) inBuffer;
            byte val;
            for(int i = 0; i < inSize; i++)
            {
                if (i > 0 && inSeparatorInterval != 0 && (i % inSeparatorInterval) == 0)
                {
                    ioStringBuilder.Append(inSeparator);
                }
                val = *bytes++;
                ioStringBuilder.Append(HexChars[val / 16]).Append(HexChars[val % 16]);
            }
        }

        /// <summary>
        /// Dumps the given memory contents to a StringBuilder.
        /// </summary>
        static public void DumpMemory(void* inBuffer, long inSize, StringBuilder ioStringBuilder, char inSeparator = (char) 0, int inSeparatorInterval = 0)
        {
            byte* bytes = (byte*) inBuffer;
            byte val;
            for(long i = 0; i < inSize; i++)
            {
                if (i > 0 && inSeparatorInterval != 0 && (i % inSeparatorInterval) == 0)
                {
                    ioStringBuilder.Append(inSeparator);
                }
                val = *bytes++;
                ioStringBuilder.Append(HexChars[val / 16]).Append(HexChars[val % 16]);
            }
        }

        #endregion // Debug
    }
}