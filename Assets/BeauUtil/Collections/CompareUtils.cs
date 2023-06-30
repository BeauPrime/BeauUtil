/*
 * Copyright (C) 2022. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    29 Sept 2022
 * 
 * File:    CompareUtils.cs
 * Purpose: Comparison utilities.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine.Scripting;

namespace BeauUtil
{
    /// <summary>
    /// Comparison utilities.
    /// </summary>
    static public class CompareUtils
    {
        static private readonly Type s_UnityObjectType = typeof(UnityEngine.Object);

        /// <summary>
        /// Retrieves the default equality comparer for the given type.
        /// For UnityEngine.Object-derived types this is more efficient than the standard comparer.
        /// </summary>
        [Obsolete("DefaultComparer has been renamed to DefaultEquals, to avoid conflicting with DefaultSort")]
        static public IEqualityComparer<T> DefaultComparer<T>()
        {
            return DefaultEquals<T>();
        }

        /// <summary>
        /// Retrieves the default equality comparer for the given type.
        /// For UnityEngine.Object-derived types this is more efficient than the standard comparer.
        /// </summary>
        static public IEqualityComparer<T> DefaultEquals<T>()
        {
            IEqualityComparer<T> comparer = Cache<T>.DefaultEquals;
            if (comparer == null)
            {
                Type type = typeof(T);
                if (type == typeof(string))
                {
                    comparer = (IEqualityComparer<T>) StringComparer.Ordinal;
                }
                else if (type.IsDefined(typeof(DefaultEqualityComparerAttribute), true))
                {
                    var attr = Reflect.GetAttribute<DefaultEqualityComparerAttribute>(type, true);
                    if (Cache<T>.DefaultSort != null && Cache<T>.DefaultSort.GetType() == attr.EqualityComparerType)
                    {
                        comparer = (IEqualityComparer<T>) Cache<T>.DefaultSort;
                    }
                    else
                    {
                        comparer = (IEqualityComparer<T>) Activator.CreateInstance(attr.EqualityComparerType);
                    }
                }
                else if (s_UnityObjectType.IsAssignableFrom(type))
                {
                    comparer = ReferenceEqualityComparer<T>.Default;
                }
                else
                {
                    comparer = EqualityComparer<T>.Default;
                }
                Cache<T>.DefaultEquals = comparer;
            }
            return comparer;
        }

        /// <summary>
        /// Retrieves the default sorting comparer for the given type.
        /// For UnityEngine.Object-derived types this is more efficient than the standard comparer.
        /// </summary>
        static public IComparer<T> DefaultSort<T>()
        {
            IComparer<T> comparer = Cache<T>.DefaultSort;
            if (comparer == null)
            {
                Type type = typeof(T);
                if (type == typeof(string))
                {
                    comparer = (IComparer<T>) StringComparer.Ordinal;
                }
                else if (type.IsDefined(typeof(DefaultSorterAttribute), true))
                {
                    var attr = Reflect.GetAttribute<DefaultSorterAttribute>(type, true);
                    if (Cache<T>.DefaultEquals != null && Cache<T>.DefaultEquals.GetType() == attr.ComparerType)
                    {
                        comparer = (IComparer<T>) Cache<T>.DefaultEquals;
                    }
                    else
                    {
                        comparer = (IComparer<T>) Activator.CreateInstance(attr.ComparerType);
                    }
                }
                else
                {
                    comparer = Comparer<T>.Default;
                }
                Cache<T>.DefaultSort = comparer;
            }
            return comparer;
        }

        /// <summary>
        /// Manually specifies the default equality comparer for the given type.
        /// </summary>
        static public void OverrideEquals<T>(IEqualityComparer<T> inDefault)
        {
            Cache<T>.DefaultEquals = inDefault;
        }

        /// <summary>
        /// Manually specifies the default equality comparer for the given type.
        /// </summary>
        static public void OverrideSort<T>(IComparer<T> inDefault)
        {
            Cache<T>.DefaultSort = inDefault;
        }

        static private class Cache<T>
        {
            static public IEqualityComparer<T> DefaultEquals;
            static public IComparer<T> DefaultSort;
        }
    }

    /// <summary>
    /// Specifies the default equality comparer for a type.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface, AllowMultiple = false, Inherited = true)]
    public sealed class DefaultEqualityComparerAttribute : PreserveAttribute
    {
        public readonly Type EqualityComparerType;

        public DefaultEqualityComparerAttribute(Type inEqualityComparerType)
        {
            if (inEqualityComparerType == null)
                throw new ArgumentNullException("inEqualityComparerType");

            EqualityComparerType = inEqualityComparerType;
        }
    }

    /// <summary>
    /// Specifies the default comparer for a type.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface, AllowMultiple = false, Inherited = true)]
    public sealed class DefaultSorterAttribute : PreserveAttribute
    {
        public readonly Type ComparerType;

        public DefaultSorterAttribute(Type inComparerType)
        {
            if (inComparerType == null)
                throw new ArgumentNullException("inComparerType");

            ComparerType = inComparerType;
        }
    }
}