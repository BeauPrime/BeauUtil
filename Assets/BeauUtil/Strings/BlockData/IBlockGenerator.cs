/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    23 August 2020
 * 
 * File:    IBlockGenerator.cs
 * Purpose: Interface for generating and configuring data blocks.
 */

using BeauUtil.Tags;

namespace BeauUtil.Blocks
{
    public interface IBlockGenerator<TBlock, TPackage>
        where TBlock : class, IDataBlock
        where TPackage : class, IDataBlockPackage<TBlock>
    {
        #region Shared Object

        /// <summary>
        /// Creates a package.
        /// </summary>
        TPackage CreatePackage(string inFileName);

        /// <summary>
        /// Attempts to evaluate data through the package object.
        /// Returns if no error.
        /// </summary>
        bool TryEvaluatePackage(IBlockParserUtil inUtil, TPackage inPackage, TBlock inCurrentBlock, TagData inMetadata);

        #endregion // Shared Object

        #region Parsing Stages

        /// <summary>
        /// Executes when parsing starts.
        /// </summary>
        void OnStart(IBlockParserUtil inUtil, TPackage inPackage);

        /// <summary>
        /// Executes when the first block begins parsing.
        /// </summary>
        void OnBlocksStart(IBlockParserUtil inUtil, TPackage inPackage);

        /// <summary>
        /// Executes when block parsing ends.
        /// </summary>
        void OnEnd(IBlockParserUtil inUtil, TPackage inPackage, bool inbError);

        #endregion // Parsing Stages

        #region Block Actions

        /// <summary>
        /// Creates a new block object.
        /// Returns if handled.
        /// </summary>
        bool TryCreateBlock(IBlockParserUtil inUtil, TPackage inPackage, TagData inId, out TBlock outBlock);
        
        /// <summary>
        /// Attempts to evaluate data through the block object header.
        /// Returns if handled.
        /// </summary>
        bool TryEvaluateMeta(IBlockParserUtil inUtil, TPackage inPackage, TBlock inBlock, TagData inMetadata);
        
        /// <summary>
        /// Executes when the block object header is completed.
        /// </summary>
        void CompleteHeader(IBlockParserUtil inUtil, TPackage inPackage, TBlock inBlock, TagData inAdditionalData);
        
        /// <summary>
        /// Attempts to add content to the block object data.
        /// Returns if handled.
        /// </summary>
        bool TryAddContent(IBlockParserUtil inUtil, TPackage inPackage, TBlock inBlock, StringSlice inContent);
        
        /// <summary>
        /// Executes when the block object is completed.
        /// </summary>
        void CompleteBlock(IBlockParserUtil inUtil, TPackage inPackage, TBlock inBlock, TagData inAdditionalData, bool inbError);

        #endregion // Block Actions
    
        /// <summary>
        /// Attempts to add a comment.
        /// Returns if handled.
        /// </summary>
        bool TryAddComment(IBlockParserUtil inUtil, TPackage inPackage, TBlock inCurrentBlock, StringSlice inComment);
    }
}