/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    23 August 2020
 * 
 * File:    IReplaceProcessor.cs
 * Purpose: Replace processor.
 */

namespace BeauUtil.Tags
{
    /// <summary>
    /// Interface for parsing replace tags.
    /// </summary>
    public interface IReplaceProcessor
    {
        bool TryReplace(TagData inData, object inContext, out string outReplace);
        bool TryReplace(char inCode, object inContext, out string outReplace);
    }
}