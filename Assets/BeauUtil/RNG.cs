/*
 * Copyright (C) 2017-2019. Filament Games, LLC. All rights reserved.
 * Author:  Alex Beauchesne
 * Date:    4 April 2019
 * 
 * File:    RNG.cs
 * Purpose: Random number generation extension methods.
*/

using System;
using System.Collections.Generic;

namespace BeauUtil
{
    /// <summary>
    /// Contains extension methods for generating
    /// random values beyond Unity's static
    /// Random class.
    /// </summary>
    static public class RNG
    {
        static private Random s_CurrentRandom = new Random();

        /// <summary>
        /// Current Randomizer object.
        /// </summary>
        static public Random Instance
        {
            get { return s_CurrentRandom; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException();
                s_CurrentRandom = value;
            }
        }

        /// <summary>
        /// Retrives a random floating point value.
        /// </summary>
        static public float NextFloat(this Random inRandom)
        {
            return (float)inRandom.NextDouble();
        }

        /// <summary>
        /// Retrieves a random floating point value between
        /// 0 and the given maximum.
        /// </summary>
        static public float NextFloat(this Random inRandom, float inMax)
        {
            return NextFloat(inRandom) * inMax;
        }

        /// <summary>
        /// Retrieves a random floating point within the
        /// given range.
        /// </summary>
        static public float NextFloat(this Random inRandom, float inMin, float inMax)
        {
            return inMin + NextFloat(inRandom) * (inMax - inMin);
        }

        /// <summary>
        /// Retrieves a random bool.
        /// </summary>
        static public bool NextBool(this Random inRandom)
        {
            return inRandom.NextDouble() < 0.5;
        }

        /// <summary>
        /// Retrieves a random bool with a certain percentage
        /// chance of returning true.
        /// </summary>
        static public bool Chance(this Random inRandom, float inPercent)
        {
            return NextFloat(inRandom) < inPercent;
        }

        #region Collections

        /// <summary>
        /// Returns a random element from the given parameters.
        /// </summary>
        static public T Choose<T>(this Random inRandom, T inFirstChoice, T inSecondChoice, params T[] inMoreChoices)
        {
            int index = inRandom.Next(inMoreChoices.Length + 2);
            if (index == 0)
                return inFirstChoice;
            else if (index == 1)
                return inSecondChoice;
            else
                return inMoreChoices[index - 2];
        }

        /// <summary>
        /// Returns a random element from the given list.
        /// </summary>
        static public T Choose<T>(this Random inRandom, IList<T> inChoices)
        {
            if (inChoices.Count == 0)
                throw new IndexOutOfRangeException();
            return inChoices[inRandom.Next(inChoices.Count)];
        }

        /// <summary>
        /// Shuffles the given list.
        /// </summary>
        static public void Shuffle<T>(this Random inRandom, IList<T> inList)
        {
            int i = inList.Count;
            int j;
            while (--i > 0)
            {
                T old = inList[i];
                inList[i] = inList[j = inRandom.Next(i + 1)];
                inList[j] = old;
            }
        }

        /// <summary>
        /// Shuffles the given array.
        /// </summary>
        static public void Shuffle<T>(this Random inRandom, T[] inArray)
        {
            int i = inArray.Length;
            int j;
            while (--i > 0)
            {
                T old = inArray[i];
                inArray[i] = inArray[j = inRandom.Next(i + 1)];
                inArray[j] = old;
            }
        }

        /// <summary>
        /// Shuffles a region within the given list.
        /// </summary>
        static public void Shuffle<T>(this Random inRandom, IList<T> inList, int inIndex, int inLength)
        {
            int i = Math.Min(inIndex + inLength, inList.Count);
            int j;
            while (--i > inIndex)
            {
                T old = inList[i];
                inList[i] = inList[j = inRandom.Next(inIndex, i + 1)];
                inList[j] = old;
            }
        }

        /// <summary>
        /// Shuffles a region within the given array.
        /// </summary>
        static public void Shuffle<T>(this Random inRandom, T[] inArray, int inIndex, int inLength)
        {
            int i = Math.Min(inIndex + inLength, inArray.Length);
            int j;
            while (--i > inIndex)
            {
                T old = inArray[i];
                inArray[i] = inArray[j = inRandom.Next(inIndex, i + 1)];
                inArray[j] = old;
            }
        }

        #endregion
    }
}
