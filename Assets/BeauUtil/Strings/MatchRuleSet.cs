/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    24 August 2020
 * 
 * File:    MatchRuleSet.cs
 * Purpose: A set of string matching rules.
 */

using System.Collections;
using System.Collections.Generic;

namespace BeauUtil
{
    /// <summary>
    /// Set of string match rules
    /// </summary>
    public class MatchRuleSet<T> : ICollection<T> where T : IMatchRule
    {
        private readonly MatchRuleSet<T> m_InheritFrom;
        private List<T> m_Rules;
        private bool m_Dirty = false;

        public MatchRuleSet()
        {
            m_Rules = new List<T>();
        }

        public MatchRuleSet(int inCapacity)
        {
            m_Rules = new List<T>(inCapacity);
        }

        public MatchRuleSet(MatchRuleSet<T> inInheritFrom)
        {
            m_Rules = new List<T>();
            m_InheritFrom = inInheritFrom;
        }

        public MatchRuleSet(int inCapacity, MatchRuleSet<T> inInheritFrom)
        {
            m_Rules = new List<T>(inCapacity);
            m_InheritFrom = inInheritFrom;
        }

        /// <summary>
        /// Finds the closest matching rule for the given string.
        /// </summary>
        public T FindMatch(StringSlice inString)
        {
            EnsureSorted();

            for(int i = 0, length = m_Rules.Count; i < length; ++i)
            {
                if (m_Rules[i].Match(inString))
                    return m_Rules[i];
            }

            if (m_InheritFrom != null)
                return m_InheritFrom.FindMatch(inString);

            return default(T);
        }

        /// <summary>
        /// Ensures rules are sorted by specificity.
        /// </summary>
        public void EnsureSorted()
        {
            if (!m_Dirty)
                return;

            m_Rules.Sort();
            m_Dirty = false;
        }

        #region ICollection

        public int Count { get { return m_Rules.Count; } }

        bool ICollection<T>.IsReadOnly => throw new System.NotImplementedException();

        public void Add(T item)
        {
            m_Rules.Add(item);
            m_Dirty = true;
        }

        public void Clear()
        {
            m_Rules.Clear();
            m_Dirty = false;
        }

        public bool Contains(T item)
        {
            return m_Rules.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            m_Rules.CopyTo(array, arrayIndex);
        }

        public bool Remove(T item)
        {
            return m_Rules.Remove(item);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return m_Rules.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return m_Rules.GetEnumerator();
        }

        #endregion // ICollection
    }
}