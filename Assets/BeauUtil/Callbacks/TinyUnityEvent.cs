/*
 * Copyright (C) 2023. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    16 Dec 2023
 * 
 * File:    TinyUnityEvent.cs
 * Purpose: Non-serialized UnityEvent replacement.
 */

using System;
using System.Runtime.CompilerServices;

namespace BeauUtil
{
    /// <summary>
    /// UnityEvent replacement. Not serializable.
    /// Use where exposing the event to the inspector is not required.
    /// </summary>
    public class TinyUnityEvent : ActionEvent
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddListener(Action inAction)
        {
            Register(inAction);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveListener(Action inAction)
        {
            Deregister(inAction);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveAllListeners()
        {
            Clear();
        }
    }

    /// <summary>
    /// UnityEvent replacement. Not serializable.
    /// Use where exposing the event to the inspector is not required.
    /// </summary>
    public class TinyUnityEvent<T0> : CastableEvent<T0>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddListener(Action inAction)
        {
            Register(inAction);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddListener(Action<T0> inAction)
        {
            Register(inAction);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveListener(Action inAction)
        {
            Deregister(inAction);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveListener(Action<T0> inAction)
        {
            Deregister(inAction);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveAllListeners()
        {
            Clear();
        }
    }

    /// <summary>
    /// UnityEvent replacement. Not serializable.
    /// Use where exposing the event to the inspector is not required.
    /// </summary>
    public class TinyUnityEvent<T0, T1> : CastableEvent<T0, T1>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddListener(Action inAction)
        {
            Register(inAction);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddListener(Action<T0, T1> inAction)
        {
            Register(inAction);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveListener(Action inAction)
        {
            Deregister(inAction);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveListener(Action<T0, T1> inAction)
        {
            Deregister(inAction);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveAllListeners()
        {
            Clear();
        }
    }

    /// <summary>
    /// UnityEvent replacement. Not serializable.
    /// Use where exposing the event to the inspector is not required.
    /// </summary>
    public class TinyUnityEvent<T0, T1, T2> : CastableEvent<T0, T1, T2>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddListener(Action inAction)
        {
            Register(inAction);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddListener(Action<T0, T1, T2> inAction)
        {
            Register(inAction);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveListener(Action inAction)
        {
            Deregister(inAction);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveListener(Action<T0, T1, T2> inAction)
        {
            Deregister(inAction);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveAllListeners()
        {
            Clear();
        }
    }

    /// <summary>
    /// UnityEvent replacement. Not serializable.
    /// Use where exposing the event to the inspector is not required.
    /// </summary>
    public class TinyUnityEvent<T0, T1, T2, T3> : CastableEvent<T0, T1, T2, T3>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddListener(Action inAction)
        {
            Register(inAction);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddListener(Action<T0, T1, T2, T3> inAction)
        {
            Register(inAction);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveListener(Action inAction)
        {
            Deregister(inAction);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveListener(Action<T0, T1, T2, T3> inAction)
        {
            Deregister(inAction);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveAllListeners()
        {
            Clear();
        }
    }
}