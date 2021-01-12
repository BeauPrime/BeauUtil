/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    4 Dec 2020
 * 
 * File:    GOFilterSet.cs
 * Purpose: Set of GameObject filters
 */

using System;
using System.Collections.Generic;
using UnityEngine;

namespace BeauUtil
{
    /// <summary>
    /// Set of GameObject filters.
    /// </summary>
    public sealed class GOFilterSet<TFilter> : IGOFilterSet where TFilter : GOCompositeFilter, new()
    {
        private class GOFilterEntry : IComparable<GOFilterEntry>
        {
            public readonly StringHash32 Id;
            public readonly TFilter Filter;

            public bool Enabled;
            private int m_Specificity;

            public GOFilterEntry(StringHash32 inId)
            {
                Id = inId;
                Filter = new TFilter();
                Enabled = true;
            }

            public void Cache()
            {
                m_Specificity = Filter.CalculateSpecificity();
            }

            public int CompareTo(GOFilterEntry other)
            {
                if (m_Specificity > other.m_Specificity)
                    return -1;
                if (m_Specificity < other.m_Specificity)
                    return 1;

                return Id.CompareTo(other.Id);
            }

            public bool Evaluate(GameObject inGameObject)
            {
                return Filter.Allow(inGameObject);
            }

            public bool Evaluate(Collider inCollider)
            {
                return Filter.Allow(inCollider);
            }

            public bool Evaluate(Collider2D inCollider)
            {
                return Filter.Allow(inCollider);
            }
        }

        private List<GOFilterEntry> m_SortedEntryList = new List<GOFilterEntry>();
        private bool m_Cached = false;
        private Dictionary<StringHash32, GOFilterEntry> m_EntryMap = new Dictionary<StringHash32, GOFilterEntry>();
        private LayerMask m_TotalLayerMask;

        /// <summary>
        /// Returns the layer mask that encompasses all filters.
        /// </summary>
        public LayerMask ConcatenatedLayerMask()
        {
            Cache(false);
            return m_TotalLayerMask;
        }

        /// <summary>
        /// Returns the layer mask for the given id.
        /// </summary>
        public LayerMask LayerMask(StringHash32 inId)
        {
            GOFilterEntry entry;
            if (m_EntryMap.TryGetValue(inId, out entry))
            {
                LayerMask mask = entry.Filter.LayerMask.Mask;
                if (mask != 0)
                    return mask;
            }

            return Bits.All32;
        }

        #region Filters

        /// <summary>
        /// Adds a new filter with the given id.
        /// </summary>
        public TFilter Add(StringHash32 inId)
        {
            if (m_EntryMap.ContainsKey(inId))
                throw new ArgumentException(string.Format("A filter with the given id '{0}' already exists!", inId.ToDebugString()), "inId");

            GOFilterEntry entry = new GOFilterEntry(inId);
            m_SortedEntryList.Add(entry);
            m_EntryMap.Add(inId, entry);
            m_Cached = false;
            return entry.Filter;
        }

        /// <summary>
        /// Returns the filter with the given id.
        /// </summary>
        public TFilter GetFilter(StringHash32 inId)
        {
            GOFilterEntry entry;
            if (!m_EntryMap.TryGetValue(inId, out entry))
            {
                Debug.LogErrorFormat("[GOFilterSet] Cannot find filter with id '{0}'", inId.ToDebugString());
                return null;
            }

            return entry.Filter;
        }

        /// <summary>
        /// Enables the filter with the given id.
        /// </summary>
        public void Enable(StringHash32 inId)
        {
            GOFilterEntry entry;
            if (!m_EntryMap.TryGetValue(inId, out entry))
            {
                Debug.LogErrorFormat("[GOFilterSet] Cannot find filter with id '{0}'", inId.ToDebugString());
                return;
            }

            entry.Enabled = true;
        }

        /// <summary>
        /// Disables the filter with the given id.
        /// </summary>
        public void Disable(StringHash32 inId)
        {
            GOFilterEntry entry;
            if (!m_EntryMap.TryGetValue(inId, out entry))
            {
                Debug.LogErrorFormat("[GOFilterSet] Cannot find filter with id '{0}'", inId.ToDebugString());
                return;
            }

            entry.Enabled = false;
        }

