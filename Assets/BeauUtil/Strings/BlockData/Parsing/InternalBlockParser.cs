/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    24 August 2020
 * 
 * File:    InternalBlockParser.cs
 * Purpose: Parser with generics in header.
 */

#if UNITY_EDITOR || DEVELOPMENT_BUILD || DEBUG
#define DEVELOPMENT
#endif // DEVELOPMENT

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using BeauUtil.Tags;
using UnityEngine;

namespace BeauUtil.Blocks
{
    static internal class InternalBlockParser<TBlock, TPackage>
        where TBlock : class, IDataBlock
        where TPackage : class, IDataBlockPackage<TBlock>
    {
        #region Types

        private enum PrefixType : byte
        {
            BlockId,
            BlockMeta,
            BlockHeaderEnd,
            BlockContent,
            BlockEnd,
            PackageMeta,
        }

        private enum BlockState : byte
        {
            NotStarted,
            BlockStarted,
            InHeader,
            InData,
            BlockDone
        }

        private enum LineResult : byte
        {
            NoError,
            Empty,
            Error,
            Exception
        }

        private class ParseState : IBlockParserUtil, IDisposable
        {
            public PrefixType[] PrefixPriorities;
            public IBlockParsingRules Rules;
            public IBlockGenerator<TBlock, TPackage> Generator;
            public IDelimiterRules TagDelimiters;
            public BlockMetaCache Cache;

            public TPackage Package;
            public TBlock CurrentBlock;
            public BlockState CurrentState;
            public BlockFilePosition Position;

            public StringBuilder Builder;
            public StringBuilder ContentBuilder;
            public uint TempFlags;

            public bool Error;
            public bool BlockError;

            public StringBuilder TempBuilder { get { return Builder; } }

            BlockFilePosition IBlockParserUtil.Position { get { return Position; } }

            uint IBlockParserUtil.TempFlags { get { return TempFlags; } set { TempFlags = value; } }

            public void Dispose()
            {
                if (Builder != null)
                {
                    BlockParser.ReturnStringBuilder(Builder);
                    Builder = null;
                }

                if (ContentBuilder != null)
                {
                    BlockParser.ReturnStringBuilder(ContentBuilder);
                    ContentBuilder = null;
                }

                PrefixPriorities = null;
                Rules = null;
                Generator = null;
                TagDelimiters = null;
                Cache = null;

                Package = null;
                CurrentBlock = null;
            }
        }

        #endregion // Types

        #region Parse

        static internal IEnumerator ParseFile(string inFileName, StringSlice inFile, TPackage ioPackage, IBlockParsingRules inRules, IBlockGenerator<TBlock, TPackage> inGenerator, BlockMetaCache inCache)
        {
            var state = new ParseState();
            state.PrefixPriorities = GeneratePrefixPriority(inRules);
            state.Rules = inRules;
            state.Generator = inGenerator;
            state.TagDelimiters = new BlockParser.BlockTagDelimiters(inRules);
            state.Package = ioPackage;
            state.Builder = BlockParser.RentStringBuilder();
            state.ContentBuilder = BlockParser.RentStringBuilder();
            state.Cache = inCache ?? BlockMetaCache.Default;

            using(var disposeRef = state)
            {
                uint lineNumber = 0;
                state.Position = new BlockFilePosition(inFileName, lineNumber);
                inGenerator.OnStart(state, state.Package);

                foreach (var rawLine in SplitIntoLines(inRules, inFile))
                {
                    lineNumber++;
                    state.Position = new BlockFilePosition(inFileName, lineNumber);

                    LineResult result = ParseLine(ref state, rawLine);
                    if (result == LineResult.Exception)
                    {
                        inGenerator.OnEnd(state, state.Package, true);
                        yield break;
                    }
                    
                    yield return null;
                }

                if (state.CurrentBlock != null)
                {
                    state.Error |= !TryEndBlock(ref state, StringSlice.Empty);
                }

                inGenerator.OnEnd(state, state.Package, state.Error);
            }
        }

        static private LineResult ParseLine(ref ParseState ioState, StringSlice ioLine)
        {
            StringSlice lineContents = ioLine.TrimStart(BlockParser.TrimCharsWithSpace).TrimEnd(BlockParser.TrimCharsWithoutSpace);
            StringSlice lineComment = StringSlice.Empty;

            if (lineContents.IsEmpty)
            {
                if (ioState.CurrentState != BlockState.InData)
                    return LineResult.Empty;
            }

            int commentIdx = lineContents.IndexOf(ioState.Rules.CommentPrefix);
            if (commentIdx >= 0)
            {
                lineComment = lineContents.Substring(commentIdx + ioState.Rules.CommentPrefix.Length).TrimStart(BlockParser.TrimCharsWithSpace);
                lineContents = lineContents.Substring(0, commentIdx).TrimEnd(BlockParser.TrimCharsWithSpace);
            }

            try
            {
                bool bSuccess = true;
                bool bProcessedCommand = false;
                
                if (!lineContents.IsEmpty)
                {
                    for (int i = 0; i < ioState.PrefixPriorities.Length && !bProcessedCommand; ++i)
                    {
                        PrefixType type = ioState.PrefixPriorities[i];
                        switch (type)
                        {
                            case PrefixType.BlockId:
                                {
                                    if (lineContents.StartsWith(ioState.Rules.BlockIdPrefix))
                                    {
                                        lineContents = lineContents.Substring(ioState.Rules.BlockIdPrefix.Length);
                                        bSuccess &= TryStartBlock(ref ioState, lineContents);
                                        bProcessedCommand = true;
                                    }
                                    break;
                                }

                            case PrefixType.BlockMeta:
                                {
                                    if (ShouldCheckMeta(ref ioState) && lineContents.StartsWith(ioState.Rules.BlockMetaPrefix))
                                    {
                                        lineContents = lineContents.Substring(ioState.Rules.BlockMetaPrefix.Length);
                                        bSuccess &= TryEvaluateMeta(ref ioState, lineContents);
                                        bProcessedCommand = true;
                                    }
                                    break;
                                }

                            case PrefixType.BlockHeaderEnd:
                                {
                                    if (lineContents.StartsWith(ioState.Rules.BlockHeaderEndPrefix))
                                    {
                                        lineContents = lineContents.Substring(ioState.Rules.BlockHeaderEndPrefix.Length);
                                        bSuccess &= TryEndHeader(ref ioState, lineContents);
                                        bProcessedCommand = true;
                                    }
                                    break;
                                }

                            case PrefixType.BlockContent:
                                {
                                    if (lineContents.StartsWith(ioState.Rules.BlockContentPrefix))
                                    {
                                        lineContents = lineContents.Substring(ioState.Rules.BlockContentPrefix.Length);
                                        bSuccess &= TryAddContent(ref ioState, lineContents);
                                        bProcessedCommand = true;
                                    }
                                    break;
                                }

                            case PrefixType.BlockEnd:
                                {
                                    if (lineContents.StartsWith(ioState.Rules.BlockEndPrefix))
                                    {
                                        lineContents = lineContents.Substring(ioState.Rules.BlockEndPrefix.Length);
                                        bSuccess &= TryEndBlock(ref ioState, lineContents);
                                        bProcessedCommand = true;
                                    }
                                    break;
                                }

                            case PrefixType.PackageMeta:
                                {
                                    if (ShouldCheckPackageMeta(ref ioState) && lineContents.StartsWith(ioState.Rules.PackageMetaPrefix))
                                    {
                                        lineContents = lineContents.Substring(ioState.Rules.PackageMetaPrefix.Length);
                                        bSuccess &= TryEvaluatePackage(ref ioState, lineContents);
                                        bProcessedCommand = true;
                                    }
                                    break;
                                }
                        }
                    }
                }

                if (!bProcessedCommand)
                {
                    if (ioState.CurrentState == BlockState.InData && ioState.Rules.RequireExplicitBlockContent && !string.IsNullOrEmpty(ioState.Rules.BlockContentPrefix))
                    {
                        BlockParser.LogError(ioState.Position, "Cannot add content '{0}', must have content prefix '{1}'", lineContents, ioState.Rules.BlockContentPrefix);
                        bSuccess = false;
                    }
                    else if (!lineContents.IsEmpty || ioState.CurrentState == BlockState.InData)
                    {
                        bSuccess &= TryAddContent(ref ioState, lineContents);
                    }
                }

                if (!lineComment.IsEmpty)
                {
                    bSuccess &= TryAddComment(ref ioState, lineComment);
                }

                ioState.Error |= !bSuccess;
                return bSuccess ? LineResult.Error : LineResult.NoError;
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogException(e);
                return LineResult.Exception;
            }
        }

        #endregion // Parse

        #region Checks

        static private bool ShouldCheckMeta(ref ParseState ioState)
        {
            switch(ioState.CurrentState)
            {
                case BlockState.InData:
                    return false;

                default:
                    return true;
            }
        }

        static private bool ShouldCheckPackageMeta(ref ParseState ioState)
        {
            switch(ioState.CurrentState)
            {
                case BlockState.BlockDone:
                case BlockState.NotStarted:
                    return true;

                default:
                    return ioState.Rules.PackageMetaMode > PackageMetaMode.DisallowInBlock;
            }
        }

        #endregion // Checks

        #region Parse Commands

        // Attempts to start a new data block
        static private bool TryStartBlock(ref ParseState ioState, StringSlice inLine)
        {
            TagData data = TagData.Parse(inLine, ioState.TagDelimiters);
            if (data.IsEmpty())
            {
                BlockParser.LogError(ioState.Position, "Cannot start block with an empty id '{0}'", inLine);
                return false;
            }

            switch (ioState.CurrentState)
            {
                case BlockState.NotStarted:
                    {
                        ioState.Generator.OnBlocksStart(ioState, ioState.Package);
                        break;
                    }

                case BlockState.InHeader:
                    {
                        if (ioState.Rules.RequireExplicitBlockEnd || ioState.Rules.RequireExplicitBlockHeaderEnd)
                        {
                            BlockParser.LogError(ioState.Position, "Cannot start a new block '{0}' before previous block is closed", inLine);
                            return false;
                        }

                        ioState.Generator.CompleteHeader(ioState, ioState.Package, ioState.CurrentBlock, TagData.Empty);
                        FlushBlock(ref ioState, TagData.Empty);
                        break;
                    }

                case BlockState.BlockStarted:
                case BlockState.InData:
                    {
                        if (ioState.Rules.RequireExplicitBlockHeaderEnd)
                        {
                            BlockParser.LogError(ioState.Position, "Cannot start a new block '{0}' before previous block is closed", inLine);
                            return false;
                        }

                        if (ioState.CurrentState == BlockState.BlockStarted)
                        {
                            ioState.Generator.CompleteHeader(ioState, ioState.Package, ioState.CurrentBlock, TagData.Empty);
                        }
                        FlushBlock(ref ioState, TagData.Empty);
                        break;
                    }

                case BlockState.BlockDone:
                    break;
            }

            TBlock newBlock;
            if (!ioState.Generator.TryCreateBlock(ioState, ioState.Package, data, out newBlock))
            {
                BlockParser.LogError(ioState.Position, "Failed to create a new block '{0}'", inLine);
                ioState.CurrentBlock = null;
                ioState.CurrentState = BlockState.BlockDone;
                return false;
            }

            ioState.CurrentBlock = newBlock;
            ioState.CurrentState = BlockState.BlockStarted;
            ioState.BlockError = false;
            return true;
        }

        // Attempts to evaluate metadata with the current block header
        static private bool TryEvaluateMeta(ref ParseState ioState, StringSlice inLine)
        {
            TagData data = TagData.Parse(inLine, ioState.TagDelimiters);

            switch (ioState.CurrentState)
            {
                case BlockState.NotStarted:
                case BlockState.BlockDone:
                    {
                        BlockParser.LogError(ioState.Position, "Cannot add metadata '{0}' to block, not currently in block", inLine);
                        return false;
                    }

                case BlockState.InData:
                    {
                        BlockParser.LogError(ioState.Position, "Cannot add metadata '{0}' to block, not currently in header section", inLine);
                        return false;
                    }

                case BlockState.InHeader:
                    break;

                case BlockState.BlockStarted:
                    {
                        ioState.CurrentState = BlockState.InHeader;
                        break;
                    }
            }

            if (!data.IsEmpty())
            {
                bool bHandled = ioState.Generator.TryEvaluateMeta(ioState, ioState.Package, ioState.CurrentBlock, data);
                if (!bHandled)
                    bHandled = ioState.Cache.TryEvaluateCommand(ioState.CurrentBlock, data);
                if (!bHandled)
                    BlockParser.LogError(ioState.Position, "Unrecognized block metadata '{0}'", data);
                return bHandled;
            }

            return false;
        }

        // Attempts to end the current block header
        static private bool TryEndHeader(ref ParseState ioState, StringSlice inLine)
        {
            TagData data = TagData.Parse(inLine, ioState.TagDelimiters);

            switch (ioState.CurrentState)
            {
                case BlockState.NotStarted:
                case BlockState.BlockDone:
                    {
                        BlockParser.LogError(ioState.Position, "Cannot end block header with '{0}', not currently in block", inLine);
                        return false;
                    }

                case BlockState.InData:
                    {
                        BlockParser.LogError(ioState.Position, "Cannot end block header with '{0}', already in data section", inLine);
                        return false;
                    }

                case BlockState.BlockStarted:
                case BlockState.InHeader:
                default:
                    {
                        ioState.Generator.CompleteHeader(ioState, ioState.Package, ioState.CurrentBlock, data);
                        ioState.CurrentState = BlockState.InData;
                        return true;
                    }
            }
        }

        // Attempts to end the current block
        static private bool TryEndBlock(ref ParseState ioState, StringSlice inLine)
        {
            TagData data = TagData.Parse(inLine, ioState.TagDelimiters);

            switch (ioState.CurrentState)
            {
                case BlockState.NotStarted:
                case BlockState.BlockDone:
                    {
                        BlockParser.LogError(ioState.Position, "Cannot close block with '{0}', not currently in block", inLine);
                        return false;
                    }

                case BlockState.BlockStarted:
                    {
                        ioState.Generator.CompleteHeader(ioState, ioState.Package, ioState.CurrentBlock, TagData.Empty);
                        break;
                    }

                case BlockState.InData:
                    break;

                case BlockState.InHeader:
                    {
                        if (ioState.Rules.RequireExplicitBlockHeaderEnd)
                        {
                            BlockParser.LogError(ioState.Position, "Cannot close block with '{0}', header section not closed", inLine);
                            return false;
                        }

                        ioState.Generator.CompleteHeader(ioState, ioState.Package, ioState.CurrentBlock, TagData.Empty);
                        break;
                    }
            }

            FlushBlock(ref ioState, data);
            ioState.CurrentState = BlockState.BlockDone;
            ioState.CurrentBlock = null;
            return true;
        }

        // Attempts to add content to the current block
        static private bool TryAddContent(ref ParseState ioState, StringSlice inContent)
        {
            switch (ioState.CurrentState)
            {
                case BlockState.NotStarted:
                case BlockState.BlockDone:
                    {
                        BlockParser.LogError(ioState.Position, "Cannot add content '{0}', not currently in block", inContent);
                        return false;
                    }

                case BlockState.BlockStarted:
                    {
                        ioState.Generator.CompleteHeader(ioState, ioState.Package, ioState.CurrentBlock, TagData.Empty);
                        ioState.CurrentState = BlockState.InData;
                        break;
                    }
                
                case BlockState.InHeader:
                    {
                        if (ioState.Rules.RequireExplicitBlockHeaderEnd)
                        {
                            BlockParser.LogError(ioState.Position, "Cannot add content '{0}', block header is not exited", inContent);
                            return false;
                        }

                        ioState.Generator.CompleteHeader(ioState, ioState.Package, ioState.CurrentBlock, TagData.Empty);
                        ioState.CurrentState = BlockState.InData;
                        break;
                    }

                case BlockState.InData:
                    break;
            }

            bool bHandled = ioState.Generator.TryAddContent(ioState, ioState.Package, ioState.CurrentBlock, inContent);
            if (!bHandled)
            {
                BlockMetaCache.ContentInfo contentSetter;
                if (ioState.Cache.TryGetContent(ioState.CurrentBlock, out contentSetter))
                {
                    if (contentSetter.Mode == BlockContentMode.LineByLine)
                    {
                        StringSlice contentString = inContent;
                        if (contentString.Contains('\\'))
                        {
                            contentString = inContent.Unescape();
                        }

                        bHandled = contentSetter.Invoke(ioState.CurrentBlock, contentString, ioState.Cache.SharedResources);
                    }
                    else
                    {
                        if (ioState.ContentBuilder.Length == 0)
                        {
                            inContent = inContent.TrimStart(BlockParser.TrimCharsWithSpace);
                        }
                        else if (ioState.ContentBuilder.Length > 0 && contentSetter.LineSeparator != 0)
                        {
                            ioState.ContentBuilder.Append(contentSetter.LineSeparator);
                        }
                        ioState.ContentBuilder.AppendSlice(inContent);
                        bHandled = true;
                    }
                }
            }

            if (!bHandled)
                BlockParser.LogError(ioState.Position, "Unable to add content");

            return bHandled;
        }

        // Attempts to add a comment
        static private bool TryAddComment(ref ParseState ioState, StringSlice inComment)
        {
            if (inComment.IsEmpty)
                return true;

            if (!ioState.Generator.TryAddComment(ioState, ioState.Package, ioState.CurrentBlock, inComment))
            {
                BlockParser.LogError(ioState.Position, "Unhandled comment");
            }

            return true;
        }

        // Attempts to add package metadata
        static private bool TryEvaluatePackage(ref ParseState ioState, StringSlice inLine)
        {
            TagData data = TagData.Parse(inLine, ioState.TagDelimiters);

            if (ioState.Rules.PackageMetaMode == PackageMetaMode.DisallowInBlock)
            {
                if (ioState.CurrentState != BlockState.NotStarted && ioState.CurrentState != BlockState.BlockDone)
                {
                    BlockParser.LogError(ioState.Position, "Cannot set package metadata '{0}', currently in a block", inLine);
                    return false;
                }
            }

            if (ioState.Rules.PackageMetaMode == PackageMetaMode.ImplicitCloseBlock)
            {
                switch (ioState.CurrentState)
                {
                    case BlockState.NotStarted:
                    case BlockState.BlockDone:
                        break;

                    case BlockState.InHeader:
                    {
                        if (ioState.Rules.RequireExplicitBlockEnd || ioState.Rules.RequireExplicitBlockHeaderEnd)
                        {
                            BlockParser.LogError(ioState.Position, "Cannot close a block with meta command '{0}' before previous block is closed", inLine);
                            return false;
                        }

                        ioState.Generator.CompleteHeader(ioState, ioState.Package, ioState.CurrentBlock, TagData.Empty);
                        FlushBlock(ref ioState, TagData.Empty);
                        break;
                    }

                case BlockState.BlockStarted:
                case BlockState.InData:
                    {
                        if (ioState.Rules.RequireExplicitBlockHeaderEnd)
                        {
                            BlockParser.LogError(ioState.Position, "Cannot close a block with meta command '{0}' before previous block is closed", inLine);
                            return false;
                        }

                        if (ioState.CurrentState == BlockState.BlockStarted)
                        {
                            ioState.Generator.CompleteHeader(ioState, ioState.Package, ioState.CurrentBlock, TagData.Empty);
                        }
                        FlushBlock(ref ioState, TagData.Empty);
                        break;
                    }
                }   
            }

            if (!data.IsEmpty())
            {
                bool bHandled = ioState.Generator.TryEvaluatePackage(ioState, ioState.Package, ioState.CurrentBlock, data);
                if (!bHandled)
                    bHandled = ioState.Cache.TryEvaluateCommand(ioState.Package, data);
                if (!bHandled)
                    BlockParser.LogError(ioState.Position, "Unrecognized package metadata '{0}'", data);
                return bHandled;
            }

            return false; 
        }

        static private void FlushBlock(ref ParseState ioState, TagData inEndData)
        {
            ioState.ContentBuilder.TrimEnd(BlockParser.TrimCharsWithSpace);
            if (ioState.ContentBuilder.Length > 0)
            {
                BlockMetaCache.ContentInfo contentSetter;
                if (ioState.Cache.TryGetContent(ioState.CurrentBlock, out contentSetter))
                {
                    if (contentSetter.Mode == BlockContentMode.BatchContent)
                    {
                        string contentString = ioState.ContentBuilder.Flush();
                        if (contentString.IndexOf('\\') >= 0)
                        {
                            contentString = StringUtils.Unescape(contentString);
                        }
                        ioState.BlockError |= contentSetter.Invoke(ioState.CurrentBlock, contentString, ioState.Cache.SharedResources);
                        ioState.Error |= ioState.BlockError;
                    }
                }

                ioState.ContentBuilder.Length = 0;
            }
            
            IValidatable validatable = ioState.CurrentBlock as IValidatable;
            if (validatable != null)
                validatable.Validate();
            
            ioState.Generator.CompleteBlock(ioState, ioState.Package, ioState.CurrentBlock, inEndData, ioState.BlockError);
        }

        #endregion // Parse Commands

        #region Pre-parse steps

        // Generates a prioritized set of prefixes to analyze, from most specific to least specific
        static private PrefixType[] GeneratePrefixPriority(IBlockParsingRules inRules)
        {
            List<PriorityValue<PrefixType>> buffer = new List<PriorityValue<PrefixType>>(6);
            AddPrefixPriority(buffer, inRules.BlockIdPrefix, PrefixType.BlockId);
            AddPrefixPriority(buffer, inRules.BlockMetaPrefix, PrefixType.BlockMeta);
            AddPrefixPriority(buffer, inRules.BlockHeaderEndPrefix, PrefixType.BlockHeaderEnd);
            AddPrefixPriority(buffer, inRules.BlockContentPrefix, PrefixType.BlockContent);
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

        // Splits a string into lines based on 
        static private IEnumerable<StringSlice> SplitIntoLines(IBlockParsingRules inRules, StringSlice inString)
        {
            if (inRules.CustomLineSplitter != null)
                return inString.EnumeratedSplit(inRules.CustomLineSplitter, StringSplitOptions.None);
            return inString.EnumeratedSplit(inRules.LineDelimiters, StringSplitOptions.None);
        }

        #endregion // Pre-parse steps
    }
}