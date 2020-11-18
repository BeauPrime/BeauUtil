/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    23 August 2020
 * 
 * File:    IBlockParsingRules.cs
 * Purpose: Rules for parsing a set of data blocks.
 */

namespace BeauUtil.Blocks
{
    /// <summary>
    /// Rules for parsing data blocks.
    /// </summary>
    public interface IBlockParsingRules
    {
        /// <summary>
        /// Lines prefixed with this indicate the start of a block.
        /// </summary>
        string BlockIdPrefix { get; }

        /// <summary>
        /// Lines prefixed with this indicate a block header metadata command.
        /// </summary>
        string BlockMetaPrefix { get; }

        /// <summary>
        /// Lines prefixed with this indicate the end of the block header.
        /// </summary>
        string BlockHeaderEndPrefix { get; }

        /// <summary>
        /// Lines prefixed with this indicate block content.
        /// </summary>
        string BlockContentPrefix { get; }
        
        /// <summary>
        /// Lines prefixed with this indicate the end of a block.
        /// </summary>
        string BlockEndPrefix { get; }

        /// <summary>
        /// Lines prefixed with this indicate a package metadata command.
        /// </summary>
        string PackageMetaPrefix { get; }

        /// <summary>
        /// Any content after this prefix will be considered a comment.
        /// </summary>
        string CommentPrefix { get; }

        /// <summary>
        /// These characters will be used to divide block ids and metadata into id and additional data.
        /// Commonly used are ' ', '=', and ':'.
        /// </summary>
        char[] TagDataDelimiters { get; }

        /// <summary>
        /// These characters will be used to divide file contents into lines.
        /// </summary>
        char[] LineDelimiters { get; }
        
        /// <summary>
        /// If set, this splitter will be used to divide file contents into lines,
        /// ignoring the value of LineDelimiters.
        /// </summary>
        StringSlice.ISplitter CustomLineSplitter { get; }

        /// <summary>
        /// Indicates if the block header section must be explicitly closed
        /// before either the file ends, block contents start, or another block begins.
        /// </summary>
        bool RequireExplicitBlockHeaderEnd { get; }

        /// <summary>
        /// Indicates if block content lines must be explicitly started with the content prefix.
        /// </summary>
        bool RequireExplicitBlockContent { get; }
        
        /// <summary>
        /// Indicates if blocks must be explicitly closed
        /// before either the file ends or another block begins.
        /// </summary>
        bool RequireExplicitBlockEnd { get; }

        /// <summary>
        /// Indicates if package metas are allowed
        /// while within an unclosed block.
        /// </summary>
        bool AllowPackageMetaInBlock { get; }
    }
}