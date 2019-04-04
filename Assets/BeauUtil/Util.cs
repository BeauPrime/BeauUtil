/*
 * Copyright (C) 2017-2019. Filament Games, LLC. All rights reserved.
 * Author:  Alex Beauchesne
 * Date:    4 April 2019
 * 
 * File:    Util.cs
 * Purpose: Misc utility functions.
*/

namespace BeauUtil
{
    /// <summary>
    /// Contains utility functions.
    /// </summary>
    static public class Util
    {
        /// <summary>
        /// Remaps a value from the first range to the second.
        /// </summary>
        /// <param name="inValue">Value to remap.</param>
        /// <param name="inMin1">Min of first range.</param>
        /// <param name="inMax1">Max of first range.</param>
        /// <param name="inMin2">Min of second range.</param>
        /// <param name="inMax2">Max of second range.</param>
        static public float Remap(float inValue, float inMin1, float inMax1, float inMin2, float inMax2)
        {
            return (inValue - inMin1) / (inMax1 - inMin1) * (inMax2 - inMin2) + inMin2;
        }
    }
}
