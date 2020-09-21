/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    16 Sept 2020
 * 
 * File:    VariantCompareOperator.cs
 * Purpose: Operator for variant comparison.
 */

namespace BeauUtil.Variants
{
    /// <summary>
    /// Comparison operators
    /// </summary>
    public enum VariantCompareOperator : byte
    {
        LessThan,
        LessThanOrEqualTo,
        EqualTo,
        NotEqualTo,
        GreaterThanOrEqualTo,
        GreaterThan,

        True,
        False,
        Exists,
        DoesNotExist
    }
}