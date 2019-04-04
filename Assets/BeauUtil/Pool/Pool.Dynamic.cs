/*
 * Copyright (C) 2017-2019. Filament Games, LLC. All rights reserved.
 * Author:  Alex Beauchesne
 * Date:    4 April 2019
 * 
 * File:    Pool.Dynamic.cs
 * Purpose: Dynamic pool.
 */

using System;
using System.Collections.Generic;

namespace BeauUtil
{
    public abstract partial class Pool<T> where T : class
    {
        /// <summary>
        /// Pool with expandable capacity.
        /// </summary>
        public sealed class Dynamic : Pool<T>
        {
            private List<T> m_Pool;
            private int m_MinCapacity;

            public override int Capacity
            {
                get { return m_Pool.Capacity; }
            }

            public override int Count
            {
                get { return m_Pool.Count; }
            }

            public Dynamic(int inCapacity, Constructor inConstructor)
                : base(inConstructor)
            {
                if (inCapacity <= 0)
                    throw new ArgumentOutOfRangeException("Pool capacity must be greater than 0!");
                m_MinCapacity = inCapacity;

                m_Pool = new List<T>(m_MinCapacity);
                
                Reset();
            }

            public override void Dispose()
            {
                m_Pool.Clear();
                m_Pool = null;
            }

            public override void Reset()
            {
                while(m_Pool.Count < m_MinCapacity)
                {
                    T newObject = m_Constructor(this);
                    VerifyObject(newObject);

                    m_Pool.Add(newObject);
                }
            }

            public override T Pop()
            {
                T obj;

                if (m_Pool.Count > 0)
                {
                    int index = m_Pool.Count - 1;
                    obj = m_Pool[index];
                    m_Pool.RemoveAt(index);
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
                VerifyObject(inValue);
                m_Pool.Add(inValue);
            }
        }
    }
}
