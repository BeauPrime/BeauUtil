/*
 * Copyright (C) 2021. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    26 July 2020
 * 
 * File:    Unsafe.Arena.cs
 * Purpose: Unsafe arena allocator implementation.
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
        /// Handle for a linear allocator.
        /// </summary>
        #if HAS_DEBUGGER
        [System.Diagnostics.DebuggerDisplay("{ToDebugString()}")]
        #endif // HAS_DEBUGGER
        public struct ArenaHandle : IDebugString, IEquatable<ArenaHandle>, IAllocator
        {
            internal ArenaHeader* HeaderStart;

            internal ArenaHandle(ArenaHeader* inHeader)
            {
                HeaderStart = inHeader;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool Owns(void* inPtr)
            {
                if (ValidateArena(this))
                {
                    return ((byte*) inPtr - HeaderStart->StartPtr) < HeaderStart->Size;
                }
                return false;
            }
            
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool IsValid(void* inPtr)
            {
                if (ValidateArena(this))
                {
                    return inPtr >= HeaderStart->StartPtr && inPtr < HeaderStart->CurrentPtr;
                }
                return false;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void* Alloc(int inSize)
            {
                return Unsafe.Alloc(this, inSize);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void* AllocAligned(int inSize, uint inAlign)
            {
                return Unsafe.AllocAligned(this, inSize, inAlign);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)] public int Size() { if (ValidateArena(this)) return (int) HeaderStart->Size; return 0; }
            [MethodImpl(MethodImplOptions.AggressiveInlining)] public int FreeBytes() { if (ValidateArena(this)) return (int) HeaderStart->SizeRemaining; return 0; }
            [MethodImpl(MethodImplOptions.AggressiveInlining)] public int UsedBytes() { if (ValidateArena(this)) return (int) (HeaderStart->Size - HeaderStart->SizeRemaining); return 0; }

            /// <summary>
            /// Arena allocators cannot free memory directly.
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)] public void Free(void* inPtr) { }

            [MethodImpl(MethodImplOptions.AggressiveInlining)] public void Release() { DestroyArena(this); this = default; }
            [MethodImpl(MethodImplOptions.AggressiveInlining)] public bool Validate() { return ValidateArena(this); }

            // arena-specific

            #if UNMANAGED_CONSTRAINT
            [MethodImpl(MethodImplOptions.AggressiveInlining)] public T* AllocArray<T>(int inLength) where T : unmanaged { return Unsafe.AllocArray<T>(this, inLength); }
            [MethodImpl(MethodImplOptions.AggressiveInlining)] public UnsafeSpan<T> AllocSpan<T>(int inLength) where T : unmanaged { return Unsafe.AllocSpan<T>(this, inLength); }
            #else
            [MethodImpl(MethodImplOptions.AggressiveInlining)] public void* AllocArray<T>(int inLength) where T : struct { return Unsafe.AllocArray<T>(this, inLength); }
            #endif // UNMANAGED_CONSTRAINT

            /// <summary>
            /// Resets the arena, freeing all allocations.
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)] public void Reset() { ResetArena(this); }

            /// <summary>
            /// Pushes the current state of the allocator.
            /// This records the currently used amount of memory.
            /// This can be used in conjunction with Pop() to free contiguous chunks of allocations at the tail of the allocator without calling Reset()
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)] public void Push() { PushArena(this); }

            /// <summary>
            /// Pops the state of the allocator.
            /// This restores the last used amount of memory recorded with Push().
            /// This can be used in conjunction with Push() to free contiguous chunks of allocations at the tail of the allocator without calling Reset()
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)] public void Pop() { PopArena(this); }

            public string ToDebugString()
            {
#if HAS_DEBUGGER
                if (ValidateArena(this))
                {
                    return string.Format("[arena '{0}' size={1} remaining={2} flags={3}]", HeaderStart->Name.ToDebugString(), HeaderStart->Size, HeaderStart->SizeRemaining, HeaderStart->Flags);
                }
                else
                {
                    return "null";
                }
#else
                return "[ArenaHandle]";
#endif // HAS_DEBUGGER
            }

            #region Overrides

            public override bool Equals(object obj)
            {
                if (obj is ArenaHandle)
                {
                    return ((ArenaHandle) obj) == this;
                }
                return false;
            }

            public override int GetHashCode()
            {
                return ((ulong) HeaderStart).GetHashCode();
            }

            public bool Equals(ArenaHandle other)
            {
                return HeaderStart == other.HeaderStart;
            }

            static public bool operator==(ArenaHandle inX, ArenaHandle inY)
            {
                return inX.HeaderStart == inY.HeaderStart;
            }

            static public bool operator!=(ArenaHandle inX, ArenaHandle inY)
            {
                return inX.HeaderStart != inY.HeaderStart;
            }

            #endregion // Overrides
        }

        /// <summary>
        /// Linear allocator header block.
        /// </summary>
        internal struct ArenaHeader
        {
            internal const uint ExpectedMagic = 0xBEAA110C;
            internal const int MaxRewindStackSize = 6;
            
            internal uint Magic; // magic value, used to check for memory corruption
            internal StringHash32 Name;
            internal AllocatorFlags Flags;
            internal ushort RewindStackSize;
            internal byte* StartPtr;
            internal byte* CurrentPtr;
            internal uint Size;
            internal uint SizeRemaining;
            internal fixed uint RewindStack[MaxRewindStackSize];

            static internal readonly int HeaderSize = (int) AlignUp32((uint) sizeof(ArenaHeader));

            static internal readonly int MinimumSize = HeaderSize +
#if VALIDATE_ARENA_MEMORY
                sizeof(uint);
#else
                0;
#endif // VALIDATE_ARENA_MEMORY
        }

        #region Lifecycle

        /// <summary>
        /// Creates a linear allocation arena.
        /// </summary>
        static public ArenaHandle CreateArena(int inSize, StringHash32 inName = default, AllocatorFlags inFlags = 0)
        {
            uint arenaSize = AlignUp32((uint) inSize);
            int totalSize = (int) (ArenaHeader.MinimumSize + arenaSize);

            void* block = Alloc(totalSize);
            ArenaHeader* blockHeader = (ArenaHeader*) block;
            byte* dataStart = (byte*) block + ArenaHeader.HeaderSize;

            ArenaHeader header;
            header.Name = inName;
            header.StartPtr = header.CurrentPtr = dataStart;
            header.Size = header.SizeRemaining = arenaSize;
            header.RewindStackSize = 0;
            header.Flags = inFlags & ~AllocatorFlags.DoesNotOwnMemory; // ensure this flag is NOT set to prevent memory leaks
#if VALIDATE_ARENA_MEMORY
            header.Magic = ArenaHeader.ExpectedMagic;
#else
            header.Magic = 0;
#endif // VALIDATE_ARENA_MEMORY

#if VALIDATE_ARENA_MEMORY
            WriteDebugMemoryBoundary(header.CurrentPtr);
#endif // VALIDATE_ARENA_MEMORY

            *blockHeader = header;

            return new ArenaHandle(blockHeader);
        }

        /// <summary>
        /// Creates a linear allocation arena from the memory of another ArenaHandle.
        /// </summary>
        static public ArenaHandle CreateArena(ArenaHandle inBase, int inSize, StringHash32 inName = default, AllocatorFlags inFlags = 0)
        {
            uint arenaSize = AlignUp32((uint) inSize);
            int totalSize = (int) (ArenaHeader.MinimumSize + arenaSize);

            void* block = AllocAligned(inBase, totalSize, AlignOf<ArenaHeader>());
            if (block == null)
            {
                throw new InsufficientMemoryException(string.Format("Provided arena has insufficient memory availabe for {0} required size", totalSize));
            }

            ArenaHeader* blockHeader = (ArenaHeader*) block;
            byte* dataStart = (byte*) block + ArenaHeader.HeaderSize;

            ArenaHeader header;
            header.Name = inName;
            header.StartPtr = header.CurrentPtr = dataStart;
            header.Size = header.SizeRemaining = arenaSize;
            header.RewindStackSize = 0;
            header.Flags = AllocatorFlags.DoesNotOwnMemory | inFlags;
#if VALIDATE_ARENA_MEMORY
            header.Magic = ArenaHeader.ExpectedMagic;
#else
            header.Magic = 0;
#endif // VALIDATE_ARENA_MEMORY

#if VALIDATE_ARENA_MEMORY
            WriteDebugMemoryBoundary(header.CurrentPtr);
#endif // VALIDATE_ARENA_MEMORY

            *blockHeader = header;

            return new ArenaHandle(blockHeader);
        }

        /// <summary>
        /// Creates a linear allocation arena from an existing unmanaged buffer.
        /// </summary>
        static public ArenaHandle CreateArena(void* inUnmanaged, int inSize, StringHash32 inName = default, AllocatorFlags inFlags = 0)
        {
            // ensure this is aligned properly
            void* block = (void*) AlignUpN((ulong) inUnmanaged, AlignOf<ArenaHeader>());
            inSize -= (int) ((ulong) block - (ulong) inUnmanaged);

            int requiredSize = ArenaHeader.MinimumSize;
            int arenaSize = inSize - requiredSize;
            if (arenaSize <= 32)
            {
                throw new InsufficientMemoryException(string.Format("Provided unmanaged buffer size of {0} is insufficient for at least 32 bytes of arena size and {1} required size", inSize, requiredSize));
            }

            ArenaHeader* blockHeader = (ArenaHeader*) block;
            byte* dataStart = (byte*) block + ArenaHeader.HeaderSize;

            ArenaHeader header;
            header.Name = inName;
            header.StartPtr = header.CurrentPtr = dataStart;
            header.Size = header.SizeRemaining = (uint) arenaSize;
            header.RewindStackSize = 0;
            header.Flags = AllocatorFlags.DoesNotOwnMemory | inFlags;
#if VALIDATE_ARENA_MEMORY
            header.Magic = ArenaHeader.ExpectedMagic;
#else
            header.Magic = 0;
#endif // VALIDATE_ARENA_MEMORY

#if VALIDATE_ARENA_MEMORY
            WriteDebugMemoryBoundary(header.CurrentPtr);
#endif // VALIDATE_ARENA_MEMORY

            *blockHeader = header;

            return new ArenaHandle(blockHeader);
        }

        /// <summary>
        /// Returns if the given arena has been initialized.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public bool ArenaInitialized(ArenaHandle inArena)
        {
            return inArena.HeaderStart != null;
        }

        /// <summary>
        /// Destroys the given allocation arena.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public void DestroyArena(ArenaHandle inArena)
        {
            if (ValidateArena(inArena))
            {
                ArenaHeader* header = inArena.HeaderStart;
#if VALIDATE_ARENA_MEMORY
                CheckDebugMemoryBoundary(header->CurrentPtr);
                header->Magic = 0;
#endif // VALIDATE_ARENA_MEMORY

                // if this does not own its own memory we cannot safely free
                if ((header->Flags & AllocatorFlags.DoesNotOwnMemory) == 0)
                {
                    Free(header);
                }
            }
        }

        /// <summary>
        /// Attempts to destroy the given allocation arena.
        /// </summary>
        static public bool TryDestroyArena(ref ArenaHandle ioArena)
        {
            if (ValidateArena(ioArena))
            {
                ArenaHeader* header = ioArena.HeaderStart;
#if VALIDATE_ARENA_MEMORY
                CheckDebugMemoryBoundary(header->CurrentPtr);
                header->Magic = 0;
#endif // VALIDATE_ARENA_MEMORY

                // if this does not own its own memory we cannot safely free
                if ((header->Flags & AllocatorFlags.DoesNotOwnMemory) == 0)
                {
                    Free(header);
                }

                ioArena.HeaderStart = null;
                return true;
            }

            return false;
        }

        #endregion // Lifecycle

        #region Checks

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static internal bool ValidateArena(ArenaHandle inArena)
        {
#if VALIDATE_ARENA_MEMORY
            if (inArena.HeaderStart != null)
            {
                if (inArena.HeaderStart->Magic != ArenaHeader.ExpectedMagic)
                    throw new MemoryCorruptionException(&inArena.HeaderStart->Magic, "Arena header magic value was {0:X} but expected {1:X}", inArena.HeaderStart->Magic, ArenaHeader.ExpectedMagic);
                return true;
            }
            return false;
#else
            return inArena.HeaderStart != null;
#endif // VALIDATE_ARENA_MEMORY
        }

        #endregion // Checks

        #region Allocations

        /// <summary>
        /// Allocates from the given arena.
        /// </summary>
        static public void* Alloc(ArenaHandle inArena, int inLength)
        {
            if (!ValidateArena(inArena) || inLength <= 0)
                return null;
            
            ArenaHeader* header = inArena.HeaderStart;
#if VALIDATE_ARENA_MEMORY
            CheckDebugMemoryBoundary(header->CurrentPtr);
            inLength = (int) AlignUp4((uint) inLength);
#endif // VALIDATE_ARENA_MEMORY

            if (header->SizeRemaining < inLength)
            {
                Log.Error("[Unsafe] Unable to allocate region of size {0} in arena {1} (size remaining {2})", inLength, header->Name, header->SizeRemaining);
                return null;
            }

            void* addr = header->CurrentPtr;
            header->CurrentPtr += inLength;
            header->SizeRemaining -= (uint) inLength;

            if ((header->Flags & AllocatorFlags.ZeroOnAllocate) != 0)
            {
                Clear(addr, inLength);
            }

#if VALIDATE_ARENA_MEMORY
            WriteDebugMemoryBoundary(header->CurrentPtr);
#endif // VALIDATE_ARENA_MEMORY

            return addr;
        }

        /// <summary>
        /// Allocates from the given arena with the given alignment.
        /// </summary>
        static internal void* AllocAligned(ArenaHandle inArena, int inLength, uint inAlignment)
        {
            if (!ValidateArena(inArena) || inLength <= 0)
                return null;

            ArenaHeader* header = inArena.HeaderStart;
#if VALIDATE_ARENA_MEMORY
            CheckDebugMemoryBoundary(header->CurrentPtr);
            inLength = (int) AlignUp4((uint) inLength);
#endif // VALIDATE_ARENA_MEMORY
            
            byte* aligned = (byte*) AlignUpN((ulong) header->CurrentPtr, inAlignment);
            uint padding = (uint) (aligned - header->CurrentPtr);
            if (header->SizeRemaining < padding + inLength)
            {
                Log.Error("[Unsafe] Unable to allocate region of size {0} and alignment {1} in arena {2} (size remaining {3})", inLength, inAlignment, header->Name, header->SizeRemaining);
                return null;
            }

            header->CurrentPtr += inLength + padding;
            header->SizeRemaining -= (uint) inLength + padding;

            if ((header->Flags & AllocatorFlags.ZeroOnAllocate) != 0)
            {
                Clear(aligned, inLength);
            }

#if VALIDATE_ARENA_MEMORY
            WriteDebugMemoryBoundary(header->CurrentPtr);
#endif // VALIDATE_ARENA_MEMORY

            return aligned;
        }

        /// <summary>
        /// Resets the given allocation arena, freeing all its allocations at once.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public void ResetArena(ArenaHandle inArena)
        {
            if (ValidateArena(inArena))
            {
                ArenaHeader* header = inArena.HeaderStart;
#if VALIDATE_ARENA_MEMORY
                CheckDebugMemoryBoundary(header->CurrentPtr);
#endif // VALIDATE_ARENA_MEMORY

                header->SizeRemaining = header->Size;
                header->CurrentPtr = header->StartPtr;
                header->RewindStackSize = 0;
                
#if VALIDATE_ARENA_MEMORY
                WriteDebugMemoryBoundary(header->CurrentPtr);
#endif // VALIDATE_ARENA_MEMORY
            }
        }

#if UNMANAGED_CONSTRAINT

        /// <summary>
        /// Allocates an instance of an unmanaged struct from the given arena.
        /// </summary>
        static public T* Alloc<T>(ArenaHandle inAlloc)
            where T : unmanaged
        {
            return (T*) AllocAligned(inAlloc, SizeOf<T>(), AlignOf<T>());
        }

        /// <summary>
        /// Allocates an array of an unmanaged struct from the given arena.
        /// </summary>
        static public T* AllocArray<T>(ArenaHandle inAlloc, int inLength)
            where T : unmanaged
        {
            return (T*) AllocAligned(inAlloc, SizeOf<T>() * inLength, AlignOf<T>());
        }

        /// <summary>
        /// Allocates an array of an unmanaged struct from the given arena.
        /// </summary>
        static public UnsafeSpan<T> AllocSpan<T>(ArenaHandle inAlloc, int inLength)
            where T : unmanaged
        {
            return new UnsafeSpan<T>((T*) AllocAligned(inAlloc, SizeOf<T>() * inLength, AlignOf<T>()), (uint) inLength);
        }

#else

        /// <summary>
        /// Allocates an instance of an unmanaged struct from the given arena.
        /// </summary>
        static public void* Alloc<T>(ArenaHandle inAlloc)
            where T : struct
        {
            return AllocAligned(inAlloc, SizeOf<T>(), AlignOf<T>());
        }

        /// <summary>
        /// Allocates an array of an unmanaged struct from the given arena.
        /// </summary>
        static public void* AllocArray<T>(ArenaHandle inAlloc, int inLength)
            where T : struct
        {
            return AllocAligned(inAlloc, SizeOf<T>() * inLength, AlignOf<T>());
        }

#endif // UNMANAGED_CONSTRAINT

        #endregion // Allocations

        #region Rewind

        /// <summary>
        /// Pushes the state of the given arena.
        /// This can be used in conjunction with PopArena to rewind the arena's usage to a previous state.
        /// </summary>
        static public void PushArena(ArenaHandle inArena)
        {
            if (ValidateArena(inArena))
            {
                ArenaHeader* header = inArena.HeaderStart;
#if VALIDATE_ARENA_MEMORY
                CheckDebugMemoryBoundary(header->CurrentPtr);
#endif // VALIDATE_ARENA_MEMORY

                if (header->RewindStackSize >= ArenaHeader.MaxRewindStackSize)
                {
                    throw new InvalidOperationException(string.Format("Arena allocator '{0}' is already the maximum number of stack pushes " + ArenaHeader.MaxRewindStackSize, header->Name.ToDebugString()));
                }

                header->RewindStack[header->RewindStackSize++] = (uint) (header->CurrentPtr - header->StartPtr);
            }
        }

        /// <summary>
        /// Pops the state of the given arena.
        /// This can be used in conjunction with PushArena to rewind the arena's usage to a previous state.
        /// </summary>
        static public void PopArena(ArenaHandle inArena)
        {
            if (ValidateArena(inArena))
            {
                ArenaHeader* header = inArena.HeaderStart;
#if VALIDATE_ARENA_MEMORY
                CheckDebugMemoryBoundary(header->CurrentPtr);
#endif // VALIDATE_ARENA_MEMORY

                if (header->RewindStackSize <= 0)
                {
                    throw new InvalidOperationException(string.Format("Arena allocator '{0}' has no stack pushes", header->Name.ToDebugString()));
                }

                header->RewindStackSize--;
                uint poppedOffset = header->RewindStack[header->RewindStackSize];

                header->CurrentPtr = header->StartPtr + poppedOffset;
                header->SizeRemaining = header->Size - poppedOffset;

#if VALIDATE_ARENA_MEMORY
                WriteDebugMemoryBoundary(header->CurrentPtr);
#endif // VALIDATE_ARENA_MEMORY
            }
        }

        #endregion // Rewind
    }
}