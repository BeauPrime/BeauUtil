/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    9 Dec 2019
 * 
 * File:    ListUtils.cs
 * Purpose: List modification utilities.
 */

using System;
using System.Collections.Generic;
using UnityEngine;

namespace BeauUtil
{
    /// <summary>
    /// Utility methods for dealing with lists.
    /// </summary>
    static public class ListUtils
    {
        /// <summary>
        /// Ensures a certain capacity for the list.
        /// </summary>
        static public void EnsureCapacity<T>(ref List<T> ioList, int inCapacity)
        {
            if (ioList == null)
            {
                ioList = new List<T>(inCapacity);
            }
            else if (ioList.Capacity < inCapacity)
            {
                ioList.Capacity = inCapacity;
            }
        }

        /// <summary>
        /// Removes an element from the given list by swapping.
        /// Does not preserve order.
        /// </summary>
        static public void FastRemoveAt<T>(IList<T> ioList, int inIndex)
        {
            int end = ioList.Count - 1;
            if (inIndex != end)
                ioList[inIndex] = ioList[end];
            ioList.RemoveAt(end);
        }
    }
}