/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    24 August 2020
 * 
 * File:    BlockParsingRules.cs
 * Purpose: Sets of default parsing rules.
 */

namespace BeauUtil.Blocks
{
    /// <summary>
    /// Rules for parsing data blocks.
    /// </summary>
    static public class BlockParsingRules
    {
        static public readonly IBlockParsingRules Default = new DefaultParsingRules();

        private class DefaultParsingRules : IBlockParsingRules
        {
            public string BlockIdPrefix { get { return "::"; } }

            public string BlockMetaPrefix { get { return "@"; } }

            public string BlockEndPrefix { get { return "==="; } }

            public string PackageMetaPrefix { get { return "#"; } }

            public string CommentPrefix { get { return "//"; } }

            public char[] TagDataDelimiters { get { return TagDataDelims; } }

            public char[] LineDelimiters { get { return NewlineDelim; } }

            public bool RequireExplicitBlockEnd { get { return false; } }

            public PackageMetaMode PackageMetaMode { get { return PackageMetaMode.AllowInBlock; } }
        }

        static internal readonly char[] NewlineDelim = new char[]
        {
            '\n'
        };

        static internal readonly char[] TagDataDelims = new char[]
        {
            ':', '=', ' ', '\t'
        };
    }
}