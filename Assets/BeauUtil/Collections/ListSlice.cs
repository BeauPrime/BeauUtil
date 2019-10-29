/*
 * Copyright (C) 2017-2019. Filament Games, LLC. All rights reserved.
 * Author:  Alex Beauchesne
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
    public struct ListSlice<T> : IEnumerable<T>, IReadOnlyList<T>, IEquatable<ListSlice<T>>
    {
        private readonly IReadOnlyList<T> m_Source;

        public readonly int StartIndex;
        public readonly int Length;

        public ListSlice(T[] inArray) : this(inArray, 0, inArray.Length) { }

        public ListSlice(T[] inArray, int inStartIdx) : this(inArray, inStartIdx, inArray.Length - inStartIdx) { }

        public ListSlice(T[] inArray, int inStartIdx, int inLength)
        {
            if (inArray == null)
            {
                m_Source = null;
                StartIndex = 0;
                Length = 0;
            }
            else
            {
                CollectionUtils.ClampRange(inArray.Length, inStartIdx, ref inLength);

                m_Source = inArray;
                StartIndex = inStartIdx;
                Length = inLength;
            }
        }

        public ListSlice(IReadOnlyList<T> inList) : this(inList, 0, inList.Count) { }

        public ListSlice(IReadOnlyList<T> inList, int inStartIdx) : this(inList, inStartIdx, inList.Count - inStartIdx) { }

        public ListSlice(IReadOnlyList<T> inList, int inStartIdx, int inLength)
        {
            if (inList == null)
            {
                m_Source = null;
                StartIndex = 0;
                Length = 0;
            }
            else
            {
                CollectionUtils.ClampRange(inList.Count, inStartIdx, ref inLength);

                m_Source = inList;
                StartIndex = inStartIdx;
                Length = inLength;
            }
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

            CollectionUtils.ClampRange(Length, inStartIdx, ref inCount);

            var comparer = EqualityComparer<T>.Default;
            for (int i = 0; i < inCount; ++i)
            {
                if (comparer.Equals(m_Source[StartIndex + inStartIdx + i], inItem))
                    return i;
            }

            return -1;
        }

        public int LastIndexOf(T inItem)
        {
            return LastIndexOf(inItem, 0, Length);
        }

        public int LastIndexOf(T inItem, int inStartIdx)
        {
            return LastIndexOf(inItem, inStartIdx, Length - inStartIdx);
        }

        public int LastIndexOf(T inItem, int inStartIdx, int inCount)
        {
            if (m_Source == null)
                return -1;

            CollectionUtils.ClampRange(Length, inStartIdx, ref inCount);

            var comparer = EqualityComparer<T>.Default;
            for (int i = inCount - 1; i >= 0; --i)
            {
                if (comparer.Equals(m_Source[StartIndex + inStartIdx + i], inItem))
                    return i;
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
            CollectionUtils.ClampRange(Length, inStartIndex, ref inCount);

            int destCount = inCount;
            CollectionUtils.ClampRange(inArray.Length, inArrayIdx, ref destCount);
            if (destCount != inCount)
                throw new ArgumentException("Not enough room to copy " + inCount + " items to destination");

            for (int i = 0; i < inCount; ++i)
            {
                inArray[inArrayIdx + i] = m_Source[StartIndex + inStartIndex + i];
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
                arr[i] = m_Source[StartIndex + i];
            }
            return arr;
        }

        #endregion // Export

        #region IReadOnlyList

        public T this[int index]
        {
            get
            {
                if (index < 0 || index >= Length)
                    throw new IndexOutOfRangeException();
                return m_Source[StartIndex + index];
            }
        }

        int IReadOnlyCollection<T>.Count { get { return Length; } }

        #endregion // IReadOnlyList

        #region IEnumerable

        public IEnumerator<T> GetEnumerator()
        {
            for (int i = 0; i < Length; ++i)
                yield return m_Source[StartIndex + i];
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
                StartIndex == other.StartIndex &&
                Length == other.Length;
        }

        #endregion // IEquatable

        #region Overrides

        public override bool Equals(object obj)
        {
            if (obj is ListSlice<T>)
                return Equals((ListSlice<T>) obj);
            if (obj is IReadOnlyList<T>)
            {
                var list = (IReadOnlyList<T>) obj;
                return m_Source == list &&
                    StartIndex == 0 &&
                    Length == list.Count;
            }

            return false;
        }

        public override int GetHashCode()
        {
            int hash = StartIndex.GetHashCode() ^ Length.GetHashCode();
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

        #endregion // Overrides
    }
}