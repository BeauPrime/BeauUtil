/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    31 August 2020
 * 
 * File:    TempList16.cs
 * Purpose: Temporary list with up to 16 elements.
 */

using System;
using System.Collections;
using System.Collections.Generic;

namespace BeauUtil
{
    /// <summary>
    /// Temporary list, going up to 8 elements.
    /// </summary>
    public struct TempList8<T> : ITempList<T>
    {
        private T m_0;
        private T m_1;
        private T m_2;
        private T m_3;
        private T m_4;
        private T m_5;
        private T m_6;
        private T m_7;
        private byte m_Count;

        public TempList8(IReadOnlyList<T> inSource)
            : this()
        {
            if (inSource.Count > 8)
                throw new ArgumentException("Source list has more than 8 elements");
                
            m_Count = (byte) inSource.Count;
            for(int i = 0; i < inSource.Count; ++i)
            {
                SetSlow(i, inSource[i]);
            }
        }

        public T this[int index]
        {
            get
            {
                if (index == 0 && m_Count > 0)
                    return m_0;
                return GetSlow(index);
            }
            set
            {
                if (index == 0 && m_Count > 0)
                {
                    m_0 = value;
                }
                else
                {
                    SetSlow(index, value);
                }
            }
        }

        public int Count { get { return m_Count; } }

        public int Capacity { get { return 8; } }

        public T[] ToArray()
        {
            T[] arr = new T[m_Count];
            if (m_Count > 0)
                arr[0] = m_0;
            if (m_Count > 1)
                arr[1] = m_1;
            if (m_Count > 2)
                arr[2] = m_2;
            if (m_Count > 3)
                arr[3] = m_3;
            if (m_Count > 4)
                arr[4] = m_4;
            if (m_Count > 5)
                arr[5] = m_5;
            if (m_Count > 6)
                arr[6] = m_6;
            if (m_Count > 7)
                arr[7] = m_7;
            return arr;
        }

        #region IList

        bool ICollection<T>.IsReadOnly { get { return false; } }

        public void Add(T item)
        {
            if (++m_Count > 8)
                throw new InvalidOperationException("Cannot exceed maximum of 8 entries in a TempList8");

            SetSlow(m_Count - 1, item);
        }

        public void Clear()
        {
            m_7 = default(T);
            m_6 = default(T);
            m_5 = default(T);
            m_4 = default(T);
            m_3 = default(T);
            m_2 = default(T);
            m_1 = default(T);
            m_0 = default(T);
            m_Count = 0;
        }

        public bool Contains(T item)
        {
            return IndexOf(item) >= 0;
        }

        void ICollection<T>.CopyTo(T[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<T> GetEnumerator()
        {
            for(int i = 0; i < m_Count; ++i)
                yield return GetSlow(i);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int IndexOf(T item)
        {
            var eq = CompareUtils.DefaultEquals<T>();
            for(int i = 0; i < m_Count; ++i)
            {
                if (eq.Equals(GetSlow(i), item))
                    return i;
            }
            return -1;
        }

        void IList<T>.Insert(int index, T item)
        {
            throw new NotSupportedException("TempList does not support insertion");
        }

        bool ICollection<T>.Remove(T item)
        {
            throw new NotSupportedException("TempList does not support remove");
        }

        void IList<T>.RemoveAt(int index)
        {
            throw new NotSupportedException("TempList does not support remove");
        }

        #endregion // IList

        #region Internal

        private T GetSlow(int inIndex)
        {
            if (inIndex < 0 || inIndex >= m_Count)
                throw new ArgumentOutOfRangeException("inIndex");

            switch(inIndex)
            {
                case 0:
                    return m_0;
                case 1:
                    return m_1;
                case 2:
                    return m_2;
                case 3:
                    return m_3;
                case 4:
                    return m_4;
                case 5:
                    return m_5;
                case 6:
                    return m_6;
                case 7:
                    return m_7;

                default:
                    throw new InvalidOperationException();
            }
        }

        private void SetSlow(int inIndex, T inValue)
        {
            if (inIndex < 0 || inIndex >= m_Count)
                throw new ArgumentOutOfRangeException("inIndex");

            switch(inIndex)
            {
                case 0:
                    m_0 = inValue;
                    break;
                case 1:
                    m_1 = inValue;
                    break;
                case 2:
                    m_2 = inValue;
                    break;
                case 3:
                    m_3 = inValue;
                    break;
                case 4:
                    m_4 = inValue;
                    break;
                case 5:
                    m_5 = inValue;
                    break;
                case 6:
                    m_6 = inValue;
                    break;
                case 7:
                    m_7 = inValue;
                    break;

                default:
                    throw new InvalidOperationException();
            }
        }

        #endregion // Internal
    }
}