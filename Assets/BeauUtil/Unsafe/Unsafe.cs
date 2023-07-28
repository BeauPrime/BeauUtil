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
using System.Collections.Generic;
using System.Reflection;
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

        #region Reinterpret

        #if UNMANAGED_CONSTRAINT

        /// <summary>
        /// Reinterprets a value as a value of another type.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public TTo Reinterpret<TFrom, TTo>(TFrom inValue)
            where TFrom : unmanaged
            where TTo : unmanaged
        {
            byte* buffer = stackalloc byte[Math.Max(sizeof(TFrom), sizeof(TTo))];
            *((TFrom*) buffer) = inValue;
            return *((TTo*) buffer);
        }

        /// <summary>
        /// Reinterprets an indirect value as a value of another type.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public TTo Reinterpret<TFrom, TTo>(TFrom* inValuePtr)
            where TFrom : unmanaged
            where TTo : unmanaged
        {
            return *((TTo*) inValuePtr);
        }

        /// <summary>
        /// Reinterprets a value as a value of another type.
        /// Behavior is undefined if <c>sizeof(TTo) > sizeof(TFrom)</c>
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public TTo FastReinterpret<TFrom, TTo>(TFrom inValue)
            where TFrom : unmanaged
            where TTo : unmanaged
        {
            return *((TTo*) &inValue);
        }

        #endif // UNMANAGED_CONSTRAINT

        #endregion // Reinterpret

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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public uint AlignOf<T>()
            where T : struct
        {
            return AlignHelper<T>.Alignment;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public int AlignUp4(int val)
        {
            return (val + 4 - 1) & ~(4 - 1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public uint AlignUp4(uint val)
        {
            return (val + 4u - 1) & ~(4u - 1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public ulong AlignUp4(ulong val)
        {
            return (val + 4u - 1) & ~(ulong) (4u - 1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public int AlignDown4(int val)
        {
            return (val) & ~(4 - 1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public uint AlignDown4(uint val)
        {
            return (val) & ~(4u - 1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public ulong AlignDown4(ulong val)
        {
            return (val) & ~(ulong) (4u - 1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public int AlignUp8(int val)
        {
            return (val + 8 - 1) & ~(8 - 1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public uint AlignUp8(uint val)
        {
            return (val + 8u - 1) & ~(8u - 1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public ulong AlignUp8(ulong val)
        {
            return (val + 8u - 1) & ~(ulong) (8u - 1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public int AlignDown8(int val)
        {
            return (val) & ~(8 - 1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public uint AlignDown8(uint val)
        {
            return (val) & ~(8u - 1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public ulong AlignDown8(ulong val)
        {
            return (val) & ~(ulong) (8u - 1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public int AlignUp16(int val)
        {
            return (val + 16 - 1) & ~(16 - 1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public uint AlignUp16(uint val)
        {
            return (val + 16u - 1) & ~(16u - 1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public ulong AlignUp16(ulong val)
        {
            return (val + 16u - 1) & ~(ulong) (16u - 1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public int AlignDown16(int val)
        {
            return (val) & ~(16 - 1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public uint AlignDown16(uint val)
        {
            return (val) & ~(16u - 1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public ulong AlignDown16(ulong val)
        {
            return (val) & ~(ulong) (16u - 1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public int AlignUp32(int val)
        {
            return (val + 32 - 1) & ~(32 - 1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public uint AlignUp32(uint val)
        {
            return (val + 32u - 1) & ~(32u - 1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public ulong AlignUp32(ulong val)
        {
            return (val + 32u - 1) & ~(ulong) (32u - 1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public int AlignDown32(int val)
        {
            return (val) & ~(32 - 1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public uint AlignDown32(uint val)
        {
            return (val) & ~(32u - 1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public ulong AlignDown32(ulong val)
        {
            return (val) & ~(ulong) (32u - 1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public int AlignUp64(int val)
        {
            return (val + 64 - 1) & ~(64 - 1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public uint AlignUp64(uint val)
        {
            return (val + 64u - 1) & ~(64u - 1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public ulong AlignUp64(ulong val)
        {
            return (val + 64u - 1) & ~(ulong) (64u - 1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public int AlignDown64(int val)
        {
            return (val) & ~(64 - 1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public uint AlignDown64(uint val)
        {
            return (val) & ~(64u - 1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public ulong AlignDown64(ulong val)
        {
            return (val) & ~(ulong) (64u - 1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public int AlignUpN(int val, int n)
        {
            return (val + n - 1) & ~(n - 1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public uint AlignUpN(uint val, uint n)
        {
            return (val + n - 1) & ~(n - 1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public ulong AlignUpN(ulong val, uint n)
        {
            return (val + n - 1) & ~(ulong) (n - 1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public int AlignDownN(int val, int n)
        {
            return (val) & ~(n - 1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public uint AlignDownN(uint val, uint n)
        {
            return (val) & ~(n - 1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public int SizeOf<T>()
            where T : unmanaged
        {
            return sizeof(T);
        }

#else

        /// <summary>
        /// Returns the size of the unmanaged type.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public int SizeOf<T>()
            where T : struct
        {
            return Marshal.SizeOf(typeof(T));
        }

#endif // UNMANAGED_CONSTRAINT

        #endregion // Size

        #region Size Conversions

        /// <summary>
        /// Gigabyte/gibibyte to bytes conversion.
        /// </summary>
        public const int GiB = 1024 * 1024 * 1024;

        /// <summary>
        /// Megabyte/mebibyte to bytes conversion.
        /// </summary>
        public const int MiB = 1024 * 1024;

        /// <summary>
        /// Kilobyte/Kibibyte to bytes conversion.
        /// </summary>
        public const int KiB = 1024;

        #endregion // Size Conversions

        #region Unity Native

#if NATIVE_ARRAY_EXT

#if UNMANAGED_CONSTRAINT

        /// <summary>
        /// Returns a pointer to the start of the given native buffer.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public T* NativePointer<T>(Unity.Collections.NativeArray<T> inArray)
            where T : unmanaged
        {
            return (T*) Unity.Collections.LowLevel.Unsafe.NativeArrayUnsafeUtility.GetUnsafePtr(inArray);
        }

        /// <summary>
        /// Returns a read-only pointer to the start of the given native buffer.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public T* NativePointerReadOnly<T>(Unity.Collections.NativeArray<T> inArray)
            where T : unmanaged
        {
            return (T*) Unity.Collections.LowLevel.Unsafe.NativeArrayUnsafeUtility.GetUnsafeReadOnlyPtr(inArray);
        }

        /// <summary>
        /// Returns a pointer to the start of the given native slice.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public T* NativePointer<T>(Unity.Collections.NativeSlice<T> inSlice)
            where T : unmanaged
        {
            return (T*) Unity.Collections.LowLevel.Unsafe.NativeSliceUnsafeUtility.GetUnsafePtr(inSlice);
        }

        /// <summary>
        /// Returns a NativeArray wrapping the given unsafe buffer.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public Unity.Collections.NativeArray<T> NativeArray<T>(T* inBuffer, int inCount)
            where T : unmanaged
        {
            Unity.Collections.NativeArray<T> nativeArray = Unity.Collections.LowLevel.Unsafe.NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<T>(inBuffer, inCount, Unity.Collections.Allocator.None);
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            Unity.Collections.LowLevel.Unsafe.NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref nativeArray, Unity.Collections.LowLevel.Unsafe.AtomicSafetyHandle.GetTempUnsafePtrSliceHandle());
#endif // ENABLE_UNITY_COLLECTIONS_CHECKS
            return nativeArray;
        }

#else

        /// <summary>
        /// Returns a pointer to the start of the given native buffer.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public void* NativePointer<T>(Unity.Collections.NativeArray<T> inArray)
            where T : struct
        {
            return Unity.Collections.LowLevel.Unsafe.NativeArrayUnsafeUtility.GetUnsafePtr(inArray);
        }

        /// <summary>
        /// Returns a read-only pointer to the start of the given native buffer.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public void* NativePointerReadOnly<T>(Unity.Collections.NativeArray<T> inArray)
            where T : struct
        {
            return Unity.Collections.LowLevel.Unsafe.NativeArrayUnsafeUtility.GetUnsafeReadOnlyPtr(inArray);
        }

        /// <summary>
        /// Returns a pointer to the start of the given native slice.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public void* NativePointer<T>(Unity.Collections.NativeSlice<T> inSlice)
            where T : struct
        {
            return Unity.Collections.LowLevel.Unsafe.NativeSliceUnsafeUtility.GetUnsafePtr(inSlice);
        }

#endif // UNMANAGED_CONSTRAINT

#endif // NATIVE_ARRAY_EXT

        #endregion // Unity Native

        #region Pinning

        static private FieldInfo s_StringBuilderChunkField;

        static private class PinFieldCache<T>
        {
            static internal FieldInfo ListItemsField;
        }

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
            internal int Stride;
#endif // UNMANAGED_CONSTRAINT
            public int Length;

            internal GCHandle Handle;

            internal PinnedArrayHandle(T[] inSource)
                : this(inSource, inSource.Length)
            {
            }

            internal PinnedArrayHandle(T[] inSource, int inLength)
            {
                Handle = GCHandle.Alloc(inSource, GCHandleType.Pinned);

#if UNMANAGED_CONSTRAINT
                Address = (T*) Marshal.UnsafeAddrOfPinnedArrayElement(inSource, 0);
#else
                Address = (void*) Marshal.UnsafeAddrOfPinnedArrayElement(inSource, 0);
                Stride = SizeOf<T>();
#endif // UNMANAGED_CONSTRAINT

                Length = inLength;
            }

            internal PinnedArrayHandle(string inSource)
            {
                Handle = GCHandle.Alloc(inSource, GCHandleType.Pinned);

#if UNMANAGED_CONSTRAINT
                Address = (T*) Handle.AddrOfPinnedObject();
#else
                Address = (void*) Handle.AddrOfPinnedObject();
                Stride = SizeOf<T>();
#endif // UNMANAGED_CONSTRAINT

                Length = inSource.Length;
            }

#if UNMANAGED_CONSTRAINT
            public T* ElementAddress(int inIndex)
            {
                return Address + inIndex;
            }
#else
            public void* ElementAddress(int inIndex)
            {
                return (byte*) Address + Stride * inIndex;
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
        /// Pins the given list in memory.
        /// </summary>
        static public PinnedArrayHandle<T> PinList<T>(List<T> inList)
#if UNMANAGED_CONSTRAINT
            where T : unmanaged
#else
            where T : struct
#endif // UNMANAGED_CONSTRAINT
        {
            if (inList == null)
                throw new ArgumentNullException("inList", "Cannot pin null list");

            if (PinFieldCache<T>.ListItemsField == null)
            {
                PinFieldCache<T>.ListItemsField = typeof(List<T>).GetField("_items", BindingFlags.NonPublic | BindingFlags.Instance);
                if (PinFieldCache<T>.ListItemsField == null)
                    throw new InvalidOperationException("List pin not supported");
            }

            T[] items = (T[]) PinFieldCache<T>.ListItemsField.GetValue(inList);
            return new PinnedArrayHandle<T>(items, inList.Count);
        }

        /// <summary>
        /// Pins the given string in memory.
        /// </summary>
        static public PinnedArrayHandle<char> PinString(string inString)
        {
            if (inString == null)
                throw new ArgumentNullException("inString", "Cannot pin null string");

            return new PinnedArrayHandle<char>(inString);
        }

        /// <summary>
        /// Pins the given StringBuilder in memory.
        /// </summary>
        static public PinnedArrayHandle<char> PinString(StringBuilder inString)
        {
            if (inString == null)
                throw new ArgumentNullException("inString", "Cannot pin null StringBuilder");

            if (inString.Length > 8000)
                throw new ArgumentException("inString", "Cannot pin a StringBuilder with more than one chunk");

            if (s_StringBuilderChunkField == null)
            {
                s_StringBuilderChunkField = typeof(StringBuilder).GetField("m_ChunkChars", BindingFlags.NonPublic | BindingFlags.Instance);
                if (s_StringBuilderChunkField == null)
                    throw new InvalidOperationException("StringBuilder pin not supported");
            }

            char[] chunkChars = (char[]) s_StringBuilderChunkField.GetValue(inString);
            return new PinnedArrayHandle<char>(chunkChars);
        }

        #endregion // Pinning

        #region Debug

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

                for (int i = 0; i < inSize; i++)
                {
                    if (i > 0 && inSeparatorInterval != 0 && (i % inSeparatorInterval) == 0)
                    {
                        *writeHead++ = inSeparator;
                    }
                    val = *bytes++;
                    *writeHead++ = StringUtils.HexCharsUpper[val / 16];
                    *writeHead++ = StringUtils.HexCharsUpper[val % 16];
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

                for (int i = 0; i < inSize; i++)
                {
                    if (i > 0 && inSeparatorInterval != 0 && (i % inSeparatorInterval) == 0)
                    {
                        *writeHead++ = inSeparator;
                    }
                    val = *bytes++;
                    *writeHead++ = StringUtils.HexCharsUpper[val / 16];
                    *writeHead++ = StringUtils.HexCharsUpper[val % 16];
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
            for (int i = 0; i < inSize; i++)
            {
                if (i > 0 && inSeparatorInterval != 0 && (i % inSeparatorInterval) == 0)
                {
                    ioStringBuilder.Append(inSeparator);
                }
                val = *bytes++;
                ioStringBuilder.Append(StringUtils.HexCharsUpper[val / 16]).Append(StringUtils.HexCharsUpper[val % 16]);
            }
        }

        /// <summary>
        /// Dumps the given memory contents to a StringBuilder.
        /// </summary>
        static public void DumpMemory(void* inBuffer, long inSize, StringBuilder ioStringBuilder, char inSeparator = (char) 0, int inSeparatorInterval = 0)
        {
            byte* bytes = (byte*) inBuffer;
            byte val;
            for (long i = 0; i < inSize; i++)
            {
                if (i > 0 && inSeparatorInterval != 0 && (i % inSeparatorInterval) == 0)
                {
                    ioStringBuilder.Append(inSeparator);
                }
                val = *bytes++;
                ioStringBuilder.Append(StringUtils.HexCharsUpper[val / 16]).Append(StringUtils.HexCharsUpper[val % 16]);
            }
        }

        #endregion // Debug

        #region Hashing

        /// <summary>
        /// Hashes the given data.
        /// </summary>
        static public uint Hash32(void* inData, int inLength)
        {
            if (inLength <= 0)
                return 0;

            // fnv-1a
            uint hash = 2166136261;

            byte* ptr = (byte*) inData;
            while (inLength-- > 0)
            {
                hash = (hash ^ *ptr++) * 16777619;
            }

            return hash;
        }

        /// <summary>
        /// Hashes the given data.
        /// </summary>
        static public ulong Hash64(void* inData, int inLength)
        {
            if (inLength <= 0)
                return 0;

            // fnv-1a
            ulong hash = 14695981039346656037;

            byte* ptr = (byte*) inData;
            while (inLength-- > 0)
            {
                hash = (hash ^ *ptr++) * 1099511628211;
            }

            return hash;
        }

        /// <summary>
        /// Combines one hash with another.
        /// </summary>
        static public uint CombineHash32(uint inInitial, void* inData, int inLength)
        {
            if (inLength <= 0)
                return 0;

            // fnv-1a
            uint hash = inInitial;

            byte* ptr = (byte*) inData;
            while (inLength-- > 0)
            {
                hash = (hash ^ *ptr++) * 16777619;
            }

            return hash;
        }

        /// <summary>
        /// Combines one hash with another.
        /// </summary>
        static public ulong CombineHash64(ulong inInitial, void* inData, int inLength)
        {
            if (inLength <= 0)
                return 0;

            // fnv-1a
            ulong hash = 14695981039346656037;

            byte* ptr = (byte*) inData;
            while (inLength-- > 0)
            {
                hash = (hash ^ *ptr++) * 1099511628211;
            }

            return hash;
        }

#if UNMANAGED_CONSTRAINT

        /// <summary>
        /// Hashes the given data.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public uint Hash32<T>(T inValue) where T : unmanaged
        {
            return Hash32(&inValue, sizeof(T));
        }

        /// <summary>
        /// Hashes the given data.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public ulong Hash64<T>(T inValue) where T : unmanaged
        {
            return Hash64(&inValue, sizeof(T));
        }

        /// <summary>
        /// Combines one hash with another.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public uint CombineHash32<T>(uint inInitial, T inValue) where T : unmanaged
        {
            return CombineHash32(inInitial, &inValue, sizeof(T));
        }

        /// <summary>
        /// Combines one hash with another.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public ulong CombineHash64<T>(ulong inInitial, T inValue) where T : unmanaged
        {
            return CombineHash64(inInitial, &inValue, sizeof(T));
        }

#endif // UNMANAGED_CONSTRAINT

        #endregion // Hashing

        #region Read/Write

        #if UNMANAGED_CONSTRAINT

        /// <summary>
        /// Reads a value of the given type from the given byte buffer.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public T Read<T>(byte** ioBufferPtr, int* ioBytesRemaining)
            where T : unmanaged
        {
            int readSize = sizeof(T);
            if (*ioBytesRemaining < readSize)
            {
                throw new InsufficientMemoryException(string.Format("No space left for reading {0} (size {1} vs remaining {2})", typeof(T).FullName, readSize, *ioBytesRemaining));
            }

            T val = default(T);
            if ((((ulong) *ioBufferPtr) % AlignOf<T>()) == 0)
            {
                val = Reinterpret<byte, T>(*ioBufferPtr);
            }
            else
            {
                Copy(*ioBufferPtr, readSize, &val, readSize);
            }

            *ioBytesRemaining -= readSize;
            *ioBufferPtr += readSize;
            return val;
        }

        /// <summary>
        /// Reads a value of the given type from the given byte buffer.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public void Read<T>(ref T ioValue, byte** ioBufferPtr, int* ioBytesRemaining)
            where T : unmanaged
        {
            ioValue = Read<T>(ioBufferPtr, ioBytesRemaining);
        }

        /// <summary>
        /// Reads a value of the given type from the given byte buffer.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public T Read<T>(ref byte* ioBufferPtr, ref int ioBytesRemaining)
            where T : unmanaged
        {
            int readSize = sizeof(T);
            if (ioBytesRemaining < readSize)
            {
                throw new InsufficientMemoryException(string.Format("No space left for reading {0} (size {1} vs remaining {2})", typeof(T).FullName, readSize, ioBytesRemaining));
            }

            T val = default(T);
            if ((((ulong) *ioBufferPtr) % AlignOf<T>()) == 0)
            {
                val = Reinterpret<byte, T>(*ioBufferPtr);
            }
            else
            {
                Copy(ioBufferPtr, readSize, &val, readSize);
            }

            ioBytesRemaining -= readSize;
            ioBufferPtr += readSize;
            return val;
        }

        /// <summary>
        /// Reads a value of the given type from the given byte buffer.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public void Read<T>(ref T ioValue, ref byte* ioBufferPtr, ref int ioBytesRemaining)
            where T : unmanaged
        {
            ioValue = Read<T>(ref ioBufferPtr, ref ioBytesRemaining);
        }

        /// <summary>
        /// Reads a UTF-8 string from the given byte buffer.
        /// </summary>
        static public string ReadUTF8(byte** ioBufferPtr, int* ioBytesRemaining)
        {
            ushort byteLength = Read<ushort>(ioBufferPtr, ioBytesRemaining);
            if (byteLength > 0)
            {
                if (*ioBytesRemaining < byteLength)
                {
                    throw new InsufficientMemoryException(string.Format("No space left for reading a UTF8 string (size {0} vs remaining {1})", byteLength, *ioBytesRemaining));
                }

                char* charBuffer = stackalloc char[byteLength];
                int charLength = StringUtils.DecodeUFT8(*ioBufferPtr, byteLength, charBuffer, byteLength);

                *ioBufferPtr += byteLength;
                *ioBytesRemaining -= byteLength;

                return new string(charBuffer, 0, charLength);
            }
            else
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Reads a UTF-8 string from the given byte buffer.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public void ReadUTF8(ref string ioValue, byte** ioBufferPtr, int* ioBytesRemaining)
        {
            ioValue = ReadUTF8(ioBufferPtr, ioBytesRemaining);
        }

        /// <summary>
        /// Reads a UTF-8 string from the given byte buffer.
        /// </summary>
        static public string ReadUTF8(ref byte* ioBufferPtr, ref int ioBytesRemaining)
        {
            ushort byteLength = Read<ushort>(ref ioBufferPtr, ref ioBytesRemaining);
            if (byteLength > 0)
            {
                if (ioBytesRemaining < byteLength)
                {
                    throw new InsufficientMemoryException(string.Format("No space left for reading a UTF8 string (size {0} vs remaining {1})", byteLength, ioBytesRemaining));
                }

                char* charBuffer = stackalloc char[byteLength];
                int charLength = StringUtils.DecodeUFT8(ioBufferPtr, byteLength, charBuffer, byteLength);

                ioBufferPtr += byteLength;
                ioBytesRemaining -= byteLength;

                return new string(charBuffer, 0, charLength);
            }
            else
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Reads a UTF-8 string from the given byte buffer.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public void ReadUTF8(ref string ioValue, ref byte* ioBufferPtr, ref int ioBytesRemaining)
        {
            ioValue = ReadUTF8(ref ioBufferPtr, ref ioBytesRemaining);
        }

        /// <summary>
        /// Writes a value of the given type to the given byte buffer.
        /// </summary
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public void Write<T>(T inValue, byte** ioBufferPtr, int* ioBytesWritten, int inBufferCapacity)
            where T : unmanaged
        {
            int writeSize = sizeof(T);
            if (*ioBytesWritten + writeSize > inBufferCapacity)
            {
                throw new InsufficientMemoryException(string.Format("No space left for writing {0} (size {1} vs remaining {2})", typeof(T).FullName, writeSize, inBufferCapacity - *ioBytesWritten));
            }

            Copy(&inValue, writeSize, *ioBufferPtr);
            *ioBytesWritten += writeSize;
            *ioBufferPtr += writeSize;
        }

        /// <summary>
        /// Writes a value of the given type to the given byte buffer.
        /// </summary
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public void Write<T>(T inValue, ref byte* ioBufferPtr, ref int ioBytesWritten, int inBufferCapacity)
            where T : unmanaged
        {
            int writeSize = sizeof(T);
            if (ioBytesWritten + writeSize > inBufferCapacity)
            {
                throw new InsufficientMemoryException(string.Format("No space left for writing {0} (size {1} vs remaining {2})", typeof(T).FullName, writeSize, inBufferCapacity - ioBytesWritten));
            }

            Copy(&inValue, writeSize, ioBufferPtr);
            ioBytesWritten += writeSize;
            ioBufferPtr += writeSize;
        }

        /// <summary>
        /// Writes a UTF8 string to the given byte buffer.
        /// </summary>
        static public void WriteUTF8(string inValue, byte** ioBufferPtr, int* ioBytesWritten, int inBufferCapacity)
        {
            if (string.IsNullOrEmpty(inValue))
            {
                Write((ushort) 0, ioBufferPtr, ioBytesWritten, inBufferCapacity);
                return;
            }

            int potentialWriteSize = StringUtils.EncodeSizeUTF8(inValue.Length);
            if (*ioBytesWritten + potentialWriteSize > inBufferCapacity)
            {
                throw new InsufficientMemoryException(string.Format("No space left for writing a UTF8 string of size {0} (max size {1} vs remaining {2})", inValue.Length, potentialWriteSize, inBufferCapacity - *ioBytesWritten));
            }

            byte* lengthMarker = *ioBufferPtr;
            Write((ushort) inValue.Length, ioBufferPtr, ioBytesWritten, inBufferCapacity);
            fixed (char* strChars = inValue)
            {
                int utf8bytesRaw = StringUtils.EncodeUFT8(strChars, inValue.Length, *ioBufferPtr, inBufferCapacity - *ioBytesWritten);
                if (utf8bytesRaw > ushort.MaxValue)
                {
                    throw new NotSupportedException("String encoded to a byte length greater than ushort.MaxValue - not supported by Unsave.WriteUTF8");
                }
                ushort utf8bytes = (ushort) utf8bytesRaw;
                Copy(&utf8bytes, sizeof(ushort), lengthMarker, sizeof(ushort));
                *ioBufferPtr += utf8bytes;
                *ioBytesWritten += utf8bytes;
            }
        }

        /// <summary>
        /// Writes a UTF8 string to the given byte buffer.
        /// </summary>
        static public void WriteUTF8(string inValue, ref byte* ioBufferPtr, ref int ioBytesWritten, int inBufferCapacity)
        {
            if (string.IsNullOrEmpty(inValue))
            {
                Write((ushort) 0, ref ioBufferPtr, ref ioBytesWritten, inBufferCapacity);
                return;
            }

            int potentialWriteSize = StringUtils.EncodeSizeUTF8(inValue.Length);
            if (ioBytesWritten + potentialWriteSize > inBufferCapacity)
            {
                throw new InsufficientMemoryException(string.Format("No space left for writing a UTF8 string of size {0} (max size {1} vs remaining {2})", inValue.Length, potentialWriteSize, inBufferCapacity - ioBytesWritten));
            }

            byte* lengthMarker = ioBufferPtr;
            Write((ushort) inValue.Length, ref ioBufferPtr, ref ioBytesWritten, inBufferCapacity);
            fixed (char* strChars = inValue)
            {
                int utf8bytesRaw = StringUtils.EncodeUFT8(strChars, inValue.Length, ioBufferPtr, inBufferCapacity - ioBytesWritten);
                if (utf8bytesRaw > ushort.MaxValue)
                {
                    throw new NotSupportedException("String encoded to a byte length greater than ushort.MaxValue - not supported by Unsave.WriteUTF8");
                }
                ushort utf8bytes = (ushort) utf8bytesRaw;
                Copy(&utf8bytes, sizeof(ushort), lengthMarker, sizeof(ushort));
                ioBufferPtr += utf8bytes;
                ioBytesWritten += utf8bytes;
            }
        }

        #endif // UNMANAGED_CONSTRAINT

        #endregion // Read/Write

        #region Endianness

        /// <summary>
        /// Swaps the byte order of the given 2-byte value.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public void SwapEndian(ref short value)
        {
            ushort temp = FastReinterpret<short, ushort>(value);
            SwapEndian(ref temp);
            value = FastReinterpret<ushort, short>(temp);
        }

        /// <summary>
        /// Swaps the byte order of the given 2-byte value.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public void SwapEndian(ref ushort value)
        {
            value = (ushort) (((value & 0xFF00u) >> 8) | ((value & 0x00FFu) << 8));
        }

        /// <summary>
        /// Swaps the byte order of the given 4-byte value.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public void SwapEndian(ref int value)
        {
            uint temp = FastReinterpret<int, uint>(value);
            SwapEndian(ref temp);
            value = FastReinterpret<uint, int>(temp);
        }

        /// <summary>
        /// Swaps the byte order of the given 4-byte value.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public void SwapEndian(ref uint value)
        {
            value = (uint) (((value & 0xFF000000u) >> 24)
                | ((value & 0xFF0000u) >> 8)
                | ((value & 0xFF00u) << 8)
                | ((value & 0xFFu) << 24));
        }

        /// <summary>
        /// Swaps the byte order of the given 4-byte value.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public void SwapEndian(ref float value)
        {
            uint temp = FastReinterpret<float, uint>(value);
            SwapEndian(ref temp);
            value = FastReinterpret<uint, float>(temp);
        }

        /// <summary>
        /// Swaps the byte order of the given 8-byte value.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public void SwapEndian(ref long value)
        {
            ulong temp = FastReinterpret<long, ulong>(value);
            SwapEndian(ref temp);
            value = FastReinterpret<ulong, long>(temp);
        }

        /// <summary>
        /// Swaps the byte order of the given 8-byte value.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public void SwapEndian(ref ulong value)
        {
            value = (ulong) (
                ((value & 0xFF00000000000000u) >> 56)
                | ((value & 0xFF000000000000u) >> 40)
                | ((value & 0xFF0000000000u) >> 24)
                | ((value & 0xFF00000000u) >> 8)
                | ((value & 0xFF000000u) << 8)
                | ((value & 0xFF0000u) << 24)
                | ((value & 0xFF00u) << 40)
                | ((value & 0xFFu) << 56)
                );
        }

        /// <summary>
        /// Swaps the byte order of the given 8-byte value.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public void SwapEndian(ref double value)
        {
            ulong temp = FastReinterpret<double, ulong>(value);
            SwapEndian(ref temp);
            value = FastReinterpret<ulong, double>(temp);
        }

        #endregion // Endianness
    }
}