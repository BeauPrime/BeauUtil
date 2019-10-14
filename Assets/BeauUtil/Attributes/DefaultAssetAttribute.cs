/*
 * Copyright (C) 2017-2019. Filament Games, LLC. All rights reserved.
 * Author:  Alex Beauchesne
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