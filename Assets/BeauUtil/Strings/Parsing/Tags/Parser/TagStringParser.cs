/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    1 August 2020
 * 
 * File:    TagStringProcessor.cs
 * Purpose: String tag processor.
 */

#if CSHARP_7_3_OR_NEWER
#define EXPANDED_REFS
#endif // CSHARP_7_3_OR_NEWER

using System;
using System.Collections.Generic;
using System.Text;
using BeauUtil.Debugger;
using UnityEngine;

namespace BeauUtil.Tags
{
    /// <summary>
    /// Parser to turn a string into a TagString.
    /// </summary>
    public partial struct TagStringParser
    {
        static public readonly string VisibleRichTagChar = char.ToString((char) 1);
        public const int DefaultCapacity = 1024;
        public const int MinCapacity = 32;
        public const int MaxCapacity = ushort.MaxValue;

        #region Local Vars

        // processors

        private DelimiterRules m_Delimiters;
        private IEventProcessor m_EventProcessor;
        private IReplaceProcessor m_ReplaceProcessor;
        private int m_BufferSize;
        private Unsafe.ArenaHandle m_BufferArena;

        #endregion // Local Vars

        public TagStringParser(DelimiterRules inDelimiterRules, int inBufferSize = DefaultCapacity, Unsafe.ArenaHandle inArena = default)
        {
            m_Delimiters = inDelimiterRules;
            m_EventProcessor = null;
            m_ReplaceProcessor = null;

            if (inBufferSize < MinCapacity || inBufferSize > MaxCapacity)
                throw new ArgumentOutOfRangeException("Buffer size must be between " + MinCapacity + " and " + MaxCapacity);

            m_BufferSize = inBufferSize;
            m_BufferArena = inArena;
        }

        #region Defaults

        static private readonly TagStringParser s_DefaultParser = new TagStringParser(RichTextDelimiters);

        /// <summary>
        /// Default parser. Parses rich text tags.
        /// </summary>
        static public TagStringParser Default
        {
            get { return s_DefaultParser; }
        }

        #endregion // Defaults

        #region Processors

        /// <summary>
        /// Delimiter rules for parsing tags.
        /// </summary>
        public DelimiterRules Delimiters
        {
            get { return m_Delimiters; }
            set { m_Delimiters = value; }
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

        /// <summary>
        /// Size of the internal character buffer.
        /// </summary>
        public int BufferSize
        {
            get { return m_BufferSize; }
            set
            {
                if (value < MinCapacity || value > MaxCapacity)
                    throw new ArgumentOutOfRangeException("Buffer size must be between " + MinCapacity + " and " + MaxCapacity);
                m_BufferSize = value;
            }
        }

        #endregion // Processors

        #region Public API

        /// <summary>
        /// Parses the given string into a TagString.
        /// </summary>
        public TagString Parse(StringSlice inInput, object inContext = default(object))
        {
            TagString str = new TagString();
            Parse(ref str, inInput, inContext);
            return str;
        }

        /// <summary>
        /// Parses the given string into a TagString.
        /// </summary>
        public TagString Parse(UnsafeString inInput, object inContext = default(object))
        {
            TagString str = new TagString();
            Parse(ref str, inInput, inContext);
            return str;
        }

        /// <summary>
        /// Parses the given string into a TagString.
        /// </summary>
        public unsafe bool Parse(ref TagString outTarget, StringSlice inInput, object inContext = default(object))
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
                return false;

            if (inInput.Length > m_BufferSize)
            {
                Log.Error("Input string is too long ({0}) - max buffer size is {1}", inInput.Length, m_BufferSize);
                BypassProcessing(outTarget, inInput);
                return false;
            }

            if (!ShouldParse(this))
            {
                BypassProcessing(outTarget, inInput);
                return false;
            }

            char* buffer;
            bool pushedArena;
            if (m_BufferArena.FreeBytes() >= m_BufferSize)
            {
                buffer = m_BufferArena.AllocArray<char>(m_BufferSize);
                m_BufferArena.Push();
                pushedArena = true;
            }
            else
            {
                char* stackBuff = stackalloc char[m_BufferSize];
                buffer = stackBuff;
                pushedArena = false;
            }

            Assert.NotNull(buffer);
            inInput.CopyTo(buffer, m_BufferSize);

            ParseState parseState = default;
            InitParseState(this, ref parseState, buffer);
            PrepareParseState(ref parseState, outTarget, inInput.Length, inContext);

            bool bModified = ProcessInput(ref parseState);
            if (pushedArena)
            {
                m_BufferArena.Pop();
            }
            return bModified;
        }

