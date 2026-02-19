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
using Unity.IL2CPP.CompilerServices;

namespace BeauUtil
{
    /// <summary>
    /// 16-bit unique id allocator.
    /// </summary>
    public sealed class UniqueIdAllocator16
    {
        private byte[] m_Versions;
        private ushort[] m_FreeList;
        private int m_FreeCount;
        private int m_Capacity = 0;
        private int m_MaxFree = 0;
        private readonly BufferExpansionPolicy m_ExpansionPolicy;

        public UniqueIdAllocator16(int inInitialCapacity, bool inbFlexible = true)
            : this(inInitialCapacity, inbFlexible ? BufferExpansionPolicy.ExponentialAligned(4) : BufferExpansionPolicy.Fixed)
        {
        }

        public UniqueIdAllocator16(int inInitialCapacity, BufferExpansionPolicy inExpansionPolicy)
        {
            if (inInitialCapacity < 1)
                throw new ArgumentOutOfRangeException("inInitialCapacity", "Initial capacity must be at least 1");
            m_Capacity = Unsafe.AlignUp16(inInitialCapacity);
            if (m_Capacity > UniqueId16.MaxIndex)
                throw new ArgumentOutOfRangeException("inInitialCapacity", "Capacity, rounded up, exceeds maximum UniqueId16 index");

            m_ExpansionPolicy = inExpansionPolicy;
            m_Versions = new byte[m_Capacity];
            m_FreeList = new ushort[m_Capacity];

            m_MaxFree = 1;
            m_Versions[0] = 1;
            m_FreeList[0] = 0;
            m_FreeCount = 1;
        }

        #region Alloc/Free

        /// <summary>
        /// Allocates a new unique identifier.
        /// </summary>
        [Il2CppSetOption(Option.NullChecks, false)]
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        public UniqueId16 Alloc()
        {
            Reserve(1);
            ushort index = m_FreeList[--m_FreeCount];
            ushort version = m_Versions[index];
            return new UniqueId16(index, version);
        }

