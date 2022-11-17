/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    24 August 2020
 * 
 * File:    MatchRuleSet.cs
 * Purpose: A set of string matching rules.
 */

#if CSHARP_7_3_OR_NEWER
#define UNMANAGED_CONSTRAINT
#endif // CSHARP_7_3_OR_NEWER

using System;
using System.Collections;
using System.Collections.Generic;
using Unity.IL2CPP.CompilerServices;

namespace BeauUtil
{
    internal struct MatchRuleSetEntry
    {
        public int Specificity;
        public StringHash32 Id;
        public ushort PatternIndex;
        public ushort RuleIdx;

        #if UNMANAGED_CONSTRAINT
        static internal readonly unsafe Unsafe.ComparisonPtr<MatchRuleSetEntry> Sort = (MatchRuleSetEntry* a, MatchRuleSetEntry* b) => {
            return b->Specificity - a->Specificity;
        };
        #else
        static internal readonly unsafe Unsafe.ComparisonPtr<MatchRuleSetEntry> Sort = (void* a, void* b) => {
            return ((MatchRuleSetEntry*) b)->Specificity - ((MatchRuleSetEntry*) a)->Specificity;
        };
        #endif // UNMANAGED_CONSTRAINT
    }

    /// <summary>
    /// Set of string match rules
    /// </summary>
    public class MatchRuleSet<TRule>
    {
        private readonly MatchRuleSet<TRule> m_InheritFrom;

        private MatchRuleSetEntry[] m_Entries;
        private int m_EntryCount;
        private int m_CaseSensitiveDirectCount;
        private int m_CaseInsensitiveDirectCount;

        private readonly List<WildcardMatch> m_PatternBank;
        private readonly List<TRule> m_RuleBank;
        private bool m_Dirty = false;

        public MatchRuleSet()
        {
            m_RuleBank = new List<TRule>();
            m_PatternBank = new List<WildcardMatch>();
            m_Entries = Array.Empty<MatchRuleSetEntry>();
        }

        public MatchRuleSet(int inCapacity)
        {
            m_RuleBank = new List<TRule>(inCapacity);
            m_PatternBank = new List<WildcardMatch>(inCapacity);
            m_Entries = new MatchRuleSetEntry[inCapacity];
        }

        public MatchRuleSet(MatchRuleSet<TRule> inInheritFrom)
            : this()
        {
            m_InheritFrom = inInheritFrom;
        }

        public MatchRuleSet(int inCapacity, MatchRuleSet<TRule> inInheritFrom)
            : this(inCapacity)
        {
            m_InheritFrom = inInheritFrom;
        }

        /// <summary>
        /// Finds the closest matching rule for the given string.
        /// </summary>
        public TRule FindMatch(StringSlice inString)
        {
            EnsureSorted();

            StringHash32 id = StringHash32.Fast(inString), idUpper = StringHash32.FastCaseInsensitive(inString);
            TRule rule;
            bool bFound = FastMatches(id, idUpper, out rule) || SlowMatches(inString, out rule);
            return rule;
        }

