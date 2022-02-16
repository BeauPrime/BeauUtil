/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    24 August 2020
 * 
 * File:    BlockParser.cs
 * Purpose: Parser for generating blocks of data.
 */

#if UNITY_EDITOR || DEVELOPMENT_BUILD || DEBUG
#define DEVELOPMENT
#endif // DEVELOPMENT

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using BeauUtil.Tags;
using UnityEngine;
using LineBuffer = BeauUtil.RingBuffer<BeauUtil.StringSlice>;

namespace BeauUtil.Blocks
{
    public class BlockParser
    {
        static public readonly string NullFilename = "<anonymous>";

        protected BlockParser() {
            throw new NotImplementedException();
        }

        #region Parse

        /// <summary>
        /// Parses the given file contents into blocks.
        /// </summary>
        static public TPackage Parse<TBlock, TPackage>(string inFileName, StringSlice inFile, IBlockParsingRules inRules, IBlockGenerator<TBlock, TPackage> inGenerator, BlockMetaCache inCache = null)
            where TBlock : class, IDataBlock
            where TPackage : class, IDataBlockPackage<TBlock>
        {
            IEnumerator parser;
            TPackage package = ParseAsync(inFileName, inFile, inRules, inGenerator, inCache, out parser);
            if (parser != null)
            {
                using(parser as IDisposable)
                {
                    while (parser.MoveNext()) ;
                }
            }

            return package;
        }

        /// <summary>
        /// Parses the given file contents into blocks
        /// and merges into the given package.
        /// </summary>
        static public void Parse<TBlock, TPackage>(ref TPackage ioPackage, string inFileName, StringSlice inFile, IBlockParsingRules inRules, IBlockGenerator<TBlock, TPackage> inGenerator, BlockMetaCache inCache = null)
            where TBlock : class, IDataBlock
            where TPackage : class, IDataBlockPackage<TBlock>
        {
            IEnumerator parser = ParseAsync(ref ioPackage, inFileName, inFile, inRules, inGenerator, inCache);
            if (parser != null)
            {
                using(parser as IDisposable)
                {
                    while (parser.MoveNext()) ;
                }
            }
        }

        /// <summary>
        /// Parses the given file contents into blocks asynchronously.
        /// Each MoveNext() call on the returned IEnumerator will parse one line.
        /// </summary>
        static public TPackage ParseAsync<TBlock, TPackage>(string inFileName, StringSlice inFile, IBlockParsingRules inRules, IBlockGenerator<TBlock, TPackage> inGenerator, out IEnumerator outLoader)
            where TBlock : class, IDataBlock
            where TPackage : class, IDataBlockPackage<TBlock>
        {
            return ParseAsync<TBlock, TPackage>(inFileName, inFile, inRules, inGenerator, null, out outLoader);
        }

        /// <summary>
        /// Parses the given file contents into blocks asynchronously.
        /// Each MoveNext() call on the returned IEnumerator will parse one line.
        /// </summary>
        static public TPackage ParseAsync<TBlock, TPackage>(string inFileName, StringSlice inFile, IBlockParsingRules inRules, IBlockGenerator<TBlock, TPackage> inGenerator, BlockMetaCache inCache, out IEnumerator outLoader)
            where TBlock : class, IDataBlock
            where TPackage : class, IDataBlockPackage<TBlock>
        {
            if (inFile.IsEmpty)
            {
                outLoader = null;
                return null;
            }

            if (inRules == null)
                throw new ArgumentNullException("inRules");
            if (inGenerator == null)
                throw new ArgumentNullException("inGenerator");

            string fileName = string.IsNullOrEmpty(inFileName) ? NullFilename : inFileName;
            TPackage package = inGenerator.CreatePackage(fileName);
            outLoader = InternalBlockParser<TBlock, TPackage>.ParseFile(fileName, inFile, package, inRules, inGenerator, inCache);
            return package;
        }

