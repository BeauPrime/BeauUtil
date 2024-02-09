///*
// * Copyright (C) 2023. Autumn Beauchesne. All rights reserved.
// * Author:  Autumn Beauchesne
// * Date:    6 Nov 2023
// * 
// * File:    Unsafe.Pool.cs
// * Purpose: Unsafe pool allocator implementation.
// */

//#if CSHARP_7_3_OR_NEWER
//#define UNMANAGED_CONSTRAINT
//#endif // CSHARP_7_3_OR_NEWER

//#if (UNITY_EDITOR && !IGNORE_UNITY_EDITOR) || DEVELOPMENT_BUILD || DEVELOPMENT
//#define HAS_DEBUGGER
//#define VALIDATE_ARENA_MEMORY
//#endif // (UNITY_EDITOR && !IGNORE_UNITY_EDITOR) || DEVELOPMENT_BUILD || DEVELOPMENT

//using System;
//using System.Collections.Generic;
//using System.Runtime.CompilerServices;
//using System.Runtime.InteropServices;
//using BeauUtil.Debugger;

//namespace BeauUtil
//{
//    /// <summary>
//    /// Contains unsafe utility functions.
//    /// </summary>
//    static public unsafe partial class Unsafe
//    {
//        /// <summary>
//        /// Handle for a pool allocator.
//        /// </summary>
//#if HAS_DEBUGGER
//        [System.Diagnostics.DebuggerDisplay("{ToDebugString()}")]
//#endif // HAS_DEBUGGER
//        public struct PoolHandle : IDebugString, IEquatable<PoolHandle>
//        {
//            internal PoolHeader* HeaderStart;

//            internal PoolHandle(PoolHeader* inHeader)
//            {
//                HeaderStart = inHeader;
//            }

//            [MethodImpl(MethodImplOptions.AggressiveInlining)]
//            public bool Owns(void* inPtr)
//            {
//                if (ValidatePool(this))
//                {
//                    return ((byte*) inPtr - HeaderStart->ChunkPtr) < HeaderStart->Size;
//                }
//                return false;
//            }

//            [MethodImpl(MethodImplOptions.AggressiveInlining)]
//            public bool IsValid(void* inPtr)
//            {
//                if (ValidatePool(this))
//                {
//                    return inPtr >= HeaderStart->ChunkPtr && inPtr < HeaderStart->CurrentPtr;
//                }
//                return false;
//            }

//            [MethodImpl(MethodImplOptions.AggressiveInlining)] public void* Alloc() { return Unsafe.Alloc(this); }

//            [MethodImpl(MethodImplOptions.AggressiveInlining)] public int Size() { if (ValidatePool(this)) return (int) HeaderStart->Size; return 0; }
//            [MethodImpl(MethodImplOptions.AggressiveInlining)] public int FreeBytes() { if (ValidatePool(this)) return (int) HeaderStart->SizeRemaining; return 0; }
//            [MethodImpl(MethodImplOptions.AggressiveInlining)] public int UsedBytes() { if (ValidatePool(this)) return (int) (HeaderStart->Size - HeaderStart->SizeRemaining); return 0; }

//            [MethodImpl(MethodImplOptions.AggressiveInlining)] public void Free(void* inPtr) { Unsafe.Free(this, inPtr); }

//            [MethodImpl(MethodImplOptions.AggressiveInlining)] public void Release() { DestroyPool(this); this = default; }
//            [MethodImpl(MethodImplOptions.AggressiveInlining)] public bool Validate() { return ValidatePool(this); }

//            /// <summary>
//            /// Resets the pool, freeing all allocations.
//            /// </summary>
//            [MethodImpl(MethodImplOptions.AggressiveInlining)] public void Reset() { ResetPool(this); }

//            public string ToDebugString()
//            {
//#if HAS_DEBUGGER
//                if (ValidatePool(this))
//                {
//                    return string.Format("[pool '{0}' size={1} remaining={2} flags={3}]", HeaderStart->Name.ToDebugString(), HeaderStart->Size, HeaderStart->SizeRemaining, HeaderStart->Flags);
//                }
//                else
//                {
//                    return "null";
//                }
//#else
//                return "[PoolHandle]";
//#endif // HAS_DEBUGGER
//            }

