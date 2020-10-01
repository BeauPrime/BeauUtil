/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    1 August 2020
 * 
 * File:    TagStringParser.Types.cs
 * Purpose: Additional types for TagStringParser.
 */

using System;
using System.Collections.Generic;
using System.Text;

namespace BeauUtil.Tags
{
    public partial class TagStringParser
    {
        #region Delimiters

        /// <summary>
        /// Delimiter rules for tags with the format "<tag>"
        /// </summary>
        static public readonly IDelimiterRules RichTextDelimiters = new RichTextRules();

        /// <summary>
        /// Delimiter rules for tags with the format "{tag}"
        /// </summary>
        static public readonly IDelimiterRules CurlyBraceDelimiters = new CurlyBraceTextRules();

        /// <summary>
        /// Delimiter rules for tags with the format "@{tag}"
        /// </summary>
        static public readonly IDelimiterRules AtCurlyBraceDelimiters = new CurlyBraceTextRules();

        static private readonly char[] DefaultDataDelimiters = new char[] { '=', ' ', ':', '\t' };

        private class RichTextRules : IDelimiterRules
        {
            public string TagStartDelimiter { get { return "<"; } }
            public string TagEndDelimiter { get { return ">"; } }
            public char[] TagDataDelimiters { get { return DefaultDataDelimiters; } }
            public char RegionCloseDelimiter { get { return '/'; } }

            public bool RichText { get { return true; } }
            public IEnumerable<string> AdditionalRichTextTags { get { return null; } }
        }

        private class CurlyBraceTextRules : IDelimiterRules
        {
            public string TagStartDelimiter { get { return "{"; } }
            public string TagEndDelimiter { get { return "}"; } }
            public char[] TagDataDelimiters { get { return DefaultDataDelimiters; } }
            public char RegionCloseDelimiter { get { return '/'; } }

            public bool RichText { get { return true; } }
            public IEnumerable<string> AdditionalRichTextTags { get { return null; } }
        }

        private class AtCurlyBraceTextRules : IDelimiterRules
        {
            public string TagStartDelimiter { get { return "@{"; } }
            public string TagEndDelimiter { get { return "}"; } }
            public char[] TagDataDelimiters { get { return DefaultDataDelimiters; } }
            public char RegionCloseDelimiter { get { return '/'; } }

            public bool RichText { get { return true; } }
            public IEnumerable<string> AdditionalRichTextTags { get { return null; } }
        }

        static protected bool HasSameDelims(IDelimiterRules inA, IDelimiterRules inB)
        {
            if (inA == inB)
                return true;

            return (inA.TagStartDelimiter == inB.TagStartDelimiter
                && inA.TagEndDelimiter == inB.TagEndDelimiter
                && inA.RegionCloseDelimiter == inB.RegionCloseDelimiter
                && ArrayUtils.ContentEquals(inA.TagDataDelimiters, inB.TagDataDelimiters));
        }

        #endregion // Delimiters
    }
}