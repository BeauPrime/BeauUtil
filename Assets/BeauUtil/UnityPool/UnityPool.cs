/*
 * Copyright (C) 2017-2019. Filament Games, LLC. All rights reserved.
 * Author:  Alex Beauchesne
 * Date:    4 April 2019
 * 
 * File:    UnityPool.cs
 * Purpose: Unity-specific prefab pool.
 */

using System;
using UnityEngine;

namespace BeauUtil
{
    /// <summary>
    /// Fixed pool of prefabs.
    /// </summary>
    public class UnityPool<T> : Pool<T> where T : Component
    {
        private struct Entry
        {
            public T Object;
            public bool Active;
        }

        private Entry[] m_Entries;

        private string m_BaseName;
        private int m_Capacity;
        private int m_NumActive;
        private Transform m_InactiveRoot;
        private Action<T> m_OnActivate;
        private Action<T> m_OnRecycle;

        public UnityPool(T inPrefab, int inCapacity, Transform inRoot, Action<T> inOnSpawn = null)
            : base(New(inPrefab, inRoot, inOnSpawn))
        {
            m_BaseName = inPrefab.name;
            m_Capacity = inCapacity;
            m_NumActive = 0;
            m_InactiveRoot = inRoot;

            m_Entries = new Entry[m_Capacity];

            if (typeof(IPoolableBehavior).IsAssignableFrom(typeof(T)))
            {
                m_OnActivate = (obj) => { ((IPoolableBehavior)obj).OnActivate(); };
                m_OnRecycle = (obj) => { ((IPoolableBehavior)obj).OnRecycle(); };
            }

            Reset();
        }

        public override void Dispose()
        {
            for(int i = 0; i < m_Entries.Length; ++i)
            {
                if (ReferenceEquals(m_Entries[i].Object, null))
                    continue;

                if (m_Entries[i].Object)
                {
                    if (m_Entries[i].Active)
                    {
                        if (m_OnActivate != null)
                            m_OnActivate(m_Entries[i].Object);
                        m_Entries[i].Active = false;
                    }

                    GameObject.Destroy(m_Entries[i].Object.gameObject);
                }
                m_Entries[i].Object = null;
            }

            m_NumActive = 0;
            m_Entries = null;
        }

        public override int Capacity
        {
            get { return m_Capacity; }
        }

        public override int Count
        {
            get { return m_Capacity - m_NumActive; }
        }

        public override void Reset()
        {
            for(int i = 0; i < m_Capacity; ++i)
            {
                if (ReferenceEquals(m_Entries[i].Object, null))
                {
                    m_Entries[i].Object = m_Constructor(this);
                    m_Entries[i].Object.name = string.Format("{0} ({1})", m_BaseName, i + 1);
                    m_Entries[i].Active = false;
                }
                else if (m_Entries[i].Active)
                {
                    if (m_OnRecycle != null)
                        m_OnRecycle(m_Entries[i].Object);
                    m_Entries[i].Object.transform.SetParent(m_InactiveRoot, false);
                    
                    --m_NumActive;
                    m_Entries[i].Active = false;
                }
            }
        }

        public override T Pop()
        {
            for(int i = 0; i < m_Capacity; ++i)
            {
                if (!ReferenceEquals(m_Entries[i].Object, null) && !m_Entries[i].Active)
                {
                    m_Entries[i].Active = true;
                    ++m_NumActive;

                    T obj = m_Entries[i].Object;
                    obj.transform.SetParent(null, false);

                    if (m_OnActivate != null)
                        m_OnActivate(obj);
                    return obj;
                }
            }

            throw new Exception("Out of objects to spawn!");
        }

        public override void Push(T inValue)
        {
            for(int i = 0; i < m_Capacity; ++i)
            {
                if (ReferenceEquals(m_Entries[i].Object, inValue))
                {
                    if (!m_Entries[i].Active)
                        throw new InvalidOperationException("Cannot push the same object twice!");

                    --m_NumActive;
                    m_Entries[i].Active = false;
                    
                    if (m_OnRecycle != null)
                        m_OnRecycle(inValue);

                    inValue.transform.SetParent(m_InactiveRoot, false);
                    break;
                }
            }
        }

        static private Pool<T>.Constructor New(T inPrefab, Transform inRoot, Action<T> inOnSpawn)
        {
            return (p) =>
            {
                T obj = GameObject.Instantiate<T>(inPrefab, inRoot, false);
                if (inOnSpawn != null)
                    inOnSpawn(obj);
                return obj;
            };
        }
    }
}
