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
    /// Maps an auto-generated unique identifier to a value.
    /// </summary>
    public class UniqueIdMap16<TValue>
    {
        private readonly UniqueIdAllocator16 m_IdAllocator;
        private TValue[] m_Data;
        private readonly TValue m_Default;

        public UniqueIdMap16(int inInitialCapacity, TValue inDefault = default(TValue))
        {
            m_IdAllocator = new UniqueIdAllocator16(inInitialCapacity);
            m_Data = new TValue[m_IdAllocator.Capacity];
            m_Default = inDefault;
        }

        public TValue this[UniqueId16 inId]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return Get(inId); }
        }

        /// <summary>
        /// Adds a value to the map and retrieves the identifier for it.
        /// </summary>
        public UniqueId16 Add(TValue inValue)
        {
            UniqueId16 id = m_IdAllocator.Alloc();
            int index = id.Index;
            if (index >= m_Data.Length)
            {
                Array.Resize(ref m_Data, m_IdAllocator.Capacity);
            }
            m_Data[index] = inValue;
            return id;
        }

        /// <summary>
        /// Removes the value for the given identifier from the map.
        /// </summary>
        public bool Remove(UniqueId16 inId)
        {
            if (m_IdAllocator.Free(inId))
            {
                m_Data[inId.Index] = default(TValue);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Removes the value for the given identifier from the map.
        /// </summary>
        public bool Remove(UniqueId16 inId, out TValue outData)
        {
            if (m_IdAllocator.Free(inId))
            {
                outData = m_Data[inId.Index];
                m_Data[inId.Index] = default(TValue);
                return true;
            }

            outData = default(TValue);
            return false;
        }

        /// <summary>
        /// Returns if the given identifier is present in the map.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(UniqueId16 inId)
        {
            return m_IdAllocator.IsValid(inId);
        }

        /// <summary>
        /// Returns the value for the given identifier.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TValue Get(UniqueId16 inId)
        {
            if (m_IdAllocator.IsValid(inId))
                return m_Data[inId.Index];
            return m_Default;
        }

        /// <summary>
        /// Removes all entries.
        /// </summary>
        public void Clear()
        {
            Array.Clear(m_Data, 0, m_Data.Length);
            m_IdAllocator.Reset();
        }
    }

    /// <summary>
    /// Maps an auto-generated unique identifier to a value.
    /// </summary>
    public class UniqueIdMap32<TValue>
    {
        private readonly UniqueIdAllocator32 m_IdAllocator;
        private TValue[] m_Data;
        private readonly TValue m_Default;

        public UniqueIdMap32(int inInitialCapacity, TValue inDefault = default(TValue))
        {
            m_IdAllocator = new UniqueIdAllocator32(inInitialCapacity);
            m_Data = new TValue[m_IdAllocator.Capacity];
            m_Default = inDefault;
        }

        public TValue this[UniqueId32 inId]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return Get(inId); }
        }

        /// <summary>
        /// Adds a value to the map and retrieves the identifier for it.
        /// </summary>
        public UniqueId32 Add(TValue inValue)
        {
            UniqueId32 id = m_IdAllocator.Alloc();
            int index = id.Index;
            if (index >= m_Data.Length)
            {
                Array.Resize(ref m_Data, m_IdAllocator.Capacity);
            }
            m_Data[index] = inValue;
            return id;
        }

        /// <summary>
        /// Removes the value for the given identifier from the map.
        /// </summary>
        public bool Remove(UniqueId32 inId)
        {
            if (m_IdAllocator.Free(inId))
            {
                m_Data[inId.Index] = default(TValue);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Removes the value for the given identifier from the map.
        /// </summary>
        public bool Remove(UniqueId32 inId, out TValue outData)
        {
            if (m_IdAllocator.Free(inId))
            {
                outData = m_Data[inId.Index];
                m_Data[inId.Index] = default(TValue);
                return true;
            }

            outData = default(TValue);
            return false;
        }

        /// <summary>
        /// Returns if the given identifier is present in the map.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(UniqueId32 inId)
        {
            return m_IdAllocator.IsValid(inId);
        }

        /// <summary>
        /// Returns the value for the given identifier.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TValue Get(UniqueId32 inId)
        {
            if (m_IdAllocator.IsValid(inId))
                return m_Data[inId.Index];
            return m_Default;
        }

        /// <summary>
        /// Removes all entries.
        /// </summary>
        public void Clear()
        {
            Array.Clear(m_Data, 0, m_Data.Length);
            m_IdAllocator.Reset();
        }
    }
}