        /// <summary>
        /// Parses the given string into a TagString.
        /// </summary>
        public unsafe bool Parse(ref TagString outTarget, UnsafeString inInput, object inContext = default(object))
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
                return false;

            if (inInput.Length > m_BufferSize)
            {
                Log.Error("Input string is too long ({0}) - max buffer size is {1}", inInput.Length, m_BufferSize);
                BypassProcessing(outTarget, inInput);
                return false;
            }

            if (!ShouldParse(this))
            {
                BypassProcessing(outTarget, inInput);
                return false;
            }

            char* buffer;
            bool pushedArena;
            if (m_BufferArena.FreeBytes() >= m_BufferSize)
            {
                buffer = m_BufferArena.AllocArray<char>(m_BufferSize);
                m_BufferArena.Push();
                pushedArena = true;
            }
            else
            {
                char* stackBuff = stackalloc char[m_BufferSize];
                buffer = stackBuff;
                pushedArena = false;
            }

            Assert.NotNull(buffer);
            inInput.CopyTo(buffer, m_BufferSize);

            ParseState parseState = default;
            InitParseState(this, ref parseState, buffer);
            PrepareParseState(ref parseState, outTarget, inInput.Length, inContext);

            bool bModified = ProcessInput(ref parseState);
            if (pushedArena)
            {
                m_BufferArena.Pop();
            }
            return bModified;
        }

        #endregion // Public API

        #region Setup

#if EXPANDED_REFS
        static private bool ShouldParse(in TagStringParser inParser)
#else
        static private bool ShouldParse(TagStringParser inParser)
#endif // EXPANDED_REFS
        {
            return inParser.m_BufferSize >= MinCapacity && (inParser.m_Delimiters.RichText || inParser.m_EventProcessor != null || inParser.m_ReplaceProcessor != null);
        }

        static private void BypassProcessing(TagString outTarget, StringSlice inInput)
        {
            inInput.AppendTo(outTarget.RichText);
            inInput.AppendTo(outTarget.VisibleText);
            outTarget.AddNode(TagNodeData.TextNode(0, (ushort) inInput.Length, 0, (ushort) inInput.Length));
        }

        static private void BypassProcessing(TagString outTarget, StringBuilder inInput)
        {
            outTarget.RichText.Append(inInput);
            outTarget.VisibleText.Append(inInput);
            outTarget.AddNode(TagNodeData.TextNode(0, (ushort) inInput.Length, 0, (ushort) inInput.Length));
        }

        static private void BypassProcessing(TagString outTarget, UnsafeString inInput)
        {
            inInput.AppendTo(outTarget.RichText);
            inInput.AppendTo(outTarget.VisibleText);
            outTarget.AddNode(TagNodeData.TextNode(0, (ushort) inInput.Length, 0, (ushort) inInput.Length));
        }

#if EXPANDED_REFS
        static private unsafe void InitParseState(in TagStringParser inParser, ref ParseState outState, char* inBuffer)
#else
        static private unsafe void InitParseState(TagStringParser inParser, ref ParseState outState, char* inBuffer)
#endif // EXPANDED_REFS
        {
            outState.InputBuffer = inBuffer;
            outState.InputBufferSize = inParser.m_BufferSize;

            outState.Delimiters = inParser.Delimiters;
            outState.ReplaceProcessor = inParser.m_ReplaceProcessor;
            outState.EventProcessor = inParser.m_EventProcessor;
        }

        static private unsafe void PrepareParseState(ref ParseState outState, TagString inTarget, int inInputLength, object inContext)
        {
            outState.Input = new UnsafeString(outState.InputBuffer, inInputLength);

            outState.Target = inTarget;
            outState.Context = inContext;

            outState.RichOutput = inTarget.RichText;
            outState.VisibleOutput = inTarget.VisibleText;
            outState.TagAllocator = inTarget.TagAllocator;

            outState.CopyStart = 0;
            outState.RichStart = -1;
            outState.TagStart = -1;
        }

        #endregion // Setup

        #region Processing