//            #region Overrides

//            public override bool Equals(object obj)
//            {
//                if (obj is PoolHandle)
//                {
//                    return ((PoolHandle) obj) == this;
//                }
//                return false;
//            }

//            public override int GetHashCode()
//            {
//                return ((ulong) HeaderStart).GetHashCode();
//            }

//            public bool Equals(PoolHandle other)
//            {
//                return HeaderStart == other.HeaderStart;
//            }

//            static public bool operator ==(PoolHandle inX, PoolHandle inY)
//            {
//                return inX.HeaderStart == inY.HeaderStart;
//            }

//            static public bool operator !=(PoolHandle inX, PoolHandle inY)
//            {
//                return inX.HeaderStart != inY.HeaderStart;
//            }

//            #endregion // Overrides
//        }

//        /// <summary>
//        /// Pool allocator header block.
//        /// </summary>
//        internal struct PoolHeader
//        {
//            internal const uint ExpectedMagic = 0xBEAAD001;

//            internal uint Magic; // magic value, used to check for memory corruption
//            internal StringHash32 Name;
//            internal AllocatorFlags Flags;

//            internal byte* BitSetPtr;
//            internal byte* ChunkPtr;
//            internal uint Size;
//            internal uint ChunkSize;
//            internal uint ChunkCount;

//            static internal readonly int HeaderSize = (int) AlignUp32((uint) sizeof(PoolHeader));
//            static internal readonly int MinimumSize = HeaderSize;
//        }

//        internal struct PoolChunk
//        {
//            public PoolChunk* Next;
//        }

//        #region Lifecycle

//        /// <summary>
//        /// Creates a pool allocator.
//        /// </summary>
//        static public PoolHandle CreatePool(int inChunkSize, int inNumChunks, StringHash32 inName = default, AllocatorFlags inFlags = 0)
//        {
//            uint arenaSize = AlignUp32((uint) inSize);
//            int totalSize = (int) (PoolHeader.MinimumSize + arenaSize);

//            void* block = Alloc(totalSize);
//            PoolHeader* blockHeader = (PoolHeader*) block;
//            byte* dataStart = (byte*) block + PoolHeader.HeaderSize;

//            PoolHeader header;
//            header.Name = inName;
//            header.StartPtr = header.CurrentPtr = dataStart;
//            header.Size = header.SizeRemaining = arenaSize;
//            header.Flags = inFlags & ~AllocatorFlags.DoesNotOwnMemory; // ensure this flag is NOT set to prevent memory leaks
//#if VALIDATE_ARENA_MEMORY
//            header.Magic = PoolHeader.ExpectedMagic;
//#else
//            header.Magic = 0;
//#endif // VALIDATE_ARENA_MEMORY

//#if VALIDATE_ARENA_MEMORY
//            WriteDebugMemoryBoundary(header.CurrentPtr);
//#endif // VALIDATE_ARENA_MEMORY

//            *blockHeader = header;

//            return new PoolHandle(blockHeader);
//        }

//        /// <summary>
//        /// Creates a pool allocator from the memory of another ArenaHandle.
//        /// </summary>
//        static public PoolHandle CreatePool(ArenaHandle inBase, int inChunkSize, int inNumChunks, StringHash32 inName = default, AllocatorFlags inFlags = 0)
//        {
//            uint arenaSize = AlignUp32((uint) inSize);
//            int totalSize = (int) (PoolHeader.MinimumSize + arenaSize);

//            void* block = AllocAligned(inBase, totalSize, AlignOf<PoolHeader>());
//            PoolHeader* blockHeader = (PoolHeader*) block;
//            byte* dataStart = (byte*) block + PoolHeader.HeaderSize;

//            PoolHeader header;
//            header.Name = inName;
//            header.StartPtr = header.CurrentPtr = dataStart;
//            header.Size = header.SizeRemaining = arenaSize;
//            header.Flags = AllocatorFlags.DoesNotOwnMemory | inFlags;
//#if VALIDATE_ARENA_MEMORY
//            header.Magic = PoolHeader.ExpectedMagic;
//#else
//            header.Magic = 0;
//#endif // VALIDATE_ARENA_MEMORY

