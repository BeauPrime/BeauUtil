/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    8 Oct 2019
 * 
 * File:    DefaultAssetAttribute.cs
 * Purpose: Specifies a default asset path for an asset reference field.
 */

using System;

namespace BeauUtil
{
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class DefaultAssetAttribute : Attribute
    {
        public string AssetName { get; private set; }

        public DefaultAssetAttribute(string inName)
        {
            AssetName = inName;
        }
    }
}