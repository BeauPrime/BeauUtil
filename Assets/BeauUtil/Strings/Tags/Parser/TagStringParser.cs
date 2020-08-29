/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    1 August 2020
 * 
 * File:    TagStringProcessor.cs
 * Purpose: String tag processor.
 */

using System;
using System.Text;
using UnityEngine;

namespace BeauUtil.Tags
{
    /// <summary>
    /// Parser to turn a string into a TagString.
    /// </summary>
    public partial class TagStringParser : IDisposable
    {
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
            Parse(inInput, ref str, inContext);
            return str;
        }

        /// <summary>
        /// Parses the given string into a TagString.
        /// </summary>
        public void Parse(StringSlice inInput, ref TagString outTarget, object inContext = null)
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
            ProcessInput(inInput, outTarget, inContext);
            outTarget.RichText = m_RichBuilder.Flush();
            outTarget.VisibleText = m_StrippedBuilder.Flush();
        }

        #endregion // Public API

        #region Processing

        protected void ProcessInput(StringSlice inInput, TagString outTarget, object inContext)
        {
            bool bTrackRichText = m_Delimiters.RichText;
            bool bTrackTags = !bTrackRichText || !HasSameDelims(m_Delimiters, RichTextDelimiters);

            if (!bTrackRichText && m_EventProcessor == null && m_ReplaceProcessor == null)
            {
                // if we're not considering rich text, and we have no processors, there's nothing to do here
                outTarget.AddNode(TagNodeData.TextNode((uint) inInput.Length));
                inInput.AppendTo(m_RichBuilder);
                inInput.AppendTo(m_StrippedBuilder);
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
                    else if (state.Input.AttemptMatch(charIdx, ">"))
                    {
                        StringSlice richSlice = state.Input.Substring(state.RichStart, charIdx - state.RichStart + 1);
                        TagData richTag = TagData.Parse(richSlice, RichTextDelimiters);

                        CopyNonRichText(ref state, state.RichStart);

                        bool bRichHandled = false;

                        if (RecognizeRichText(richTag, m_Delimiters))
                        {
                            CopyRichTag(ref state, charIdx + 1);
                            bRichHandled = true;
                        }
                        else if (!bTrackTags)
                        {
                            if (m_ReplaceProcessor != null)
                            {
                                string replace;
                                if (m_ReplaceProcessor.TryReplace(richTag, state.Context, out replace))
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
                    else if (state.Input.AttemptMatch(charIdx, m_Delimiters.TagEndDelimiter))
                    {
                        StringSlice tagSlice = state.Input.Substring(state.TagStart, charIdx - state.TagStart + 1);
                        TagData tag = TagData.Parse(tagSlice, m_Delimiters);

                        CopyNonRichText(ref state, state.TagStart);

                        bool bTagHandled = false;

                        if (m_ReplaceProcessor != null)
                        {
                            string replace;
                            if (m_ReplaceProcessor.TryReplace(tag, state.Context, out replace))
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

                        state.TagStart = -1;
                        state.RichStart = -1;
                        bHandled = true;
                    }
                }

                ++charIdx;
            }

            CopyNonRichText(ref state, length);
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
    }
}