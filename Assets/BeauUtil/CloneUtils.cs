/*
 * Copyright (C) 2017-2020. Filament Games, LLC. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    4 Feb 2020
 * 
 * File:    CloneUtils.cs
 * Purpose: Utility methods for cloning and copying objects.
 */

using System;
using System.Collections.Generic;

namespace BeauUtil
{
    static public class CloneUtils
    {
        #region Clone

        /// <summary>
        /// Returns a copy of the given object.
        /// </summary>
        static public T Clone<T>(T inObject) where T : ICopyCloneable<T>
        {
            if (inObject == null)
                return default(T);

            return inObject.Clone();
        }

        /// <summary>
        /// Returns a shallow copy of the given array.
        /// </summary>
        static public T[] Clone<T>(T[] inArray)
        {
            if (inArray == null)
                return null;

            return (T[]) inArray.Clone();
        }

        /// <summary>
        /// Returns a shallow copy of the given list.
        /// </summary>
        static public List<T> Clone<T>(List<T> inArray)
        {
            if (inArray == null)
                return null;

            return new List<T>(inArray);
        }

        /// <summary>
        /// Returns a shallow copy of the given hashset.
        /// </summary>
        static public HashSet<T> Clone<T>(HashSet<T> inSet)
        {
            if (inSet == null)
                return null;

            return new HashSet<T>(inSet);
        }

        /// <summary>
        /// Returns a shallow copy of the given dictionary.
        /// </summary>
        static public Dictionary<K, V> Clone<K, V>(Dictionary<K, V> inMap)
        {
            if (inMap == null)
                return null;

            return new Dictionary<K, V>(inMap);
        }

        /// <summary>
        /// Returns a deep copy of the given array.
        /// </summary>
        static public T[] DeepClone<T>(T[] inArray) where T : ICopyCloneable<T>
        {
            if (inArray == null)
                return null;

            int arrLength = inArray.Length;
            T[] clone = new T[arrLength];
            for (int i = 0; i < arrLength; ++i)
            {
                clone[i] = Clone(inArray[i]);
            }
            return clone;
        }

        /// <summary>
        /// Returns a deep copy of the given list.
        /// </summary>
        static public List<T> DeepClone<T>(List<T> inArray) where T : ICopyCloneable<T>
        {
            if (inArray == null)
                return null;

            int arrLength = inArray.Count;
            List<T> clone = new List<T>(inArray.Capacity);
            for (int i = 0; i < arrLength; ++i)
            {
                clone.Add(Clone(inArray[i]));
            }
            return clone;
        }

        /// <summary>
        /// Returns a shallow copy of the given hashset.
        /// </summary>
        static public HashSet<T> DeepClone<T>(HashSet<T> inSet) where T : ICopyCloneable<T>
        {
            if (inSet == null)
                return null;

            HashSet<T> clone = new HashSet<T>();
            foreach (var val in inSet)
            {
                clone.Add(Clone(val));
            }
            return clone;
        }

        /// <summary>
        /// Returns a shallow copy of the given dictionary.
        /// </summary>
        static public Dictionary<K, V> DeepClone<K, V>(Dictionary<K, V> inMap) where V : ICopyCloneable<V>
        {
            if (inMap == null)
                return null;

            Dictionary<K, V> clone = new Dictionary<K, V>();
            foreach (var kv in inMap)
            {
                clone.Add(kv.Key, Clone(kv.Value));
            }
            return clone;
        }

        #endregion // Clone

        #region Copy

        /// <summary>
        /// Copys data from one object into another.
        /// </summary>
        static public void CopyFrom<T>(ref T ioDest, T inSource) where T : ICopyCloneable<T>
        {
            if (inSource == null)
            {
                ioDest = default(T);
            }
            else
            {
                if (ioDest == null || ioDest.GetType() != inSource.GetType())
                    ioDest = inSource.Clone();
                else
                    ioDest.CopyFrom(ioDest);
            }
        }

