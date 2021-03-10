/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    24 Oct 2019
 * 
 * File:    ArrayUtils.cs
 * Purpose: Array modification utilities.
 */

using System;
using System.Collections.Generic;

namespace BeauUtil
{
    /// <summary>
    /// Utility methods for dealing with arrays.
    /// </summary>
    static public class ArrayUtils
    {
        /// <summary>
        /// Creates an array from the given collection.
        /// </summary>
        static public T[] CreateFrom<T>(IReadOnlyCollection<T> inSource)
        {
            if (inSource == null)
                return null;

            T[] copy = new T[inSource.Count];
            int idx = 0;
            foreach(var obj in inSource)
                copy[idx++] = obj;
            return copy;
        }

        /// <summary>
        /// Creates an array from the given collection.
        /// </summary>
        static public C[] MapFrom<T, C>(IReadOnlyCollection<T> inSource, Converter<T, C> inConverter)
        {
            if (inSource == null)
                return null;
            if (inConverter == null)
                throw new ArgumentNullException("inMapper");

            C[] copy = new C[inSource.Count];
            int idx = 0;
            foreach(var obj in inSource)
                copy[idx++] = inConverter(obj);
            return copy;
        }

        /// <summary>
        /// Creates an array from the given collection.
        /// </summary>
        static public C[] MapFrom<T, C>(T[] inSource, Converter<T, C> inConverter)
        {
            if (inSource == null)
                return null;
            if (inConverter == null)
                throw new ArgumentNullException("inMapper");

            return Array.ConvertAll(inSource, inConverter);
        }

        /// <summary>
        /// Creates an array from the given collection.
        /// </summary>
        static public C[] MapFrom<T, C>(IReadOnlyList<T> inSource, Converter<T, C> inConverter)
        {
            if (inSource == null)
                return null;
            if (inConverter == null)
                throw new ArgumentNullException("inMapper");

            C[] copy = new C[inSource.Count];
            for(int i = 0; i < copy.Length; ++i)
                copy[i] = inConverter(inSource[i]);
            return copy;
        }

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
        /// Removes the element at the given index by swapping it with the last element.
        /// </summary>
        static public void FastRemoveAt<T>(T[] ioArray, ref int ioLength, int inIndex)
        {
            if (ioArray == null || inIndex < 0 || inIndex >= ioLength)
                return;

            int end = ioLength - 1;
            if (inIndex != end)
            {
                T endCopy = ioArray[end];
                ioArray[end] = default(T);
                ioArray[inIndex] = endCopy;
            }

            --ioLength;
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

        /// <summary>
        /// Reverses the given array.
        /// </summary>
        static public void Reverse<T>(T[] ioArray)
        {
            if (ioArray != null)
            {
                Reverse<T>(ioArray, 0, ioArray.Length);
            }
        }

        /// <summary>
        /// Reverses the given array.
        /// </summary>
        static public void Reverse<T>(T[] ioArray, int inIndex, int inLength)
        {
            if (ioArray != null)
            {
                if (inLength < 0 || inLength > ioArray.Length)
                    throw new ArgumentOutOfRangeException("inLength");
                if (inIndex < 0 || inIndex + inLength > ioArray.Length)
                    throw new ArgumentOutOfRangeException("inIndex");

                int left = inIndex;
                int right = inIndex + inLength - 1;
                while(left < right)
                {
                    T leftVal = ioArray[left];
                    ioArray[left] = ioArray[right];
                    ioArray[right] = leftVal;

                    left++;
                    right--;
                }
            }
        }

        /// <summary>
        /// Clears all elements from the array and sets the reference to null.
        /// </summary>
        static public void Dispose<T>(ref T[] ioArray)
        {
            if (ioArray != null)
            {
                Array.Clear(ioArray, 0, ioArray.Length);
                ioArray = null;
            }
        }

        /// <summary>
        /// Returns if two arrays have the same content.
        /// </summary>
        static public bool ContentEquals<T>(T[] inA, T[] inB) where T : IEquatable<T>
        {
            if (inA == null)
                return inB == null;
            if (inB == null)
                return false;

            if (inA.Length != inB.Length)
                return false;

            var comparer = EqualityComparer<T>.Default;

            for(int i = 0, len = inA.Length; i < len; ++i)
            {
                if (!comparer.Equals(inA[i], inB[i]))
                    return false;
            }

            return true;
        }
    }
}