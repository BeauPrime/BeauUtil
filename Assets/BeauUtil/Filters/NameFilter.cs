/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    4 Dec 2020
 * 
 * File:    NameFilter.cs
 * Purpose: Filter for a GameObject vs a Name.
 */

using UnityEngine;

namespace BeauUtil
{
    /// <summary>
    /// Filter for a GameObject vs a Name.
    /// </summary>
    public struct NameFilter : IObjectFilter<GameObject>
    {
        public string Pattern;

        public bool Allow(GameObject inObject)
        {
            if (string.IsNullOrEmpty(Pattern))
                return true;

            return StringUtils.WildcardMatch(inObject.name, Pattern);
        }
    }
}