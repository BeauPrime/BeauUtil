/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    9 Sept 2020
 * 
 * File:    RandomDeck.cs
 * Purpose: Set of items, ordered in a random manner.
 */

using System;
using System.Collections;
using System.Collections.Generic;

namespace BeauUtil
{
    /// <summary>
    /// Set of items, ordered in a random manner.
    /// If no duplicates, ensures that the same item is not repeated twice in a row.
    /// </summary>
    public class RandomDeck<T> : IList<T>
    {
        private readonly List<T> m_Entries;
        private int m_CurrentIdx = -1;

        public RandomDeck()
        {
            m_Entries = new List<T>();
        }

        public RandomDeck(IEnumerable<T> inSource)
        {
            m_Entries = new List<T>(inSource);
        }

        public RandomDeck(int inCapacity)
        {
            m_Entries = new List<T>(inCapacity);
        }

        /// <summary>
        /// Returns the next item in the deck.
        /// </summary>
        public T Next()
        {
            return Next(RNG.Instance);
        }

        /// <summary>
        /// Returns the next item in the deck,
        /// using the given random source.
        /// </summary>
        public T Next(Random inRandom)
        {
            if (m_Entries.Count <= 0)
                return default(T);

            if (m_Entries.Count == 1)
                return m_Entries[0];

            if (m_CurrentIdx < 0)
            {
                inRandom.Shuffle(m_Entries);
                m_CurrentIdx = 0;
            }
            else if (++m_CurrentIdx >= m_Entries.Count)
            {
                inRandom.Shuffle(m_Entries, 0, m_Entries.Count - 1);
                inRandom.Shuffle(m_Entries, 1, m_Entries.Count - 1);
                m_CurrentIdx = 0;
            }

            return m_Entries[m_CurrentIdx];
        }

        /// <summary>
        /// Resets the random order.
        /// This is automatically called when the deck is modified.
        /// </summary>
        public void Reset()
        {
            m_CurrentIdx = -1;
        }

        #region IList

        public T this[int index]
        {
            get { return m_Entries[index]; }
            set { m_Entries[index] = value; Reset(); }
        }

        public int Count { get { return m_Entries.Count; } }

        bool ICollection<T>.IsReadOnly { get { return false; } }

        /// <summary>
        /// Adds an item to the deck.
        /// Modifying the deck resets the random order.
        /// </summary>
        public void Add(T item)
        {
            m_Entries.Add(item);
            Reset();
        }

        /// <summary>
        /// Clears all items from the deck.
        /// Modifying the deck resets the random order.
        /// </summary>
        public void Clear()
        {
            m_Entries.Clear();
            Reset();
        }

        public bool Contains(T item)
        {
            return m_Entries.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            m_Entries.CopyTo(array, arrayIndex);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return m_Entries.GetEnumerator();
        }

        public int IndexOf(T item)
        {
            return m_Entries.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            m_Entries.Insert(index, item);
            Reset();
        }

        /// <summary>
        /// Removes an item from the deck.
        /// Modifying the deck resets the random order.
        /// </summary>
        public bool Remove(T item)
        {
            if (m_Entries.Remove(item))
            {
                Reset();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Removes an item from the deck.
        /// Modifying the deck resets the random order.
        /// </summary>
        public void RemoveAt(int index)
        {
            m_Entries.RemoveAt(index);
            Reset();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return m_Entries.GetEnumerator();
        }

        #endregion // IList
    }
}