        /// <summary>
        /// Finds the closest matching rule for the given string.
        /// </summary>
        public TRule FindMatch(StringBuilderSlice inString)
        {
            EnsureSorted();

            StringHash32 id = StringHash32.Fast(inString), idUpper = StringHash32.FastCaseInsensitive(inString);
            TRule rule;
            bool bFound = FastMatches(id, idUpper, out rule) || SlowMatches(inString, out rule);
            return rule;
        }

        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        [Il2CppSetOption(Option.NullChecks, false)]
        private bool FastMatches(StringHash32 inHash, StringHash32 inUpperHash, out TRule outRule)
        {
            int idx = 0;
            MatchRuleSetEntry e;
            for(int i = 0; i < m_CaseSensitiveDirectCount; i++)
            {
                e = m_Entries[idx++];
                if (e.Id == inHash)
                {
                    outRule = m_RuleBank[e.RuleIdx];
                    return true;
                }
            }

            for(int i = 0; i < m_CaseInsensitiveDirectCount; i++)
            {
                e = m_Entries[idx++];
                if (e.Id == inUpperHash)
                {
                    outRule = m_RuleBank[e.RuleIdx];
                    return true;
                }
            }

            if (m_InheritFrom != null)
                return m_InheritFrom.FastMatches(inHash, inUpperHash, out outRule);

            outRule = default(TRule);
            return false;
        }

        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        [Il2CppSetOption(Option.NullChecks, false)]
        private bool SlowMatches(StringSlice inString, out TRule outRule)
        {
            MatchRuleSetEntry e;
            for(int idx = m_CaseInsensitiveDirectCount + m_CaseSensitiveDirectCount; idx < m_EntryCount;)
            {
                e = m_Entries[idx++];
                if (m_PatternBank[e.PatternIndex].Match(inString))
                {
                    outRule = m_RuleBank[e.RuleIdx];
                    return true;
                }
            }

            if (m_InheritFrom != null)
                return m_InheritFrom.SlowMatches(inString, out outRule);

            outRule = default(TRule);
            return false;
        }

        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        [Il2CppSetOption(Option.NullChecks, false)]
        private bool SlowMatches(StringBuilderSlice inString, out TRule outRule)
        {
            MatchRuleSetEntry e;
            for(int idx = m_CaseInsensitiveDirectCount + m_CaseSensitiveDirectCount; idx < m_EntryCount;)
            {
                e = m_Entries[idx++];
                if (m_PatternBank[e.PatternIndex].Match(inString))
                {
                    outRule = m_RuleBank[e.RuleIdx];
                    return true;
                }
            }

            if (m_InheritFrom != null)
                return m_InheritFrom.SlowMatches(inString, out outRule);

            outRule = default(TRule);
            return false;
        }

        /// <summary>
        /// Ensures rules are sorted by specificity.
        /// </summary>
        public void EnsureSorted()
        {
            if (!m_Dirty)
                return;

            unsafe
            {
                fixed(MatchRuleSetEntry* ptr = m_Entries)
                {
                    Unsafe.Quicksort<MatchRuleSetEntry>(ptr, m_EntryCount, MatchRuleSetEntry.Sort);
                }
            }

            m_Dirty = false;
        }

        /// <summary>
        /// Number of patterns.
        /// </summary>
        public int Count { get { return m_RuleBank.Count; } }

        /// <summary>
        /// Adds a new rule to the rule set.
        /// </summary>
        public void Add(string inPattern, TRule inRule, bool inbIgnoreCase = true)
        {
            int ruleIdx = m_RuleBank.IndexOf(inRule);
            if (ruleIdx < 0)
            {
                ruleIdx = m_RuleBank.Count;
                m_RuleBank.Add(inRule);
            }

            MatchRuleSetEntry newEntry;
            newEntry.RuleIdx = (ushort) ruleIdx;

            if (inPattern.IndexOf('*') < 0)
            {
                newEntry.Id = inbIgnoreCase ? StringHash32.CaseInsensitive(inPattern) : new StringHash32(inPattern);
                newEntry.Specificity = (inbIgnoreCase ? WildcardMatch.CaseInsensitivePatternMatchSpecificityBase : WildcardMatch.ExactPatternMatchSpecificityBase) - inPattern.Length;

                newEntry.PatternIndex = ushort.MaxValue;

                if (inbIgnoreCase)
                    m_CaseInsensitiveDirectCount++;
                else
                    m_CaseSensitiveDirectCount++;
            }
            else
            {
                WildcardMatch pattern = WildcardMatch.Compile(inPattern, '*', inbIgnoreCase);
                newEntry.Id = null;
                newEntry.Specificity = pattern.Specificity();

                newEntry.PatternIndex = (ushort) m_PatternBank.Count;
                m_PatternBank.Add(pattern);
            }

            if (m_EntryCount >= m_Entries.Length)
            {
                Array.Resize(ref m_Entries, (int) Unsafe.AlignUp8((uint) m_Entries.Length + 1));
            }

            m_Entries[m_EntryCount++] = newEntry;
            
            m_Dirty = true;
        }

        public void Clear()
        {
            Array.Clear(m_Entries, 0, m_EntryCount);
            m_EntryCount = 0;
            m_PatternBank.Clear();
            m_RuleBank.Clear();
            m_Dirty = false;
        }
    }
}