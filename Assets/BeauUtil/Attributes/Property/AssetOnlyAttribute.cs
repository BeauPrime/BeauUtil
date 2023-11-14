/*
 * Copyright (C) 2023. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    13 Nov 2023
 * 
 * File:    AssetOnlyAttribute.cs
 * Purpose: Marks a serializable object reference as requiring
 *          an asset, excluding scene references.
 */

using UnityEngine;

namespace BeauUtil
{
    /// <summary>
    /// Marks an object reference as requiring an asset, not a scene reference.
    /// </summary>
    public sealed class AssetOnlyAttribute : PropertyAttribute
    {
    }
}