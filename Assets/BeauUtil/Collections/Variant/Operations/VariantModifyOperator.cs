/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    16 Sept 2020
 * 
 * File:    VariantModifyOperator.cs
 * Purpose: Operator for variant modification.
 */

namespace BeauUtil.Variants
{
    /// <summary>
    /// Modify operators
    /// </summary>
    public enum VariantModifyOperator : byte
    {
        Set,
        Add,
        Subtract,
        Multiply,
        Divide
    }
}