/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    4 April 2019
 * 
 * File:    RNG.cs
 * Purpose: Random number generation extension methods.
 */

using System;
using System.Collections.Generic;
using UnityEngine;

using Random = System.Random;

namespace BeauUtil
{
    /// <summary>
    /// Contains extension methods for generating
    /// random values beyond Unity's static
    /// Random class.
    /// </summary>
    static public class RNG
    {
        static RNG()
        {
            Default = new Random();
            s_CurrentRandom = Default;
        }

        static private Random s_CurrentRandom;

        /// <summary>
        /// Default Random source.
        /// </summary>
        static public readonly Random Default;

        /// <summary>
        /// Current Random source.
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

        /// <summary>
        /// Returns a random sign, either -1, 0, or 1.
        /// </summary>
        static public int Sign(this Random inRandom)
        {
            return inRandom.Next(3) - 1;
        }

        /// <summary>
        /// Returns a random non-zero sign.
        /// </summary>
        static public int SignNonZero(this Random inRandom)
        {
            return Chance(inRandom, 0.5f) ? -1 : 1;
        }

        #region Bell

        /// <summary>
        /// Returns a random value, distributed with a bell curve in the given range.
        /// </summary>
        static public float BellCurve(this Random inRandom, float inMin, float inMax, int inCount = 3)
        {
            return BellCurve(inRandom, inMin, inMax, inMin, inMax, inCount);
        }

        /// <summary>
        /// Returns a random value, distributed with a bell curve in the given range.
        /// </summary>
        static public float BellCurve(this Random inRandom, float inMin, float inMax, float inOutMin, float inOutMax, int inCount = 3)
        {
            int i;
            float result;
            do
            {
                result = 0;
                for (i = 0; i < inCount; ++i)
                {
                    result += NextFloat(inRandom, inMin, inMax);
                }
                if (result < 0)
                {
                    result -= (inCount - 1);
                }
                result /= inCount;
            }
            while(result < inOutMin || result > inOutMax);
            
            return result;
        }

        #endregion // Bell

        #region Vector2

        /// <summary>
        /// Returns a normalized random vector.
        /// </summary>
        static public Vector2 NextVector2(this Random inRandom)
        {
            Vector2 vec = new Vector2(NextFloat(inRandom) - 0.5f, NextFloat(inRandom) - 0.5f);
            vec.Normalize();
            return vec;
        }

        /// <summary>
        /// Returns a normalized random vector with the given random distance.
        /// </summary>
        static public Vector2 NextVector2(this Random inRandom, float inMinDistance, float inMaxDistance)
        {
            float x = NextFloat(inRandom) - 0.5f;
            float y = NextFloat(inRandom) - 0.5f;

            float magnitude = (float)Math.Sqrt(x * x + y * y);
            x /= magnitude;
            y /= magnitude;

            float c = (float)Math.Sqrt(NextFloat(inRandom, inMinDistance * inMinDistance, inMaxDistance * inMaxDistance));

            return new Vector2(c * x, c * y);
        }

        /// <summary>
        /// Returns a random vector between the given vectors.
        /// </summary>
        static public Vector2 NextVector2(this Random inRandom, Vector2 inStart, Vector2 inEnd)
        {
            return new Vector2(inRandom.NextFloat(inStart.x, inEnd.x), inRandom.NextFloat(inStart.y, inEnd.y));
        }

        /// <summary>
        /// Returns a random vector within the given rect.
        /// </summary>
        static public Vector2 NextVector2(this Random inRandom, Rect inRect)
        {
            return NextVector2(inRandom, inRect.min, inRect.max);
        }