        static private unsafe bool ProcessInput(ref ParseState ioState)
        {
            bool bModified = false;
            bool bTrackRichText = ioState.Delimiters.RichText;
            bool bHasRichDelims = TagStringParser.HasSameDelims(ioState.Delimiters, TagStringParser.RichTextDelimiters);
            bool bTrackTags = !bTrackRichText || !bHasRichDelims;
            bool bIsCurlyBrace = TagStringParser.HasSameDelims(ioState.Delimiters, TagStringParser.CurlyBraceDelimiters);
            bool bHasHandlers = ioState.EventProcessor != null || ioState.ReplaceProcessor != null;

            int length = ioState.Input.Length;
            int charIdx = 0;

            while(charIdx < length)
            {
                bool bHandled = false;

                if (bTrackRichText)
                {
                    if (ioState.Input[charIdx] == '<')
                    {
                        ioState.RichStart = charIdx;
                    }
                    else if (ioState.RichStart >= 0 && ioState.Input[charIdx] == '>')
                    {
                        UnsafeString richSlice = ioState.Input.Substring(ioState.RichStart + 1, charIdx - ioState.RichStart - 1);
                        UnsafeString richId = TagData.ExtractId(richSlice, ioState.Delimiters);

                        CopyNonRichText(ref ioState, ioState.RichStart);

                        bool bRichHandled = false;

                        if (RecognizeRichText(richId, ioState.Delimiters))
                        {
                            CopyRichTag(ref ioState, charIdx + 1);
                            if (StringUtils.RichText.GeneratesVisibleCharacter(richId))
                            {
                                CopyOnlyNonRichText(ref ioState, VisibleRichTagChar);
                            }
                            bRichHandled = true;
                        }
                        else if (!bTrackTags && bHasHandlers)
                        {
                            StringSlice allocatedSlice = ioState.TagAllocator.Alloc(richSlice);
                            TagData richTag = TagData.Parse(allocatedSlice, TagStringParser.RichTextDelimiters);

                            if (ioState.ReplaceProcessor != null)
                            {
                                string replace;
                                if (ioState.ReplaceProcessor.TryReplace(richTag, allocatedSlice, ioState.Context, out replace))
                                {
                                    if (ContainsPotentialTags(replace, ioState.Delimiters, bHasRichDelims))
                                    {
                                        RestartString(ref ioState, replace, charIdx + 1);
                                        charIdx = -1;
                                        length = ioState.Input.Length;
                                    }
                                    else
                                    {
                                        SkipText(ref ioState, charIdx + 1);
                                        AddNonRichText(ref ioState, replace);
                                    }
                                    bRichHandled = true;
                                }
                            }

                            if (!bRichHandled && ioState.EventProcessor != null)
                            {
                                TagEventData eventData;
                                if (ioState.EventProcessor.TryProcess(richTag, ioState.Context, out eventData))
                                {
                                    ioState.Target.AddEvent(eventData);
                                    SkipText(ref ioState, charIdx + 1);
                                    bRichHandled = true;
                                }
                            }
                        }

                        if (!bRichHandled)
                        {
                            Debug.LogWarningFormat("[TagStringParser] Unrecognized text tag '{0}' in source '{1}'", richSlice, ioState.Input.ToString());
                            CopyNonRichText(ref ioState, charIdx + 1);
                        }
                        else
                        {
                            bModified = true;
                        }

                        ioState.RichStart = -1;
                        ioState.TagStart = -1;
                        bHandled = true;
                    }
                }

                if (!bHandled && bTrackTags)
                {
                    if (ioState.Input.AttemptMatch(charIdx, ioState.Delimiters.TagStartDelimiter))
                    {
                        ioState.TagStart = charIdx;
                    }
                    else if (ioState.TagStart >= 0 && ioState.Input.AttemptMatch(charIdx, ioState.Delimiters.TagEndDelimiter))
                    {
                        UnsafeString tagSlice = ioState.Input.Substring(ioState.TagStart + ioState.Delimiters.TagStartDelimiter.Length, charIdx - ioState.TagStart + 1 - ioState.Delimiters.TagEndDelimiter.Length - ioState.Delimiters.TagStartDelimiter.Length);
                        StringSlice allocatedSlice = ioState.TagAllocator.Alloc(tagSlice);
                        TagData tag = TagData.Parse(allocatedSlice, ioState.Delimiters);

                        CopyNonRichText(ref ioState, ioState.TagStart);

                        bool bTagHandled = false;

                        if (bIsCurlyBrace && tag.Data.IsEmpty)
                        {
                            int argIndex;
                            if (StringParser.TryParseInt(tag.Id, out argIndex))
                            {
                                CopyNonRichText(ref ioState, charIdx + 1);
                                bTagHandled = true;
                            }
                        }

                        if (!bTagHandled && ioState.ReplaceProcessor != null)
                        {
                            string replace;
                            if (ioState.ReplaceProcessor.TryReplace(tag, allocatedSlice, ioState.Context, out replace))
                            {
                                if (ContainsPotentialTags(replace, ioState.Delimiters, bHasRichDelims))
                                {
                                    RestartString(ref ioState, replace, charIdx + 1);
                                    charIdx = -1;
                                    length = ioState.Input.Length;
                                }
                                else
                                {
                                    SkipText(ref ioState, charIdx + 1);
                                    AddNonRichText(ref ioState, replace);
                                }
                                bTagHandled = true;
                            }
                        }

                        if (!bTagHandled && ioState.EventProcessor != null)
                        {
                            TagEventData eventData;
                            if (ioState.EventProcessor.TryProcess(tag, ioState.Context, out eventData))
                            {
                                ioState.Target.AddEvent(eventData);
                                SkipText(ref ioState, charIdx + 1);
                                bTagHandled = true;
                            }
                        }

                        if (!bTagHandled)
                        {
                            Debug.LogWarningFormat("[TagStringParser] Unrecognized text tag '{0}' in source '{1}'", tagSlice, ioState.Input.ToString());
                            CopyNonRichText(ref ioState, charIdx + 1);
                        }
                        else
                        {
                            bModified = true;
                        }

                        ioState.TagStart = -1;
                        ioState.RichStart = -1;
                        bHandled = true;
                    }
                }

                if (!bHandled && ioState.TagStart == -1 && ioState.RichStart == -1 && ioState.ReplaceProcessor != null)
                {
                    string charCodeReplace;
                    if (ioState.ReplaceProcessor.TryReplace(ioState.Input[charIdx], ioState.Context, out charCodeReplace))
                    {
                        CopyNonRichText(ref ioState, charIdx);
                        if (ContainsPotentialTags(charCodeReplace, ioState.Delimiters, bHasRichDelims))
                        {
                            RestartString(ref ioState, charCodeReplace, charIdx + 1);
                            charIdx = -1;
                            length = ioState.Input.Length;
                        }
                        else
                        {
                            SkipText(ref ioState, charIdx + 1);
                            AddNonRichText(ref ioState, charCodeReplace);
                        }
                    }
                }

                ++charIdx;
            }

            CopyNonRichText(ref ioState, length);
            return bModified;
        }

