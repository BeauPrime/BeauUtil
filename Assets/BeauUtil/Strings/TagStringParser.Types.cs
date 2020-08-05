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

namespace BeauUtil
{
    public partial class TagStringParser
    {
        #region Delimiters

        /// <summary>
        /// Parsing character rules.
        /// </summary>
        public interface IDelimiterRules
        {
            string TagStartDelimiter { get; }
            string TagEndDelimiter { get; }

            char[] TagDataDelimiters { get; }
            char RegionCloseDelimiter { get; }

            bool RichText { get; }
            IEnumerable<string> AdditionalRichTextTags { get; }
        }

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

        static private readonly char[] DefaultDataDelimiters = new char[] { '=', ' ', ':' };

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

        #region Tag Data

        /// <summary>
        /// Tag information.
        /// </summary>
        public struct TagData
        {
            public StringSlice Id;
            public StringSlice Data;
            
            internal ClosingTagState CloseState;

            public bool IsEnd()
            {
                return (CloseState & ClosingTagState.Start) != 0;
            }

            public bool IsSelfContained()
            {
                return CloseState == 0 || (CloseState & ClosingTagState.End) != 0;
            }
        }

        /// <summary>
        /// Where a closing delimiter is present in a tag.
        /// </summary>
        [Flags]
        internal enum ClosingTagState
        {
            None = 0,
            Start = 0x01,
            End = 0x02
        }

        /// <summary>
        /// Parses a tag's contents into data.
        /// </summary>
        static public TagData ParseTag(StringSlice inSlice, IDelimiterRules inDelimiters)
        {
            TagData data;
            ParseTag(inSlice, inDelimiters, out data);
            return data;
        }

        /// <summary>
        /// Parses a tag's contents into data.
        /// </summary>
        static public void ParseTag(StringSlice inSlice, IDelimiterRules inDelimiters, out TagData outTagData)
        {
            if (inDelimiters == null)
                throw new ArgumentNullException(nameof(IDelimiterRules));

            StringSlice tag = inSlice;
            tag = tag.Trim(TagWhitespaceChars);

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
                tag = tag.Trim(TagWhitespaceChars);

            if (inSlice.Length == 0)
            {
                outTagData = default(TagData);
                return;
            }

            ClosingTagState closeState = 0;

            char endDelim = inDelimiters.RegionCloseDelimiter;
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

            if (closeState != 0)
            {
                tag = tag.Trim(TagWhitespaceChars);
            }

            char[] dataDelims = inDelimiters.TagDataDelimiters;
            int dataDelimIdx = tag.Length;
            foreach (var delim in dataDelims)
            {
                int idx = tag.IndexOf(delim);
                if (idx >= 0 && idx < dataDelimIdx)
                    dataDelimIdx = idx;
            }

            if (dataDelimIdx >= tag.Length)
            {
                outTagData.Id = tag;
                outTagData.Data = StringSlice.Empty;
            }
            else
            {
                outTagData.Id = tag.Substring(0, dataDelimIdx).TrimEnd(TagWhitespaceChars);
                outTagData.Data = tag.Substring(dataDelimIdx).TrimStart(dataDelims).TrimStart(TagWhitespaceChars);
            }

            outTagData.CloseState = closeState;
        }

        static private readonly char[] TagWhitespaceChars = new char[]
        {
            ' ', '\n', '\r', '\t', '\f', '\0'
        };

        #endregion // Tag Data

        #region Tag Processors

        /// <summary>
        /// Interface for parsing event tags.
        /// </summary>
        public interface IEventProcessor
        {
            bool TryProcess(TagData inData, out TagString.EventData outEvent);
        }

        /// <summary>
        /// Interface for parsing replace tags.
        /// </summary>
        public interface ITextProcessor
        {
            bool TryReplace(TagData inData, out string outReplace);
        }

        #endregion // Tag Processors
    
        #region Parsing State

        protected struct ParseState
        {
            public StringSlice Input;
            public TagString Target;

            public StringBuilder RichOutput;
            public StringBuilder StrippedOutput;
            public StringBuilder RegenBuilder;

            public int CopyStart;
            public int RichStart;
            public int TagStart;

            public void Initialize(StringSlice inInput, TagString inTarget, StringBuilder inRichOutput, StringBuilder inStrippedOutput, StringBuilder inRegenBuilder)
            {
                Input = inInput;
                Target = inTarget;

                RichOutput = inRichOutput;
                StrippedOutput = inStrippedOutput;
                RegenBuilder = inRegenBuilder;

                CopyStart = 0;
                RichStart = -1;
                TagStart = -1;
            }

            public int CopyLengthExclusive(int inEnd)
            {
                return inEnd - CopyStart;
            }
        }

        #endregion // Parsing State
    }
}