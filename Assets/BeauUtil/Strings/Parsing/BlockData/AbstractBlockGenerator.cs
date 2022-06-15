/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    24 August 2020
 * 
 * File:    AbstractBlockGenerator.cs
 * Purpose: Rules for parsing a set of tagged blocks.
 */

using System.Text;
using BeauUtil.Tags;

namespace BeauUtil.Blocks
{
    public abstract class AbstractBlockGenerator<TBlock, TPackage> : IBlockGenerator<TBlock, TPackage>
        where TBlock : class, IDataBlock
        where TPackage : class, IDataBlockPackage<TBlock>
    {
        #region Shared Object

        public abstract TPackage CreatePackage(string inFileName);

        public virtual bool TryEvaluatePackage(IBlockParserUtil inUtil, TPackage inPackage, TBlock inCurrentBlock, TagData inMetadata)
        {
            return false;
        }

        #endregion // Shared Object

        #region Parse Stages

        public virtual void OnStart(IBlockParserUtil inUtil, TPackage inPackage) { }

        public virtual void OnBlocksStart(IBlockParserUtil inUtil, TPackage inPackage) { }

        public virtual void OnEnd(IBlockParserUtil inUtil, TPackage inPackage, bool inbError) { }

        #endregion // Parse Stages

        #region Block Actions
        
        public abstract bool TryCreateBlock(IBlockParserUtil inUtil, TPackage inPackage, TagData inId, out TBlock outBlock);

        public virtual bool TryEvaluateMeta(IBlockParserUtil inUtil, TPackage inPackage, TBlock inBlock, TagData inMetadata)
        {
            return false;
        }

        public virtual void CompleteHeader(IBlockParserUtil inUtil, TPackage inPackage, TBlock inBlock) { }

        public virtual bool TryAddContent(IBlockParserUtil inUtil, TPackage inPackage, TBlock inBlock, StringBuilder inContent) { return false; }
        
        public virtual void CompleteBlock(IBlockParserUtil inUtil, TPackage inPackage, TBlock inBlock, bool inbError) { }

        #endregion // Block Actions

        #region Text

        public virtual void ProcessLine(IBlockParserUtil inUtil, TPackage inPackage, TBlock inBlock, StringBuilder ioLine) { }

        #endregion // Text
    }
}