/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    10 June 2020
 * 
 * File:    SearchFlags.cs
 * Purpose: Flags to inform search behavior.
 */

#if CSHARP_7_3_OR_NEWER
#define EXPANDED_REFS
#endif // CSHARP_7_3_OR_NEWER

using System;
using System.Collections.Generic;

namespace BeauUtil
{
    /// <summary>
    /// Additional flags for searching.
    /// </summary>
    [Flags]
    public enum SearchFlags : byte
    {
        IsReversed = 0x01,
    }

    /// <summary>
    /// Wrapper for IComparer that accounts for flags.
    /// </summary>
#if EXPANDED_REFS
    public readonly struct CompareWrapper<T> : IComparer<T>
#else
    public struct CompareWrapper<T> : IComparer<T>
#endif // EXPANDED_REFS
    {
        private readonly IComparer<T> m_Source;
        private readonly SearchFlags m_Flags;
        private readonly int m_Sign;

        public CompareWrapper(IComparer<T> inComparer, SearchFlags inFlags)
        {
            if (inComparer == null)
                throw new ArgumentNullException(nameof(inComparer));
            
            m_Source = inComparer;
            m_Flags = inFlags;
            m_Sign = ((m_Flags & SearchFlags.IsReversed) != 0 ? -1 : 1);
        }

        public int Compare(T x, T y)
        {
            return m_Source.Compare(x, y) * m_Sign;
        }

        static public IComparer<T> Wrap(IComparer<T> inComparer, SearchFlags inFlags)
        {
            if (inFlags == 0)
                return inComparer;
            return new CompareWrapper<T>(inComparer, inFlags);
        }
    }
}