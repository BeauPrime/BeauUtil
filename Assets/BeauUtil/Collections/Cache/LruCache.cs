/*
 * Copyright (C) 2023. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    19 June 2023
 * 
 * File:    LRUCache.cs
 * Purpose: Least recently used cache implementation.
*/

#if (UNITY_EDITOR && !IGNORE_UNITY_EDITOR) || DEVELOPMENT_BUILD
#define DEVELOPMENT
#endif // (UNITY_EDITOR && !IGNORE_UNITY_EDITOR) || DEVELOPMENT_BUILD

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace BeauUtil
{
    /// <summary>
    /// Least-recently-used cache implementation.
    /// </summary>
    public sealed class LruCache<TKey, TValue> : ICache<TKey, TValue>
        where TKey : struct
    {
        private readonly Dictionary<TKey, int> m_KeyLookup;
        private readonly TKey[] m_KeyTable;
        private readonly TValue[] m_ValueTable;
        private readonly LLTable m_UseTable;
        private readonly CacheCallbacks<TKey, TValue> m_Config;

        private bool m_Locked;
        private LLIndexList m_UseTableList;

        private int m_EntryCount;
        private readonly int m_Capacity;

#if DEVELOPMENT
        private CacheStats<TKey> m_Stats;
#endif // DEVELOPMENT

        /// <summary>
        /// Invoked when an operation happens for a value already in the cache.
        /// </summary>
        public event CacheReportDelegate<TKey> OnHit;

        /// <summary>
        /// Invoked when an operation happens for a value that is not already in the cache.
        /// </summary>
        public event CacheReportDelegate<TKey> OnMiss;

        /// <summary>
        /// Invoked when a value is evicted from the cache.
        /// </summary>
        public event CacheReportDelegate<TKey> OnEvict;

        /// <summary>
        /// Cache statistics.
        /// </summary>
        public CacheStats<TKey> Stats
        {
            get
            {
#if DEVELOPMENT
                return m_Stats;
#else
                return default(CacheStats<TKey>);
#endif // DEVELOPMENT
            }
        }

        public LruCache(int inCapacity, CacheCallbacks<TKey, TValue> inConfig = default(CacheCallbacks<TKey, TValue>))
        {
            CacheCallbacks<TKey, TValue>.FillDefaults(ref inConfig);

            if (inCapacity < 4)
                throw new ArgumentOutOfRangeException("inCapacity", "Capacity must be at least 4");

            m_Capacity = inCapacity;
            m_KeyTable = new TKey[inCapacity];
            m_ValueTable = new TValue[inCapacity];
            m_UseTable = new LLTable(inCapacity, false);
            m_Config = inConfig;
            m_KeyLookup = new Dictionary<TKey, int>(inCapacity, CompareUtils.DefaultEquals<TKey>());
        }

        #region Checks

        /// <summary>
        /// Returns if an entry with the given key
        /// is present in the cache.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(TKey inKey)
        {
            return FindIndex(inKey) >= 0;
        }

        /// <summary>
        /// Returns the number of elements cached.
        /// </summary>
        public int Count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return m_EntryCount; }
        }
       
        /// <summary>
        /// Returns the capacity of the cache.
        /// </summary>
        public int Capacity
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return m_Capacity; }
        }

        #endregion // Checks

        #region Read/Write

        /// <summary>
        /// Returns the cached value for the given key.
        /// If the value is not present, it is fetched.
        /// </summary>
        public TValue Read(TKey inKey)
        {
            int index = FindIndex(inKey);
            TValue value;
            if (index < 0)
            {
#if DEVELOPMENT
                m_Stats.m_ReadStats.Miss++;
#endif // DEVELOPMENT
                OnMiss?.Invoke(inKey, CacheOperation.Read);

                value = Fetch(inKey);

                if (!m_Locked)
                {
                    index = Insert(inKey, CacheOperation.Read);
                    m_ValueTable[index] = value;
                }
            }
            else
            {
#if DEVELOPMENT
                m_Stats.m_ReadStats.Hit++;
#endif // DEVELOPMENT
                OnHit?.Invoke(inKey, CacheOperation.Read);

                value = m_ValueTable[index];
                m_UseTable.MoveToFront(ref m_UseTableList, index);
            }

            return value;
        }

        /// <summary>
        /// Tries to read the value for the given key.
        /// Returns if it was present in the cache.
        /// </summary>
        public bool TryRead(TKey inKey, out TValue outValue)
        {
            int index = FindIndex(inKey);
            if (index < 0)
            {
#if DEVELOPMENT
                m_Stats.m_ReadStats.Miss++;
#endif // DEVELOPMENT
                OnMiss?.Invoke(inKey, CacheOperation.Read);
                outValue = default;
                return false;
            }

#if DEVELOPMENT
            m_Stats.m_ReadStats.Hit++;
#endif // DEVELOPMENT
            OnHit?.Invoke(inKey, CacheOperation.Read);

            m_UseTable.MoveToFront(ref m_UseTableList, index);
            outValue = m_ValueTable[index];
            return true;
        }

        /// <summary>
        /// Ensures the value for the given key is cached.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Poke(TKey inKey)
        {
            int index = FindIndex(inKey);
            if (index < 0)
            {
#if DEVELOPMENT
                m_Stats.m_ReadStats.Miss++;
#endif // DEVELOPMENT
                OnMiss?.Invoke(inKey, CacheOperation.Read);

                if (!m_Locked)
                {
                    TValue value = Fetch(inKey);
                    index = Insert(inKey, CacheOperation.Read);
                    m_ValueTable[index] = value;
                }
            }
            else
            {
#if DEVELOPMENT
                m_Stats.m_ReadStats.Hit++;
#endif // DEVELOPMENT
                OnHit?.Invoke(inKey, CacheOperation.Read);
                m_UseTable.MoveToFront(ref m_UseTableList, index);
            }
        }

        /// <summary>
        /// Overwrites the value for the given key.
        /// </summary>
        public TValue Write(TKey inKey, TValue inValue)
        {
            int index = FindIndex(inKey);
            if (index < 0)
            {
#if DEVELOPMENT
                m_Stats.m_WriteStats.Miss++;
#endif // DEVELOPMENT
                OnMiss?.Invoke(inKey, CacheOperation.Write);

                if (!m_Locked)
                {
                    index = Insert(inKey, CacheOperation.Write);
                    m_ValueTable[index] = inValue;
                }
            }
            else
            {
#if DEVELOPMENT
                m_Stats.m_WriteStats.Hit++;
#endif // DEVELOPMENT
                OnHit?.Invoke(inKey, CacheOperation.Write);

                Overwrite(index, ref inValue);
                m_UseTable.MoveToFront(ref m_UseTableList, index);
            }

            return inValue;
        }

        /// <summary>
        /// Overwrites the value for the given key.
        /// </summary>
        public void Write(TKey inKey, ref TValue ioValue)
        {
            int index = FindIndex(inKey);
            if (index < 0)
            {
#if DEVELOPMENT
                m_Stats.m_WriteStats.Miss++;
#endif // DEVELOPMENT
                OnMiss?.Invoke(inKey, CacheOperation.Write);

                if (!m_Locked)
                {
                    index = Insert(inKey, CacheOperation.Write);
                    m_ValueTable[index] = ioValue;
                }
            }
            else
            {
#if DEVELOPMENT
                m_Stats.m_WriteStats.Hit++;
#endif // DEVELOPMENT
                OnHit?.Invoke(inKey, CacheOperation.Write);

                Overwrite(index, ref ioValue);
                m_UseTable.MoveToFront(ref m_UseTableList, index);
            }
        }

        /// <summary>
        /// Invalidates the entry for the given key.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Invalidate(TKey inKey)
        {
            if (m_Locked)
                return;

            int index = FindIndex(inKey);
            if (index >= 0)
            {
                Evict(index);
            }
        }

        /// <summary>
        /// Invalidates all entries in the cache.
        /// </summary>
        public void InvalidateAll()
        {
            if (m_Locked)
                return;

            while (m_EntryCount > 0)
            {
                Evict(FindLRUIndex());
            }
        }

        /// <summary>
        /// Invalidates the specified number of entries from the cache.
        /// </summary>
        public void InvalidateCount(int inCount)
        {
            if (m_Locked)
                return;

            inCount = Math.Min(inCount, m_EntryCount);

            while(m_EntryCount > 0 && inCount-- > 0)
            {
                Evict(FindLRUIndex());
            }
        }

        #endregion // Read/Write

        #region Locking

        /// <summary>
        /// Returns if the cache is locked.
        /// No values can be added or ejected while locked.
        /// </summary>
        public bool IsLocked()
        {
            return m_Locked;
        }

        /// <summary>
        /// Locks the cache.
        /// No values can be added or ejected while locked.
        /// </summary>
        public void Lock()
        {
            m_Locked = true;
        }

        /// <summary>
        /// Unlocks the cache.
        /// No values can be added or ejected while locked.
        /// </summary>
        public void Unlock()
        {
            m_Locked = false;
        }

        #endregion // Locking

        #region Operations

        /// <summary>
        /// Finds the index for the given key.
        /// </summary>
        private int FindIndex(TKey inKey)
        {
            if (!m_KeyLookup.TryGetValue(inKey, out int index))
                index = -1;
            return index;
        }

        /// <summary>
        /// Fetches the value for the given key.
        /// </summary>
        private TValue Fetch(TKey inKey)
        {
            return m_Config.Fetch(inKey);
        }

        /// <summary>
        /// Overwrites the value at the given index.
        /// </summary>
        private void Overwrite(int inIndex, ref TValue ioValue)
        {
            ref TValue current = ref m_ValueTable[inIndex];
            m_Config.Overwrite(ref current, ioValue);
            ioValue = current;
        }
    
        /// <summary>
        /// Inserts a new entry with the given key.
        /// </summary>
        private int Insert(TKey inKey, CacheOperation inOperation)
        {
            if (m_EntryCount == m_Capacity)
            {
                Evict(FindLRUIndex(), inOperation);
            }

            int idx = m_UseTable.PushFront(ref m_UseTableList);
            m_KeyTable[idx] = inKey;
            m_KeyLookup[inKey] = idx;
            return m_EntryCount++;
        }

        /// <summary>
        /// Finds the index of the least recently used element.
        /// </summary>
        private int FindLRUIndex()
        {
            return m_UseTableList.Tail;
        }

        /// <summary>
        /// Evicts the entry at the given index.
        /// </summary>
        private void Evict(int inIndex, CacheOperation inOperation = CacheOperation.Evict)
        {
            TKey key = m_KeyTable[inIndex];
            m_KeyLookup.Remove(key);

#if DEVELOPMENT
            switch (inOperation)
            {
                case CacheOperation.Read:
                    m_Stats.m_ReadStats.Evict++;
                    break;
                case CacheOperation.Write:
                    m_Stats.m_WriteStats.Evict++;
                    break;
            }
#endif // DEVELOPMENT
            
            OnEvict?.Invoke(key, inOperation);

            ref TValue current = ref m_ValueTable[inIndex];
            m_Config.Evict(ref current);
            current = default(TValue);

            m_UseTable.Remove(ref m_UseTableList, inIndex);
            m_KeyTable[inIndex] = default;
            m_ValueTable[inIndex] = default;
            m_EntryCount--;
        }

        #endregion // Operations
    }

}