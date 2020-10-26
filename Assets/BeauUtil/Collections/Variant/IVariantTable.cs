/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    15 Oct 2020
 * 
 * File:    IVariantTable.cs
 * Purpose: Interface for variant tables.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace BeauUtil.Variants
{
    /// <summary>
    /// Collection of named variant values.
    /// </summary>
    public interface IVariantTable : IDisposable, IEnumerable<NamedVariant>, IReadOnlyList<NamedVariant>
    {
        StringHash32 Name { get; set; }
        IVariantTable Base { get; set; }

        int Capacity { get; set; }
        
        void CopyTo(IVariantTable inTarget);
        void Optimize();
        
        Variant Get(StringHash32 inId);
        void Set(StringHash32 inId, Variant inValue);
        bool Has(StringHash32 inId);
        bool Delete(StringHash32 inId);
        void Clear();
        void Reset();
        
        Variant this[StringHash32 inId] { get; set; }
        
        bool TryLookup(StringHash32 inId, out Variant outValue);
        void Modify(StringHash32 inId, VariantModifyOperator inOperator, Variant inOperand);
        
        string ToDebugString();
    }
}