/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    1 August 2020
 * 
 * File:    TagStringParser.Types.cs
 * Purpose: Additional types for TagStringParser.
 */

#if CSHARP_7_3_OR_NEWER
#define EXPANDED_REFS
#endif // CSHARP_7_3_OR_NEWER

using System;
using System.Collections.Generic;
using System.Text;

namespace BeauUtil.Tags
{
    public partial struct TagStringParser
    {
        #region Delimiters

        /// <summary>
        /// Delimiter rules for tags with the format "<tag>"
        /// </summary>
        static public readonly DelimiterRules RichTextDelimiters = new DelimiterRules()
        {
            TagStartDelimiter = "<",
            TagEndDelimiter = ">",
            TagDataDelimiters = TagData.DefaultDataDelimiters,
            RegionCloseDelimiter = '/',
            RichText = true,
            AdditionalRichTextTags = null,
        };

        /// <summary>
        /// Delimiter rules for tags with the format "{tag}"
        /// </summary>
        static public readonly DelimiterRules CurlyBraceDelimiters = new DelimiterRules()
        {
            TagStartDelimiter = "{",
            TagEndDelimiter = "}",
            TagDataDelimiters = TagData.DefaultDataDelimiters,
            RegionCloseDelimiter = '/',
            RichText = true,
            AdditionalRichTextTags = null,
        };

        /// <summary>
        /// Delimiter rules for tags with the format "@{tag}"
        /// </summary>
        static public readonly DelimiterRules AtCurlyBraceDelimiters = new DelimiterRules()
        {
            TagStartDelimiter = "{@",
            TagEndDelimiter = "}",
            TagDataDelimiters = TagData.DefaultDataDelimiters,
            RegionCloseDelimiter = '/',
            RichText = true,
            AdditionalRichTextTags = null,
        };

        static internal bool HasSameDelims(DelimiterRules inA, DelimiterRules inB)
        {
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
        static public bool ContainsText(StringSlice inString, DelimiterRules inDelimiters, ICollection<string> inTextTags = null)
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