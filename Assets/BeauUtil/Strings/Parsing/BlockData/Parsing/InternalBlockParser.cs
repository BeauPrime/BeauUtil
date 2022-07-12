/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    24 August 2020
 * 
 * File:    InternalBlockParser.cs
 * Purpose: Parser with generics in header.
 */

#if UNITY_EDITOR || DEVELOPMENT_BUILD || DEBUG || DEVELOPMENT
#define ENABLE_LOGGING_BEAUUTIL
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD || DEBUG || DEVELOPMENT

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using BeauUtil.Streaming;
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

            public void InsertStream(CharStreamParams inStream, string inFileName = null)
            {
                Base.PushStream(inStream, inFileName);
            }

            #endregion // IBlockParserUtil

            #region IEnumerator

            object IEnumerator.Current { get { return null; }}

            unsafe bool IEnumerator.MoveNext()
            {
                if (Base.StackOffset < 0)
                {
                    if (CurrentBlock != null)
                    {
                        Base.Error |= !TryEndBlock(this, Base);
                    }

                    Generator.OnEnd(this, Package, Base.Error);
                    return false;
                }

                char* tempChars = stackalloc char[s_BlockSize + 32];
                int charsRead;
                if (Base.LeftoverCount > 0)
                {
                    charsRead = Base.LeftoverStream.ReadChars(Math.Min(s_BlockSize, Base.LeftoverCount), tempChars, 0, s_BlockSize + 32);
                    Base.LeftoverCount -= charsRead;

                    // Debug.Log("Read leftover string: \"" + new string(tempChars, 0, charsRead) + "\"");
                }
                else
                {
                    ref CharStream buffer = ref Base.StreamStack[Base.StackOffset];
                    charsRead = buffer.ReadChars(s_BlockSize, tempChars, 0, s_BlockSize + 32);
                    if (charsRead == -1)
                    {
                        Base.PopStream();
                        ParseLine(this, Base, Base.LineBuilder, false);
                        return true;
                    }

                    // Debug.Log("Read string from buffer: \"" + new string(tempChars, 0, charsRead) + "\"");
                }

                if (charsRead == 0)
                {
                    return true;
                }

                Base.ParseFlags &= ~ParseStateFlags.EarlyBreakForInsertion;

                bool skipWhitespace = (Base.ParseFlags & ParseStateFlags.SkipWhitespace) != 0;
                bool inComment = (Base.ParseFlags & ParseStateFlags.InComment) != 0;
                char* charPtr = tempChars;
                char* charPtrEnd = tempChars + charsRead;
                while(charPtr < charPtrEnd)
                {
                    if (skipWhitespace)
                    {
                        // skip over whitespace at start
                        while(charPtr < charPtrEnd && ArrayUtils.Contains(TrimLeadingChars, *charPtr))
                            charPtr++;
                        if (charPtr == charPtrEnd)
                            break;
                        
                        Base.ParseFlags &= ~ParseStateFlags.SkipWhitespace;
                        skipWhitespace = false;
                    }

                    char* begin = charPtr;
                    int commentOverlapBuilder, commentOverlapBuffer;
                    if (!inComment)
                    {
                        while(charPtr < charPtrEnd && *charPtr != '\n')
                        {
                            if (MatchLookBack(Base.LineBuilder, begin, charPtr, Base.Rules.CommentPrefix, out commentOverlapBuilder, out commentOverlapBuffer))
                            {
                                Base.LineBuilder.Length -= commentOverlapBuilder;
                                Base.LineBuilder.Append(begin, (int) (charPtr - begin - commentOverlapBuffer + 1));
                                Base.LineBuilder.TrimEnd(TrimCharsWithSpace);
                                Base.ParseFlags |= ParseStateFlags.InComment;
                                inComment = true;
                                charPtr++;
                                break;
                            }
                            charPtr++;
                        }
                    }

                    if (inComment)
                    {
                        while(charPtr < charPtrEnd && *charPtr != '\n')
                        {
                            charPtr++;
                        }
                    }
                    else
                    {
                        Base.LineBuilder.Append(begin, (int) (charPtr - begin));
                    }

                    if (charPtr != charPtrEnd)
                    {
                        if (!Base.PositionInline)
                        {
                            Base.Position = new BlockFilePosition(Base.Position.FileName, Base.Position.LineNumber + 1);
                        }
                        Base.ParseFlags |= ParseStateFlags.SkipWhitespace;
                        Base.ParseFlags &= ~ParseStateFlags.InComment;
                        skipWhitespace = true;
                        inComment = false;

                        int leftover = (int) (charPtrEnd - charPtr - 1);
                        Base.LeftoverCount = leftover;

                        LineResult result = ParseLine(this, Base, Base.LineBuilder, false);
                        if (result == LineResult.Exception)
                        {
                            Generator.OnEnd(this, Package, true);
                            return false;
                        }

                        charPtr++;

                        if ((Base.ParseFlags & ParseStateFlags.EarlyBreakForInsertion) != 0)
                        {
                            if (leftover > 0)
                            {
                                Base.LeftoverStream.InsertChars(charPtr, leftover);
                            }
                            return true;
                        }

                        Base.LeftoverCount = 0;
                    }
                }

                return true;
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

        static internal IEnumerator ParseFile(string inFileName, CharStreamParams inInitialStream, TPackage ioPackage, IBlockParsingRules inRules, IBlockGenerator<TBlock, TPackage> inGenerator, BlockMetaCache inCache)
        {
            var state = new Parser();
            var buffer = RentParseBuffer(inRules, inCache);
            state.Base = buffer;
            state.Generator = inGenerator;
            state.Package = ioPackage;

            buffer.PushStream(inInitialStream, inFileName);
            inGenerator.OnStart(state, state.Package);
            return state;
        }

        static private LineResult ParseLine(Parser ioParser, ParseBuffer ioState, StringBuilder ioLine, bool inbCloseBlock)
        {
            ioLine.TrimEnd(TrimCharsWithSpace);

            if (!inbCloseBlock && ioLine.Length > 0 && ioLine[ioLine.Length - 1] == '\\')
            {
                ioLine.Length -= 1;
                ioLine.TrimEnd(TrimCharsWithSpace);
                ioLine.Append('\n');
                return LineResult.Continue;
            }

            return TryFlushLine(ioParser, ioState, inbCloseBlock);
        }

        static private LineResult TryFlushLine(Parser ioParser, ParseBuffer ioState, bool inbCloseBlock)
        {
            LineResult result = LineResult.Empty;
            if (ioState.LineBuilder.Length > 0 || ioState.CurrentState == BlockState.InData)
            {
                StringUtils.UnescapeInline(ioState.LineBuilder);
                ioState.LineBuilder.TrimEnd(TrimCharsWithSpace);
                result = ParseLineCommand(ioParser, ioState, ioState.LineBuilder, inbCloseBlock);
            }

            return result;
        }

        static private LineResult ParseLineCommand(Parser ioParser, ParseBuffer ioState, StringBuilder ioLine, bool inbCloseBlock)
        {
            try
            {
                ioParser.Generator.ProcessLine(ioParser, ioParser.Package, ioParser.CurrentBlock, ioLine);

                bool bSuccess = true;
                bool bProcessedCommand = TryProcessCommand(ioParser, ioState, ioLine);
                
                if (!bProcessedCommand)
                {
                    if (ioLine.Length != 0 || ioState.CurrentState == BlockState.InData)
                    {
                        bSuccess &= TryAddContent(ioParser, ioState, ioLine, inbCloseBlock);
                    }
                }

                ioLine.Length = 0;
                ioState.Error |= !bSuccess;
                return !bSuccess ? LineResult.Error : LineResult.NoError;
            }
            catch (Exception e)
            {
                ioLine.Length = 0;
                UnityEngine.Debug.LogException(e);
                return LineResult.Exception;
            }
        }

        static private bool TryProcessCommand(Parser ioParser, ParseBuffer ioState, StringBuilder ioLine)
        {
            if (ioLine.Length == 0)
                return false;

            for (int i = 0; i < PrefixPriority.Length; i++)
            {
                PrefixType type = ioState.PrefixPriorities[i];
                switch (type)
                {
                    case PrefixType.BlockId:
                        {
                            if (ioLine.AttemptMatch(0, ioState.Rules.BlockIdPrefix))
                            {
                                StringBuilderSlice data = new StringBuilderSlice(ioLine, ioState.Rules.BlockIdPrefix.Length);
                                ioState.BlockError |= !TryStartBlock(ioParser, ioState, data);
                                return true;
                            }
                            break;
                        }

                    case PrefixType.BlockMeta:
                        {
                            if (ShouldCheckMeta(ioState) && ioLine.AttemptMatch(0, ioState.Rules.BlockMetaPrefix))
                            {
                                StringBuilderSlice data = new StringBuilderSlice(ioLine, ioState.Rules.BlockMetaPrefix.Length);
                                ioState.BlockError |= !TryEvaluateMeta(ioParser, ioState, data);
                                return true;
                            }
                            break;
                        }

                    case PrefixType.BlockEnd:
                        {
                            if (ioLine.AttemptMatch(0, ioState.Rules.BlockEndPrefix))
                            {
                                ioState.BlockError |= !TryEndBlock(ioParser, ioState);
                                return true;
                            }
                            break;
                        }

                    case PrefixType.PackageMeta:
                        {
                            if (ShouldCheckPackageMeta(ioState) && ioLine.AttemptMatch(0, ioState.Rules.PackageMetaPrefix))
                            {
                                StringBuilderSlice data = new StringBuilderSlice(ioLine, ioState.Rules.PackageMetaPrefix.Length);
                                ioState.Error |= !TryEvaluatePackage(ioParser, ioState, data);
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
            return inBuffer.CurrentState != BlockState.InData;
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
        static private bool TryStartBlock(Parser ioParser, ParseBuffer ioState, StringBuilderSlice inLine)
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
                        ioParser.Generator.CompleteHeader(ioParser, ioParser.Package, ioParser.CurrentBlock);
                        FlushBlock(ioParser, ioState, false);
                        break;
                    }

                case BlockState.BlockStarted:
                case BlockState.InData:
                    {
                        if (ioState.CurrentState == BlockState.BlockStarted)
                        {
                            ioParser.Generator.CompleteHeader(ioParser, ioParser.Package, ioParser.CurrentBlock);
                        }
                        FlushBlock(ioParser, ioState, false);
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
        static private bool TryEvaluateMeta(Parser ioParser, ParseBuffer ioState, StringBuilderSlice inLine)
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
                bool bHandled = ioParser.Generator.TryEvaluateMeta(ioParser, ioParser.Package, ioParser.CurrentBlock, data, ioState.LineBuilder);
                if (!bHandled)
                    bHandled = ioState.Cache.TryEvaluateCommand(ioParser.CurrentBlock, data);
                if (!bHandled)
                    LogError(ioState.Position, "Unrecognized block metadata '{0}'", data);
                return bHandled;
            }

            return false;
        }

        // Attempts to end the current block header
        static private bool TryEndHeader(Parser ioParser, ParseBuffer ioState)
        {
            switch (ioState.CurrentState)
            {
                case BlockState.NotStarted:
                case BlockState.BlockDone:
                    {
                        LogError(ioState.Position, "Cannot end block header, not currently in block");
                        return false;
                    }

                case BlockState.InData:
                    {
                        LogError(ioState.Position, "Cannot end block header, already in data section");
                        return false;
                    }

                case BlockState.BlockStarted:
                case BlockState.InHeader:
                default:
                    {
                        ioParser.Generator.CompleteHeader(ioParser, ioParser.Package, ioParser.CurrentBlock);
                        ioState.CurrentState = BlockState.InData;
                        return true;
                    }
            }
        }

        // Attempts to end the current block
        static private bool TryEndBlock(Parser ioParser, ParseBuffer ioState)
        {
            switch (ioState.CurrentState)
            {
                case BlockState.NotStarted:
                case BlockState.BlockDone:
                    {
                        LogError(ioState.Position, "Cannot close block, not currently in block");
                        return false;
                    }

                case BlockState.BlockStarted:
                    {
                        ioParser.Generator.CompleteHeader(ioParser, ioParser.Package, ioParser.CurrentBlock);
                        break;
                    }

                case BlockState.InData:
                    break;

                case BlockState.InHeader:
                    {
                        ioParser.Generator.CompleteHeader(ioParser, ioParser.Package, ioParser.CurrentBlock);
                        break;
                    }
            }

            FlushBlock(ioParser, ioState, false);
            ioState.CurrentState = BlockState.BlockDone;
            ioParser.CurrentBlock = null;
            return true;
        }

        // Attempts to add content to the current block
        static private bool TryAddContent(Parser ioParser, ParseBuffer ioState, StringBuilder inContent, bool inbCloseBlock)
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
                        ioParser.Generator.CompleteHeader(ioParser, ioParser.Package, ioParser.CurrentBlock);
                        ioState.CurrentState = BlockState.InData;
                        break;
                    }
                
                case BlockState.InHeader:
                    {
                        ioParser.Generator.CompleteHeader(ioParser, ioParser.Package, ioParser.CurrentBlock);
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
                        bHandled = contentSetter.Invoke(ioParser.CurrentBlock, inContent.Flush(), ioState.Cache.SharedResources);
                    }
                    else
                    {
                        if (ioState.ContentBuilder.Length > 0 && contentSetter.LineSeparator != 0)
                        {
                            ioState.ContentBuilder.Append(contentSetter.LineSeparator);
                        }
                        ioState.ContentBuilder.Append(inContent);
                        bHandled = true;
                    }
                }
            }

            if (!bHandled)
                LogError(ioState.Position, "Unable to add content");

            return bHandled;
        }

        // Attempts to add package metadata
        static private bool TryEvaluatePackage(Parser ioParser, ParseBuffer ioState, StringBuilderSlice inLine)
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

                        ioParser.Generator.CompleteHeader(ioParser, ioParser.Package, ioParser.CurrentBlock);
                        FlushBlock(ioParser, ioState, false);
                        break;
                    }

                case BlockState.BlockStarted:
                case BlockState.InData:
                    {
                        if (ioState.CurrentState == BlockState.BlockStarted)
                        {
                            ioParser.Generator.CompleteHeader(ioParser, ioParser.Package, ioParser.CurrentBlock);
                        }
                        FlushBlock(ioParser, ioState, false);
                        break;
                    }
                }   
            }

            if (!data.IsEmpty())
            {
                bool bHandled = ioParser.Generator.TryEvaluatePackage(ioParser, ioParser.Package, ioParser.CurrentBlock, data, ioState.LineBuilder);
                if (!bHandled)
                    bHandled = ioState.Cache.TryEvaluateCommand(ioParser.Package, data);
                if (!bHandled)
                    LogError(ioState.Position, "Unrecognized package metadata '{0}'", data);
                return bHandled;
            }

            return false; 
        }

        static private void FlushBlock(Parser ioParser, ParseBuffer ioState, bool inbFlushLine)
        {
            if (inbFlushLine)
            {
                TryFlushLine(ioParser, ioState, true);
            }
            if (ioState.ContentBuilder.Length > 0)
            {
                BlockMetaCache.ContentInfo contentSetter;
                if (ioState.Cache.TryGetContent(ioParser.CurrentBlock, out contentSetter))
                {
                    if (contentSetter.Mode == BlockContentMode.BatchContent)
                    {
                        ioState.ContentBuilder.TrimEnd(TrimCharsWithSpace);
                        string contentString = ioState.ContentBuilder.Flush();
                        ioState.BlockError |= !contentSetter.Invoke(ioParser.CurrentBlock, contentString, ioState.Cache.SharedResources);
                        ioState.Error |= ioState.BlockError;
                    }
                }

                ioState.ContentBuilder.Length = 0;
            }
            
            IValidatable validatable = ioParser.CurrentBlock as IValidatable;
            if (validatable != null)
                validatable.Validate();
            
            ioParser.Generator.CompleteBlock(ioParser, ioParser.Package, ioParser.CurrentBlock, ioState.BlockError);
        }

        #endregion // Parse Commands
    }
}