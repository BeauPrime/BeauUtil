/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    9 June 2020
 * 
 * File:    WeightedSet.cs
 * Purpose: Set of items with weights.
 */

#if CSHARP_7_3_OR_NEWER
#define EXPANDED_REFS
#endif // CSHARP_7_3_OR_NEWER

using System.Collections.Generic;

namespace BeauUtil
{
    /// <summary>
    /// Set of weighted items.
    /// </summary>
    public class WeightedSet<T>
    {
        private struct Entry
        {
            public readonly T Value;
            public float Weight;

            public double LookupStart;
            public double LookupWeight;

            public Entry(T inValue, float inWeight)
            {
                Value = inValue;
                Weight = inWeight;
                LookupStart = 0;
                LookupWeight = 0;
            }

            public override string ToString()
            {
                return string.Format("{0}:{1}", Value, Weight);
            }
        }

        private readonly RingBuffer<Entry> m_Entries;
        private float m_TotalWeight;
        private bool m_Cached;

        public WeightedSet()
        {
            m_Entries = new RingBuffer<Entry>();
            m_Cached = true;
        }

        public WeightedSet(int inCapacity)
        {
            m_Entries = new RingBuffer<Entry>(inCapacity, RingBufferMode.Expand);
            m_Cached = true;
        }

        /// <summary>
        /// Total weight of all items in the set.
        /// </summary>
        public float TotalWeight
        {
            get { return m_TotalWeight; }
        }

        /// <summary>
        /// Number of items in the set.
        /// </summary>
        public int Count
        {
            get { return m_Entries.Count; }
        }

        /// <summary>
        /// Adds an item to the set with a given weight.
        /// If the item already exists in the set, this will add the weight
        /// to the existing entry instead.
        /// </summary>
#if EXPANDED_REFS
        public WeightedSet<T> Add(in T inValue, float inWeight)
#else
        public WeightedSet<T> Add(T inValue, float inWeight)
#endif // EXPANDED_REFS
        {
            if (inWeight <= 0)
                return this;

            m_TotalWeight += inWeight;
            m_Cached = false;
            
            var eq = CompareUtils.DefaultComparer<T>();
            for(int i = m_Entries.Count - 1; i >= 0; --i)
            {
#if EXPANDED_REFS
                ref Entry e = ref m_Entries[i];
#else
                Entry e = m_Entries[i];
#endif // !EXPANDED_REFS
                if (eq.Equals(e.Value, inValue))
                {
                    e.Weight += inWeight;
#if !EXPANDED_REFS
                    m_Entries[i] = e;
#endif // !EXPANDED_REFS
                    return this;
                }
            }

            m_Entries.PushBack(new Entry(inValue, inWeight));
            return this;
        }

        /// <summary>
        /// Adds an item to the set with a given weight.
        /// If the item already exists in the set, this will add the weight
        /// to the existing entry instead.
        /// </summary>
#if EXPANDED_REFS
        public WeightedSet<T> Add(in PriorityValue<T> inValue)
#else
        public WeightedSet<T> Add(PriorityValue<T> inValue)
#endif // EXPANDED_REFS
        {
            return Add(inValue.Value, inValue.Priority);
        }

        /// <summary>
        /// Modifies the weight for an item in the set.
        /// </summary>
#if EXPANDED_REFS
        public float ChangeWeight(in T inValue, float inWeightChange)
#else
        public float ChangeWeight(T inValue, float inWeightChange)
#endif // EXPANDED_REFS
        {
            var eq = CompareUtils.DefaultComparer<T>();
            for(int i = m_Entries.Count - 1; i >= 0; --i)
            {
#if EXPANDED_REFS
                ref Entry e = ref m_Entries[i];
#else
                Entry e = m_Entries[i];
#endif // !EXPANDED_REFS
                if (eq.Equals(e.Value, inValue))
                {
                    if (inWeightChange == 0)
                    {
                        return e.Weight;
                    }

                    float newWeight = e.Weight + inWeightChange;
                    if (newWeight <= 0)
                    {
                        m_TotalWeight -= e.Weight;
                        m_Entries.FastRemoveAt(i);
                        m_Cached = false;
                        return 0;
                    }

                    m_Cached = false;
                    m_TotalWeight += inWeightChange;
                    e.Weight = newWeight;
#if !EXPANDED_REFS
                    m_Entries[i] = e;
#endif // !EXPANDED_REFS
                    return newWeight;
                }
            }

            if (inWeightChange <= 0)
            {
                return 0;
            }

            m_Entries.PushBack(new Entry(inValue, inWeightChange));
            m_TotalWeight += inWeightChange;
            m_Cached = false;
            return inWeightChange;
        }

        /// <summary>
        /// Inserts and sets the weight for an item in the set.
        /// </summary>
#if EXPANDED_REFS
        public WeightedSet<T> SetWeight(in T inValue, float inWeight)
#else
        public WeightedSet<T> SetWeight(T inValue, float inWeight)
#endif // EXPANDED_REFS
        {
            if (inWeight <= 0)
            {
                Remove(inValue);
                return this;
            }

            var eq = CompareUtils.DefaultComparer<T>();
            for(int i = m_Entries.Count - 1; i >= 0; --i)
            {
#if EXPANDED_REFS
                ref Entry e = ref m_Entries[i];
#else
                Entry e = m_Entries[i];
#endif // !EXPANDED_REFS
                if (eq.Equals(e.Value, inValue))
                {
                    float weightDiff = inWeight - e.Weight;
                    if (weightDiff != 0)
                    {
                        m_Cached = false;
                        m_TotalWeight += weightDiff;
                        e.Weight = inWeight;
#if !EXPANDED_REFS
                        m_Entries[i] = e;
#endif // !EXPANDED_REFS
                    }
                    return this;
                }
            }

            m_Entries.PushBack(new Entry(inValue, inWeight));
            m_TotalWeight += inWeight;
            m_Cached = false;
            return this;
        }