        /// <summary>
        /// Returns a random vector on the edge the given rect.
        /// </summary>
        static public Vector2 NextVector2OnEdge(this Random inRandom, Rect inRect)
        {
            float x, y;

            int side = inRandom.Next(0, 4);
            switch (side)
            {
                // bottom
                case 0:
                    x = NextFloat(inRandom, inRect.xMin, inRect.xMax);
                    y = inRect.yMin;
                    break;

                // left
                case 1:
                    x = inRect.xMin;
                    y = NextFloat(inRandom, inRect.yMin, inRect.yMax);
                    break;

                // top
                case 2:
                    x = NextFloat(inRandom, inRect.xMin, inRect.xMax);
                    y = inRect.yMax;
                    break;

                // right
                case 3:
                default:
                    x = inRect.xMax;
                    y = NextFloat(inRandom, inRect.yMin, inRect.yMax);
                    break;
            }

            return new Vector2(x, y);
        }

        #endregion // Vector2

        #region Vector3

        /// <summary>
        /// Returns a normalized random vector.
        /// </summary>
        static public Vector3 NextVector3(this Random inRandom)
        {
            Vector3 vec = new Vector3(NextFloat(inRandom) - 0.5f, NextFloat(inRandom) - 0.5f, NextFloat(inRandom) - 0.5f);
            vec.Normalize();
            return vec;
        }

        /// <summary>
        /// Returns a normalized random vector with the given random distance.
        /// </summary>
        static public Vector3 NextVector3(this Random inRandom, float inMinDistance, float inMaxDistance)
        {
            float x = NextFloat(inRandom) - 0.5f;
            float y = NextFloat(inRandom) - 0.5f;
            float z = NextFloat(inRandom) - 0.5f;

            float magnitude = (float)Math.Sqrt(x * x + y * y + z * z);
            x /= magnitude;
            y /= magnitude;
            z /= magnitude;

            float c = (float)Math.Pow(NextFloat(inRandom, inMinDistance * inMinDistance * inMinDistance, inMaxDistance * inMaxDistance * inMaxDistance), 1f / 3);

            return new Vector3(c * x, c * y, c * z);
        }

        /// <summary>
        /// Returns a random vector between the given vectors.
        /// </summary>
        static public Vector3 NextVector3(this Random inRandom, Vector3 inStart, Vector3 inEnd)
        {
            return new Vector3(inRandom.NextFloat(inStart.x, inEnd.x), inRandom.NextFloat(inStart.y, inEnd.y), inRandom.NextFloat(inStart.z, inEnd.z));
        }

        /// <summary>
        /// Returns a random vector within the given bounds.
        /// </summary>
        static public Vector3 NextVector3(this Random inRandom, Bounds inBounds)
        {
            return NextVector3(inRandom, inBounds.min, inBounds.max);
        }

        /// <summary>
        /// Returns a random vector on the edge of the given bounds.
        /// </summary>
        static public Vector3 NextVector3OnEdge(this Random inRandom, Bounds inBounds)
        {
            float x, y, z;

            Vector3 min = inBounds.min;
            Vector3 max = inBounds.max;

            int face = inRandom.Next(0, 6);
            switch (face)
            {
                // bottom
                case 0:
                    x = NextFloat(inRandom, min.x, max.x);
                    y = min.y;
                    z = NextFloat(inRandom, min.z, max.z);
                    break;

                // left
                case 1:
                    x = min.x;
                    y = NextFloat(inRandom, min.y, max.y);
                    z = NextFloat(inRandom, min.z, max.z);
                    break;

                // front
                case 2:
                    x = NextFloat(inRandom, min.x, max.x);
                    y = NextFloat(inRandom, min.y, max.y);
                    z = min.z;
                    break;

                // right
                case 3:
                    x = max.x;
                    y = NextFloat(inRandom, min.y, max.y);
                    z = NextFloat(inRandom, min.z, max.z);
                    break;

                // back
                case 4:
                    x = NextFloat(inRandom, min.x, max.x);
                    y = NextFloat(inRandom, min.y, max.y);
                    z = max.z;
                    break;

                // top
                case 5:
                default:
                    x = NextFloat(inRandom, min.x, max.x);
                    y = max.y;
                    z = NextFloat(inRandom, min.z, max.z);
                    break;
            }

            return new Vector3(x, y, z);
        }

