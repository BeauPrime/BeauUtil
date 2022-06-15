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
    /// <summary>
    /// Rule for string matching.
    /// </summary>
    public interface IMatchRule
    {
        /// <summary>
        /// Returns how specific this rule is for matching.
        /// </summary>
        int Specificity();

        /// <summary>
        /// Returns if this rule is case-sensitive.
        /// </summary>
        bool IsCaseSensitive();

        /// <summary>
        /// Returns if the rule matches the given string.
        /// </summary>
        bool Match(StringSlice inString);
    }
}