        /// <summary>
        /// Copys an array into another array.
        /// </summary>
        static public void CopyFrom<T>(ref T[] ioDest, T[] inSource)
        {
            if (inSource == null)
            {
                ArrayUtils.Clear(ref ioDest);
                ioDest = null;
            }
            else
            {
                int arrLength = inSource.Length;
                Array.Resize(ref ioDest, arrLength);
                Array.Copy(inSource, ioDest, arrLength);
            }
        }

        /// <summary>
        /// Copys a list into another list.
        /// </summary>
        static public void CopyFrom<T>(ref List<T> ioDest, List<T> inSource)
        {
            if (inSource == null)
            {
                ioDest?.Clear();
                ioDest = null;
            }
            else
            {
                int arrLength = inSource.Count;
                if (ioDest != null)
                {
                    ioDest.Clear();
                    if (ioDest.Capacity < arrLength)
                        ioDest.Capacity = arrLength;
                }
                else
                {
                    ioDest = new List<T>(inSource.Capacity);
                }

                ioDest.AddRange(inSource);
            }
        }

        /// <summary>
        /// Copys a list into another hashset.
        /// </summary>
        static public void CopyFrom<T>(ref HashSet<T> ioDest, HashSet<T> inSource)
        {
            if (inSource == null)
            {
                ioDest?.Clear();
                ioDest = null;
            }
            else
            {
                if (ioDest != null)
                {
                    ioDest.Clear();
                }
                else
                {
                    ioDest = new HashSet<T>();
                }

                foreach (var val in inSource)
                {
                    ioDest.Add(val);
                }
            }
        }

        /// <summary>
        /// Copys a dictionary into another dictionary.
        /// </summary>
        static public void CopyFrom<K, V>(ref Dictionary<K, V> ioDest, Dictionary<K, V> inSource)
        {
            if (inSource == null)
            {
                ioDest?.Clear();
                ioDest = null;
            }
            else
            {
                if (ioDest != null)
                {
                    ioDest.Clear();
                    foreach (var kv in inSource)
                    {
                        ioDest.Add(kv.Key, kv.Value);
                    }
                }
                else
                {
                    ioDest = new Dictionary<K, V>(inSource);
                }
            }
        }

        /// <summary>
        /// Performs a deep copy from one array into another.
        /// </summary>
        static public void DeepCopyFrom<T>(ref T[] ioDest, T[] inSource) where T : ICopyCloneable<T>
        {
            if (inSource == null)
            {
                ArrayUtils.Clear(ref ioDest);
                ioDest = null;
            }
            else
            {
                int arrLength = inSource.Length;
                Array.Resize(ref ioDest, arrLength);
                for (int i = 0; i < arrLength; ++i)
                    CopyFrom(ref ioDest[i], inSource[i]);
            }
        }

        /// <summary>
        /// Performs a deep copy from one list into another.
        /// </summary>
        static public void DeepCopyFrom<T>(ref List<T> ioDest, List<T> inSource) where T : ICopyCloneable<T>
        {
            if (inSource == null)
            {
                ioDest?.Clear();
                ioDest = null;
            }
            else
            {
                int arrLength = inSource.Count;
                if (ioDest != null)
                {
                    if (ioDest.Capacity < arrLength)
                        ioDest.Capacity = arrLength;
                }
                else
                {
                    ioDest = new List<T>(inSource.Capacity);
                }

                for (int i = 0; i < ioDest.Count; ++i)
                {
                    T element = ioDest[i];
                    CopyFrom(ref element, inSource[i]);
                    ioDest[i] = element;
                }

                for (int i = ioDest.Count; i < arrLength; ++i)
                {
                    T element = inSource[i].Clone();
                    ioDest.Add(element);
                }

                if (ioDest.Count > arrLength)
                {
                    ioDest.RemoveRange(arrLength, ioDest.Count - arrLength);
                }
            }
        }

        // TODO(Beau): Write implementations for deep copies of hashset and dictionary

        #endregion // Copy
    }
}