//#if VALIDATE_ARENA_MEMORY
//            WriteDebugMemoryBoundary(header.CurrentPtr);
//#endif // VALIDATE_ARENA_MEMORY

//            *blockHeader = header;

//            return new PoolHandle(blockHeader);
//        }

//        /// <summary>
//        /// Creates a pool allocator from an existing unmanaged buffer.
//        /// </summary>
//        static public PoolHandle CreatePool(void* inUnmanaged, int inSize, int inChunkSize, StringHash32 inName = default, AllocatorFlags inFlags = 0)
//        {
//            // ensure this is aligned properly
//            void* block = (void*) AlignUpN((ulong) inUnmanaged, AlignOf<PoolHeader>());
//            inSize -= (int) ((ulong) block - (ulong) inUnmanaged);

//            int requiredSize = PoolHeader.MinimumSize;
//            int arenaSize = inSize - requiredSize;
//            if (arenaSize <= 32)
//            {
//                throw new InsufficientMemoryException(string.Format("Provided unmanaged buffer size of {0} is insufficient for at least 32 bytes of pool size and {1} required size", inSize, requiredSize));
//            }

//            PoolHeader* blockHeader = (PoolHeader*) block;
//            byte* dataStart = (byte*) block + PoolHeader.HeaderSize;

//            PoolHeader header;
//            header.Name = inName;
//            header.StartPtr = header.CurrentPtr = dataStart;
//            header.Size = header.SizeRemaining = (uint) arenaSize;
//            header.Flags = AllocatorFlags.DoesNotOwnMemory | inFlags;
//#if VALIDATE_ARENA_MEMORY
//            header.Magic = PoolHeader.ExpectedMagic;
//#else
//            header.Magic = 0;
//#endif // VALIDATE_ARENA_MEMORY

//            *blockHeader = header;

//            return new PoolHandle(blockHeader);
//        }

//        /// <summary>
//        /// Returns if the given arena has been initialized.
//        /// </summary>
//        [MethodImpl(MethodImplOptions.AggressiveInlining)]
//        static public bool PoolInitialized(PoolHandle inArena)
//        {
//            return inArena.HeaderStart != null;
//        }

//        /// <summary>
//        /// Destroys the given allocation arena.
//        /// </summary>
//        [MethodImpl(MethodImplOptions.AggressiveInlining)]
//        static public void DestroyPool(PoolHandle inArena)
//        {
//            if (ValidatePool(inArena))
//            {
//                PoolHeader* header = inArena.HeaderStart;
//#if VALIDATE_ARENA_MEMORY
//                CheckDebugMemoryBoundary(header->CurrentPtr);
//                header->Magic = 0;
//#endif // VALIDATE_ARENA_MEMORY

//                // if this does not own its own memory we cannot safely free
//                if ((header->Flags & AllocatorFlags.DoesNotOwnMemory) == 0)
//                {
//                    Free(header);
//                }
//            }
//        }

//        /// <summary>
//        /// Attempts to destroy the given allocation arena.
//        /// </summary>
//        static public bool TryDestroyPool(ref PoolHandle ioArena)
//        {
//            if (ValidatePool(ioArena))
//            {
//                PoolHeader* header = ioArena.HeaderStart;
//#if VALIDATE_ARENA_MEMORY
//                CheckDebugMemoryBoundary(header->CurrentPtr);
//                header->Magic = 0;
//#endif // VALIDATE_ARENA_MEMORY

//                // if this does not own its own memory we cannot safely free
//                if ((header->Flags & AllocatorFlags.DoesNotOwnMemory) == 0)
//                {
//                    Free(header);
//                }

//                ioArena.HeaderStart = null;
//                return true;
//            }

//            return false;
//        }

