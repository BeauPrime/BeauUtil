/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    1 August 2020
 * 
 * File:    TagStringProcessor.cs
 * Purpose: String tag processor.
 */

using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace BeauUtil.Tags
{
    /// <summary>
    /// Parser to turn a string into a TagString.
    /// </summary>
    public partial class TagStringParser : IDisposable
    {
        static public readonly string VisibleRichTagChar = char.ToString((char) 1);

        #region Local Vars

        // processors

        protected IDelimiterRules m_Delimiters = RichTextDelimiters;
        protected IEventProcessor m_EventProcessor;
        protected IReplaceProcessor m_ReplaceProcessor;

        // string state

        protected StringBuilder m_RichBuilder = new StringBuilder(512);
        protected StringBuilder m_StrippedBuilder = new StringBuilder(512);
        protected StringBuilder m_SpliceBuilder = new StringBuilder(512);

        #endregion // Local Vars
        
        #region Processors

        /// <summary>
        /// Delimiter rules for parsing tags.
        /// </summary>
        public IDelimiterRules Delimiters
        {
            get { return m_Delimiters; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value", "Cannot set null delimiter rules");
                m_Delimiters = value;
            }
        }

        /// <summary>
        /// Event processor.
        /// </summary>
        public IEventProcessor EventProcessor
        {
            get { return m_EventProcessor; }
            set { m_EventProcessor = value; }
        }

        /// <summary>
        /// Replace processor.
        /// </summary>
        public IReplaceProcessor ReplaceProcessor
        {
            get { return m_ReplaceProcessor; }
            set { m_ReplaceProcessor = value; }
        }

        #endregion // Processors

        #region Public API

        /// <summary>
        /// Parses the given string into a TagString.
        /// </summary>
        public TagString Parse(StringSlice inInput, object inContext = null)
        {
            TagString str = new TagString();
            Parse(ref str, inInput, inContext);
            return str;
        }

        /// <summary>
        /// Parses the given string into a TagString.
        /// </summary>
        public void Parse(ref TagString outTarget, StringSlice inInput, object inContext = null)
        {
            if (outTarget == null)
            {
                outTarget = new TagString();
            }
            else
            {
                outTarget.Clear();
            }

            if (inInput.IsEmpty)
                return;

            m_RichBuilder.Length = 0;
            bool bModified;
            ProcessInput(inInput, outTarget, inContext, out bModified);
            if (bModified)
            {
                outTarget.RichText = m_RichBuilder.Flush();
                outTarget.VisibleText = m_StrippedBuilder.Flush();
            }
            else
            {
                string originalString = inInput.ToString();
                outTarget.RichText = outTarget.VisibleText = originalString;
                m_RichBuilder.Length = 0;
                m_StrippedBuilder.Length = 0;
            }
        }

        #endregion // Public API

        #region Processing

        protected void ProcessInput(StringSlice inInput, TagString outTarget, object inContext, out bool outbModified)
        {
            bool bModified = false;
            bool bTrackRichText = m_Delimiters.RichText;
            bool bTrackTags = !bTrackRichText || !HasSameDelims(m_Delimiters, RichTextDelimiters);
            bool bIsCurlyBrace = HasSameDelims(m_Delimiters, CurlyBraceDelimiters);

            if (!bTrackRichText && m_EventProcessor == null && m_ReplaceProcessor == null)
            {
                // if we're not considering rich text, and we have no processors, there's nothing to do here
                outTarget.AddNode(TagNodeData.TextNode((uint) inInput.Length));
                outbModified = false;
                return;
            }

            ParseState state = new ParseState();
            state.Initialize(inInput, outTarget, inContext, m_RichBuilder, m_StrippedBuilder, m_SpliceBuilder);

            int length = state.Input.Length;
            int charIdx = 0;

            while(charIdx < length)
            {
                bool bHandled = false;

                if (bTrackRichText)
                {
                    if (state.Input.AttemptMatch(charIdx, "<"))
                    {
                        state.RichStart = charIdx;
                    }
                    else if (state.RichStart >= 0 && state.Input.AttemptMatch(charIdx, ">"))
                    {
                        StringSlice richSlice = state.Input.Substring(state.RichStart + 1, charIdx - state.RichStart - 1);
                        TagData richTag = TagData.Parse(richSlice, RichTextDelimiters);

                        CopyNonRichText(ref state, state.RichStart);

                        bool bRichHandled = false;

                        if (RecognizeRichText(richTag, m_Delimiters))
                        {
                            CopyRichTag(ref state, charIdx + 1);
                            if (StringUtils.RichText.GeneratesVisibleCharacter(richTag.Id.ToString()))
                            {
                                CopyOnlyNonRichText(ref state, VisibleRichTagChar);
                            }
                            bRichHandled = true;
                        }
                        else if (!bTrackTags)
                        {
                            if (m_ReplaceProcessor != null)
                            {
                                string replace;
                                if (m_ReplaceProcessor.TryReplace(richTag, richSlice, state.Context, out replace))
                                {
                                    if (ContainsPotentialTags(replace, m_Delimiters))
                                    {
                                        RestartString(ref state, replace, charIdx + 1);
                                        charIdx = -1;
                                        length = state.Input.Length;
                                    }
                                    else
                                    {
                                        SkipText(ref state, charIdx + 1);
                                        AddNonRichText(ref state, replace);
                                    }
                                    bRichHandled = true;
                                }
                            }

                            if (!bRichHandled && m_EventProcessor != null)
                            {
                                TagEventData eventData;
                                if (m_EventProcessor.TryProcess(richTag, state.Context, out eventData))
                                {
                                    outTarget.AddEvent(eventData);
                                    SkipText(ref state, charIdx + 1);
                                    bRichHandled = true;
                                }
                            }
                        }

                        if (!bRichHandled)
                        {
                            Debug.LogWarningFormat("[TagStringParser] Unrecognized text tag '{0}' in source '{1}'", richSlice, inInput);
                            CopyNonRichText(ref state, charIdx + 1);
                        }
                        else
                        {
                            bModified = true;
                        }

                        state.RichStart = -1;
                        state.TagStart = -1;
                        bHandled = true;
                    }
                }

                if (!bHandled && bTrackTags)
                {
                    if (state.Input.AttemptMatch(charIdx, m_Delimiters.TagStartDelimiter))
                    {
                        state.TagStart = charIdx;
                    }
                    else if (state.TagStart >= 0 && state.Input.AttemptMatch(charIdx, m_Delimiters.TagEndDelimiter))
                    {
                        StringSlice tagSlice = state.Input.Substring(state.TagStart + m_Delimiters.TagStartDelimiter.Length, charIdx - state.TagStart + 1 - m_Delimiters.TagEndDelimiter.Length - m_Delimiters.TagStartDelimiter.Length);
                        TagData tag = TagData.Parse(tagSlice, m_Delimiters);

                        CopyNonRichText(ref state, state.TagStart);

                        bool bTagHandled = false;

                        if (bIsCurlyBrace && tag.Data.IsEmpty)
                        {
                            int argIndex;
                            if (StringParser.TryParseInt(tag.Id, out argIndex))
                            {
                                CopyNonRichText(ref state, charIdx + 1);
                                bTagHandled = true;
                            }
                        }

                        if (!bTagHandled && m_ReplaceProcessor != null)
                        {
                            string replace;
                            if (m_ReplaceProcessor.TryReplace(tag, tagSlice, state.Context, out replace))
                            {
                                if (ContainsPotentialTags(replace, m_Delimiters))
                                {
                                    RestartString(ref state, replace, charIdx + 1);
                                    charIdx = -1;
                                    length = state.Input.Length;
                                }
                                else
                                {
                                    SkipText(ref state, charIdx + 1);
                                    AddNonRichText(ref state, replace);
                                }
                                bTagHandled = true;
                            }
                        }

                        if (!bTagHandled && m_EventProcessor != null)
                        {
                            TagEventData eventData;
                            if (m_EventProcessor.TryProcess(tag, state.Context, out eventData))
                            {
                                outTarget.AddEvent(eventData);
                                SkipText(ref state, charIdx + 1);
                                bTagHandled = true;
                            }
                        }

                        if (!bTagHandled)
                        {
                            Debug.LogWarningFormat("[TagStringParser] Unrecognized text tag '{0}' in source '{1}'", tagSlice, inInput);
                            CopyNonRichText(ref state, charIdx + 1);
                        }
                        else
                        {
                            bModified = true;
                        }

                        state.TagStart = -1;
                        state.RichStart = -1;
                        bHandled = true;
                    }
                }

                if (!bHandled && state.TagStart == -1 && state.RichStart == -1 && m_ReplaceProcessor != null)
                {
                    string charCodeReplace;
                    if (m_ReplaceProcessor.TryReplace(state.Input[charIdx], inContext, out charCodeReplace))
                    {
                        CopyNonRichText(ref state, charIdx);
                        if (ContainsPotentialTags(charCodeReplace, m_Delimiters))
                        {
                            RestartString(ref state, charCodeReplace, charIdx + 1);
                            charIdx = -1;
                            length = state.Input.Length;
                        }
                        else
                        {
                            SkipText(ref state, charIdx + 1);
                            AddNonRichText(ref state, charCodeReplace);
                        }
                    }
                }

                ++charIdx;
            }

            CopyNonRichText(ref state, length);
            outbModified = bModified;
        }

        static protected void SkipText(ref ParseState ioState, int inIdx)
        {
            int length = ioState.CopyLengthExclusive(inIdx);
            if (length <= 0)
                return;

            ioState.CopyStart += length;
            // Debug.LogFormat("[TagStringParser] Skipped {0} characters", length);
        }

        static protected void CopyNonRichText(ref ParseState ioState, int inIdx)
        {
            int copyLength = ioState.CopyLengthExclusive(inIdx);
            if (copyLength <= 0)
                return;
            
            StringSlice copySlice = ioState.Input.Substring(ioState.CopyStart, copyLength);

            if (ioState.RichOutput.Length <= 0)
            {
                copySlice = copySlice.TrimStart();
            }

            if (copySlice.Length > 0)
            {
                ioState.Target.AddText((uint) copySlice.Length);
                copySlice.AppendTo(ioState.RichOutput);
                copySlice.AppendTo(ioState.StrippedOutput);
            }

            ioState.CopyStart += copyLength;

            // Debug.LogFormat("[TagStringParser] Copied non-rich text '{0}'", copySlice);
        }

        static protected void CopyOnlyNonRichText(ref ParseState ioState, StringSlice inString)
        {
            int copyLength = inString.Length;
            if (copyLength <= 0)
                return;
            
            if (ioState.StrippedOutput.Length <= 0)
            {
                inString = inString.TrimStart();
            }

            if (inString.Length > 0)
            {
                ioState.Target.AddText((uint) inString.Length);
                inString.AppendTo(ioState.StrippedOutput);
            }
        }

        static protected void CopyRichTag(ref ParseState ioState, int inIdx)
        {
            int copyLength = ioState.CopyLengthExclusive(inIdx);
            if (copyLength <= 0)
                return;
            
            StringSlice copySlice = ioState.Input.Substring(ioState.CopyStart, copyLength);
            copySlice.AppendTo(ioState.RichOutput);
            ioState.CopyStart += copyLength;

            // Debug.LogFormat("[TagStringParser] Copied rich tag '{0}'", copySlice);
        }

        static protected void AddNonRichText(ref ParseState ioState, StringSlice inString)
        {
            if (inString.IsEmpty)
                return;

            if (ioState.RichOutput.Length <= 0)
            {
                inString = inString.TrimStart();
            }

            if (inString.Length > 0)
            {
                ioState.Target.AddText((uint) inString.Length);
                inString.AppendTo(ioState.RichOutput);
                inString.AppendTo(ioState.StrippedOutput);
            }

            // Debug.LogFormat("[TagStringParser] Added non-rich text '{0}'", inString);
        }

        static protected bool RecognizeRichText(TagData inData, IDelimiterRules inDelimiters)
        {
            // recognize hex colors
            if (inData.Id.StartsWith('#'))
            {
                StringSlice colorCheck = inData.Id.Substring(1);
                if (colorCheck.Length == 6 || colorCheck.Length == 8)
                {
                    bool bIsColor = true;
                    for(int i = 0; i < colorCheck.Length; ++i)
                    {
                        char c = char.ToLowerInvariant(colorCheck[i]);
                        bool bIsHex = char.IsNumber(c) || (c >= 'a' && c <= 'f');
                        if (!bIsHex)
                        {
                            bIsColor = false;
                            break;
                        }
                    }

                    if (bIsColor)
                        return true;
                }
            }

            foreach(var tag in StringUtils.RichText.RecognizedRichTags)
            {
                if (inData.Id.Equals(tag, true))
                    return true;
            }

            if (inDelimiters.AdditionalRichTextTags != null)
            {
                foreach(var tag in inDelimiters.AdditionalRichTextTags)
                {
                    if (inData.Id.Equals(tag, true))
                        return true;
                }
            }

            return false;
        }

        static protected void RestartString(ref ParseState ioState, StringSlice inInitial, int inIndex)
        {
            StringSlice originalRemaining = ioState.Input.Substring(inIndex);
            if (originalRemaining.Length <= 0)
            {
                ioState.Input = inInitial;
            }
            else
            {
                inInitial.AppendTo(ioState.RegenBuilder);
                originalRemaining.AppendTo(ioState.RegenBuilder);
                ioState.Input = ioState.RegenBuilder.Flush();
            }

            ioState.CopyStart = 0;
        }

        static protected bool ContainsPotentialTags(StringSlice inSlice, IDelimiterRules inDelimiters)
        {
            if (inDelimiters.RichText)
            {
                bool bIdenticalDelims = inDelimiters.TagStartDelimiter == "<" && inDelimiters.TagEndDelimiter == ">";
                if (inSlice.Contains("<") || inSlice.Contains(">"))
                    return true;

                if (bIdenticalDelims)
                    return false;
            }

            return inSlice.Contains(inDelimiters.TagStartDelimiter) && inSlice.Contains(inDelimiters.TagEndDelimiter);
        }

        #endregion // Processing

        #region Parsing State

        protected struct ParseState
        {
            public StringSlice Input;
            public TagString Target;

            public object Context;

            public StringBuilder RichOutput;
            public StringBuilder StrippedOutput;
            public StringBuilder RegenBuilder;

            public int CopyStart;
            public int RichStart;
            public int TagStart;

            public void Initialize(StringSlice inInput, TagString inTarget, object inContext, StringBuilder inRichOutput, StringBuilder inStrippedOutput, StringBuilder inRegenBuilder)
            {
                Input = inInput;
                Target = inTarget;

                Context = inContext;

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

        #region IDisposable

        public virtual void Dispose()
        {
            if (m_RichBuilder != null)
            {
                m_RichBuilder.Length = 0;
                m_RichBuilder = null;
            }

            m_Delimiters = null;
            
        }
    
        #endregion // IDisposable
    
        #region Utilities


        /// <summary>
        /// Returns if the given string contains any text or rich text tags.
        /// </summary>
        static public bool ContainsText(StringSlice inString, IDelimiterRules inDelimiters, ICollection<string> inTextTags = null)
        {
            bool bTrackRichText = inDelimiters.RichText;
            bool bTrackTags = !bTrackRichText || !HasSameDelims(inDelimiters, RichTextDelimiters);

            int length = inString.Length;
            int charIdx = 0;
            int richStart = -1;
            int tagStart = -1;
            int copyStart = 0;

            while(charIdx < length)
            {
                if (bTrackRichText)
                {
                    if (inString.AttemptMatch(charIdx, "<"))
                    {
                        richStart = charIdx;
                    }
                    else if (inString.AttemptMatch(charIdx, ">"))
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
                    else if (inString.AttemptMatch(charIdx, inDelimiters.TagEndDelimiter))
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