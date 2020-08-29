/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    23 August 2020
 * 
 * File:    IDataBlockPackage.cs
 * Purpose: Package of data blocks.
 */

using System.Collections.Generic;

namespace BeauUtil.Blocks
{
    public interface IDataBlockPackage<T> : IReadOnlyCollection<T>
        where T : class, IDataBlock
    {
    }
}