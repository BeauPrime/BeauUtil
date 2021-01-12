/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    4 Dec 2020
 * 
 * File:    TagsFilter.cs
 * Purpose: Filter for a GameObject vs a set of tags.
 */

using UnityEngine;

namespace BeauUtil
{
    /// <summary>
    /// Filter a GameObject by up to 8 tags.
    /// </summary>
    public struct TagsFilter : IObjectFilter<GameObject>
    {
        public TempList8<string> Tags;

        public TagsFilter(string inTag)
        {
            Tags = default(TempList8<string>);
            Tags.Add(inTag);
        }

        public bool Allow(GameObject inObject)
        {
            if (Tags.Count == 0)
                return true;

            for(int i = Tags.Count - 1; i >= 0; --i)
            {
                if (inObject.CompareTag(Tags[i]))
                    return true;
            }

            return false;
        }
    }
}