        /// <summary>
        /// Returns a random vector within the given sphere.
        /// </summary>
        static public Vector3 NextVector3(this Random inRandom, Vector3 inCenter, float inMinDistance, float inMaxDistance)
        {
            return NextVector3(inRandom, inMinDistance, inMaxDistance) + inCenter;
        }

        /// <summary>
        /// Returns a random vector within the given bounding sphere.
        /// </summary>
        static public Vector3 NextVector3(this Random inRandom, BoundingSphere inSphere)
        {
            return NextVector3(inRandom, inSphere.position, 0, inSphere.radius);
        }

        /// <summary>
        /// Returns a random vector within the given bounding sphere.
        /// </summary>
        static public Vector3 NextVector3OnEdge(this Random inRandom, BoundingSphere inSphere)
        {
            return NextVector3(inRandom, inSphere.position, inSphere.radius, inSphere.radius);
        }

        #endregion // Vector3

        #region Color

        /// <summary>
        /// Returns a random color.
        /// </summary>
        static public Color NextColor(this Random inRandom)
        {
            return new Color(NextFloat(inRandom), NextFloat(inRandom), NextFloat(inRandom), NextFloat(inRandom));
        }

        /// <summary>
        /// Returns a random color between the given colors.
        /// </summary>
        static public Color NextColor(this Random inRandom, Color inStart, Color inEnd)
        {
            return new Color(inRandom.NextFloat(inStart.r, inEnd.r), inRandom.NextFloat(inStart.g, inEnd.g), inRandom.NextFloat(inStart.b, inEnd.b), inRandom.NextFloat(inStart.a, inEnd.a));
        }

        /// <summary>
        /// Returns a random color on the given gradient.
        /// </summary>
        static public Color NextColor(this Random inRandom, Gradient inGradient)
        {
            return inGradient.Evaluate(inRandom.NextFloat());
        }

        #endregion // Color

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
        static public T Choose<T>(this Random inRandom, IReadOnlyList<T> inChoices)
        {
            if (inChoices.Count == 0)
                throw new IndexOutOfRangeException();
            return inChoices[inRandom.Next(inChoices.Count)];
        }

        /// <summary>
        /// Returns a random element from the given array.
        /// </summary>
        static public T Choose<T>(this Random inRandom, T[] inChoices)
        {
            if (inChoices.Length == 0)
                throw new IndexOutOfRangeException();
            return inChoices[inRandom.Next(inChoices.Length)];
        }

        /// <summary>
        /// Returns a random element from the given set.
        /// </summary>
        static public T Choose<T>(this Random inRandom, WeightedSet<T> inChoices)
        {
            if (inChoices.Count == 0)
                throw new IndexOutOfRangeException();
            return inChoices.GetItemNormalized(inRandom.NextFloat());
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
        /// Shuffles the given array.
        /// </summary>
        static public void Shuffle<T>(this Random inRandom, RingBuffer<T> inBuffer)
        {
            int i = inBuffer.Count;
            int j;
            while (--i > 0)
            {
                T old = inBuffer[i];
                inBuffer[i] = inBuffer[j = inRandom.Next(i + 1)];
                inBuffer[j] = old;
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

        /// <summary>
        /// Shuffles a region within the given array.
        /// </summary>
        static public void Shuffle<T>(this Random inRandom, RingBuffer<T> inBuffer, int inIndex, int inLength)
        {
            int i = Math.Min(inIndex + inLength, inBuffer.Count);
            int j;
            while (--i > inIndex)
            {
                T old = inBuffer[i];
                inBuffer[i] = inBuffer[j = inRandom.Next(inIndex, i + 1)];
                inBuffer[j] = old;
            }
        }

        #endregion
    }
}