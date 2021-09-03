/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    24 August 2020
 * 
 * File:    MatchRule.cs
 * Purpose: Rule for string matching.
 */

using System;

namespace BeauUtil
{
    public abstract class MatchRule : IMatchRule, IComparable<MatchRule>
    {
        private string m_IdMatch;
        private string[] m_Aliases;
        private bool m_CaseSensitive;
        private int m_Specificity;

        #region Accessors

        /// <summary>
        /// Returns how specific this rule is for matching.
        /// </summary>
        public int Specificity() { return m_Specificity; }

        /// <summary>
        /// Returns if this rule is case-sensitive.
        /// </summary>
        public bool IsCaseSensitive() { return m_CaseSensitive; }

        #endregion // Accessors

        #region Modifiers

        public string Id() { return m_IdMatch; }

        protected void SetId(string inId)
        {
            if (m_IdMatch != inId)
            {
                m_IdMatch = inId;
                RecalculateSpecificity();
            }
        }

        protected void SetCaseSentitive(bool inbCaseSensitive)
        {
            if (m_CaseSensitive != inbCaseSensitive)
            {
                m_CaseSensitive = inbCaseSensitive;
                RecalculateSpecificity();
            }
        }

        protected void AddAliases(params string[] inAliases)
        {
            if (inAliases == null || inAliases.Length == 0)
                return;

            if (m_Aliases == null)
            {
                m_Aliases = (string[]) inAliases.Clone();
            }
            else
            {
                int copyIdx = m_Aliases.Length;
                Array.Resize(ref m_Aliases, m_Aliases.Length + inAliases.Length);
                Array.Copy(inAliases, 0, m_Aliases, copyIdx, inAliases.Length);
            }

            RecalculateSpecificity();
        }

        private void RecalculateSpecificity()
        {
            int specificity = CalculateSpecificity(m_IdMatch, m_CaseSensitive);
            if (m_Aliases != null)
            {
                foreach (string alias in m_Aliases)
                {
                    int aliasSpecificity = CalculateSpecificity(alias, m_CaseSensitive);
                    if (aliasSpecificity < specificity)
                        specificity = aliasSpecificity;
                }
            }

            m_Specificity = specificity;
        }

        #endregion // Modifiers

        /// <summary>
        /// Returns if the rule matches the given string.
        /// </summary>
        public bool Match(StringSlice inString)
        {
            if (StringUtils.WildcardMatch(inString, m_IdMatch, '*', !m_CaseSensitive))
                return true;

            if (m_Aliases != null)
                return StringUtils.WildcardMatch(inString, m_Aliases, '*', !m_CaseSensitive);

            return false;
        }

        #region IComparable

        int IComparable<MatchRule>.CompareTo(MatchRule other)
        {
            int diff = m_Specificity - other.m_Specificity;
            if (diff > 0)
                return -1;
            if (diff < 0)
                return 1;
            return 0;
        }

        #endregion // IComparable

        #region Specificity

        static public int CalculateSpecificity(StringSlice inMatchRule, bool inbCaseSensitive, char inWildcard = '*')
        {
            if (inMatchRule.IsEmpty)
                return 0;

            int specificity = (int.MaxValue / 2) - inMatchRule.Length;

            bool bWildcardStart = inMatchRule.StartsWith(inWildcard);
            bool bWildcardEnd = inMatchRule.EndsWith(inWildcard);
            if (bWildcardStart && bWildcardEnd)
            {
                specificity = inMatchRule.Length - 2;
            }
            else if (bWildcardStart || bWildcardEnd)
            {
                specificity = inMatchRule.Length - 1;
            }
            
            if (specificity < 0)
                specificity = 0;

            if (inbCaseSensitive)
                specificity *= 2;

            return specificity;
        }

        #endregion // Specificity
    }

    /// <summary>
    /// Rule for string matching.
    /// </summary>
    public abstract class MatchRule<TSelf> : MatchRule where TSelf : MatchRule<TSelf>
    {
        #region Builder

        /// <summary>
        /// Sets the id for matching.
        /// Wildcard matching is supported.
        /// </summary>
        public TSelf WithId(string inId)
        {
            SetId(inId);
            return (TSelf) this;
        }

        /// <summary>
        /// Adds a set of aliases for matching.
        /// Wildcard matching is supported.
        /// </summary>
        public TSelf WithAliases(params string[] inAliases)
        {
            AddAliases(inAliases);
            return (TSelf) this;
        }

        /// <summary>
        /// Matching will be case-sensitive.
        /// By default, matching is case-insensitive.
        /// </summary>
        public TSelf CaseSensitive()
        {
            SetCaseSentitive(true);
            return (TSelf) this;
        }

        /// <summary>
        /// Matching will be case-insensitive.
        /// By default, matching is case-insensitive.
        /// </summary>
        public TSelf CaseInsensitive()
        {
            SetCaseSentitive(false);
            return (TSelf) this;
        }

        #endregion // Builder
    }
}