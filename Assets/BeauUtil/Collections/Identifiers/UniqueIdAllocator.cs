/*
 * Copyright (C) 2023. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    29 June 2023
 * 
 * File:    UniqueIdAllocator.cs
 * Purpose: Unique identifier allocators.
*/

using System;
using System.Runtime.CompilerServices;

namespace BeauUtil
{
    /// <summary>
    /// 16-bit unique id allocator.
    /// </summary>
    public sealed class UniqueIdAllocator16
    {
        private byte[] m_Versions;
        private RingBuffer<ushort> m_FreeList;
        private int m_Capacity = 0;
        private int m_MaxFree = 0;

        public UniqueIdAllocator16(int inInitialCapacity)
        {
            if (inInitialCapacity < 1)
                throw new ArgumentOutOfRangeException("inInitialCapacity", "Initial capacity must be at least 1");
            m_Capacity = Unsafe.AlignUp16(inInitialCapacity);
            if (m_Capacity > UniqueId16.MaxIndex)
                throw new ArgumentOutOfRangeException("inInitialCapacity", "Capacity, rounded up, exceeds maximum UniqueId16 index");

            m_Versions = new byte[m_Capacity];
            m_FreeList = new RingBuffer<ushort>(m_Capacity, RingBufferMode.Expand);

            m_MaxFree = 1;
            m_Versions[0] = 1;
            m_FreeList.PushBack(0);
        }

        #region Alloc/Free

        /// <summary>
        /// Allocates a new unique identifier.
        /// </summary>
        public UniqueId16 Alloc()
        {
            Reserve(1);
            ushort index = m_FreeList.PopFront();
            ushort version = m_Versions[index];
            return new UniqueId16(index, version);
        }

        /// <summary>
        /// Frees the given identifier.
        /// </summary>
        public bool Free(UniqueId16 inId)
        {
            int index = inId.Index;
            int version = inId.Version;
            if (index < m_Capacity && m_Versions[index] == version)
            {
                m_FreeList.PushBack((ushort) index);
                version = version == UniqueId16.VersionMask ? 1 : version + 1;
                m_Versions[index] = (byte) version;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Reserves the given number of unique identifiers.
        /// </summary>
        public void Reserve(int inCount)
        {
            if (m_FreeList.Count >= inCount)
                return;

            if (m_MaxFree < m_Capacity)
            {
                m_Versions[m_MaxFree] = 1;
                m_FreeList.PushBack((ushort) m_MaxFree++);
                return;
            }

            if (m_Capacity + inCount > UniqueId16.MaxIndex)
            {
                throw new InvalidOperationException(string.Format("UniqueIdAllocator16 can only have a maximum of {0} identifiers at once", UniqueId16.MaxIndex));
            }

            int newLength = Math.Min(m_Capacity * 2, UniqueId16.MaxIndex);
            Array.Resize(ref m_Versions, newLength);
            m_FreeList.SetCapacity(newLength);
            m_FreeList.PushBack((ushort) m_MaxFree++);
        }

        /// <summary>
        /// Resets all allocated handles.
        /// </summary>
        public void Reset()
        {
            for (int i = 0; i < m_Capacity; i++)
                m_Versions[i] = 1;
            m_MaxFree = 1;
            m_FreeList.Clear();
            m_FreeList.PushBack(0);
        }

        #endregion // Alloc/Free

        #region Checks

        /// <summary>
        /// Returns the current capacity.
        /// </summary>
        public int Capacity
        {
            get { return m_Capacity; }
        }

        /// <summary>
        /// Returns the number of identifiers in use.
        /// </summary>
        public int InUse
        {
            get { return m_MaxFree - m_FreeList.Count; }
        }

        /// <summary>
        /// Returns if the given id is valid.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsValid(UniqueId16 inId)
        {
            int index = inId.Index;
            return index < m_MaxFree && m_Versions[index] == inId.Version;
        }

        #endregion // Checks
    }

    /// <summary>
    /// 32-bit unique id allocator.
    /// </summary>
    public sealed class UniqueIdAllocator32
    {
        private byte[] m_Versions;
        private RingBuffer<uint> m_FreeList;
        private int m_Capacity = 0;
        private int m_MaxFree;

        public UniqueIdAllocator32(int inInitialCapacity)
        {
            if (inInitialCapacity < 1)
                throw new ArgumentOutOfRangeException("inInitialCapacity", "Initial capacity must be at least 1");
            m_Capacity = Unsafe.AlignUp16(inInitialCapacity);
            if (m_Capacity > UniqueId32.MaxIndex)
                throw new ArgumentOutOfRangeException("inInitialCapacity", "Capacity, rounded up, exceeds maximum UniqueId16 index");

            m_Versions = new byte[m_Capacity];
            m_FreeList = new RingBuffer<uint>(m_Capacity, RingBufferMode.Expand);

            m_Versions[0] = 1;
            m_MaxFree = 1;
            m_FreeList.PushBack(0);
        }

        #region Alloc/Free

        /// <summary>
        /// Allocates a new unique identifier.
        /// </summary>
        public UniqueId32 Alloc()
        {
            Reserve(1);
            uint index = m_FreeList.PopFront();
            ushort version = m_Versions[index];
            return new UniqueId32(index, version);
        }

        /// <summary>
        /// Frees the given identifier.
        /// </summary>
        public bool Free(UniqueId32 inId)
        {
            int index = inId.Index;
            int version = inId.Version;
            if (index < m_Capacity && m_Versions[index] == version)
            {
                m_FreeList.PushBack((uint) index);
                version = version == UniqueId32.VersionMask ? 1 : version + 1;
                m_Versions[index] = (byte) version;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Reserves the given number of unique identifiers.
        /// </summary>
        public void Reserve(int inCount)
        {
            if (m_FreeList.Count >= inCount)
                return;

            if (m_MaxFree < m_Capacity)
            {
                m_Versions[m_MaxFree] = 1;
                m_FreeList.PushBack((uint) m_MaxFree++);
                return;
            }

            if (m_Capacity + inCount > UniqueId32.MaxIndex)
            {
                throw new InvalidOperationException(string.Format("UniqueIdAllocator32 can only have a maximum of {0} identifiers at once", UniqueId32.MaxIndex));
            }

            int newLength = Math.Min(m_Capacity * 2, UniqueId32.MaxIndex);
            Array.Resize(ref m_Versions, newLength);
            m_FreeList.SetCapacity(newLength);
            m_FreeList.PushBack((ushort) m_MaxFree++);
        }

        /// <summary>
        /// Resets all allocated handles.
        /// </summary>
        public void Reset()
        {
            for (int i = 0; i < m_Capacity; i++)
                m_Versions[i] = 1;
            m_MaxFree = 1;
            m_FreeList.Clear();
            m_FreeList.PushBack(0);
        }

        #endregion // Alloc/Free

        #region Checks

        /// <summary>
        /// Returns the current capacity.
        /// </summary>
        public int Capacity
        {
            get { return m_Capacity; }
        }

        /// <summary>
        /// Returns the number of identifiers in use.
        /// </summary>
        public int InUse
        {
            get { return m_MaxFree - m_FreeList.Count; }
        }

        /// <summary>
        /// Returns if the given id is valid.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsValid(UniqueId32 inId)
        {
            int index = inId.Index;
            return index < m_MaxFree && m_Versions[index] == inId.Version;
        }

        #endregion // Checks
    }
}