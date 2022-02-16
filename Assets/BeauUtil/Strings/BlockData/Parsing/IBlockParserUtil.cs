/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    26 August 2020
 * 
 * File:    IBlockParserUtil.cs
 * Purpose: Interface to several block parsing utilities.
 */

using System.Text;

namespace BeauUtil.Blocks
{
    /// <summary>
    /// Block parsing utility.
    /// </summary>
    public interface IBlockParserUtil
    {
        BlockFilePosition Position { get; }

        StringBuilder TempBuilder { get; }
        uint TempFlags { get; set; }

        char[] LineBreakCharacters { get; }
        void InsertText(StringSlice inText);
        void InsertText(string inFileName, StringSlice inContents);
    }
}