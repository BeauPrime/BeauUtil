/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    24 August 2020
 * 
 * File:    BlockParser.cs
 * Purpose: Parser for generating blocks of data.
 */

#if (UNITY_EDITOR && !IGNORE_UNITY_EDITOR) || DEVELOPMENT_BUILD || DEBUG || DEVELOPMENT
#define ENABLE_LOGGING_BEAUUTIL
#endif // (UNITY_EDITOR && !IGNORE_UNITY_EDITOR) || DEVELOPMENT_BUILD || DEBUG || DEVELOPMENT

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using BeauUtil.Streaming;
using BeauUtil.Tags;
using UnityEngine;

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
        static public TPackage Parse<TBlock, TPackage>(CharStreamParams inStream, IBlockParsingRules inRules, IBlockGenerator<TBlock, TPackage> inGenerator, BlockMetaCache inCache = null)
            where TBlock : class, IDataBlock
            where TPackage : class, IDataBlockPackage<TBlock>
        {
            IEnumerator parser;
            TPackage package = ParseAsync(inStream, inRules, inGenerator, inCache, out parser);
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
        static public void Parse<TBlock, TPackage>(ref TPackage ioPackage, CharStreamParams inStream, IBlockParsingRules inRules, IBlockGenerator<TBlock, TPackage> inGenerator, BlockMetaCache inCache = null)
            where TBlock : class, IDataBlock
            where TPackage : class, IDataBlockPackage<TBlock>
        {
            IEnumerator parser = ParseAsync(ref ioPackage, inStream, inRules, inGenerator, inCache);
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
        /// Each MoveNext() call on the returned IEnumerator will parse one block of data.
        /// </summary>
        static public TPackage ParseAsync<TBlock, TPackage>(CharStreamParams inStream, IBlockParsingRules inRules, IBlockGenerator<TBlock, TPackage> inGenerator, out IEnumerator outLoader)
            where TBlock : class, IDataBlock
            where TPackage : class, IDataBlockPackage<TBlock>
        {
            return ParseAsync<TBlock, TPackage>(inStream, inRules, inGenerator, null, out outLoader);
        }

        /// <summary>
        /// Parses the given file contents into blocks asynchronously.
        /// Each MoveNext() call on the returned IEnumerator will parse one block of data.
        /// </summary>
        static public TPackage ParseAsync<TBlock, TPackage>(CharStreamParams inStream, IBlockParsingRules inRules, IBlockGenerator<TBlock, TPackage> inGenerator, BlockMetaCache inCache, out IEnumerator outLoader)
            where TBlock : class, IDataBlock
            where TPackage : class, IDataBlockPackage<TBlock>
        {
            if (inStream.Type == 0)
            {
                outLoader = null;
                return null;
            }

            if (inRules == null)
                throw new ArgumentNullException("inRules");
            if (inGenerator == null)
                throw new ArgumentNullException("inGenerator");

            string fileName = string.IsNullOrEmpty(inStream.Name) ? NullFilename : inStream.Name;
            TPackage package = inGenerator.CreatePackage(fileName);

            outLoader = InternalBlockParser<TBlock, TPackage>.ParseFile(fileName, inStream, package, inRules, inGenerator, inCache);
            return package;
        }

        /// <summary>
        /// Parses the given file contents into blocks asynchronously
        /// and merges into the given package.
        /// Each MoveNext() call on the returned IEnumerator will parse one block of data.
        /// </summary>
        static public IEnumerator ParseAsync<TBlock, TPackage>(ref TPackage ioPackage, CharStreamParams inStream, IBlockParsingRules inRules, IBlockGenerator<TBlock, TPackage> inGenerator, BlockMetaCache inCache = null)
            where TBlock : class, IDataBlock
            where TPackage : class, IDataBlockPackage<TBlock>
        {
            if (inStream.Type == 0)
            {
                return null;
            }

            if (inRules == null)
                throw new ArgumentNullException("inRules");
            if (inGenerator == null)
                throw new ArgumentNullException("inGenerator");

            string fileName = string.IsNullOrEmpty(inStream.Name) ? NullFilename : inStream.Name;
            if (ioPackage == null)
            {
                ioPackage = inGenerator.CreatePackage(fileName);
            }

            return InternalBlockParser<TBlock, TPackage>.ParseFile(fileName, inStream, ioPackage, inRules, inGenerator, inCache);
        }

        static protected int s_BlockSize = 64;

        /// <summary>
        /// Gets/sets the parsing block size.
        /// This should be a multiple of 32.
        /// </summary>
        static public uint BlockSize
        {
            get { return (uint) s_BlockSize; }
            set { s_BlockSize = (int) Unsafe.AlignUp32(value); }
        }

        #endregion // Parse

        #region Parse Shortcuts

        /// <summary>
        /// Parses the given file contents into blocks.
        /// </summary>
        [MethodImpl(256)]
        static public TPackage Parse<TBlock, TPackage>(CustomTextAsset inAsset, IBlockParsingRules inRules, IBlockGenerator<TBlock, TPackage> inGenerator, BlockMetaCache inCache = null)
            where TBlock : class, IDataBlock
            where TPackage : class, IDataBlockPackage<TBlock>
        {
            return Parse(CharStreamParams.FromCustomTextAsset(inAsset), inRules, inGenerator, inCache);
        }

        /// <summary>
        /// Parses the given file contents into blocks
        /// and merges into the given package.
        /// </summary>
        [MethodImpl(256)]
        static public void Parse<TBlock, TPackage>(ref TPackage ioPackage, CustomTextAsset inAsset, IBlockParsingRules inRules, IBlockGenerator<TBlock, TPackage> inGenerator, BlockMetaCache inCache = null)
            where TBlock : class, IDataBlock
            where TPackage : class, IDataBlockPackage<TBlock>
        {
            Parse(ref ioPackage, CharStreamParams.FromCustomTextAsset(inAsset), inRules, inGenerator, inCache);
        }

        /// <summary>
        /// Parses the given file contents into blocks asynchronously.
        /// Each MoveNext() call on the returned IEnumerator will parse one block of data.
        /// </summary>
        [MethodImpl(256)]
        static public TPackage ParseAsync<TBlock, TPackage>(CustomTextAsset inAsset, IBlockParsingRules inRules, IBlockGenerator<TBlock, TPackage> inGenerator, out IEnumerator outLoader)
            where TBlock : class, IDataBlock
            where TPackage : class, IDataBlockPackage<TBlock>
        {
            return ParseAsync(CharStreamParams.FromCustomTextAsset(inAsset), inRules, inGenerator, out outLoader);
        }

        /// <summary>
        /// Parses the given file contents into blocks asynchronously.
        /// Each MoveNext() call on the returned IEnumerator will parse one block of data.
        /// </summary>
        [MethodImpl(256)]
        static public TPackage ParseAsync<TBlock, TPackage>(CustomTextAsset inAsset, IBlockParsingRules inRules, IBlockGenerator<TBlock, TPackage> inGenerator, BlockMetaCache inCache, out IEnumerator outLoader)
            where TBlock : class, IDataBlock
            where TPackage : class, IDataBlockPackage<TBlock>
        {
            return ParseAsync(CharStreamParams.FromCustomTextAsset(inAsset), inRules, inGenerator, inCache, out outLoader);
        }

        /// <summary>
        /// Parses the given file contents into blocks asynchronously
        /// and merges into the given package.
        /// Each MoveNext() call on the returned IEnumerator will parse one block of data.
        /// </summary>
        [MethodImpl(256)]
        static public IEnumerator ParseAsync<TBlock, TPackage>(ref TPackage ioPackage, CustomTextAsset inAsset, IBlockParsingRules inRules, IBlockGenerator<TBlock, TPackage> inGenerator, BlockMetaCache inCache = null)
            where TBlock : class, IDataBlock
            where TPackage : class, IDataBlockPackage<TBlock>
        {
            return ParseAsync(ref ioPackage, CharStreamParams.FromCustomTextAsset(inAsset), inRules, inGenerator, inCache);
        }

        /// <summary>
        /// Parses the given file contents into blocks.
        /// </summary>
        [MethodImpl(256)]
        static public TPackage Parse<TBlock, TPackage>(TextAsset inAsset, IBlockParsingRules inRules, IBlockGenerator<TBlock, TPackage> inGenerator, BlockMetaCache inCache = null)
            where TBlock : class, IDataBlock
            where TPackage : class, IDataBlockPackage<TBlock>
        {
            return Parse(CharStreamParams.FromTextAsset(inAsset), inRules, inGenerator, inCache);
        }

        /// <summary>
        /// Parses the given file contents into blocks
        /// and merges into the given package.
        /// </summary>
        [MethodImpl(256)]
        static public void Parse<TBlock, TPackage>(ref TPackage ioPackage, TextAsset inAsset, IBlockParsingRules inRules, IBlockGenerator<TBlock, TPackage> inGenerator, BlockMetaCache inCache = null)
            where TBlock : class, IDataBlock
            where TPackage : class, IDataBlockPackage<TBlock>
        {
            Parse(ref ioPackage, CharStreamParams.FromTextAsset(inAsset), inRules, inGenerator, inCache);
        }

        /// <summary>
        /// Parses the given file contents into blocks asynchronously.
        /// Each MoveNext() call on the returned IEnumerator will parse one block of data.
        /// </summary>
        [MethodImpl(256)]
        static public TPackage ParseAsync<TBlock, TPackage>(TextAsset inAsset, IBlockParsingRules inRules, IBlockGenerator<TBlock, TPackage> inGenerator, out IEnumerator outLoader)
            where TBlock : class, IDataBlock
            where TPackage : class, IDataBlockPackage<TBlock>
        {
            return ParseAsync(CharStreamParams.FromTextAsset(inAsset), inRules, inGenerator, out outLoader);
        }

        /// <summary>
        /// Parses the given file contents into blocks asynchronously.
        /// Each MoveNext() call on the returned IEnumerator will parse one block of data.
        /// </summary>
        [MethodImpl(256)]
        static public TPackage ParseAsync<TBlock, TPackage>(TextAsset inAsset, IBlockParsingRules inRules, IBlockGenerator<TBlock, TPackage> inGenerator, BlockMetaCache inCache, out IEnumerator outLoader)
            where TBlock : class, IDataBlock
            where TPackage : class, IDataBlockPackage<TBlock>
        {
            return ParseAsync(CharStreamParams.FromTextAsset(inAsset), inRules, inGenerator, inCache, out outLoader);
        }

        /// <summary>
        /// Parses the given file contents into blocks asynchronously
        /// and merges into the given package.
        /// Each MoveNext() call on the returned IEnumerator will parse one block of data.
        /// </summary>
        [MethodImpl(256)]
        static public IEnumerator ParseAsync<TBlock, TPackage>(ref TPackage ioPackage, TextAsset inAsset, IBlockParsingRules inRules, IBlockGenerator<TBlock, TPackage> inGenerator, BlockMetaCache inCache = null)
            where TBlock : class, IDataBlock
            where TPackage : class, IDataBlockPackage<TBlock>
        {
            return ParseAsync(ref ioPackage, CharStreamParams.FromTextAsset(inAsset), inRules, inGenerator, inCache);
        }

        /// <summary>
        /// Parses the given stream contents into blocks.
        /// The stream will be disposed once parsing is complete.
        /// </summary>
        [MethodImpl(256)]
        static public TPackage Parse<TBlock, TPackage>(Stream inAsset, IBlockParsingRules inRules, IBlockGenerator<TBlock, TPackage> inGenerator, BlockMetaCache inCache = null)
            where TBlock : class, IDataBlock
            where TPackage : class, IDataBlockPackage<TBlock>
        {
            return Parse(CharStreamParams.FromStream(inAsset, null), inRules, inGenerator, inCache);
        }

        /// <summary>
        /// Parses the given stream contents into blocks
        /// and merges into the given package.
        /// The stream will be disposed once parsing is complete.
        /// </summary>
        [MethodImpl(256)]
        static public void Parse<TBlock, TPackage>(ref TPackage ioPackage, Stream inAsset, IBlockParsingRules inRules, IBlockGenerator<TBlock, TPackage> inGenerator, BlockMetaCache inCache = null)
            where TBlock : class, IDataBlock
            where TPackage : class, IDataBlockPackage<TBlock>
        {
            Parse(ref ioPackage, CharStreamParams.FromStream(inAsset, null), inRules, inGenerator, inCache);
        }

        /// <summary>
        /// Parses the given stream contents into blocks asynchronously.
        /// Each MoveNext() call on the returned IEnumerator will parse one block of data.
        /// The stream will be disposed once parsing is complete.
        /// </summary>
        [MethodImpl(256)]
        static public TPackage ParseAsync<TBlock, TPackage>(Stream inAsset, IBlockParsingRules inRules, IBlockGenerator<TBlock, TPackage> inGenerator, out IEnumerator outLoader)
            where TBlock : class, IDataBlock
            where TPackage : class, IDataBlockPackage<TBlock>
        {
            return ParseAsync(CharStreamParams.FromStream(inAsset, null), inRules, inGenerator, out outLoader);
        }

        /// <summary>
        /// Parses the given stream contents into blocks asynchronously.
        /// Each MoveNext() call on the returned IEnumerator will parse one block of data.
        /// The stream will be disposed once parsing is complete.
        /// </summary>
        [MethodImpl(256)]
        static public TPackage ParseAsync<TBlock, TPackage>(Stream inAsset, IBlockParsingRules inRules, IBlockGenerator<TBlock, TPackage> inGenerator, BlockMetaCache inCache, out IEnumerator outLoader)
            where TBlock : class, IDataBlock
            where TPackage : class, IDataBlockPackage<TBlock>
        {
            return ParseAsync(CharStreamParams.FromStream(inAsset, null), inRules, inGenerator, inCache, out outLoader);
        }

        /// <summary>
        /// Parses the given stream contents into blocks asynchronously
        /// and merges into the given package.
        /// Each MoveNext() call on the returned IEnumerator will parse one block of data.
        /// The stream will be disposed once parsing is complete.
        /// </summary>
        [MethodImpl(256)]
        static public IEnumerator ParseAsync<TBlock, TPackage>(ref TPackage ioPackage, Stream inAsset, IBlockParsingRules inRules, IBlockGenerator<TBlock, TPackage> inGenerator, BlockMetaCache inCache = null)
            where TBlock : class, IDataBlock
            where TPackage : class, IDataBlockPackage<TBlock>
        {
            return ParseAsync(ref ioPackage, CharStreamParams.FromStream(inAsset, null), inRules, inGenerator, inCache);
        }

        #endregion // Parse Shortcuts

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

        [Conditional("ENABLE_LOGGING_BEAUUTIL")]
        static protected void LogError(BlockFilePosition inPosition, string inContent)
        {
            UnityEngine.Debug.LogErrorFormat("[BlockParser] Parsing Error at {0}: {1}", inPosition, inContent);
        }

        [Conditional("ENABLE_LOGGING_BEAUUTIL")]
        static protected void LogError(BlockFilePosition inPosition, string inContent, object inParam)
        {
            UnityEngine.Debug.LogErrorFormat("[BlockParser] Parsing Error at {0}: {1}", inPosition, string.Format(inContent, inParam));
        }

        [Conditional("ENABLE_LOGGING_BEAUUTIL")]
        static protected void LogError(BlockFilePosition inPosition, string inContent, params object[] inParams)
        {
            UnityEngine.Debug.LogErrorFormat("[BlockParser] Parsing Error at {0}: {1}", inPosition, string.Format(inContent, inParams));
        }

        #endregion // Logging

        #region Delimiters

        static private Dictionary<IBlockParsingRules, PrefixPriority> s_PrefixPriorityCache;

        static private PrefixPriority CachePrefixPriorities(IBlockParsingRules inRules)
        {
            PrefixPriority prefixes = default;
            if (s_PrefixPriorityCache != null && s_PrefixPriorityCache.TryGetValue(inRules, out prefixes))
                return prefixes;

            s_PrefixPriorityCache = s_PrefixPriorityCache ?? new Dictionary<IBlockParsingRules, PrefixPriority>();
            prefixes = GeneratePrefixPriority(inRules);
            s_PrefixPriorityCache.Add(inRules, prefixes);
            return prefixes;
        }

        // Generates a prioritized set of prefixes to analyze, from most specific to least specific
        static private unsafe PrefixPriority GeneratePrefixPriority(IBlockParsingRules inRules)
        {
            PrioritizedPrefix* buffer = stackalloc PrioritizedPrefix[4];
            int length = 0;
            AddPrefixPriority(buffer, inRules.BlockIdPrefix, PrefixType.BlockId, ref length);
            AddPrefixPriority(buffer, inRules.BlockMetaPrefix, PrefixType.BlockMeta, ref length);
            AddPrefixPriority(buffer, inRules.BlockEndPrefix, PrefixType.BlockEnd, ref length);
            AddPrefixPriority(buffer, inRules.PackageMetaPrefix, PrefixType.PackageMeta, ref length);
            Unsafe.Quicksort<PrioritizedPrefix>(buffer, length, PrioritizedPrefix.Compare);

            PrefixPriority priority = default;
            for(int i = 0; i < length; i++)
            {
                priority[i] = buffer[i].Type;
            }
            return priority;
        }

        private struct PrioritizedPrefix
        {
            public PrefixType Type;
            public byte Length;

            public PrioritizedPrefix(PrefixType inType, int inLength)
            {
                Type = inType;
                Length = (byte) inLength;
            }

            static public int Compare(PrioritizedPrefix x, PrioritizedPrefix y)
            {
                return x.Length > y.Length ? -1 : (x.Length < y.Length ? 1 : 0);
            }
        }

        // Adds a prefix priority to the buffer
        static private unsafe void AddPrefixPriority(PrioritizedPrefix* ioBuffer, string inString, PrefixType inPrefix, ref int ioLength)
        {
            if (!string.IsNullOrEmpty(inString))
            {
                ioBuffer[ioLength++] = new PrioritizedPrefix(inPrefix, inString.Length);
            }
        }

        static protected readonly char[] TrimCharsWithSpace = new char[]
        {
            ' ', '\n', '\r', '\t', '\f', '\0'
        };

        static protected readonly char[] TrimLeadingChars = new char[]
        {
            ' ', '\r', '\t', '\f', '\0'
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

        static protected unsafe bool MatchLookBack(StringBuilder inBuilder, char* inBuffer, char* inBufferEnd, string inMatch, out int outBuilderOverlap, out int outBufferOverlap)
        {
            int matchLength = inMatch.Length;
            int builderLength = inBuilder.Length;
            int bufferLength = (int) (inBufferEnd - inBuffer) + 1;

            outBuilderOverlap = 0;
            outBufferOverlap = 0;
            if (builderLength + bufferLength < matchLength)
            {
                return false;
            }

            int bufferMatch = Math.Min(matchLength, bufferLength);
            int builderMatch = matchLength - bufferMatch;
            outBufferOverlap = bufferMatch;
            outBuilderOverlap = builderMatch;

            fixed(char* matchChars = inMatch)
            {
                char* matchEnd = matchChars + matchLength - 1;
                while(bufferMatch > 0)
                {
                    if (*inBufferEnd != *matchEnd)
                        return false;
                    
                    inBufferEnd--;
                    matchEnd--;
                    bufferMatch--;
                }

                int builderEnd = builderLength - 1;
                while(builderMatch > 0)
                {
                    if (inBuilder[builderEnd] != *matchEnd)
                        return false;

                    builderEnd--;
                    matchEnd--;
                    builderMatch--;
                }

                return true;
            }
        }

        #endregion // String Builder

        #region Parse Buffer

        private const int MaxNestingDepth = 8;

        protected sealed class ParseBuffer : IDisposable
        {
            public readonly DelimiterRules TagDelimiters = new DelimiterRules();
            
            public readonly PositionFrame[] PositionStack = new PositionFrame[MaxNestingDepth];
            public readonly CharStream[] StreamStack = new CharStream[MaxNestingDepth];
            public readonly byte[] UnpackBuffer = new byte[64];
            public readonly char[] LeftoverBuffer = new char[512];

            public BlockMetaCache Cache;
            public PrefixPriority PrefixPriorities;
            public IBlockParsingRules Rules;

            public BlockState CurrentState;
            public ParseStateFlags ParseFlags;
            public BlockFilePosition Position;
            public bool PositionInline;
            public int LeftoverCount;
            public int StackOffset = -1;

            public StringBuilder Builder;
            public StringBuilder LineBuilder;
            public StringBuilder ContentBuilder;
            public CharStream LeftoverStream;
            public uint TempFlags;

            public bool Error;
            public bool BlockError;

            public void PushStream(CharStreamParams inStream, string inFileName = null)
            {
                StackOffset++;
                PositionStack[StackOffset] = new PositionFrame(Position, PositionInline, LeftoverCount);
                StreamStack[StackOffset].LoadParams(inStream, UnpackBuffer);
                ParseFlags |= ParseStateFlags.EarlyBreakForInsertion;
                if (string.IsNullOrEmpty(inFileName))
                {
                    PositionInline = true;
                }
                else
                {
                    PositionInline = false;
                    Position = new BlockFilePosition(inFileName, inStream.FilePath, 0);
                }

                ParseFlags |= ParseStateFlags.SkipWhitespace;
                ParseFlags &= ~ParseStateFlags.InComment;
                LeftoverCount = 0;
            }

            public void PopStream()
            {
                PositionFrame oldState = PositionStack[StackOffset];
                Position = oldState.Position;
                PositionInline = oldState.IsInline;
                LeftoverCount = oldState.LeftoverCount;
                StreamStack[StackOffset].Dispose();
                StackOffset--;

                ParseFlags |= ParseStateFlags.SkipWhitespace;
                ParseFlags &= ~ParseStateFlags.InComment;
                ParseFlags &= ~ParseStateFlags.EarlyBreakForInsertion;
            }

            public void Dispose()
            {
                ReturnStringBuilder(ref Builder);
                ReturnStringBuilder(ref LineBuilder);
                ReturnStringBuilder(ref ContentBuilder);
                LeftoverStream.Dispose();

                Cache = null;
                PrefixPriorities = default;
                Rules = null;
                ParseFlags = 0;

                CurrentState = BlockState.NotStarted;
                Position = default;
                PositionInline = false;
                TempFlags = 0;
                Error = false;
                BlockError = false;
                LeftoverCount = 0;
                
                while(StackOffset >= 0)
                {
                    PositionStack[StackOffset] = default;
                    StreamStack[StackOffset].Dispose();
                    StackOffset--;
                }
            }
        }

        [Flags]
        protected enum ParseStateFlags : uint
        {
            SkipWhitespace = 0x01,
            InComment = 0x02,
            EarlyBreakForInsertion = 0x04,
        }

        protected struct PositionFrame
        {
            public readonly BlockFilePosition Position;
            public readonly int LeftoverCount;
            public readonly bool IsInline;

            public PositionFrame(BlockFilePosition inPosition, bool inbInline, int inLeftoverCount)
            {
                Position = inPosition;
                LeftoverCount = inLeftoverCount;
                IsInline = inbInline;
            }
        }

        static private void InitParseBuffer(ParseBuffer ioBuffer, IBlockParsingRules inRules, BlockMetaCache inCache)
        {
            ioBuffer.Builder = RentStringBuilder();
            ioBuffer.LineBuilder = RentStringBuilder();
            ioBuffer.ContentBuilder = RentStringBuilder();
            ioBuffer.TagDelimiters.TagDataDelimiters = inRules.TagDataDelimiters;
            ioBuffer.Rules = inRules;
            ioBuffer.PrefixPriorities = CachePrefixPriorities(inRules);
            ioBuffer.Cache = inCache ?? BlockMetaCache.Default;
            ioBuffer.LeftoverStream.LoadCharBuffer(ioBuffer.LeftoverBuffer);
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
            NONE = 0,
            BlockId,
            BlockMeta,
            BlockHeaderEnd,
            BlockEnd,
            PackageMeta,
        }

        protected struct PrefixPriority
        {
            public uint Packed;

            public PrefixType this[int index]
            {
                get
                {
                    return (PrefixType) ((Packed >> (index * 8)) & 0xFF);
                }
                set
                {
                    Packed |= (((uint) value) & 0xFF) << (index * 8);
                }
            }

            public override int GetHashCode()
            {
                return (int) Packed;
            }
            
            public const int Length = 4;
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