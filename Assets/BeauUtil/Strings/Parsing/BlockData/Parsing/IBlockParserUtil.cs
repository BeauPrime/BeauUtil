/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    26 August 2020
 * 
 * File:    IBlockParserUtil.cs
 * Purpose: Interface to several block parsing utilities.
 */

using System.Text;
using BeauUtil.Streaming;

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

        void InsertStream(CharStreamParams inStream, string inFileName = null);
    }

    static public class BlockParserUtilsExtensions
    {
        static public void InsertText(this IBlockParserUtil ioParser, string inText)
        {
            if (!string.IsNullOrEmpty(inText))
            {
                ioParser.InsertStream(CharStreamParams.FromString(inText));
            }
        }

        static public void InsertText(this IBlockParserUtil ioParser, string inText, string inFileName)
        {
            if (!string.IsNullOrEmpty(inText))
            {
                ioParser.InsertStream(CharStreamParams.FromString(inText), inFileName);
            }
        }

        static public void InsertBytes(this IBlockParserUtil ioParser, byte[] inData)
        {
            if (inData.Length > 0)
            {
                ioParser.InsertStream(CharStreamParams.FromBytes(inData));
            }
        }

        static public void InsertBytes(this IBlockParserUtil ioParser, byte[] inData, string inFileName)
        {
            if (inData.Length > 0)
            {
                ioParser.InsertStream(CharStreamParams.FromBytes(inData), inFileName);
            }
        }

        static public void InsertFile(this IBlockParserUtil ioParser, CustomTextAsset inAsset)
        {
            if (inAsset.ByteLength() > 0)
            {
                ioParser.InsertStream(CharStreamParams.FromCustomTextAsset(inAsset), inAsset.name);
            }
        }
    }
}