        /// <summary>
        /// Parses the given file contents into blocks asynchronously
        /// and merges into the given package.
        /// Each MoveNext() call on the returned IEnumerator will parse one line.
        /// </summary>
        static public IEnumerator ParseAsync<TBlock, TPackage>(ref TPackage ioPackage, string inFileName, StringSlice inFile, IBlockParsingRules inRules, IBlockGenerator<TBlock, TPackage> inGenerator, BlockMetaCache inCache = null)
            where TBlock : class, IDataBlock
            where TPackage : class, IDataBlockPackage<TBlock>
        {
            if (inFile.IsEmpty)
            {
                return null;
            }

            if (inRules == null)
                throw new ArgumentNullException("inRules");
            if (inGenerator == null)
                throw new ArgumentNullException("inGenerator");

            string fileName = string.IsNullOrEmpty(inFileName) ? NullFilename : inFileName;
            if (ioPackage == null)
            {
                ioPackage = inGenerator.CreatePackage(fileName);
            }
            return InternalBlockParser<TBlock, TPackage>.ParseFile(fileName, inFile, ioPackage, inRules, inGenerator, inCache);
        }

        #endregion // Parse

        #region Cache

        /// <summary>
        /// Clears cached data.
        /// </summary>
        static public void ClearCache()
        {
            BlockMetaCache.Default.ClearCache();
            if (s_PrefixPriorityCache != null)
            {
                s_PrefixPriorityCache.Clear();
                s_PrefixPriorityCache = null;
            }
        }

        #endregion // Cache

        #region Logging

        [Conditional("DEVELOPMENT")]
        static protected void LogError(BlockFilePosition inPosition, string inContent)
        {
            UnityEngine.Debug.LogErrorFormat("[BlockParser] Parsing Error at {0}: {1}", inPosition, inContent);
        }

        [Conditional("DEVELOPMENT")]
        static protected void LogError(BlockFilePosition inPosition, string inContent, object inParam)
        {
            UnityEngine.Debug.LogErrorFormat("[BlockParser] Parsing Error at {0}: {1}", inPosition, string.Format(inContent, inParam));
        }

        [Conditional("DEVELOPMENT")]
        static protected void LogError(BlockFilePosition inPosition, string inContent, params object[] inParams)
        {
            UnityEngine.Debug.LogErrorFormat("[BlockParser] Parsing Error at {0}: {1}", inPosition, string.Format(inContent, inParams));
        }

        #endregion // Logging

        #region Delimiters

        static private Dictionary<IBlockParsingRules, PrefixType[]> s_PrefixPriorityCache;

        static private PrefixType[] CachePrefixPriorities(IBlockParsingRules inRules)
        {
            PrefixType[] prefixes = null;
            if (s_PrefixPriorityCache != null && s_PrefixPriorityCache.TryGetValue(inRules, out prefixes))
                return prefixes;

            s_PrefixPriorityCache = new Dictionary<IBlockParsingRules, PrefixType[]>();
            prefixes = GeneratePrefixPriority(inRules);
            s_PrefixPriorityCache.Add(inRules, prefixes);
            return prefixes;
        }

        // Generates a prioritized set of prefixes to analyze, from most specific to least specific
        static private PrefixType[] GeneratePrefixPriority(IBlockParsingRules inRules)
        {
            List<PriorityValue<PrefixType>> buffer = new List<PriorityValue<PrefixType>>(4);
            AddPrefixPriority(buffer, inRules.BlockIdPrefix, PrefixType.BlockId);
            AddPrefixPriority(buffer, inRules.BlockMetaPrefix, PrefixType.BlockMeta);
            AddPrefixPriority(buffer, inRules.BlockEndPrefix, PrefixType.BlockEnd);
            AddPrefixPriority(buffer, inRules.PackageMetaPrefix, PrefixType.PackageMeta);
            buffer.Sort();

            PrefixType[] priority = new PrefixType[buffer.Count];
            for (int i = 0; i < buffer.Count; ++i)
            {
                priority[i] = buffer[i].Value;
            }
            return priority;
        }

        // Adds a prefix priority to the buffer
        static private void AddPrefixPriority(List<PriorityValue<PrefixType>> ioBuffer, string inString, PrefixType inPrefix)
        {
            if (!string.IsNullOrEmpty(inString))
                ioBuffer.Add(new PriorityValue<PrefixType>(inPrefix, inString.Length));
        }

