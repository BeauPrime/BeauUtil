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
    internal class InternalBlockParser<TBlock, TPackage> : BlockParser
        where TBlock : class, IDataBlock
        where TPackage : class, IDataBlockPackage<TBlock>
    {
        #region Types

        private sealed class Parser : IBlockParserUtil, IDisposable, IEnumerator
        {
            public ParseBuffer Base;
            public IBlockGenerator<TBlock, TPackage> Generator;
            public TPackage Package;
            public TBlock CurrentBlock;

            #region IBlockParserUtil

            StringBuilder IBlockParserUtil.TempBuilder { get { return Base.Builder; } }

            BlockFilePosition IBlockParserUtil.Position { get { return Base.Position; } }

            uint IBlockParserUtil.TempFlags
            {
                get { return Base.TempFlags; }
                set { Base.TempFlags = value; }
            }

            char[] IBlockParserUtil.LineBreakCharacters { get { return Base.Rules.LineDelimiters; } }

            public void InsertText(StringSlice inText)
            {
                if (inText.IsEmpty)
                {
                    return;
                }

                Base.PushPosition();
                Base.PositionInline = true;
                Base.LineCount = PrependLinesToExistingBuffer(Base.Buffer, inText, Base.Rules);
            }

            public void InsertText(string inFileName, StringSlice inContents)
            {
                if (inContents.IsEmpty)
                {
                    return;
                }

                Base.PushPosition();
                Base.PositionInline = false;
                Base.LineCount = PrependLinesToExistingBuffer(Base.Buffer, inContents, Base.Rules);
                Base.Position = new BlockFilePosition(inFileName, 0u);
            }

            #endregion // IBlockParserUtil

            #region IEnumerator

            object IEnumerator.Current { get { return null; }}

            bool IEnumerator.MoveNext()
            {
                // if we've exhausted the current frame, then pop
                if (Base.LineCount == 0)
                {
                    Base.PopPosition();
                }

                StringSlice line;
                if (Base.Buffer.TryPopFront(out line))
                {
                    if (!Base.PositionInline)
                    {
                        Base.Position = new BlockFilePosition(Base.Position.FileName, Base.Position.LineNumber + 1);
                    }

                    if (Base.LineCount > 0)
                    {
                        Base.LineCount--;
                    }

                    LineResult result = ParseLine(this, Base, line);
                    if (result == LineResult.Exception)
                    {
                        Generator.OnEnd(this, Package, true);
                        return false;
                    }

                    // if we've exhausted the frame, then pop
                    if (Base.LineCount == 0)
                    {
                        Base.PopPosition();
                    }

                    return true;
                }

                if (CurrentBlock != null)
                {
                    Base.Error |= !TryEndBlock(this, Base, StringSlice.Empty);
                }

                Generator.OnEnd(this, Package, Base.Error);
                return false;
            }

            void IEnumerator.Reset()
            {
                throw new NotSupportedException();
            }

            #endregion // IEnumerator

            #region IDisposable

            public void Dispose()
            {
                ReturnParseBuffer(ref Base);

                Generator = null;
                Package = null;
                CurrentBlock = null;
            }
        
            #endregion // IDisposable
        }

        #endregion // Types

        #region Parse

        static internal IEnumerator ParseFile(string inFileName, StringSlice inFile, TPackage ioPackage, IBlockParsingRules inRules, IBlockGenerator<TBlock, TPackage> inGenerator, BlockMetaCache inCache)
        {
            var state = new Parser();
            var buffer = RentParseBuffer(inRules, inCache);
            state.Base = buffer;
            state.Generator = inGenerator;
            state.Package = ioPackage;

            SplitIntoLines(buffer.Rules, inFile, buffer.Buffer);
            buffer.Position = new BlockFilePosition(inFileName, 0u);
            buffer.PositionInline = false;
            buffer.LineCount = -1;

            inGenerator.OnStart(state, state.Package);
            return state;
        }

        static private LineResult ParseLine(Parser ioParser, ParseBuffer ioState, StringSlice ioLine)
        {
            StringSlice lineContents = ioLine.TrimStart(TrimCharsWithSpace).TrimEnd(TrimCharsWithoutSpace);

            if (lineContents.IsEmpty)
            {
                return TryFlushLine(ioParser, ioState, lineContents, false);
            }

            int commentIdx = lineContents.IndexOf(ioState.Rules.CommentPrefix);
            if (commentIdx >= 0)
            {
                lineContents = lineContents.Substring(0, commentIdx).TrimEnd(TrimCharsWithSpace);
            }

            // if line ends with continue character
            if (lineContents.EndsWith('\\'))
            {
                lineContents = lineContents.Substring(0, lineContents.Length - 1).TrimEnd(TrimCharsWithSpace);
                lineContents.Unescape(ioState.LineBuilder);
                ioState.LineBuilder.Append(ioState.Rules.LineDelimiters[0]);
                return LineResult.Continue;
            }

            LineResult flushResult = TryFlushLine(ioParser, ioState, lineContents, false);
            if (flushResult != LineResult.Empty)
            {
                return flushResult;
            }

            return ParseLineCommand(ioParser, ioState, lineContents, false);
        }

        static private LineResult TryFlushLine(Parser ioParser, ParseBuffer ioState, StringSlice inLine, bool inbCloseBlock)
        {
            if (ioState.LineBuilder.Length > 0 || ioState.CurrentState == BlockState.InData)
            {
                if (inLine.Length > 0)
                {
                    inLine.Unescape(ioState.LineBuilder);
                }
                ioState.LineBuilder.TrimEnd(TrimCharsWithSpace);
                inLine = ioState.LineBuilder.Flush();

                return ParseLineCommand(ioParser, ioState, inLine, inbCloseBlock);
            }

            return LineResult.Empty;
        }

        static private LineResult ParseLineCommand(Parser ioParser, ParseBuffer ioState, StringSlice ioLine, bool inbCloseBlock)
        {
            try
            {
                ioParser.Generator.ProcessLine(ioParser, ioParser.Package, ioParser.CurrentBlock, ref ioLine);

                bool bSuccess = true;
                bool bProcessedCommand = TryProcessCommand(ioParser, ioState, ioLine);
                
                if (!bProcessedCommand)
                {
                    if (!ioLine.IsEmpty || ioState.CurrentState == BlockState.InData)
                    {
                        bSuccess &= TryAddContent(ioParser, ioState, ioLine, inbCloseBlock);
                    }
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

        static private bool TryProcessCommand(Parser ioParser, ParseBuffer ioState, StringSlice ioLine)
        {
            if (ioLine.IsEmpty)
                return false;

            for (int i = 0; i < ioState.PrefixPriorities.Length; i++)
            {
                PrefixType type = ioState.PrefixPriorities[i];
                switch (type)
                {
                    case PrefixType.BlockId:
                        {
                            if (ioLine.StartsWith(ioState.Rules.BlockIdPrefix))
                            {
                                ioLine = ioLine.Substring(ioState.Rules.BlockIdPrefix.Length);
                                ioState.Error |= !TryStartBlock(ioParser, ioState, ioLine);
                                return true;
                            }
                            break;
                        }

                    case PrefixType.BlockMeta:
                        {
                            if (ShouldCheckMeta(ioState) && ioLine.StartsWith(ioState.Rules.BlockMetaPrefix))
                            {
                                ioLine = ioLine.Substring(ioState.Rules.BlockMetaPrefix.Length);
                                ioState.Error |= !TryEvaluateMeta(ioParser, ioState, ioLine);
                                return true;
                            }
                            break;
                        }

                    case PrefixType.BlockEnd:
                        {
                            if (ioLine.StartsWith(ioState.Rules.BlockEndPrefix))
                            {
                                ioLine = ioLine.Substring(ioState.Rules.BlockEndPrefix.Length);
                                ioState.Error |= !TryEndBlock(ioParser, ioState, ioLine);
                                return true;
                            }
                            break;
                        }

                    case PrefixType.PackageMeta:
                        {
                            if (ShouldCheckPackageMeta(ioState) && ioLine.StartsWith(ioState.Rules.PackageMetaPrefix))
                            {
                                ioLine = ioLine.Substring(ioState.Rules.PackageMetaPrefix.Length);
                                ioState.Error |= !TryEvaluatePackage(ioParser, ioState, ioLine);
                                return true;
                            }
                            break;
                        }
                }
            }

            return false;
        }

        #endregion // Parse

        #region Checks

        static private bool ShouldCheckMeta(ParseBuffer inBuffer)
        {
            switch(inBuffer.CurrentState)
            {
                case BlockState.InData:
                    return false;

                default:
                    return true;
            }
        }

        static private bool ShouldCheckPackageMeta(ParseBuffer inBuffer)
        {
            switch(inBuffer.CurrentState)
            {
                case BlockState.BlockDone:
                case BlockState.NotStarted:
                    return true;

                default:
                    return inBuffer.Rules.PackageMetaMode > PackageMetaMode.DisallowInBlock;
            }
        }

        #endregion // Checks

        #region Parse Commands

        // Attempts to start a new data block
        static private bool TryStartBlock(Parser ioParser, ParseBuffer ioState, StringSlice inLine)
        {
            TagData data = TagData.Parse(inLine, ioState.TagDelimiters);
            if (data.IsEmpty())
            {
                LogError(ioState.Position, "Cannot start block with an empty id '{0}'", inLine);
                return false;
            }

            switch (ioState.CurrentState)
            {
                case BlockState.NotStarted:
                    {
                        ioParser.Generator.OnBlocksStart(ioParser, ioParser.Package);
                        break;
                    }

                case BlockState.InHeader:
                    {
                        ioParser.Generator.CompleteHeader(ioParser, ioParser.Package, ioParser.CurrentBlock, TagData.Empty);
                        FlushBlock(ioParser, ioState, TagData.Empty);
                        break;
                    }

                case BlockState.BlockStarted:
                case BlockState.InData:
                    {
                        if (ioState.CurrentState == BlockState.BlockStarted)
                        {
                            ioParser.Generator.CompleteHeader(ioParser, ioParser.Package, ioParser.CurrentBlock, TagData.Empty);
                        }
                        FlushBlock(ioParser, ioState, TagData.Empty);
                        break;
                    }

                case BlockState.BlockDone:
                    break;
            }

            TBlock newBlock;
            if (!ioParser.Generator.TryCreateBlock(ioParser, ioParser.Package, data, out newBlock))
            {
                LogError(ioState.Position, "Failed to create a new block '{0}'", inLine);
                ioParser.CurrentBlock = null;
                ioState.CurrentState = BlockState.BlockDone;
                return false;
            }

            ioParser.CurrentBlock = newBlock;
            ioState.CurrentState = BlockState.BlockStarted;
            ioState.BlockError = false;
            return true;
        }

        // Attempts to evaluate metadata with the current block header
        static private bool TryEvaluateMeta(Parser ioParser, ParseBuffer ioState, StringSlice inLine)
        {
            TagData data = TagData.Parse(inLine, ioState.TagDelimiters);

            switch (ioState.CurrentState)
            {
                case BlockState.NotStarted:
                case BlockState.BlockDone:
                    {
                        LogError(ioState.Position, "Cannot add metadata '{0}' to block, not currently in block", inLine);
                        return false;
                    }

                case BlockState.InData:
                    {
                        LogError(ioState.Position, "Cannot add metadata '{0}' to block, not currently in header section", inLine);
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
                bool bHandled = ioParser.Generator.TryEvaluateMeta(ioParser, ioParser.Package, ioParser.CurrentBlock, data);
                if (!bHandled)
                    bHandled = ioState.Cache.TryEvaluateCommand(ioParser.CurrentBlock, data);
                if (!bHandled)
                    LogError(ioState.Position, "Unrecognized block metadata '{0}'", data);
                return bHandled;
            }

            return false;
        }

        // Attempts to end the current block header
        static private bool TryEndHeader(Parser ioParser, ParseBuffer ioState, StringSlice inLine)
        {
            TagData data = TagData.Parse(inLine, ioState.TagDelimiters);

            switch (ioState.CurrentState)
            {
                case BlockState.NotStarted:
                case BlockState.BlockDone:
                    {
                        LogError(ioState.Position, "Cannot end block header with '{0}', not currently in block", inLine);
                        return false;
                    }

                case BlockState.InData:
                    {
                        LogError(ioState.Position, "Cannot end block header with '{0}', already in data section", inLine);
                        return false;
                    }

                case BlockState.BlockStarted:
                case BlockState.InHeader:
                default:
                    {
                        ioParser.Generator.CompleteHeader(ioParser, ioParser.Package, ioParser.CurrentBlock, data);
                        ioState.CurrentState = BlockState.InData;
                        return true;
                    }
            }
        }

        // Attempts to end the current block
        static private bool TryEndBlock(Parser ioParser, ParseBuffer ioState, StringSlice inLine)
        {
            TagData data = TagData.Parse(inLine, ioState.TagDelimiters);

            switch (ioState.CurrentState)
            {
                case BlockState.NotStarted:
                case BlockState.BlockDone:
                    {
                        LogError(ioState.Position, "Cannot close block with '{0}', not currently in block", inLine);
                        return false;
                    }

                case BlockState.BlockStarted:
                    {
                        ioParser.Generator.CompleteHeader(ioParser, ioParser.Package, ioParser.CurrentBlock, TagData.Empty);
                        break;
                    }

                case BlockState.InData:
                    break;

                case BlockState.InHeader:
                    {
                        ioParser.Generator.CompleteHeader(ioParser, ioParser.Package, ioParser.CurrentBlock, TagData.Empty);
                        break;
                    }
            }

            FlushBlock(ioParser, ioState, data);
            ioState.CurrentState = BlockState.BlockDone;
            ioParser.CurrentBlock = null;
            return true;
        }

        // Attempts to add content to the current block
        static private bool TryAddContent(Parser ioParser, ParseBuffer ioState, StringSlice inContent, bool inbCloseBlock)
        {
            switch (ioState.CurrentState)
            {
                case BlockState.NotStarted:
                case BlockState.BlockDone:
                    {
                        LogError(ioState.Position, "Cannot add content '{0}', not currently in block", inContent);
                        return false;
                    }

                case BlockState.BlockStarted:
                    {
                        ioParser.Generator.CompleteHeader(ioParser, ioParser.Package, ioParser.CurrentBlock, TagData.Empty);
                        ioState.CurrentState = BlockState.InData;
                        break;
                    }
                
                case BlockState.InHeader:
                    {
                        ioParser.Generator.CompleteHeader(ioParser, ioParser.Package, ioParser.CurrentBlock, TagData.Empty);
                        ioState.CurrentState = BlockState.InData;
                        break;
                    }

                case BlockState.InData:
                    break;
            }

            bool bHandled = ioParser.Generator.TryAddContent(ioParser, ioParser.Package, ioParser.CurrentBlock, inContent);
            if (!bHandled)
            {
                BlockMetaCache.ContentInfo contentSetter;
                if (ioState.Cache.TryGetContent(ioParser.CurrentBlock, out contentSetter))
                {
                    if (contentSetter.Mode == BlockContentMode.LineByLine || inbCloseBlock)
                    {
                        StringSlice contentString = inContent;
                        if (contentString.Contains('\\'))
                        {
                            contentString = inContent.Unescape();
                        }
                        contentString = contentString.TrimEnd(TrimCharsWithSpace);

                        bHandled = contentSetter.Invoke(ioParser.CurrentBlock, contentString, ioState.Cache.SharedResources);
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
                LogError(ioState.Position, "Unable to add content");

            return bHandled;
        }

        // Attempts to add package metadata
        static private bool TryEvaluatePackage(Parser ioParser, ParseBuffer ioState, StringSlice inLine)
        {
            TagData data = TagData.Parse(inLine, ioState.TagDelimiters);

            if (ioState.Rules.PackageMetaMode == PackageMetaMode.DisallowInBlock)
            {
                if (ioState.CurrentState != BlockState.NotStarted && ioState.CurrentState != BlockState.BlockDone)
                {
                    LogError(ioState.Position, "Cannot set package metadata '{0}', currently in a block", inLine);
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
                        if (ioState.Rules.RequireExplicitBlockEnd)
                        {
                            LogError(ioState.Position, "Cannot close a block with meta command '{0}' before previous block is closed", inLine);
                            return false;
                        }

                        ioParser.Generator.CompleteHeader(ioParser, ioParser.Package, ioParser.CurrentBlock, TagData.Empty);
                        FlushBlock(ioParser, ioState, TagData.Empty);
                        break;
                    }

                case BlockState.BlockStarted:
                case BlockState.InData:
                    {
                        if (ioState.CurrentState == BlockState.BlockStarted)
                        {
                            ioParser.Generator.CompleteHeader(ioParser, ioParser.Package, ioParser.CurrentBlock, TagData.Empty);
                        }
                        FlushBlock(ioParser, ioState, TagData.Empty);
                        break;
                    }
                }   
            }

            if (!data.IsEmpty())
            {
                bool bHandled = ioParser.Generator.TryEvaluatePackage(ioParser, ioParser.Package, ioParser.CurrentBlock, data);
                if (!bHandled)
                    bHandled = ioState.Cache.TryEvaluateCommand(ioParser.Package, data);
                if (!bHandled)
                    LogError(ioState.Position, "Unrecognized package metadata '{0}'", data);
                return bHandled;
            }

            return false; 
        }

        static private void FlushBlock(Parser ioParser, ParseBuffer ioState, TagData inEndData)
        {
            TryFlushLine(ioParser, ioState, inEndData.Data, true);
            if (ioState.ContentBuilder.Length > 0)
            {
                BlockMetaCache.ContentInfo contentSetter;
                if (ioState.Cache.TryGetContent(ioParser.CurrentBlock, out contentSetter))
                {
                    if (contentSetter.Mode == BlockContentMode.BatchContent)
                    {
                        string contentString = ioState.ContentBuilder.Flush();
                        if (contentString.IndexOf('\\') >= 0)
                        {
                            contentString = StringUtils.Unescape(contentString);
                        }
                        contentString = contentString.TrimEnd(TrimCharsWithSpace);
                        ioState.BlockError |= contentSetter.Invoke(ioParser.CurrentBlock, contentString, ioState.Cache.SharedResources);
                        ioState.Error |= ioState.BlockError;
                    }
                }

                ioState.ContentBuilder.Length = 0;
            }
            
            IValidatable validatable = ioParser.CurrentBlock as IValidatable;
            if (validatable != null)
                validatable.Validate();
            
            ioParser.Generator.CompleteBlock(ioParser, ioParser.Package, ioParser.CurrentBlock, inEndData, ioState.BlockError);
        }

        #endregion // Parse Commands
    }
}