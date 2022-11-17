/*
 * Copyright (C) 2017-2021. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    12 Jan 2021
 * 
 * File:    ScoringUtils.cs
 * Purpose: Scoring utilities for collections of objects.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace BeauUtil
{
    /// <summary>
    /// Scoring utilities for collections of objects.
    /// </summary>
    static public class ScoringUtils
    {
        public delegate float ScoreFunction<T>(T inElement);

        /// <summary>
        /// Returns the minimum-scoring element from the given list.
        /// </summary>
        static public T GetMinElement<T>(IEnumerable<T> inList, ScoreFunction<T> inDelegate)
        {
            T minVal = default(T);
            float minScore = float.MaxValue;

            float score;
            foreach(var element in inList)
            {
                score = inDelegate(element);
                if (score < minScore)
                {
                    minScore = score;
                    minVal = element;
                }
            }

            return minVal;
        }

        /// <summary>
        /// Returns the minimum-scoring element from the given list.
        /// </summary>
        static public T GetMinElement<T>(ListSlice<T> inList, ScoreFunction<T> inDelegate)
        {
            T minVal = default(T);
            float minScore = float.MaxValue;

            float score;
            T element;
            for(int i = 0; i < inList.Length; ++i)
            {
                element = inList[i];
                score = inDelegate(element);

                if (score < minScore)
                {
                    minScore = score;
                    minVal = element;
                }
            }

            return minVal;
        }

        /// <summary>
        /// Returns the minimum-scoring element from the given list.
        /// </summary>
        static public T GetMinElement<T>(T[] inList, int inStartIndex, int inLength, ScoreFunction<T> inDelegate)
        {
            T minVal = default(T);
            float minScore = float.MaxValue;

            float score;
            T element;
            for(int i = 0; i < inLength; ++i)
            {
                element = inList[inStartIndex + i];
                score = inDelegate(element);

                if (score < minScore)
                {
                    minScore = score;
                    minVal = element;
                }
            }

            return minVal;
        }

        /// <summary>
        /// Returns the minimum-scoring element from the given list.
        /// </summary>
        [MethodImpl(256)]
        static public T GetMinElement<T>(T[] inList, ScoreFunction<T> inDelegate)
        {
            return GetMinElement(inList, 0, inList.Length, inDelegate);
        }

        /// <summary>
        /// Returns the maximum-scoring element from the given list.
        /// </summary>
        static public T GetMaxElement<T>(IEnumerable<T> inList, ScoreFunction<T> inDelegate)
        {
            T maxVal = default(T);
            float maxScore = float.MinValue;

            float score;
            foreach(var element in inList)
            {
                score = inDelegate(element);
                if (score > maxScore)
                {
                    maxScore = score;
                    maxVal = element;
                }
            }

            return maxVal;
        }

        /// <summary>
        /// Returns the maximum-scoring element from the given list.
        /// </summary>
        static public T GetMaxElement<T>(ListSlice<T> inList, ScoreFunction<T> inDelegate)
        {
            T maxVal = default(T);
            float maxScore = float.MinValue;

            float score;
            T element;
            for(int i = 0; i < inList.Length; ++i)
            {
                element = inList[i];
                score = inDelegate(element);
                if (score > maxScore)
                {
                    maxScore = score;
                    maxVal = element;
                }
            }

            return maxVal;
        }

        /// <summary>
        /// Returns the maximum-scoring element from the given list.
        /// </summary>
        static public T GetMaxElement<T>(T[] inList, int inStartIndex, int inLength, ScoreFunction<T> inDelegate)
        {
            T maxVal = default(T);
            float maxScore = float.MinValue;

            float score;
            T element;
            for(int i = 0; i < inLength; ++i)
            {
                element = inList[inStartIndex + i];
                score = inDelegate(element);
                if (score > maxScore)
                {
                    maxScore = score;
                    maxVal = element;
                }
            }

            return maxVal;
        }

        /// <summary>
        /// Returns the maximum-scoring element from the given list.
        /// </summary>
        [MethodImpl(256)]
        static public T GetMaxElement<T>(T[] inList, ScoreFunction<T> inDelegate)
        {
            return GetMaxElement(inList, 0, inList.Length, inDelegate);
        }
    }
}