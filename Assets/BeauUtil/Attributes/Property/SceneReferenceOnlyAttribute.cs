/*
 * Copyright (C) 2023. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    13 Nov 2023
 * 
 * File:    SceneReferenceOnlyAttribute.cs
 * Purpose: Marks a serializable object reference as requiring
 *          a scene reference, excluding assets.
 */

using UnityEngine;

namespace BeauUtil
{
    /// <summary>
    /// Marks an object reference as requiring a scene reference, not an asset.
    /// </summary>
    public sealed class SceneReferenceOnlyAttribute : PropertyAttribute
    {
    }
}