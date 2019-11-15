/*
 * Copyright (C) 2017-2019. Filament Games, LLC. All rights reserved.
 * Author:  Alex Beauchesne
 * Date:    24 Oct 2019
 * 
 * File:    ArrayUtils.cs
 * Purpose: Array modification utilities.
 */

using System;

namespace BeauUtil
{
    /// <summary>
    /// Utility methods for dealing with arrays.
    /// </summary>
    static public class ArrayUtils
    {
        /// <summary>
        /// Casts an array to an array of a more derived type.
        /// </summary>
        static public C[] CastDown<T, C>(T[] inArray) where C : T
        {
            if (inArray == null)
                return null;

            C[] copy = new C[inArray.Length];
            for (int i = 0; i < inArray.Length; ++i)
            {
                copy[i] = (C) inArray[i];
            }
            return copy;
        }

        /// <summary>
        /// Casts an array to an array of a base type.
        /// </summary>
        static public C[] CastUp<T, C>(T[] inArray) where T : C
        {
            if (inArray == null)
                return null;

            C[] copy = new C[inArray.Length];
            for (int i = 0; i < inArray.Length; ++i)
            {
                copy[i] = (C) inArray[i];
            }
            return copy;
        }

        /// <summary>
        /// Adds an element to the end of an array.
        /// </summary>
        static public void Add<T>(ref T[] ioArray, T inItem)
        {
            int length = ioArray == null ? 0 : ioArray.Length;
            Array.Resize(ref ioArray, length + 1);
            ioArray[length] = inItem;
        }

        /// <summary>
        /// Inserts an element at an index in an array.
        /// </summary>
        static public void Insert<T>(ref T[] ioArray, int inIndex, T inItem)
        {
            if (ioArray == null)
            {
                ioArray = new T[1] { inItem };
                return;
            }

            T[] newArr = new T[ioArray.Length + 1];
            if (inIndex < 0)
                inIndex = 0;
            else if (inIndex > newArr.Length)
                inIndex = newArr.Length;

            if (inIndex > 0)
                Array.Copy(ioArray, 0, newArr, 0, inIndex);
            if (inIndex < ioArray.Length - 1)
                Array.Copy(ioArray, inIndex, newArr, inIndex + 1, ioArray.Length - inIndex - 1);

            newArr[inIndex] = inItem;
        }

        /// <summary>
        /// Returns the index of an element in an array.
        /// </summary>
        static public int IndexOf<T>(T[] inArray, T inItem)
        {
            if (inArray == null || inArray.Length == 0)
                return -1;

            return Array.IndexOf(inArray, inItem);
        }

        /// <summary>
        /// Returns if an element is present in an array.
        /// </summary>
        static public bool Contains<T>(T[] inArray, T inItem)
        {
            return IndexOf(inArray, inItem) >= 0;
        }

        /// <summary>
        /// Removes an element from an array.
        /// </summary>
        static public void Remove<T>(ref T[] ioArray, T inItem)
        {
            int index = IndexOf(ioArray, inItem);
            if (index >= 0)
                RemoveAt(ref ioArray, index);
        }

        /// <summary>
        /// Removes the element at the given index from an array.
        /// </summary>
        static public void RemoveAt<T>(ref T[] ioArray, int inIndex)
        {
            if (ioArray == null || inIndex < 0 || inIndex >= ioArray.Length)
                return;

            T[] newArr = new T[ioArray.Length - 1];
            if (inIndex > 0)
                Array.Copy(ioArray, 0, newArr, 0, inIndex);
            if (inIndex < ioArray.Length - 1)
                Array.Copy(ioArray, inIndex + 1, newArr, inIndex, ioArray.Length - inIndex - 1);

            ioArray = newArr;
        }

        /// <summary>
        /// Clears all elements from an array.
        /// </summary>
        static public void Clear<T>(ref T[] ioArray)
        {
            if (ioArray != null)
            {
                Array.Clear(ioArray, 0, ioArray.Length);
                Array.Resize(ref ioArray, 0);
            }
        }
    }
}