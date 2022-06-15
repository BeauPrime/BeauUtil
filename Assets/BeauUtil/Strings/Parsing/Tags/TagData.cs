/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    23 August 2020
 * 
 * File:    TagData.cs
 * Purpose: Information about a tag.
 */

using System;
using System.Text;

namespace BeauUtil.Tags
{
    /// <summary>
    /// Tag information.
    /// </summary>
    public struct TagData
    {
        public StringSlice Id;
        public StringSlice Data;
        
        private ClosingTagState m_CloseState;

        /// <summary>
        /// Returns if this has no contents.
        /// </summary>
        public bool IsEmpty()
        {
            return Id.IsEmpty;
        }

        /// <summary>
        /// Returns if this has a closing tag at its beginning.
        /// This is generally paired with a previous tag,
        /// and is used to indicate the end of a region.
        /// </summary>
        public bool IsClosing()
        {
            return (m_CloseState & ClosingTagState.Start) != 0;
        }

        /// <summary>
        /// Returns if this has a closing tag at its end.
        /// This can be used to indicate the tag is
        /// self-contained in some way.
        /// </summary>
        public bool IsSelfClosed()
        {
            return (m_CloseState & ClosingTagState.End) != 0;
        }

        /// <summary>
        /// Where a closing delimiter is present in a tag.
        /// </summary>
        [Flags]
        private enum ClosingTagState
        {
            None = 0,
            Start = 0x01,
            End = 0x02
        }

        #region Static

        static private readonly TagData s_Empty = default(TagData);
        
        /// <summary>
        /// Empty tag.
        /// </summary>
        static public TagData Empty { get { return s_Empty; } }

        #endregion // Static

        #region Overrides

        public override string ToString()
        {
            if (Id.IsEmpty)
            {
                return Data.ToString();
            }
            if (Data.IsEmpty)
            {
                return Id.ToString();
            }

            return string.Format("{0}: {1}", Id, Data);
        }

        #endregion // Overrides

        #region Parse

        /// <summary>
        /// Parses a tag's contents into data.
        /// </summary>
        static public TagData Parse(StringSlice inSlice, IDelimiterRules inDelimiters)
        {
            TagData data;
            Parse(inSlice, inDelimiters, out data);
            return data;
        }

        /// <summary>
        /// Parses a tag's contents into data.
        /// </summary>
        static public void Parse(StringSlice inSlice, IDelimiterRules inDelimiters, out TagData outTagData)
        {
            if (inDelimiters == null)
                throw new ArgumentNullException("inDelimiters");

            StringSlice tag = inSlice;
            tag = tag.Trim(MinimalWhitespaceChars);

            bool bRemovedTagBoundaries = false;
            if (tag.StartsWith(inDelimiters.TagStartDelimiter))
            {
                tag = tag.Substring(inDelimiters.TagStartDelimiter.Length);
                bRemovedTagBoundaries = true;
            }
            if (tag.EndsWith(inDelimiters.TagEndDelimiter))
            {
                tag = tag.Substring(0, tag.Length - inDelimiters.TagEndDelimiter.Length);
                bRemovedTagBoundaries = true;
            }

            if (bRemovedTagBoundaries)
                tag = tag.Trim(MinimalWhitespaceChars);

            if (tag.Length == 0)
            {
                outTagData = default(TagData);
                return;
            }

            ClosingTagState closeState = 0;

            char endDelim = inDelimiters.RegionCloseDelimiter;
            if (endDelim != 0)
            {
                if (tag.StartsWith(endDelim))
                {
                    closeState |= ClosingTagState.Start;
                    tag = tag.Substring(1);
                }
                if (tag.EndsWith(endDelim))
                {
                    closeState |= ClosingTagState.End;
                    tag = tag.Substring(0, tag.Length - 1);
                }
            }

            if (closeState != 0)
            {
                tag = tag.Trim(MinimalWhitespaceChars);
            }

            char[] dataDelims = inDelimiters.TagDataDelimiters;
            int dataDelimIdx = tag.IndexOfAny(dataDelims);

            if (dataDelimIdx < 0)
            {
                outTagData.Id = tag;
                outTagData.Data = StringSlice.Empty;
            }
            else
            {
                outTagData.Id = tag.Substring(0, dataDelimIdx).TrimEnd(MinimalWhitespaceChars);
                outTagData.Data = tag.Substring(dataDelimIdx).TrimStart(dataDelims).TrimStart(MinimalWhitespaceChars);
            }

            outTagData.m_CloseState = closeState;
        }

        /// <summary>
        /// Parses a tag's contents into data.
        /// </summary>
        static public TagData Parse(StringBuilder inBuilder, IDelimiterRules inDelimiters)
        {
            TagData data;
            Parse(inBuilder, inDelimiters, out data);
            return data;
        }

