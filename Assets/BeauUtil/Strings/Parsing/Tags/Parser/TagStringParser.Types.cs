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

        private class RichTextRules : IDelimiterRules
        {
            public string TagStartDelimiter { get { return "<"; } }
            public string TagEndDelimiter { get { return ">"; } }
            public char[] TagDataDelimiters { get { return TagData.DefaultDataDelimiters; } }
            public char RegionCloseDelimiter { get { return '/'; } }

            public bool RichText { get { return true; } }
            public IEnumerable<string> AdditionalRichTextTags { get { return null; } }
        }

        private class CurlyBraceTextRules : IDelimiterRules
        {
            public string TagStartDelimiter { get { return "{"; } }
            public string TagEndDelimiter { get { return "}"; } }
            public char[] TagDataDelimiters { get { return TagData.DefaultDataDelimiters; } }
            public char RegionCloseDelimiter { get { return '/'; } }

            public bool RichText { get { return true; } }
            public IEnumerable<string> AdditionalRichTextTags { get { return null; } }
        }

        private class AtCurlyBraceTextRules : IDelimiterRules
        {
            public string TagStartDelimiter { get { return "@{"; } }
            public string TagEndDelimiter { get { return "}"; } }
            public char[] TagDataDelimiters { get { return TagData.DefaultDataDelimiters; } }
            public char RegionCloseDelimiter { get { return '/'; } }

            public bool RichText { get { return true; } }
            public IEnumerable<string> AdditionalRichTextTags { get { return null; } }
        }

        static internal bool HasSameDelims(IDelimiterRules inA, IDelimiterRules inB)
        {
            if (inA == inB)
                return true;

            return (inA.TagStartDelimiter == inB.TagStartDelimiter
                && inA.TagEndDelimiter == inB.TagEndDelimiter
                && inA.RegionCloseDelimiter == inB.RegionCloseDelimiter
                && ArrayUtils.ContentEquals(inA.TagDataDelimiters, inB.TagDataDelimiters));
        }

        #endregion // Delimiters

        #region Utilities


        /// <summary>
        /// Returns if the given string contains any text or rich text tags.
        /// </summary>
        static public bool ContainsText(StringSlice inString, IDelimiterRules inDelimiters, ICollection<string> inTextTags = null)
        {
            bool bTrackRichText = inDelimiters.RichText;
            bool bTrackTags = !bTrackRichText || !TagStringParser.HasSameDelims(inDelimiters, TagStringParser.RichTextDelimiters);

            int length = inString.Length;
            int charIdx = 0;
            int richStart = -1;
            int tagStart = -1;
            int copyStart = 0;

            while (charIdx < length)
            {
                if (bTrackRichText)
                {
                    if (inString.AttemptMatch(charIdx, "<"))
                    {
                        richStart = charIdx;
                    }
                    else if (richStart >= 0 && inString.AttemptMatch(charIdx, ">"))
                    {
                        return true;
                    }
                }

                if (bTrackTags)
                {
                    if (inString.AttemptMatch(charIdx, inDelimiters.TagStartDelimiter))
                    {
                        tagStart = charIdx;
                    }
                    else if (tagStart >= 0 && inString.AttemptMatch(charIdx, inDelimiters.TagEndDelimiter))
                    {
                        StringSlice check = inString.Substring(copyStart, tagStart - copyStart);
                        if (!check.IsWhitespace)
                            return true;

                        if (inTextTags != null)
                        {
                            TagData data = TagData.Parse(inString.Substring(tagStart, charIdx - tagStart + 1), inDelimiters);
                            if (inTextTags.Contains(data.Id.ToString()))
                            {
                                return true;
                            }
                        }

                        copyStart = charIdx + 1;
                        tagStart = -1;
                        richStart = -1;
                    }
                }

                ++charIdx;
            }

            StringSlice finalCheck = inString.Substring(copyStart, length - copyStart);
            return !finalCheck.IsWhitespace;
        }

        #endregion // Utilities
    }
}