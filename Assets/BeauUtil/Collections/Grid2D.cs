/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    4 April 2019
 * 
 * File:    Grid2D.cs
 * Purpose: Two-dimensional table of values.
 */

#if CSHARP_7_3_OR_NEWER
#define EXPANDED_REFS
#endif // CSHARP_7_3_OR_NEWER

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BeauUtil
{
    /// <summary>
    /// Table of values.
    /// </summary>
    [Serializable]
    public class Grid2D<T> : IEnumerable<T>
    {
        [SerializeField] protected T[] m_Data;
        [SerializeField] protected int m_Width;
        [SerializeField] protected int m_Height;

        public Grid2D()
            : this(1, 1)
        { }

        public Grid2D(int inWidth, int inHeight)
        {
            m_Width = inWidth;
            m_Height = inHeight;

            m_Data = new T[inWidth * inHeight];
        }

        public Grid2D(int inWidth, int inHeight, T inDefault)
            : this(inWidth, inHeight)
        {
            for (int i = 0; i < m_Data.Length; ++i)
                m_Data[i] = inDefault;
        }

        public Grid2D(Grid2D<T> inGrid)
        {
            m_Width = inGrid.m_Width;
            m_Height = inGrid.m_Height;

            m_Data = (T[]) inGrid.m_Data.Clone();
        }

        public int Width
        {
            get { return m_Width; }
        }

        public int Height
        {
            get { return m_Height; }
        }

        public int Count
        {
            get { return m_Width * m_Height; }
        }

        #if EXPANDED_REFS

        public ref T this[int inX, int inY]
        {
            get
            {
                if (!IsValid(inX, inY))
                    throw new ArgumentOutOfRangeException();
                
                return ref m_Data[inX + inY * m_Width];
            }
        }

        public ref T this[int inIndex]
        {
            get
            {
                if (inIndex < 0 || inIndex >= m_Data.Length)
                    throw new ArgumentOutOfRangeException("inIndex");
                
                return ref m_Data[inIndex];
            }
        }

        #else

        public T this[int inX, int inY]
        {
            get
            {   if (!IsValid(inX, inY))
                    throw new ArgumentOutOfRangeException();

                return m_Data[inX + inY * m_Width];
            }
            set
            {
                if (!IsValid(inX, inY))
                    throw new ArgumentOutOfRangeException();
                
                m_Data[inX + inY * m_Width] = value;
            }
        }

        public T this[int inIndex]
        {
            get 
            {
                if (inIndex < 0 || inIndex >= m_Data.Length)
                    throw new ArgumentOutOfRangeException("inIndex");

                return m_Data[inIndex];
            }
            set
            {
                if (inIndex < 0 || inIndex >= m_Data.Length)
                    throw new ArgumentOutOfRangeException("inIndex");

                m_Data[inIndex] = value;
            }
        }

        #endif // EXPANDED_REFS

        public bool IsValid(int inX, int inY)
        {
            return inX > 0 && inY > 0 && inX < m_Width && inY < m_Height;
        }

        public int TryGetValue(int inX, int inY, out T outData)
        {
            if (!IsValid(inX, inY))
            {
                outData = default(T);
                return -1;
            }

            int index = inX + inY * m_Width;
            outData = this[inX, inY];
            return index;
        }

        public int GetX(int inIndex)
        {
            return inIndex % m_Width;
        }

        public int GetY(int inIndex)
        {
            return (int)(inIndex / m_Width);
        }

        public void GetXY(int inIndex, out int outX, out int outY)
        {
            if (inIndex < 0 || inIndex >= m_Data.Length)
                throw new ArgumentOutOfRangeException("inIndex");
            
            outX = inIndex % m_Width;
            outY = (int)(inIndex / m_Width);
        }

        public int GetIndex(int inX, int inY)
        {
            if (!IsValid(inX, inY))
                throw new ArgumentOutOfRangeException();
            
            return inX + inY * m_Width;
        }

        public void Resize(int inNewWidth, int inNewHeight)
        {
            if (m_Width == inNewWidth)
            {
                if (m_Height == inNewHeight)
                    return;

                m_Height = inNewHeight;
                Array.Resize<T>(ref m_Data, m_Width * m_Height);
                return;
            }

            T[] newData = new T[inNewWidth * inNewHeight];
            int copyWidth = inNewWidth < m_Width ? inNewWidth : m_Width;
            int copyHeight = inNewHeight < m_Height ? inNewHeight : m_Height;
            for (int y = 0; y < copyHeight; ++y)
            {
                for (int x = 0; x < copyWidth; ++x)
                {
                    T oldValue = this[x, y];
                    newData[x + y * inNewWidth] = oldValue;
                }
            }

            m_Data = newData;
            m_Width = inNewWidth;
            m_Height = inNewHeight;
        }

        public void Clear()
        {
            Clear(default(T));
        }

        public void Clear(T inValue)
        {
            for (int i = 0; i < m_Data.Length; ++i)
                m_Data[i] = inValue;
        }

        public IEnumerator<T> GetEnumerator()
        {
            // If we're a reference type, we can
            // skip over the null entries
            if (s_IsClass)
            {
                for (int i = 0; i < m_Data.Length; ++i)
                    if (m_Data[i] != null)
                        yield return m_Data[i];
            }
            else
            {
                for (int i = 0; i < m_Data.Length; ++i)
                    yield return m_Data[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public T[] GetArray()
        {
            return m_Data;
        }

        static private readonly bool s_IsClass = typeof(T).IsClass;
    }
}