        /// <summary>
        /// Frees the given identifier.
        /// </summary>
        [Il2CppSetOption(Option.NullChecks, false)]
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        public bool Free(UniqueId16 inId)
        {
            int index = inId.Index;
            int version = inId.Version;
            if (index < m_Capacity && m_Versions[index] == version)
            {
                m_FreeList[m_FreeCount++] = (ushort) index;
                version = version == UniqueId16.VersionMask ? 1 : version + 1;
                m_Versions[index] = (byte) version;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Reserves the given number of unique identifiers.
        /// </summary>
        [Il2CppSetOption(Option.NullChecks, false)]
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        public void Reserve(int inCount)
        {
            if (m_FreeCount >= inCount)
                return;

            if (m_MaxFree + inCount <= m_Capacity)
            {
                while (inCount-- > 0)
                {
                    m_Versions[m_MaxFree] = 1;
                    m_FreeList[m_FreeCount++] = (ushort) m_MaxFree++;
                }
                return;
            }

            if (!m_ExpansionPolicy.TryExpand(m_Capacity + inCount, UniqueId16.MaxIndex, out int newLength))
            {
                throw new InvalidOperationException(string.Format("Unable to expand UniqueIdAllocator16 to accomodate more than {0} entries with policy {1}", m_Capacity, m_ExpansionPolicy.Type));
            }

            Array.Resize(ref m_Versions, newLength);
            Array.Resize(ref m_FreeList, newLength);
            m_Capacity = newLength;

            while (inCount-- > 0)
            {
                m_Versions[m_MaxFree] = 1;
                m_FreeList[m_FreeCount++] = (ushort) m_MaxFree++;
            }
        }

        /// <summary>
        /// Resets all allocated handles.
        /// </summary>
        [Il2CppSetOption(Option.NullChecks, false)]
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        public void Reset()
        {
            for (int i = 0; i < m_Capacity; i++)
                m_Versions[i] = 1;
            m_MaxFree = 1;
            m_FreeList[0] = 0;
            m_FreeCount = 1;
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
            get { return m_MaxFree - m_FreeCount; }
        }

        /// <summary>
        /// Returns if no more identifiers can be allocated.
        /// </summary>
        public bool IsFull
        {
            get { return m_FreeCount == 0 && (m_ExpansionPolicy.Type == BufferExpansionType.Fixed || m_Capacity == UniqueId16.MaxIndex); }
        }

        /// <summary>
        /// Returns if the given id is valid.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Il2CppSetOption(Option.NullChecks, false)]
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
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
        private uint[] m_FreeList;
        private int m_FreeCount;
        private int m_Capacity = 0;
        private int m_MaxFree;
        private readonly bool m_Flexible;

        public UniqueIdAllocator32(int inInitialCapacity, bool inbFlexible = true)
        {
            if (inInitialCapacity < 1)
                throw new ArgumentOutOfRangeException("inInitialCapacity", "Initial capacity must be at least 1");
            m_Capacity = Unsafe.AlignUp16(inInitialCapacity);
            if (m_Capacity > UniqueId32.MaxIndex)
                throw new ArgumentOutOfRangeException("inInitialCapacity", "Capacity, rounded up, exceeds maximum UniqueId32 index");

            m_Flexible = inbFlexible;
            m_Versions = new byte[m_Capacity];
            m_FreeList = new uint[m_Capacity];

            m_Versions[0] = 1;
            m_MaxFree = 1;
            m_FreeList[0] = 0;
            m_FreeCount = 1;
        }

        #region Alloc/Free

        /// <summary>
        /// Allocates a new unique identifier.
        /// </summary>
        [Il2CppSetOption(Option.NullChecks, false)]
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        public UniqueId32 Alloc()
        {
            Reserve(1);
            uint index = m_FreeList[--m_FreeCount];
            ushort version = m_Versions[index];
            return new UniqueId32(index, version);
        }

        /// <summary>
        /// Frees the given identifier.
        /// </summary>
        [Il2CppSetOption(Option.NullChecks, false)]
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        public bool Free(UniqueId32 inId)
        {
            int index = inId.Index;
            int version = inId.Version;
            if (index < m_Capacity && m_Versions[index] == version)
            {
                m_FreeList[m_FreeCount++] = (uint) index;
                version = version == UniqueId32.VersionMask ? 1 : version + 1;
                m_Versions[index] = (byte) version;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Reserves the given number of unique identifiers.
        /// </summary>
        [Il2CppSetOption(Option.NullChecks, false)]
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        public void Reserve(int inCount)
        {
            if (m_FreeCount >= inCount)
                return;

            if (m_MaxFree + inCount <= m_Capacity)
            {
                while (inCount-- > 0)
                {
                    m_Versions[m_MaxFree] = 1;
                    m_FreeList[m_FreeCount++] = (uint) m_MaxFree++;
                }
                return;
            }

            if (!m_Flexible)
            {
                throw new InvalidOperationException(string.Format("Inflexible UniqueIdAllocator32 - unable to expand to accomodate more than {0} entries", m_Capacity));
            }

            if (m_Capacity + inCount > UniqueId32.MaxIndex)
            {
                throw new InvalidOperationException(string.Format("UniqueIdAllocator32 can only have a maximum of {0} identifiers at once", UniqueId32.MaxIndex));
            }

            int newLength = Math.Min(m_Capacity * 2, UniqueId32.MaxIndex);
            Array.Resize(ref m_Versions, newLength);
            Array.Resize(ref m_FreeList, newLength);
            m_Capacity = newLength;

            while (inCount-- > 0)
            {
                m_Versions[m_MaxFree] = 1;
                m_FreeList[m_FreeCount++] = (uint) m_MaxFree++;
            }
        }

        /// <summary>
        /// Resets all allocated handles.
        /// </summary>
        [Il2CppSetOption(Option.NullChecks, false)]
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        public void Reset()
        {
            for (int i = 0; i < m_Capacity; i++)
                m_Versions[i] = 1;
            m_MaxFree = 1;
            m_FreeList[0] = 0;
            m_FreeCount = 1;
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
            get { return m_MaxFree - m_FreeCount; }
        }

        /// <summary>
        /// Returns if no more identifiers can be allocated.
        /// </summary>
        public bool IsFull
        {
            get { return m_FreeCount == 0 && (!m_Flexible || m_Capacity == UniqueId32.MaxIndex); }
        }

        /// <summary>
        /// Returns if the given id is valid.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Il2CppSetOption(Option.NullChecks, false)]
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        public bool IsValid(UniqueId32 inId)
        {
            int index = inId.Index;
            return index < m_MaxFree && m_Versions[index] == inId.Version;
        }

        #endregion // Checks
    }
}