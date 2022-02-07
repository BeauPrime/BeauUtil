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
using BeauUtil.Debugger;

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

        private struct AlignHelper<T>
            where T : struct
        {
            private byte _;
            private T i;

            static internal readonly uint Alignment = (uint) Marshal.OffsetOf<AlignHelper<T>>("i");
        }

        /// <summary>
        /// Returns the alignment of the given 
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

        #if UNMANAGED_CONSTRAINT

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

        static public T* AllocArray<T>(ArenaHandle inArena, int inLength)
            where T : unmanaged
        {
            return (T*) AllocAligned(inArena, inLength * sizeof(T), AlignOf<T>());
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

        static public void* AllocArray<T>(ArenaHandle inArena, int inLength)
            where T : struct
        {
            return (void*) AllocAligned(inArena, inLength * SizeOf<T>(), AlignOf<T>());
        }

        static public void* ReallocArray<T>(void* inPtr, int inLength)
            where T : struct
        {
            return (void*) Marshal.ReAllocHGlobal((IntPtr) inPtr, (IntPtr) (inLength * SizeOf<T>()));
        }

        #endif // UNMANAGED_CONSTRAINT

        static public void* Alloc(int inLength)
        {
            return (void*) Marshal.AllocHGlobal(inLength);
        }

        /// <summary>
        /// Allocates from the given arena.
        /// </summary>
        static public void* Alloc(ArenaHandle inArena, int inLength)
        {
            ArenaHeader* header = (ArenaHeader*) inArena.HeaderStart;
            if (header == null)
            {
                return null;
            }

            if (header->SizeRemaining < inLength)
            {
                Log.Warn("[Unsafe] Unable to allocate region of size {0} in arena {1} (size remaining {2})", inLength, header->Name, header->SizeRemaining);
                return null;
            }

            void* addr = header->CurrentPtr;
            header->CurrentPtr += inLength;
            header->SizeRemaining -= (uint) inLength;
            return addr;
        }

        /// <summary>
        /// Allocates from the given arena with the given alignment.
        /// </summary>
        static public void* AllocAligned(ArenaHandle inArena, int inLength, uint inAlignment)
        {
            ArenaHeader* header = (ArenaHeader*) inArena.HeaderStart;
            if (header == null)
            {
                return null;
            }
            
            byte* aligned = (byte*) AlignUpN((ulong) header->CurrentPtr, inAlignment);
            uint padding = (uint) (aligned - header->CurrentPtr);
            if (header->SizeRemaining < padding + inLength)
            {
                Log.Warn("[Unsafe] Unable to allocate region of size {0} and alignment {1} in arena {2} (size remaining {3})", inLength, inAlignment, header->Name, header->SizeRemaining);
                return null;
            }

            header->CurrentPtr += inLength + padding;
            header->SizeRemaining -= (uint) inLength + padding;
            return aligned;
        }

        static public void* Realloc(void* inPtr, int inLength)
        {
            return (void*) Marshal.ReAllocHGlobal((IntPtr) inPtr, (IntPtr) inLength);
        }

        static public void Free(void* inPtr)
        {
            Marshal.FreeHGlobal((IntPtr) inPtr);
        }

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

        #region Arenas

        /// <summary>
        /// Handle for a linear allocator.
        /// </summary>
        public struct ArenaHandle {
            internal ArenaHeader* HeaderStart;

            internal ArenaHandle(ArenaHeader* inHeader)
            {
                HeaderStart = inHeader;
            }
        }

        internal struct ArenaHeader {
            public StringHash32 Name;
            internal byte* StartPtr;
            internal byte* CurrentPtr;
            internal uint Size;
            internal uint SizeRemaining;

            static internal readonly int HeaderSize = (int) AlignUp32((uint) sizeof(ArenaHeader));
        }

        /// <summary>
        /// Creates a linear allocation arena.
        /// </summary>
        static public ArenaHandle CreateArena(int inSize, StringHash32 inName = default)
        {
            uint arenaSize = AlignUp32((uint) inSize);
            int totalSize = (int) (ArenaHeader.HeaderSize + arenaSize);

            void* block = Alloc(totalSize);
            ArenaHeader* blockHeader = (ArenaHeader*) block;
            byte* dataStart = (byte*) block + ArenaHeader.HeaderSize;

            ArenaHeader header;
            header.Name = inName;
            header.StartPtr = header.CurrentPtr = dataStart;
            header.Size = header.SizeRemaining = arenaSize;

            *blockHeader = header;

            return new ArenaHandle(blockHeader);
        }

        /// <summary>
        /// Returns the size of the given arena.
        /// </summary>
        static public int ArenaSize(ArenaHandle inArena)
        {
            ArenaHeader* header = (ArenaHeader*) inArena.HeaderStart;
            if (header != null)
            {
                return (int) header->Size;
            }
            return 0;
        }

        /// <summary>
        /// Returns the number of free bytes in the given arena.
        /// </summary>
        /// <returns></returns>
        static public int ArenaFreeBytes(ArenaHandle inArena)
        {
            ArenaHeader* header = (ArenaHeader*) inArena.HeaderStart;
            if (header != null)
            {
                return (int) header->SizeRemaining;
            }
            return 0;
        }

        /// <summary>
        /// Resets the given allocation arena.
        /// </summary>
        static public void ResetArena(ArenaHandle inArena)
        {
            ArenaHeader* header = (ArenaHeader*) inArena.HeaderStart;
            if (header != null)
            {
                header->SizeRemaining = header->Size;
                header->CurrentPtr = header->StartPtr;
            }
        }

        /// <summary>
        /// Frees the given allocation arena.
        /// </summary>
        static public void FreeArena(ArenaHandle inArena)
        {
            Free(inArena.HeaderStart);
        }

        /// <summary>
        /// Attempts to free the given allocation arena.
        /// </summary>
        static public bool TryFreeArena(ref ArenaHandle ioArena)
        {
            if (ioArena.HeaderStart != null)
            {
                Free(ioArena.HeaderStart);
                ioArena.HeaderStart = null;
                return true;
            }

            return false;
        }

        #endregion // Arenas
    }
}