        protected class BlockTagDelimiters : IDelimiterRules
        {
            public IBlockParsingRules BlockParser;

            #region IDelimiterRules

            public string TagStartDelimiter { get { return null; } }

            public string TagEndDelimiter { get { return null; } }

            public char[] TagDataDelimiters { get { return BlockParser.TagDataDelimiters; } }

            public char RegionCloseDelimiter { get { return '\0'; } }

            public bool RichText { get { return false; } }

            public IEnumerable<string> AdditionalRichTextTags { get { return null; } }

            #endregion // IDelimiterRules
        }

        static protected readonly char[] TrimCharsWithSpace = new char[]
        {
            ' ', '\n', '\r', '\t', '\f', '\0'
        };

        static protected readonly char[] TrimCharsWithoutSpace = new char[]
        {
            '\n', '\r', '\t', '\f', '\0'
        };

        #endregion // Delimiters
    
        #region String Builder

        static private readonly Stack<StringBuilder> s_StringBuilderPool = new Stack<StringBuilder>(4);

        static protected StringBuilder RentStringBuilder()
        {
            if (s_StringBuilderPool.Count > 0)
            {
                StringBuilder sb = s_StringBuilderPool.Pop();
                sb.Length = 0;
                return sb;
            }

            return new StringBuilder(256);
        }

        static protected void ReturnStringBuilder(ref StringBuilder ioBuilder)
        {
            if (ioBuilder == null)
                return;

            #if DEVELOPMENT
            if (s_StringBuilderPool.Contains(ioBuilder))
                throw new ArgumentException("ioBuilder");
            #endif // DEVELOPMENT

            ioBuilder.Length = 0;
            s_StringBuilderPool.Push(ioBuilder);
            ioBuilder = null;
        }

        #endregion // String Builder

        #region Line Buffer

        static private readonly Stack<LineBuffer> s_LineBufferPool = new Stack<LineBuffer>(4);

        static protected LineBuffer RentLineBuffer()
        {
            if (s_LineBufferPool.Count > 0)
            {
                LineBuffer buffer = s_LineBufferPool.Pop();
                buffer.Clear();
                return buffer;
            }

            return new LineBuffer(16, RingBufferMode.Expand);
        }

        static protected void ReturnLineBuffer(ref LineBuffer ioBuffer)
        {
            if (ioBuffer == null)
                return;

            #if DEVELOPMENT
            if (s_LineBufferPool.Contains(ioBuffer))
                throw new ArgumentException("ioBuffer");
            #endif // DEVELOPMENT

            ioBuffer.Clear();
            s_LineBufferPool.Push(ioBuffer);
            ioBuffer = null;
        }

        static protected int SplitIntoLines(IBlockParsingRules inRules, StringSlice inString, LineBuffer inBuffer)
        {
            if (inRules.CustomLineSplitter != null)
                return inString.Split(inRules.CustomLineSplitter, StringSplitOptions.None, inBuffer);
            return inString.Split(inRules.LineDelimiters, StringSplitOptions.None, inBuffer);
        }

        static protected int PrependLinesToExistingBuffer(LineBuffer inBuffer, StringSlice inString, IBlockParsingRules inRules)
        {
            var tempBuffer = RentLineBuffer();
            try
            {
                int lineCount = SplitIntoLines(inRules, inString, tempBuffer);
                
                // ensure we have enough capacity
                if (inBuffer.Capacity < inBuffer.Count + lineCount)
                {
                    int newCapacity = Mathf.NextPowerOfTwo((int) (inBuffer.Count + lineCount));
                    if (newCapacity > 0 && newCapacity < 4)
                        newCapacity = 4;
                    inBuffer.SetCapacity(newCapacity);
                }

                for(int i = tempBuffer.Count - 1; i >= 0; i--)
                {
                    inBuffer.PushFront(tempBuffer[i]);
                }
                return lineCount;
            }
            finally
            {
                ReturnLineBuffer(ref tempBuffer);
            }
        }

        #endregion // Line Buffer

        #region Parse Buffer

