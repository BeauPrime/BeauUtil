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
        [MethodImpl(256)]
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
        [MethodImpl(256)]
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
        public struct ArenaHandle : IDebugString
        {
            internal ArenaHeader* HeaderStart;

            internal ArenaHandle(ArenaHeader* inHeader)
            {
                HeaderStart = inHeader;
            }

            [MethodImpl(256)]
            public bool Owns(void* inPtr)
            {
                if (ValidateArena(this))
                {
                    return ((byte*) inPtr - HeaderStart->StartPtr) < HeaderStart->Size;
                }
                return false;
            }
            
            [MethodImpl(256)]
            public bool IsValid(void* inPtr)
            {
                if (ValidateArena(this))
                {
                    return inPtr >= HeaderStart->StartPtr && inPtr < HeaderStart->CurrentPtr;
                }
                return false;
            }

            [MethodImpl(256)] public int Size() { if (ValidateArena(this)) return (int) HeaderStart->Size; return 0; }
            [MethodImpl(256)] public int FreeBytes() { if (ValidateArena(this)) return (int) HeaderStart->SizeRemaining; return 0; }
            [MethodImpl(256)] public void Reset() { ResetArena(this); }

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
        }

        /// <summary>
        /// Linear allocator header block.
        /// </summary>
        internal struct ArenaHeader
        {
            internal const uint ExpectedMagic = 0xF00DFADE;
            internal const uint CorruptionCheckMagic = 0xBAD0F00D;

            internal uint Magic; // magic value, used to check for memory corruption
            public StringHash32 Name;
            internal byte* StartPtr;
            internal byte* CurrentPtr;
            internal uint Size;
            internal uint SizeRemaining;

            static internal readonly int HeaderSize = (int) AlignUp32((uint) sizeof(ArenaHeader));
        }

        #region Lifecycle

        /// <summary>
        /// Creates a linear allocation arena.
        /// </summary>
        static public ArenaHandle CreateArena(int inSize, StringHash32 inName = default)
        {
            uint arenaSize = AlignUp32((uint) inSize);
            int totalSize = (int) (ArenaHeader.HeaderSize + arenaSize);
            #if VALIDATE_ARENA_MEMORY
            totalSize += sizeof(uint); // reserve space at end for debug marker
            #endif // VALIDATE_ARENA_MEMORY

            void* block = Alloc(totalSize);
            ArenaHeader* blockHeader = (ArenaHeader*) block;
            byte* dataStart = (byte*) block + ArenaHeader.HeaderSize;

            ArenaHeader header;
            header.Name = inName;
            header.StartPtr = header.CurrentPtr = dataStart;
            header.Size = header.SizeRemaining = arenaSize;
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
        [MethodImpl(256)]
        static public bool ArenaInitialized(ArenaHandle inArena)
        {
            return inArena.HeaderStart != null;
        }

        /// <summary>
        /// Destroys the given allocation arena.
        /// </summary>
        [MethodImpl(256)]
        static public void DestroyArena(ArenaHandle inArena)
        {
            if (ValidateArena(inArena))
            {
                ArenaHeader* header = inArena.HeaderStart;
                #if VALIDATE_ARENA_MEMORY
                CheckDebugMemoryBoundary(header->CurrentPtr);
                #endif // VALIDATE_ARENA_MEMORY
                Free(header);
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
                #endif // VALIDATE_ARENA_MEMORY
                Free(header);

                ioArena.HeaderStart = null;
                return true;
            }

            return false;
        }

        #endregion // Lifecycle

        #region Checks

        [MethodImpl(256)]
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
            if (!ValidateArena(inArena))
                return null;
            
            ArenaHeader* header = inArena.HeaderStart;
            #if VALIDATE_ARENA_MEMORY
            CheckDebugMemoryBoundary(header->CurrentPtr);
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
            if (!ValidateArena(inArena))
                return null;

            ArenaHeader* header = inArena.HeaderStart;
            #if VALIDATE_ARENA_MEMORY
            CheckDebugMemoryBoundary(header->CurrentPtr);
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
        [MethodImpl(256)]
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
            return ArenaAllocAligned(inAlloc, SizeOf<T>(), AlignOf<T>());
        }

        /// <summary>
        /// Allocates an array of an unmanaged struct from the given arena.
        /// </summary>
        static public void* AllocArray<T>(ArenaHandle inAlloc, int inLength)
            where T : struct
        {
            return ArenaAllocAligned(inAlloc, SizeOf<T>() * inLength, AlignOf<T>());
        }

        #endif // UNMANAGED_CONSTRAINT

        #endregion // Allocations

        #endregion // Arenas
    }
}