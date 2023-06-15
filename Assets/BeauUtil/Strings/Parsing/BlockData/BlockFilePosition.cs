/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    24 August 2020
 * 
 * File:    BlockFilePosition.cs
 * Purpose: Position within a block file.
 */

namespace BeauUtil.Blocks
{
    /// <summary>
    /// Position within a block file.
    /// </summary>
    public struct BlockFilePosition
    {
        public readonly string FileName;
        public readonly string ExtendedFileName;
        public readonly uint LineNumber;

        public BlockFilePosition(string inFileName, uint inLineNumber)
        {
            FileName = string.IsNullOrEmpty(inFileName) ? BlockParser.NullFilename : inFileName;
            ExtendedFileName = FileName;
            LineNumber = inLineNumber;
        }

        public BlockFilePosition(string inFileName, string inExtendedFileName, uint inLineNumber)
        {
            FileName = string.IsNullOrEmpty(inFileName) ? BlockParser.NullFilename : inFileName;
            ExtendedFileName = string.IsNullOrEmpty(inExtendedFileName) ? FileName : inExtendedFileName;
            LineNumber = inLineNumber;
        }

        public override string ToString()
        {
            return string.Format("{0}:{1}", FileName, LineNumber);
        }
    }
}