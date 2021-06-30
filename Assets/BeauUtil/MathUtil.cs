/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    4 April 2019
 * 
 * File:    MathUtil.cs
 * Purpose: Math utility functions.
*/

using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace BeauUtil
{
    /// <summary>
    /// Contains math utility functions.
    /// </summary>
    static public class MathUtil
    {
        /// <summary>
        /// Square root of 2.
        /// </summary>
        public const float SQRT_2 = 1.41421356237f;

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

        /// <summary>
        /// Performs a safe modulo that consistently wraps correctly for negative values.
        /// </summary>
        [MethodImpl(256)]
        static public float SafeMod(float inA, float inB)
        {
            return ((inA % inB) + inB) % inB;
        }

        /// <summary>
        /// Performs a safe modulo that consistently wraps correctly for negative values.
        /// </summary>
        [MethodImpl(256)]
        static public float SafeMod(int inA, int inB)
        {
            return ((inA % inB) + inB) % inB;
        }

        /// <summary>
        /// Returns the difference between two angles, in degrees.
        /// </summary>
        static public float DegreeAngleDifference(float inDegA, float inDegB)
        {
            return SafeMod(180 + inDegB - inDegA, 360) - 180;
        }

        /// <summary>
        /// Returns the difference between two angles, in radians.
        /// </summary>
        static public float RadianAngleDifference(float inRadA, float inRadB)
        {
            return SafeMod(Mathf.PI + inRadB - inRadA, Mathf.PI * 2) - Mathf.PI;
        }
    }
}
