/*
 * Copyright (C) 2022. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    10 Oct 2022
 * 
 * File:    MatchRuleBuilder.cs
 * Purpose: Builder-style interface for MatchRuleSet rules.
 */

using System;

namespace BeauUtil
{
    /// <summary>
    /// Abstract MatchRuleSet builder.
    /// </summary>
    public abstract class MatchRuleBuilder
    {
        protected string m_IdMatch;
        protected string[] m_Aliases;
        protected bool m_CaseSensitive;

        public virtual void Restart(string inId)
        {
            if (m_IdMatch != null)
            {
                Flush();
            }

            m_IdMatch = inId;
            m_Aliases = null;
            m_CaseSensitive = false;
        }

        public abstract void Flush();
    }

    /// <summary>
    /// MatchRuleSet builder.
    /// </summary>
    public abstract class MatchRuleBuilder<TRule, TSelf> : MatchRuleBuilder
        where TRule : new()
        where TSelf : MatchRuleBuilder<TRule, TSelf>
    {
        protected readonly MatchRuleSet<TRule> m_RuleSet;

        protected TRule m_Rule;

        public MatchRuleBuilder(MatchRuleSet<TRule> inRuleSet)
        {
            if (inRuleSet == null)
                throw new ArgumentNullException("inRuleSet");
            m_RuleSet = inRuleSet;
        }

        public override void Restart(string inId)
        {
            base.Restart(inId);
            m_Rule = new TRule();
        }

        public override void Flush()
        {
            if (m_IdMatch != null)
            {
                m_RuleSet.Add(m_IdMatch, m_Rule, !m_CaseSensitive);
                if (m_Aliases != null)
                {
                    for(int i = 0; i < m_Aliases.Length; i++)
                        m_RuleSet.Add(m_Aliases[i], m_Rule, !m_CaseSensitive);
                }

                m_IdMatch = null;
                m_Aliases = null;
                m_CaseSensitive = false;
                m_Rule = default(TRule);
            }
        }

        #region Builder

        /// <summary>
        /// Sets the id for matching.
        /// Wildcard matching is supported.
        /// </summary>
        public TSelf WithId(string inId)
        {
            m_IdMatch = inId ?? string.Empty;
            return (TSelf) this;
        }

        /// <summary>
        /// Adds a set of aliases for matching.
        /// Wildcard matching is supported.
        /// </summary>
        public TSelf WithAliases(params string[] inAliases)
        {
            m_Aliases = inAliases;
            return (TSelf) this;
        }

        /// <summary>
        /// Matching will be case-sensitive.
        /// By default, matching is case-insensitive.
        /// </summary>
        public TSelf CaseSensitive()
        {
            m_CaseSensitive = true;
            return (TSelf) this;
        }

        /// <summary>
        /// Matching will be case-insensitive.
        /// By default, matching is case-insensitive.
        /// </summary>
        public TSelf CaseInsensitive()
        {
            m_CaseSensitive = false;
            return (TSelf) this;
        }

        #endregion // Builder
    }
}