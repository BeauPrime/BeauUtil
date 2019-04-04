/*
 * Copyright (C) 2017-2019. Filament Games, LLC. All rights reserved.
 * Author:  Alex Beauchesne
 * Date:    4 April 2019
 * 
 * File:    Pool.Static.cs
 * Purpose: Static pool.
 */

using System;

namespace BeauUtil
{
    public abstract partial class Pool<T> where T : class
    {
        /// <summary>
        /// Pool with a fixed capacity.
        /// </summary>
        public sealed class Static : Pool<T>
        {
            private T[] m_Pool;
            private int m_Capacity;
            private int m_CurrentIndex;

            public override int Capacity
            {
                get { return m_Capacity; }
            }

            public override int Count
            {
                get { return m_CurrentIndex; }
            }

            public Static(int inCapacity, Constructor inConstructor)
                : base(inConstructor)
            {
                if (inCapacity <= 0)
                    throw new ArgumentOutOfRangeException("Pool capacity must be greater than 0!");
                m_Capacity = inCapacity;

                m_Pool = new T[m_Capacity];
                m_CurrentIndex = 0;

                Reset();
            }

            public override void Dispose()
            {
                for (int i = 0; i < m_Capacity; ++i)
                    m_Pool[i] = null;
                m_Pool = null;
            }

            public override void Reset()
            {
                while(m_CurrentIndex < m_Capacity)
                {
                    T newObject = m_Constructor(this);
                    VerifyObject(newObject);

                    m_Pool[m_CurrentIndex++] = newObject;
                }
            }

            public override T Pop()
            {
                T obj;

                if (m_CurrentIndex > 0)
                {
                    obj = m_Pool[--m_CurrentIndex];
                    m_Pool[m_CurrentIndex] = null;
                }
                else
                {
                    obj = m_Constructor(this);
                    VerifyObject(obj);
                }

                return obj;
            }

            public override void Push(T inValue)
            {
                if (m_CurrentIndex < m_Capacity)
                {
                    VerifyObject(inValue);
                    m_Pool[m_CurrentIndex++] = inValue;
                }
            }
        }
    }
}
