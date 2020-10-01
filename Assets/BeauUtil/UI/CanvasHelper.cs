/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    30 Sept 2020
 * 
 * File:    CanvasHelper.cs
 * Purpose: Helper methods for canvas objects.
 */

using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace BeauUtil
{
    /// <summary>
    /// Canvas object helpers.
    /// </summary>
    static public class CanvasHelper
    {
        /// <summary>
        /// Returns the first active toggle.
        /// </summary>
        static public Toggle ActiveToggle(this ToggleGroup inGroup)
        {
            IEnumerable<Toggle> all = inGroup.ActiveToggles();
            var iter = all.GetEnumerator();
            if (iter.MoveNext())
                return iter.Current;
            return null;
        } 
    }
}