        protected sealed class ParseBuffer : IDisposable
        {
            public readonly BlockTagDelimiters TagDelimiters = new BlockTagDelimiters();
            public readonly RingBuffer<PositionFrame> PositionStack = new RingBuffer<PositionFrame>(4, RingBufferMode.Expand);
            
            public BlockMetaCache Cache;
            public PrefixType[] PrefixPriorities;
            public IBlockParsingRules Rules;

            public BlockState CurrentState;
            public BlockFilePosition Position;
            public bool PositionInline;
            public int LineCount = -1;

            public LineBuffer Buffer;

            public StringBuilder Builder;
            public StringBuilder LineBuilder;
            public StringBuilder ContentBuilder;
            public uint TempFlags;

            public bool Error;
            public bool BlockError;

            public void PushPosition()
            {
                PositionStack.PushBack(new PositionFrame(Position, PositionInline, LineCount));
            }

            public void PopPosition()
            {
                PositionFrame oldState = PositionStack.PopBack();
                Position = oldState.Position;
                PositionInline = oldState.IsInline;
                LineCount = oldState.LineCount;
            }

            public void Dispose()
            {
                ReturnLineBuffer(ref Buffer);
                ReturnStringBuilder(ref Builder);
                ReturnStringBuilder(ref ContentBuilder);
                ReturnStringBuilder(ref LineBuilder);

                TagDelimiters.BlockParser = null;

                Cache = null;
                PrefixPriorities = null;
                Rules = null;

                CurrentState = BlockState.NotStarted;
                Position = default;
                PositionInline = false;
                LineCount = -1;
                TempFlags = 0;
                Error = false;
                BlockError = false;

                PositionStack.Clear();
            }
        }

        protected struct PositionFrame
        {
            public readonly BlockFilePosition Position;
            public readonly bool IsInline;
            public readonly int LineCount;

            public PositionFrame(BlockFilePosition inPosition, bool inbInline, int inLineCount)
            {
                Position = inPosition;
                IsInline = inbInline;
                LineCount = inLineCount;
            }
        }

        static private void InitParseBuffer(ParseBuffer inBuffer, IBlockParsingRules inRules, BlockMetaCache inCache)
        {
            inBuffer.Buffer = RentLineBuffer();
            inBuffer.Builder = RentStringBuilder();
            inBuffer.ContentBuilder = RentStringBuilder();
            inBuffer.LineBuilder = RentStringBuilder();
            inBuffer.TagDelimiters.BlockParser = inRules;
            inBuffer.Rules = inRules;
            inBuffer.PrefixPriorities = CachePrefixPriorities(inRules);
            inBuffer.Cache = inCache ?? BlockMetaCache.Default;
        }

        static private readonly Stack<ParseBuffer> s_ParseBufferPool = new Stack<ParseBuffer>(4);

        static protected ParseBuffer RentParseBuffer(IBlockParsingRules inRules, BlockMetaCache inCache)
        {
            ParseBuffer buffer;
            if (s_ParseBufferPool.Count > 0)
            {
                buffer = s_ParseBufferPool.Pop();
            }
            else
            {
                buffer = new ParseBuffer();
            }
            InitParseBuffer(buffer, inRules, inCache);
            return buffer;
        }

        static protected void ReturnParseBuffer(ref ParseBuffer ioBuffer)
        {
            if (ioBuffer == null)
                return;

            #if DEVELOPMENT
            if (s_ParseBufferPool.Contains(ioBuffer))
                throw new ArgumentException("ioBuffer");
            #endif // DEVELOPMENT

            ioBuffer.Dispose();
            s_ParseBufferPool.Push(ioBuffer);
            ioBuffer = null;
        }

        #endregion // Parse Buffer

        #region Shared Data Types

        protected enum PrefixType : byte
        {
            BlockId,
            BlockMeta,
            BlockHeaderEnd,
            BlockEnd,
            PackageMeta,

            NONE = 255
        }

        protected enum BlockState : byte
        {
            NotStarted,
            BlockStarted,
            InHeader,
            InData,
            BlockDone
        }

        protected enum LineResult : byte
        {
            NoError,
            Empty,
            Error,
            Exception,
            Continue
        }

        #endregion // Shared Data Types
    }
}