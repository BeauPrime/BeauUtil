/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    29 August 2020
 * 
 * File:    BlockMetaAttribute.cs
 * Purpose: Attribute marking a meta attribute for block data.
 */

using System;

namespace BeauUtil.Blocks
{
    /// <summary>
    /// Attribute marking a meta tag for block data.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class BlockMetaAttribute : Attribute
    {
        public string Name { get; internal set; }

        public BlockMetaAttribute() { }
        public BlockMetaAttribute(string inName) { Name = inName; }
    }
}