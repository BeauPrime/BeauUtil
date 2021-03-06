/*
 * Copyright (C) 2017-2021. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    26 Feb 20201
 * 
 * File:    IIntrusiveLLNode.cs
 * Purpose: Interface for an intrusively tracked linked list node.
 */

namespace BeauUtil
{
    /// <summary>
    /// Interface for an intrusively-tracked linked list node.
    /// </summary>
    public interface IIntrusiveLLNode<T> where T : class
    {
        T Previous { get; set; }
        T Next { get; set; }
    }
}