        static private void SkipText(ref ParseState ioState, int inIdx)
        {
            int length = ioState.CopyLengthExclusive(inIdx);
            if (length <= 0)
                return;

            ioState.CopyStart += length;
            // Debug.LogFormat("[TagStringParser] Skipped {0} characters", length);
        }

        static private void CopyNonRichText(ref ParseState ioState, int inIdx)
        {
            int copyLength = ioState.CopyLengthExclusive(inIdx);
            if (copyLength <= 0)
                return;

            UnsafeString copySlice = ioState.Input.Substring(ioState.CopyStart, copyLength);

            if (ioState.RichOutput.Length <= 0)
            {
                copySlice = copySlice.TrimStart();
            }

            if (copySlice.Length > 0)
            {
                ioState.Target.AddText((ushort) ioState.VisibleOutput.Length, (ushort) copySlice.Length, (ushort) ioState.RichOutput.Length, (ushort) copySlice.Length);
                copySlice.AppendTo(ioState.RichOutput);
                copySlice.AppendTo(ioState.VisibleOutput);
            }

            ioState.CopyStart += copyLength;

            // Debug.LogFormat("[TagStringParser] Copied non-rich text '{0}'", copySlice);
        }

        static private void CopyOnlyNonRichText(ref ParseState ioState, string inString)
        {
            int copyLength = inString.Length;
            if (copyLength <= 0)
                return;
            
            if (ioState.VisibleOutput.Length <= 0)
            {
                inString = inString.TrimStart();
            }

            if (inString.Length > 0)
            {
                ioState.Target.AddText((ushort) ioState.VisibleOutput.Length, (ushort) inString.Length, (ushort) ioState.RichOutput.Length, (ushort) inString.Length);
                ioState.VisibleOutput.Append(inString);
            }
        }

