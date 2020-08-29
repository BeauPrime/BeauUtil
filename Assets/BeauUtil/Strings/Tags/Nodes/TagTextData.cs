/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    23 August 2020
 * 
 * File:    TagTextData.cs
 * Purpose: Information for a text node.
 */

#if CSHARP_7_3_OR_NEWER
#define EXPANDED_REFS
#endif // CSHARP_7_3_OR_NEWER

using System;

namespace BeauUtil.Tags
{
    /// <summary>
    /// Arguments for a text node.
    /// </summary>
    public struct TagTextData
    {
        /// <summary>
        /// Number of visible characters to type out.
        /// </summary>
        public uint VisibleCharacterCount;

        /// <summary>
        /// Resets arguments.
        /// </summary>
        public void Reset()
        {
            VisibleCharacterCount = 0;
        }
    }
}