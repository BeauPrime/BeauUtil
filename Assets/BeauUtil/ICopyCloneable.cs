/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    4 Feb 2020
 * 
 * File:    ICloneable.cs
 * Purpose: Interface for cloning and copying objects.
 */

namespace BeauUtil
{
    /// <summary>
    /// Interface for generating a clone of a particular object,
    /// or for copying data to another object of the same type.
    /// </summary>
    public interface ICopyCloneable<T>
    {
        T Clone();
        void CopyFrom(T inClone);
    }
}