        /// <summary>
        /// Inserts and sets the weight for an item in the set.
        /// </summary>
#if EXPANDED_REFS
        public WeightedSet<T> SetWeight(in PriorityValue<T> inValue)
#else
        public WeightedSet<T> SetWeight(PriorityValue<T> inValue)
#endif // EXPANDED_REFS
        {
            return SetWeight(inValue.Value, inValue.Priority);
        }

        /// <summary>
        /// Returns the total weight of the given item.
        /// </summary>
#if EXPANDED_REFS
        public float GetWeight(in T inValue)
#else
        public float GetWeight(T inValue)
#endif // EXPANDED_REFS
        {
            var eq = CompareUtils.DefaultComparer<T>();
            for(int i = m_Entries.Count - 1; i >= 0; --i)
            {
#if EXPANDED_REFS
                ref Entry e = ref m_Entries[i];
#else
                Entry e = m_Entries[i];
#endif // EXPANDED_REFS
                if (eq.Equals(e.Value, inValue))
                {
                    return e.Weight;
                }
            }

            return 0;
        }

        /// <summary>
        /// Retrieves a random item using the default random source.
        /// </summary>
        public T GetItem()
        {
            return GetItem(RNG.Instance);
        }

        /// <summary>
        /// Retrieves a random item using the given random source.
        /// </summary>
        public T GetItem(System.Random inRandom)
        {
            return GetItemNormalized(inRandom.NextFloat());
        }

        /// <summary>
        /// Retrieves the item from the given weight value between (0 - TotalWeight).
        /// </summary>
        public T GetItem(float inRandom)
        {
            if (m_TotalWeight == 0)
                return default(T);
            
            return GetItemNormalized(inRandom / m_TotalWeight);
        }

        /// <summary>
        /// Retrieves the item from the given normalized weight value between (0-1).
        /// </summary>
        public T GetItemNormalized(double inNormalizedRandom)
        {
            if (m_TotalWeight <= 0)
                return default(T);

            if (m_Entries.Count == 1)
            {
                return m_Entries[0].Value;
            }

            Cache();

            int itemIdx = m_Entries.BinarySearch(inNormalizedRandom, s_RangePredicate);
            if (itemIdx < 0)
                return default(T);
            
            return m_Entries[itemIdx].Value;
        }

        /// <summary>
        /// Attempts to remove the entry from the given set.
        /// </summary>
#if EXPANDED_REFS
        public bool Remove(in T inValue)
#else
        public bool Remove(T inValue)
#endif // EXPANDED_REFS
        {
            var eq = CompareUtils.DefaultComparer<T>();
            for(int i = m_Entries.Count - 1; i >= 0; --i)
            {
#if EXPANDED_REFS
                ref Entry e = ref m_Entries[i];
#else
                Entry e = m_Entries[i];
#endif // EXPANDED_REFS
                if (eq.Equals(e.Value, inValue))
                {
                    m_TotalWeight -= e.Weight;
                    m_Entries.FastRemoveAt(i);
                    m_Cached = false;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Removes all entries with weights below a certain amount.
        /// </summary>
        public int FilterHigh(float inWeightThreshold)
        {
            int removeCount = 0;

            for(int i = m_Entries.Count - 1; i >= 0; --i)
            {
#if EXPANDED_REFS
                ref Entry e = ref m_Entries[i];
#else
                Entry e = m_Entries[i];
#endif // EXPANDED_REFS
                if (e.Weight < inWeightThreshold)
                {
                    m_TotalWeight -= e.Weight;
                    m_Entries.FastRemoveAt(i);
                    m_Cached = false;
                    ++removeCount;
                }
            }

            return removeCount;
        }

        /// <summary>
        /// Removes all entries with weights above a certain amount.
        /// </summary>
        public int FilterLow(float inWeightThreshold)
        {
            int removeCount = 0;

            for(int i = m_Entries.Count - 1; i >= 0; --i)
            {
#if EXPANDED_REFS
                ref Entry e = ref m_Entries[i];
#else
                Entry e = m_Entries[i];
#endif // EXPANDED_REFS
                if (e.Weight > inWeightThreshold)
                {
                    m_TotalWeight -= e.Weight;
                    m_Entries.FastRemoveAt(i);
                    m_Cached = false;
                    ++removeCount;
                }
            }

            return removeCount;
        }
        
        /// <summary>
        /// Clears all items from the set.
        /// </summary>
        public void Clear()
        {
            m_Entries.Clear();
            m_TotalWeight = 0;
            m_Cached = true;
        }

        /// <summary>
        /// Caches the lookup table for the set.
        /// </summary>
        public void Cache()
        {
            if (m_Cached)
                return;

            double accum = 0;
            int end = m_Entries.Count;
            for(int i = 0; i < end; ++i)
            {
#if EXPANDED_REFS
                ref Entry e = ref m_Entries[i];
#else
                Entry e = m_Entries[i];
#endif // EXPANDED_REFS
                e.LookupStart = accum;
                e.LookupWeight = e.Weight / m_TotalWeight;
                accum += e.LookupWeight;
#if !EXPANDED_REFS
                m_Entries[i] = e;
#endif // !EXPANDED_REFS
            }

            m_Cached = true;
        }

        static private readonly ComparePredicate<Entry, double> s_RangePredicate = (Entry entry, double val) => {
            if (entry.LookupStart + entry.LookupWeight < val)
            {
                return -1;   
            }
            if (val < entry.LookupStart)
            {
                return 1;
            }
            return 0;
        };
    }
}