        static private void CopyRichTag(ref ParseState ioState, int inIdx)
        {
            int copyLength = ioState.CopyLengthExclusive(inIdx);
            if (copyLength <= 0)
                return;

            UnsafeString copySlice = ioState.Input.Substring(ioState.CopyStart, copyLength);
            ioState.Target.AddText((ushort) ioState.VisibleOutput.Length, 0, (ushort) ioState.RichOutput.Length, (ushort) copyLength);
            copySlice.AppendTo(ioState.RichOutput);
            ioState.CopyStart += copyLength;

            // Debug.LogFormat("[TagStringParser] Copied rich tag '{0}'", copySlice);
        }

        static private void AddNonRichText(ref ParseState ioState, string inString)
        {
            if (string.IsNullOrEmpty(inString))
                return;

            if (ioState.RichOutput.Length <= 0)
            {
                inString = inString.TrimStart();
            }

            if (inString.Length > 0)
            {
                ioState.Target.AddText((ushort) ioState.VisibleOutput.Length, (ushort) inString.Length, (ushort) ioState.RichOutput.Length, (ushort) inString.Length);
                ioState.RichOutput.Append(inString);
                ioState.VisibleOutput.Append(inString);
            }

            // Debug.LogFormat("[TagStringParser] Added non-rich text '{0}'", inString);
        }

        static private bool RecognizeRichText(UnsafeString inTag, DelimiterRules inDelimiters)
        {
            // recognize hex colors
            if (inTag.StartsWith('#'))
            {
                UnsafeString colorCheck = inTag.Substring(1);
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

            string[] recognized = StringUtils.RichText.RecognizedRichTags;
            for(int i = 0; i < recognized.Length; i++)
            {
                if (inTag.Equals(recognized[i], true))
                    return true;
            }

            if (inDelimiters.AdditionalRichTextTags != null)
            {
                foreach(var tag in inDelimiters.AdditionalRichTextTags)
                {
                    if (inTag.Equals(tag, true))
                        return true;
                }
            }

            return false;
        }

        static private unsafe void RestartString(ref ParseState ioState, string inInitial, int inIndex)
        {
            inInitial = inInitial ?? string.Empty;

            UnsafeString originalRemaining = ioState.Input.Substring(inIndex);
            int newLength = inInitial.Length + originalRemaining.Length;
            if (newLength > ioState.InputBufferSize)
                throw new ArgumentOutOfRangeException("inInitial", "No more room in text buffer");

            if (originalRemaining.Length > 0)
            {
                char* originalCopy = stackalloc char[originalRemaining.Length];
                originalRemaining.CopyTo(originalCopy, originalRemaining.Length);
                originalRemaining = new UnsafeString(originalCopy, originalRemaining.Length);
            }

            StringUtils.Copy(inInitial, ioState.InputBuffer, ioState.InputBufferSize);
            originalRemaining.CopyTo(ioState.InputBuffer + inInitial.Length, ioState.InputBufferSize - inInitial.Length);

            ioState.Input = new UnsafeString(ioState.InputBuffer, newLength);
            ioState.CopyStart = 0;
        }

        static private bool ContainsPotentialTags(string inString, DelimiterRules inDelimiters, bool inbIdenticalDelims)
        {
            if (inDelimiters.RichText)
            {
                if (inString.Contains("<") || inString.Contains(">"))
                    return true;

                if (inbIdenticalDelims)
                    return false;
            }

            return inString.Contains(inDelimiters.TagStartDelimiter) && inString.Contains(inDelimiters.TagEndDelimiter);
        }

        #endregion // Processing

        #region Parsing State

        private unsafe struct ParseState
        {
            public DelimiterRules Delimiters;
            public IEventProcessor EventProcessor;
            public IReplaceProcessor ReplaceProcessor;

            public char* InputBuffer;
            public int InputBufferSize;

            public UnsafeString Input;

            public TagString Target;

            public object Context;

            public StringBuilder RichOutput;
            public StringBuilder VisibleOutput;
            public StringArena TagAllocator;

            public int CopyStart;
            public int RichStart;
            public int TagStart;

            public int CopyLengthExclusive(int inEnd)
            {
                return inEnd - CopyStart;
            }
        }

        #endregion // Parsing State
    }
}