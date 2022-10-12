/*
 * Copyright (C) 2022. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    10 Oct 2022
 * 
 * File:    WildcardMatch.cs
 * Purpose: Wildcard matching rule.
 */

using System;
using System.Collections;
using System.Collections.Generic;

namespace BeauUtil
{
    /// <summary>
    /// Wildcard matching logic.
    /// </summary>
    /// <remarks>
    /// This does not yet support wildcards in the middle of the filter.
    /// </remarks>
    public struct WildcardMatch
    {
        internal const int ExactPatternMatchSpecificityBase = 1 << 15;
        internal const int CaseInsensitivePatternMatchSpecificityBase = 1 << 14;

        [Flags]
        public enum PatternType : byte
        {
            Full = 0,
            StartsWith = 0x01,
            EndsWith = 0x02,
            Contains = StartsWith | EndsWith,

            CaseInsensitive = 0x04
        }

        public string Pattern;
        public PatternType Type;

        /// <summary>
        /// Returns if the match is an exact string match.
        /// </summary>
        public bool IsFullString
        {
            get { return (Type & PatternType.Contains) == 0; }
        }

        #region Match

        /// <summary>
        /// Returns if a pattern matches.
        /// </summary>
        public bool Match(StringSlice inMatch)
        {
            if (string.IsNullOrEmpty(Pattern))
                return true;

            bool ignoreCase = (Type & PatternType.CaseInsensitive) != 0;
            switch(Type & PatternType.Contains)
            {
                case PatternType.Full:
                    return inMatch.Equals(Pattern, ignoreCase);
                case PatternType.StartsWith:
                    return inMatch.StartsWith(Pattern, ignoreCase);
                case PatternType.EndsWith:
                    return inMatch.EndsWith(Pattern, ignoreCase);
                case PatternType.Contains:
                    return inMatch.Contains(Pattern, ignoreCase);
                default:
                    throw new InvalidOperationException("Unrecognized wildcard pattern type");
            }
        }

        /// <summary>
        /// Returns if a pattern matches.
        /// </summary>
        public bool Match(StringBuilderSlice inMatch)
        {
            if (string.IsNullOrEmpty(Pattern))
                return true;

            bool ignoreCase = (Type & PatternType.CaseInsensitive) != 0;
            switch(Type & PatternType.Contains)
            {
                case PatternType.Full:
                    return inMatch.Equals(Pattern, ignoreCase);
                case PatternType.StartsWith:
                    return inMatch.StartsWith(Pattern, ignoreCase);
                case PatternType.EndsWith:
                    return inMatch.EndsWith(Pattern, ignoreCase);
                case PatternType.Contains:
                    return inMatch.Contains(Pattern, ignoreCase);
                default:
                    throw new InvalidOperationException("Unrecognized wildcard pattern type");
            }
        }

        /// <summary>
        /// Determines if a string matches any of the given filters.
        /// Wildcard characters are supported at the start and end of the filter.
        /// </summary>
        static public bool Match(StringSlice inString, string[] inFilters, char inWildcard = '*', bool inbIgnoreCase = false)
        {
            if (inFilters.Length == 0)
                return false;

            for (int i = 0; i < inFilters.Length; ++i)
            {
                if (Match(inString, inFilters[i], inWildcard, inbIgnoreCase))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Determines if a string matches any of the given filters.
        /// Wildcard characters are supported at the start and end of the filter.
        /// </summary>
        static public bool Match(StringSlice inString, ICollection<string> inFilters, char inWildcard = '*', bool inbIgnoreCase = false)
        {
            if (inFilters.Count == 0)
                return false;

            foreach (var filter in inFilters)
            {
                if (Match(inString, filter, inWildcard, inbIgnoreCase))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Determines if a string matches the given filter.
        /// Wildcard characters are supported at the start and end of the filter.
        /// </summary>
        static public bool Match(StringSlice inString, string inFilter, char inWildcard = '*', bool inbIgnoreCase = false)
        {
            if (string.IsNullOrEmpty(inFilter))
            {
                return inString.IsEmpty;
            }

            int filterLength = inFilter.Length;
            if (filterLength == 1 && inFilter[0] == inWildcard)
                return true;

            if (filterLength == 2 && inFilter[0] == inWildcard && inFilter[1] == inWildcard)
                return true;

            bool bStart = inFilter[0] == inWildcard;
            bool bEnd = inFilter[filterLength - 1] == inWildcard;
            if (bStart || bEnd)
            {
                StringSlice filterStr = inFilter;
                int startIdx = 0;
                if (bStart)
                {
                    ++startIdx;
                    --filterLength;
                }
                if (bEnd)
                {
                    --filterLength;
                }

                filterStr = filterStr.Substring(startIdx, filterLength);
                if (bStart && bEnd)
                {
                    return inString.Contains(filterStr.ToString(), inbIgnoreCase);
                }
                if (bStart)
                {
                    return inString.EndsWith(filterStr, inbIgnoreCase);
                }
                return inString.StartsWith(filterStr, inbIgnoreCase);
            }

            return inString.Equals(inFilter, inbIgnoreCase);
        }

        #endregion // Match

        #region Specificity

        /// <summary>
        /// Calculates the specificity score for the pattern.
        /// </summary>
        public int Specificity()
        {
            if (string.IsNullOrEmpty(Pattern))
                return 0;

            bool ignoreCase = (Type & PatternType.CaseInsensitive) != 0;

            switch(Type & PatternType.Contains)
            {
                case PatternType.Full:
                    {
                        if (ignoreCase)
                            return ExactPatternMatchSpecificityBase - Pattern.Length;
                        return CaseInsensitivePatternMatchSpecificityBase - Pattern.Length;
                    }
                default:
                    {
                        if (ignoreCase)
                            return Pattern.Length;
                        return Pattern.Length * 2;
                    }
            }
        }

        /// <summary>
        /// Returns the specificity score for a given string pattern.
        /// </summary>
        static public int Specificity(StringSlice inFilter, char inWildcard = '*', bool inbIgnoreCase = false)
        {
            return Compile(inFilter, inWildcard, inbIgnoreCase).Specificity();
        }

        #endregion // Specificity

        /// <summary>
        /// Compiles a pattern into a more optimized matcher.
        /// </summary>
        static public WildcardMatch Compile(StringSlice inPattern, char inWildcard = '*', bool inbIgnoreCase = false)
        {
            WildcardMatch compiledPattern = default(WildcardMatch);
            if (inPattern.IsEmpty)
                return compiledPattern;

            if (inPattern.Length == 1 && inPattern[0] == inWildcard)
                return compiledPattern;

            bool bStart = inPattern[0] == inWildcard;
            bool bEnd = inPattern[inPattern.Length - 1] == inWildcard;

            if (inPattern.Length == 2 && bStart && bEnd)
                return compiledPattern;

            if (bStart)
            {
                compiledPattern.Type |= PatternType.EndsWith;
                inPattern = inPattern.Substring(1);
            }
            if (bEnd)
            {
                compiledPattern.Type |= PatternType.StartsWith;
                inPattern = inPattern.Substring(0, inPattern.Length - 1);
            }

            if (inbIgnoreCase)
                compiledPattern.Type |= PatternType.CaseInsensitive;

            compiledPattern.Pattern = inPattern.ToString();
            return compiledPattern;
        }
    }
}