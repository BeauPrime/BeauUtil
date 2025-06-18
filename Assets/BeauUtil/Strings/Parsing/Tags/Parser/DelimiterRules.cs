/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    23 August 2020
 * 
 * File:    DelimiterRules.cs
 * Purpose: Delimiter rules.
 */

using System;
using System.Collections.Generic;

namespace BeauUtil.Tags
{
    /// <summary>
    /// Parsing character rules.
    /// </summary>
    public sealed class DelimiterRules
    {
        public string TagStartDelimiter;
        public string TagEndDelimiter;

        public char[] TagDataDelimiters;
        public char RegionCloseDelimiter;

        public bool RichText;
        public string[] AdditionalRichTextTags;
    }
}