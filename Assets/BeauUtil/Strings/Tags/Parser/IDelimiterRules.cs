/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    23 August 2020
 * 
 * File:    IDelimiterRules.cs
 * Purpose: Delimiter rules.
 */

using System;
using System.Collections.Generic;

namespace BeauUtil.Tags
{
    /// <summary>
    /// Parsing character rules.
    /// </summary>
    public interface IDelimiterRules
    {
        string TagStartDelimiter { get; }
        string TagEndDelimiter { get; }

        char[] TagDataDelimiters { get; }
        char RegionCloseDelimiter { get; }

        bool RichText { get; }
        IEnumerable<string> AdditionalRichTextTags { get; }
    }
}