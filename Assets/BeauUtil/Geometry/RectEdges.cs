/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    21 August 2020
 * 
 * File:    RectEdge.cs
 * Purpose: Edges of a rectangle.
 */

using System;

namespace BeauUtil
{
    /// <summary>
    /// Flags representing edges of a rectangle.
    /// </summary>
    [Flags, LabeledEnum(false)]
    public enum RectEdges
    {
        Top = 0x01,
        Bottom = 0x02,
        Left = 0x04,
        Right = 0x08,

        [Hidden]
        Horizontal = Left | Right,
        
        [Hidden]
        Vertical = Top | Bottom,
        
        [Hidden]
        All = Top | Bottom | Left | Right,
    }
}