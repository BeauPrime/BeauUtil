/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    23 August 2020
 * 
 * File:    IEventProcessor.cs
 * Purpose: Event processing interface.
 */

using System;
using System.Collections.Generic;

namespace BeauUtil.Tags
{
    /// <summary>
    /// Interface for parsing event tags.
    /// </summary>
    public interface IEventProcessor
    {
        bool TryProcess(TagData inData, object inContext, out TagEventData outEvent);
    }
}