//        static internal uint CalculateChunkCountForFixedSizeWithByteMask(uint inFixedSize, uint inChunkSize)
//        {
//            // fixedSize = chunkCount * chunkSize + (chunkCount / 8)
//            // fixedSize = chunkCount * (chunkSize + 1/8)
//            // chunkCount = fixedSize / (chunkSize + 1/8)
//            // chunkCount = (fixedSize * 8) / (8 * chunkSize + 1)
//            return ((inFixedSize << 3) / ((inChunkSize << 3) + 1));
//        }

//        #endregion // Lifecycle

//        #region Checks

//        [MethodImpl(MethodImplOptions.AggressiveInlining)]
//        static internal bool ValidatePool(PoolHandle inArena)
//        {
//#if VALIDATE_ARENA_MEMORY
//            if (inArena.HeaderStart != null)
//            {
//                if (inArena.HeaderStart->Magic != PoolHeader.ExpectedMagic)
//                    throw new MemoryCorruptionException(&inArena.HeaderStart->Magic, "Pool header magic value was {0:X} but expected {1:X}", inArena.HeaderStart->Magic, PoolHeader.ExpectedMagic);
//                return true;
//            }
//            return false;
//#else
//            return inArena.HeaderStart != null;
//#endif // VALIDATE_ARENA_MEMORY
//        }

//        #endregion // Checks

//        #region Allocations

//        /// <summary>
//        /// Allocates from the given arena.
//        /// </summary>
//        static public void* Alloc(PoolHandle inArena)
//        {
//            if (!ValidatePool(inArena))
//                return null;

//            PoolHeader* header = inArena.HeaderStart;

//            if (header->SizeRemaining < inLength)
//            {
//                Log.Error("[Unsafe] Unable to allocate region of size {0} in arena {1} (size remaining {2})", inLength, header->Name, header->SizeRemaining);
//                return null;
//            }

//            void* addr = header->CurrentPtr;
//            header->CurrentPtr += inLength;
//            header->SizeRemaining -= (uint) inLength;

//            if ((header->Flags & AllocatorFlags.ZeroOnAllocate) != 0)
//            {
//                Clear(addr, inLength);
//            }

//            return addr;
//        }

//        /// <summary>
//        /// Frees back to the given arena.
//        /// </summary>
//        static public void* Free(PoolHandle inArena, void* inChunk)
//        {
//            if (!ValidatePool(inArena))
//                return null;

//            PoolHeader* header = inArena.HeaderStart;

//            if (header->SizeRemaining < inLength)
//            {
//                Log.Error("[Unsafe] Unable to allocate region of size {0} in arena {1} (size remaining {2})", inLength, header->Name, header->SizeRemaining);
//                return null;
//            }

//            void* addr = header->CurrentPtr;
//            header->CurrentPtr += inLength;
//            header->SizeRemaining -= (uint) inLength;

//            if ((header->Flags & AllocatorFlags.ZeroOnAllocate) != 0)
//            {
//                Clear(addr, inLength);
//            }

//            return addr;
//        }

//        /// <summary>
//        /// Resets the given allocation pool, freeing all its allocations at once.
//        /// </summary>
//        [MethodImpl(MethodImplOptions.AggressiveInlining)]
//        static public void ResetPool(PoolHandle inArena)
//        {
//            if (ValidatePool(inArena))
//            {
//                PoolHeader* header = inArena.HeaderStart;
//                header->SizeRemaining = header->Size;
//                header->CurrentPtr = header->StartPtr;
//            }
//        }

//#if UNMANAGED_CONSTRAINT

//        /// <summary>
//        /// Allocates an instance of an unmanaged struct from the given arena.
//        /// </summary>
//        static public T* Alloc<T>(PoolHandle inAlloc)
//            where T : unmanaged
//        {
//            return (T*) Alloc(inAlloc);
//        }

//#else

//        /// <summary>
//        /// Allocates an instance of an unmanaged struct from the given arena.
//        /// </summary>
//        static public void* Alloc<T>(PoolHandle inAlloc)
//            where T : struct
//        {
//            return Alloc(inAlloc);
//        }

//#endif // UNMANAGED_CONSTRAINT

//        #endregion // Allocations
//    }
//}