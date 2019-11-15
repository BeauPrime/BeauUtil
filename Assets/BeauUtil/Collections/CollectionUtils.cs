/*
 * Copyright (C) 2017-2019. Filament Games, LLC. All rights reserved.
 * Author:  Alex Beauchesne
 * Date:    24 Oct 2019
 * 
 * File:    CollectionUtils.cs
 * Purpose: Utility methods for collections.
 */

using System;
using System.Collections;
using System.Collections.Generic;

namespace BeauUtil
{
    static public class CollectionUtils
    {
        static internal void ClampRange(int inTotalLength, int inRangeStart, ref int ioRangeLength)
        {
            int maxLength = (inTotalLength - inRangeStart);
            if (ioRangeLength < 0)
                ioRangeLength = maxLength;
            else if (ioRangeLength > maxLength)
                ioRangeLength = maxLength;
        }
    }
}