        /// <summary>
        /// Clears all filters.
        /// </summary>
        public void Clear()
        {
            m_Cached = true;
            m_EntryMap.Clear();
            m_SortedEntryList.Clear();
            m_TotalLayerMask = 0;
        }

        #endregion // Filters

        #region Checks

        public StringHash32 Check(GameObject inObject)
        {
            Cache(false);
            
            GOFilterEntry entry;
            for(int i = 0, count = m_SortedEntryList.Count; i < count; ++i)
            {
                entry = m_SortedEntryList[i];
                if (!entry.Enabled)
                    continue;
                if (entry.Evaluate(inObject))
                    return entry.Id;
            }

            return StringHash32.Null;
        }

        public StringHash32 Check(Collider inCollider)
        {
            Cache(false);

            GOFilterEntry entry;
            for(int i = 0, count = m_SortedEntryList.Count; i < count; ++i)
            {
                entry = m_SortedEntryList[i];
                if (!entry.Enabled)
                    continue;
                if (entry.Evaluate(inCollider))
                    return entry.Id;
            }

            return StringHash32.Null;
        }

        public StringHash32 Check(Collider2D inCollider)
        {
            Cache(false);

            GOFilterEntry entry;
            for(int i = 0, count = m_SortedEntryList.Count; i < count; ++i)
            {
                entry = m_SortedEntryList[i];
                if (!entry.Enabled)
                    continue;
                if (entry.Evaluate(inCollider))
                    return entry.Id;
            }

            return StringHash32.Null;
        }

        #endregion // Checks

        #region Tests vs Single Filter

        public bool Test(GameObject inObject, StringHash32 inId)
        {
            GOFilterEntry entry;
            if (!m_EntryMap.TryGetValue(inId, out entry))
            {
                Debug.LogErrorFormat("[GOFilterSet] Cannot find filter with id '{0}'", inId.ToDebugString());
                return false;
            }

            return entry.Evaluate(inObject);
        }

        public bool Test(Collider inCollider, StringHash32 inId)
        {
            GOFilterEntry entry;
            if (!m_EntryMap.TryGetValue(inId, out entry))
            {
                Debug.LogErrorFormat("[GOFilterSet] Cannot find filter with id '{0}'", inId.ToDebugString());
                return false;
            }

            return entry.Evaluate(inCollider);
        }

        public bool Test(Collider2D inCollider, StringHash32 inId)
        {
            GOFilterEntry entry;
            if (!m_EntryMap.TryGetValue(inId, out entry))
            {
                Debug.LogErrorFormat("[GOFilterSet] Cannot find filter with id '{0}'", inId.ToDebugString());
                return false;
            }
            
            return entry.Evaluate(inCollider);
        }

        #endregion // Tests vs Single Filter
    
        #region Internal

        private void Cache(bool inbForce)
        {
            if (!inbForce && m_Cached)
                return;

            m_TotalLayerMask = 0;
            
            for(int i = 0, count = m_SortedEntryList.Count; i < count; ++i)
            {
                m_SortedEntryList[i].Cache();

                LayerMask filterMask = m_SortedEntryList[i].Filter.LayerMask.Mask;
                if (filterMask == 0)
                    m_TotalLayerMask = Bits.All32;
                else
                    m_TotalLayerMask |= filterMask;
            }

            m_SortedEntryList.Sort();
            m_Cached = true;
        }

        #endregion // Internal

        #region IFilterSet

        IObjectFilter<GameObject> IFilterSet<GameObject>.GetFilter(StringHash32 inId)
        {
            return GetFilter(inId);
        }

        IObjectFilter<Collider> IFilterSet<Collider>.GetFilter(StringHash32 inId)
        {
            return GetFilter(inId);
        }

        IObjectFilter<Collider2D> IFilterSet<Collider2D>.GetFilter(StringHash32 inId)
        {
            return GetFilter(inId);
        }

        #endregion // IFilterSet
    }
}