        /// <summary>
        /// Parses a tag's contents into data.
        /// </summary>
        static public void Parse(StringBuilder inData, IDelimiterRules inDelimiters, out TagData outTagData)
        {
            if (inDelimiters == null)
                throw new ArgumentNullException("inDelimiters");

            StringBuilderSlice tag = new StringBuilderSlice(inData);
            tag = tag.Trim(MinimalWhitespaceChars);

            bool bRemovedTagBoundaries = false;
            if (tag.StartsWith(inDelimiters.TagStartDelimiter))
            {
                tag = tag.Substring(inDelimiters.TagStartDelimiter.Length);
                bRemovedTagBoundaries = true;
            }
            if (tag.EndsWith(inDelimiters.TagEndDelimiter))
            {
                tag = tag.Substring(0, tag.Length - inDelimiters.TagEndDelimiter.Length);
                bRemovedTagBoundaries = true;
            }

            if (bRemovedTagBoundaries)
                tag = tag.Trim(MinimalWhitespaceChars);

            if (tag.Length == 0)
            {
                outTagData = default(TagData);
                return;
            }

            ClosingTagState closeState = 0;

            char endDelim = inDelimiters.RegionCloseDelimiter;
            if (endDelim != 0)
            {
                if (tag.StartsWith(endDelim))
                {
                    closeState |= ClosingTagState.Start;
                    tag = tag.Substring(1);
                }
                if (tag.EndsWith(endDelim))
                {
                    closeState |= ClosingTagState.End;
                    tag = tag.Substring(0, tag.Length - 1);
                }
            }

            if (closeState != 0)
            {
                tag = tag.Trim(MinimalWhitespaceChars);
            }

            char[] dataDelims = inDelimiters.TagDataDelimiters;
            int dataDelimIdx = tag.Length;
            foreach (var delim in dataDelims)
            {
                int idx = tag.IndexOf(delim);
                if (idx >= 0 && idx < dataDelimIdx)
                {
                    dataDelimIdx = idx;
                    if (idx <= 0)
                        break;
                }
            }

            if (dataDelimIdx >= tag.Length)
            {
                outTagData.Id = tag.ToString();
                outTagData.Data = StringSlice.Empty;
            }
            else
            {
                outTagData.Id = tag.Substring(0, dataDelimIdx).TrimEnd(MinimalWhitespaceChars).ToString();
                outTagData.Data = tag.Substring(dataDelimIdx).TrimStart(dataDelims).TrimStart(MinimalWhitespaceChars).ToString();
            }

            outTagData.m_CloseState = closeState;
        }

        /// <summary>
        /// Parses a tag's contents into data.
        /// </summary>
        internal static TagData Parse(StringBuilderSlice inBuilder, IDelimiterRules inDelimiters)
        {
            TagData data;
            Parse(inBuilder, inDelimiters, out data);
            return data;
        }

        /// <summary>
        /// Parses a tag's contents into data.
        /// </summary>
        internal static void Parse(StringBuilderSlice inData, IDelimiterRules inDelimiters, out TagData outTagData)
        {
            if (inDelimiters == null)
                throw new ArgumentNullException("inDelimiters");

            StringBuilderSlice tag = inData;
            tag = tag.Trim(MinimalWhitespaceChars);

            bool bRemovedTagBoundaries = false;
            if (tag.StartsWith(inDelimiters.TagStartDelimiter))
            {
                tag = tag.Substring(inDelimiters.TagStartDelimiter.Length);
                bRemovedTagBoundaries = true;
            }
            if (tag.EndsWith(inDelimiters.TagEndDelimiter))
            {
                tag = tag.Substring(0, tag.Length - inDelimiters.TagEndDelimiter.Length);
                bRemovedTagBoundaries = true;
            }

            if (bRemovedTagBoundaries)
                tag = tag.Trim(MinimalWhitespaceChars);

            if (tag.Length == 0)
            {
                outTagData = default(TagData);
                return;
            }

            ClosingTagState closeState = 0;

            char endDelim = inDelimiters.RegionCloseDelimiter;
            if (endDelim != 0)
            {
                if (tag.StartsWith(endDelim))
                {
                    closeState |= ClosingTagState.Start;
                    tag = tag.Substring(1);
                }
                if (tag.EndsWith(endDelim))
                {
                    closeState |= ClosingTagState.End;
                    tag = tag.Substring(0, tag.Length - 1);
                }
            }

            if (closeState != 0)
            {
                tag = tag.Trim(MinimalWhitespaceChars);
            }

            char[] dataDelims = inDelimiters.TagDataDelimiters;
            int dataDelimIdx = tag.Length;
            foreach (var delim in dataDelims)
            {
                int idx = tag.IndexOf(delim);
                if (idx >= 0 && idx < dataDelimIdx)
                {
                    dataDelimIdx = idx;
                    if (idx <= 0)
                        break;
                }
            }

            if (dataDelimIdx >= tag.Length)
            {
                outTagData.Id = tag.ToString();
                outTagData.Data = StringSlice.Empty;
            }
            else
            {
                outTagData.Id = tag.Substring(0, dataDelimIdx).TrimEnd(MinimalWhitespaceChars).ToString();
                outTagData.Data = tag.Substring(dataDelimIdx).TrimStart(dataDelims).TrimStart(MinimalWhitespaceChars).ToString();
            }

            outTagData.m_CloseState = closeState;
        }

        /// <summary>
        /// Minimal set of whitespace characters.
        /// </summary>
        static public readonly char[] MinimalWhitespaceChars = new char[]
        {
            ' ', '\n', '\r', '\t', '\f', '\0'
        };

        /// <summary>
        /// Default delimiters between the tag id and data.
        /// </summary>
        static public readonly char[] DefaultDataDelimiters = new char[] { '=', ' ', ':', '\t' };

        #endregion // Parse
    }
}