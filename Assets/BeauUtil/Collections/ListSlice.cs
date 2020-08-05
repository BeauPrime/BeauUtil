/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    24 Oct 2019
 * 
 * File:    ListSlice.cs
 * Purpose: Read-only slice of a list.
 */

using System;
using System.Collections;
using System.Collections.Generic;

namespace BeauUtil
{
    /// <summary>
    /// Read-only slice of a list or array.
    /// </summary>
    public struct ListSlice<T> : IEnumerable<T>, IReadOnlyList<T>, IEquatable<ListSlice<T>>
    {
        private readonly IReadOnlyList<T> m_Source;
        private readonly int m_StartIndex;

        public readonly int Length;

        public ListSlice(T[] inArray) : this(inArray, 0, inArray != null ? inArray.Length : 0) { }

        public ListSlice(T[] inArray, int inStartIdx) : this(inArray, inStartIdx, inArray != null ? inArray.Length - inStartIdx : 0) { }

        public ListSlice(T[] inArray, int inStartIdx, int inLength)
        {
            if (inArray == null)
            {
                m_Source = null;
                m_StartIndex = 0;
                Length = 0;
            }
            else
            {
                m_Source = inArray;
                m_StartIndex = inStartIdx;
                Length = inLength;
            }
        }

        public ListSlice(IReadOnlyList<T> inList) : this(inList, 0, inList != null ? inList.Count : 0) { }

        public ListSlice(IReadOnlyList<T> inList, int inStartIdx) : this(inList, inStartIdx, inList != null ? inList.Count - inStartIdx : 0) { }

        public ListSlice(IReadOnlyList<T> inList, int inStartIdx, int inLength)
        {
            if (inList == null)
            {
                m_Source = null;
                m_StartIndex = 0;
                Length = 0;
            }
            else
            {
                m_Source = inList;
                m_StartIndex = inStartIdx;
                Length = inLength;
            }
        }

        /// <summary>
        /// An empty slice.
        /// </summary>
        static public readonly ListSlice<T> Empty = default(ListSlice<T>);

        /// <summary>
        /// Returns if this is an empty slice.
        /// </summary>
        public bool IsEmpty
        {
            get { return Length == 0; }
        }

        #region Search

        public bool Contains(T inItem)
        {
            return IndexOf(inItem) >= 0;
        }

        public int IndexOf(T inItem)
        {
            return IndexOf(inItem, 0, Length);
        }

        public int IndexOf(T inItem, int inStartIdx)
        {
            return IndexOf(inItem, inStartIdx, Length - inStartIdx);
        }

        public int IndexOf(T inItem, int inStartIdx, int inCount)
        {
            if (m_Source == null)
                return -1;

            var comparer = EqualityComparer<T>.Default;
            for (int i = 0; i < inCount; ++i)
            {
                if (comparer.Equals(m_Source[m_StartIndex + inStartIdx + i], inItem))
                    return i;
            }

            return -1;
        }

        public int LastIndexOf(T inItem)
        {
            return LastIndexOf(inItem, Length - 1, Length);
        }

        public int LastIndexOf(T inItem, int inStartIdx)
        {
            return LastIndexOf(inItem, inStartIdx, inStartIdx + 1);
        }

        public int LastIndexOf(T inItem, int inStartIdx, int inCount)
        {
            if (m_Source == null)
                return -1;

            var comparer = EqualityComparer<T>.Default;
            for (int i = 0; i < inCount; ++i)
            {
                if (comparer.Equals(m_Source[m_StartIndex + inStartIdx - i], inItem))
                    return inStartIdx - i;
            }

            return -1;
        }

        #endregion // Search

        #region Export

        public void CopyTo(T[] inArray, int inArrayIdx)
        {
            CopyTo(0, inArray, inArrayIdx, Length);
        }

        public void CopyTo(int inStartIndex, T[] inArray, int inArrayIdx, int inCount)
        {
            if (inArray.Length < inCount)
                throw new ArgumentException("Not enough room to copy " + inCount + " items to destination");

            for (int i = 0; i < inCount; ++i)
            {
                inArray[inArrayIdx + i] = m_Source[m_StartIndex + inStartIndex + i];
            }
        }

        public void CopyTo(T[] inArray)
        {
            CopyTo(0, inArray, 0, Length);
        }

        public T[] ToArray()
        {
            T[] arr = new T[Length];
            for (int i = 0; i < Length; ++i)
            {
                arr[i] = m_Source[m_StartIndex + i];
            }
            return arr;
        }

        #endregion // Export

        #region Subslice

        public ListSlice<T> Slice(int inStartIdx)
        {
            return Slice(inStartIdx, Length - inStartIdx);
        }

        public ListSlice<T> Slice(int inStartIdx, int inLength)
        {
            return new ListSlice<T>(m_Source, m_StartIndex + inStartIdx, inLength);
        }

        #endregion // Subslice

        #region IReadOnlyList

        public T this[int index]
        {
            get
            {
                if (index < 0 || index >= Length)
                    throw new IndexOutOfRangeException();
                return m_Source[m_StartIndex + index];
            }
        }

        int IReadOnlyCollection<T>.Count { get { return Length; } }

        #endregion // IReadOnlyList

        #region IEnumerable

        public IEnumerator<T> GetEnumerator()
        {
            for (int i = 0; i < Length; ++i)
                yield return m_Source[m_StartIndex + i];
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion // IEnumerable

        #region IEquatable

        public bool Equals(ListSlice<T> other)
        {
            return m_Source == other.m_Source &&
                m_StartIndex == other.m_StartIndex &&
                Length == other.Length;
        }

        #endregion // IEquatable

        #region Overrides

        public override bool Equals(object obj)
        {
            if (obj is ListSlice<T>)
            {
                return Equals((ListSlice<T>) obj);
            }
            if (obj is T[])
            {
                var arr = (T[]) obj;
                return m_Source == arr &&
                    m_StartIndex == 0 &&
                    Length == arr.Length;
            }
            if (obj is IReadOnlyList<T>)
            {
                var list = (IReadOnlyList<T>) obj;
                return m_Source == list &&
                    m_StartIndex == 0 &&
                    Length == list.Count;
            }

            return false;
        }

        public override int GetHashCode()
        {
            int hash = m_StartIndex.GetHashCode() ^ Length.GetHashCode();
            if (m_Source != null)
                hash = (hash << 2) ^ m_Source.GetHashCode();
            return hash;
        }

        static public bool operator ==(ListSlice<T> inA, ListSlice<T> inB)
        {
            return inA.Equals(inB);
        }

        static public bool operator !=(ListSlice<T> inA, ListSlice<T> inB)
        {
            return inA.Equals(inB);
        }

        static public implicit operator ListSlice<T>(T[] inArray)
        {
            return new ListSlice<T>(inArray);
        }

        static public implicit operator ListSlice<T>(List<T> inList)
        {
            return new ListSlice<T>(inList);
        }

        static public implicit operator ListSlice<T>(RingBuffer<T> inBuffer)
        {
            return new ListSlice<T>(inBuffer);
        }

        #endregion // Overrides
    }
}