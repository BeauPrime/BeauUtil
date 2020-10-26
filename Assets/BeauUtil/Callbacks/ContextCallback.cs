/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    21 Oct 2020
 * 
 * File:    ContextCallback.cs
 * Purpose: Callback that supports three different signature types.
 */

using System;
using System.Runtime.InteropServices;

namespace BeauUtil
{
    /// <summary>
    /// Basic callback. Supports three signatures:
    /// no argument (Action)
    /// object argument (Action<object>)
    /// casted object argument (Action<T>)
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    public struct ContextCallback
    {
        [FieldOffset(0)]
        private UnityEngine.Object m_Binding;

        [FieldOffset(8)]
        private CallbackMode m_Mode;

        [FieldOffset(12)]
        private bool m_DeleteQueued;

        [FieldOffset(16)]
        private Action m_CallbackNoArgs;

        [FieldOffset(16)]
        private Action<object> m_CallbackNativeArg;

        [FieldOffset(16)]
        private MulticastDelegate m_CallbackWithCastedArg;

        [FieldOffset(24)]
        private CastedCallback m_CastedArgInvoker;

        public ContextCallback(Action inAction, UnityEngine.Object inBinding)
        {
            m_Binding = inBinding;
            m_Mode = CallbackMode.NoArg;
            m_DeleteQueued = false;
            m_CallbackNativeArg = null;
            m_CallbackWithCastedArg = null;
            m_CastedArgInvoker = null;
            m_CallbackNoArgs = inAction;
        }
    }

    internal enum CallbackMode : byte
    {
        NoArg,
        NativeArg,
        CastedArg
    }

    internal delegate void CastedCallback(MulticastDelegate inDelegate, object inInput);

    internal static class CastedCallbackInvoker<TOutput>
    {
        static internal CastedCallback Invoker
        {
            get
            {
                return s_Callback ?? (s_Callback = CastedInvoke);
            }
        }

        static private CastedCallback s_Callback;

        static private void CastedInvoke(MulticastDelegate inDelegate, object inInput)
        {
            ((Action<TOutput>) inDelegate).Invoke((TOutput) inInput);
        }
    }
}