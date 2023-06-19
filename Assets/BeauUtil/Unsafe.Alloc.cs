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

        #region Arenas

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
                    return string.Format("[arena '{0}' size={1} remaining={2}]", HeaderStart->Name.ToDebugString(), HeaderStart->Size, HeaderStart->SizeRemaining);
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
            internal const uint CorruptionCheckMagic = 0xBAD0F00D;
            internal const int MaxRewindStackSize = 6;

            internal const ushort Flag_DoesNotOwnMemory = 0x01; // indicates this allocator does not have sole ownership of its buffer
            
            internal uint Magic; // magic value, used to check for memory corruption
            public StringHash32 Name;
            internal ushort Flags;
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
        static public ArenaHandle CreateArena(int inSize, StringHash32 inName = default)
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
            header.Flags = 0;
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
        static public ArenaHandle CreateArena(ArenaHandle inBase, int inSize, StringHash32 inName = default)
        {
            uint arenaSize = AlignUp32((uint) inSize);
            int totalSize = (int) (ArenaHeader.MinimumSize + arenaSize);

            void* block = AllocAligned(inBase, totalSize, AlignOf<ArenaHeader>());
            ArenaHeader* blockHeader = (ArenaHeader*) block;
            byte* dataStart = (byte*) block + ArenaHeader.HeaderSize;

            ArenaHeader header;
            header.Name = inName;
            header.StartPtr = header.CurrentPtr = dataStart;
            header.Size = header.SizeRemaining = arenaSize;
            header.RewindStackSize = 0;
            header.Flags = ArenaHeader.Flag_DoesNotOwnMemory;
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
        static public ArenaHandle CreateArena(void* inUnmanaged, int inSize, StringHash32 inName = default)
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
            header.Flags = ArenaHeader.Flag_DoesNotOwnMemory;
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
                if ((header->Flags & ArenaHeader.Flag_DoesNotOwnMemory) == 0)
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
                if ((header->Flags & ArenaHeader.Flag_DoesNotOwnMemory) == 0)
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

        #if VALIDATE_ARENA_MEMORY

        /// <summary>
        /// Writes the debug magic value at the given address.
        /// </summary>
        static internal void WriteDebugMemoryBoundary(void* inPtr)
        {
            *((uint*) inPtr) = ArenaHeader.CorruptionCheckMagic;
        }

        /// <summary>
        /// Checks that the value at the given address is the debug magic value.
        /// </summary>
        static internal void CheckDebugMemoryBoundary(void* inPtr)
        {
            uint val = *((uint*) inPtr);
            if (val != ArenaHeader.CorruptionCheckMagic)
            {
                throw new MemoryCorruptionException(inPtr, "Arena memory boundary value was {0:X} but expected {1:X}. Most likely memory was written outside of allocated bounds.", val, ArenaHeader.CorruptionCheckMagic);
            }
        }

        #endif // VALIDATE_ARENA_MEMORY

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

        #endregion // Arenas
    }
}