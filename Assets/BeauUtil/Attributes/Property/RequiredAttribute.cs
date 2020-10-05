/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    4 April 2019
 * 
 * File:    RequiredAttribute.cs
 * Purpose: Marks a property as being required to not be default.
 */

using UnityEngine;

namespace BeauUtil
{
    /// <summary>
    /// Marks a property as being required to not be default.
    /// </summary>
    public sealed class RequiredAttribute : PropertyAttribute
    {
        public ComponentLookupDirection? AutoAssignDirection { get; private set; }

        public RequiredAttribute() { }
        public RequiredAttribute(ComponentLookupDirection inAutoAssignDirection) { AutoAssignDirection = inAutoAssignDirection; }
    }
}