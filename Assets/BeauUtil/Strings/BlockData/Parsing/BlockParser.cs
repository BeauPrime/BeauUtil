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

namespace BeauUtil.Blocks
{
    static public class BlockParser
    {
        static public readonly string NullFilename = "<anonymous>";

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
                while (parser.MoveNext()) ;
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
                while (parser.MoveNext()) ;
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

        #region Logging

        [Conditional("DEVELOPMENT")]
        static internal void LogError(BlockFilePosition inPosition, string inContent)
        {
            UnityEngine.Debug.LogErrorFormat("[BlockSetParser] Parsing Error at {0}: {1}", inPosition, inContent);
        }

        [Conditional("DEVELOPMENT")]
        static internal void LogError(BlockFilePosition inPosition, string inContent, object inParam)
        {
            UnityEngine.Debug.LogErrorFormat("[BlockSetParser] Parsing Error at {0}: {1}", inPosition, string.Format(inContent, inParam));
        }

        [Conditional("DEVELOPMENT")]
        static internal void LogError(BlockFilePosition inPosition, string inContent, params object[] inParams)
        {
            UnityEngine.Debug.LogErrorFormat("[BlockSetParser] Parsing Error at {0}: {1}", inPosition, string.Format(inContent, inParams));
        }

        #endregion // Logging

        #region Delimiters

        internal class BlockTagDelimiters : IDelimiterRules
        {
            private IBlockParsingRules m_BlockParser;

            public BlockTagDelimiters(IBlockParsingRules inBlockParser)
            {
                m_BlockParser = inBlockParser;
            }

            #region IDelimiterRules

            public string TagStartDelimiter { get { return null; } }

            public string TagEndDelimiter { get { return null; } }

            public char[] TagDataDelimiters { get { return m_BlockParser.TagDataDelimiters; } }

            public char RegionCloseDelimiter { get { return '\0'; } }

            public bool RichText { get { return false; } }

            public IEnumerable<string> AdditionalRichTextTags { get { return null; } }

            #endregion // IDelimiterRules
        }

        static internal readonly char[] TrimCharsWithSpace = new char[]
        {
            ' ', '\n', '\r', '\t', '\f', '\0'
        };

        static internal readonly char[] TrimCharsWithoutSpace = new char[]
        {
            '\n', '\r', '\t', '\f', '\0'
        };

        #endregion // Delimiters
    
        #region String Builder

        static private readonly Stack<StringBuilder> s_StringBuilderPool = new Stack<StringBuilder>(4);

        static internal StringBuilder RentStringBuilder()
        {
            if (s_StringBuilderPool.Count > 0)
            {
                StringBuilder sb = s_StringBuilderPool.Pop();
                sb.Length = 0;
                return sb;
            }

            return new StringBuilder(256);
        }

        static internal void ReturnStringBuilder(StringBuilder inBuilder)
        {
            #if DEVELOPEMNT
            if (inBuilder == null || s_StringBuilderPool.Contains(inBuilder))
                throw new ArgumentException("inBuilder");
            #endif // DEVELOPMENT

            inBuilder.Length = 0;
            s_StringBuilderPool.Push(inBuilder);
        }

        #endregion // String Builder
    }
}