/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    21 Oct 2020
 * 
 * File:    CastableAction.cs
 * Purpose: Callback that supports three different signature types.
 */

#if UNITY_2021_2_OR_NEWER && !BEAUUTIL_DISABLE_FUNCTION_POINTERS
#define SUPPORTS_FUNCTION_POINTERS
#endif // UNITY_2021_2_OR_NEWER

using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using Unity.IL2CPP.CompilerServices;

namespace BeauUtil
{
    /// <summary>
    /// Basic callback. Supports three signatures:
    /// no argument (Action)
    /// object argument (Action<T>)
    /// casted object argument (Action<U>)
    /// </summary>
    public struct CastableAction<TInput0> : IEquatable<CastableAction<TInput0>>, IEquatable<Action>, IEquatable<Action<TInput0>>, IEquatable<RefAction<TInput0>>, IEquatable<MulticastDelegate>, IDisposable
    {
        private Delegate m_CallbackObject;
#if SUPPORTS_FUNCTION_POINTERS
        private unsafe delegate*<Delegate, TInput0, void> m_CastedArgInvoker;
        private unsafe void* m_CallbackPtr;
#else
        private CastedAction<TInput0> m_CastedArgInvoker;
#endif // SUPPORTS_FUNCTION_POINTERS
        private CallbackMode m_Mode;

        private unsafe CastableAction(Action inAction)
        {
            if (inAction == null)
                throw new ArgumentNullException("inAction");

            m_Mode = CallbackMode.NoArg;
#if SUPPORTS_FUNCTION_POINTERS
            m_CallbackPtr = null;
#endif // SUPPORTS_FUNCTION_POINTERS
            m_CastedArgInvoker = null;
            m_CallbackObject = inAction;
        }

        private unsafe CastableAction(Action<TInput0> inAction)
        {
            if (inAction == null)
                throw new ArgumentNullException("inAction");

            m_Mode = CallbackMode.NativeArg;
#if SUPPORTS_FUNCTION_POINTERS
            m_CallbackPtr = null;
#endif // SUPPORTS_FUNCTION_POINTERS
            m_CallbackObject = inAction;
            m_CastedArgInvoker = null;
        }

        private unsafe CastableAction(RefAction<TInput0> inAction)
        {
            if (inAction == null)
                throw new ArgumentNullException("inAction");

            m_Mode = CallbackMode.NativeArgRef;
#if SUPPORTS_FUNCTION_POINTERS
            m_CallbackPtr = null;
#endif // SUPPORTS_FUNCTION_POINTERS
            m_CallbackObject = inAction;
            m_CastedArgInvoker = null;
        }

#if SUPPORTS_FUNCTION_POINTERS
        private unsafe CastableAction(MulticastDelegate inCastedDelegate, delegate*<Delegate, TInput0, void> inCastedInvoker)
#else
        private unsafe CastableAction(MulticastDelegate inCastedDelegate, CastedAction<TInput0> inCastedInvoker)
#endif // SUPPORTS_FUNCTION_POINTERS
        {
            if (inCastedDelegate == null)
                throw new ArgumentNullException("inCastedDelegate");
            if (inCastedInvoker == null)
                throw new ArgumentNullException("inCastedInvoker");

            m_Mode = CallbackMode.CastedArg;
#if SUPPORTS_FUNCTION_POINTERS
            m_CallbackPtr = null;
#endif // SUPPORTS_FUNCTION_POINTERS
            m_CallbackObject = inCastedDelegate;
            m_CastedArgInvoker = inCastedInvoker;
        }

#if SUPPORTS_FUNCTION_POINTERS
        private unsafe CastableAction(delegate*<void> inPointer)
        {
            if (inPointer == null)
                throw new ArgumentNullException("inPointer");

            m_Mode = CallbackMode.NoArg_Ptr;
            m_CallbackPtr = inPointer;
            m_CallbackObject = null;
            m_CastedArgInvoker = null;
        }

        private unsafe CastableAction(delegate*<TInput0, void> inPointer)
        {
            if (inPointer == null)
                throw new ArgumentNullException("inPointer");

            m_Mode = CallbackMode.NativeArg_Ptr;
            m_CallbackPtr = inPointer;
            m_CallbackObject = null;
            m_CastedArgInvoker = null;
        }

        private unsafe CastableAction(delegate*<ref TInput0, void> inPointer)
        {
            if (inPointer == null)
                throw new ArgumentNullException("inPointer");

            m_Mode = CallbackMode.NativeArgRef_Ptr;
            m_CallbackPtr = inPointer;
            m_CallbackObject = null;
            m_CastedArgInvoker = null;
        }
#endif // SUPPORTS_FUNCTION_POINTERS

        public bool IsEmpty
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return m_Mode == CallbackMode.Unassigned; }
        }

        public object Target
        {
            get { return m_CallbackObject?.Target; }
        }

        public MethodInfo Method
        {
            get { return m_CallbackObject?.Method; }
        }

        [Il2CppSetOption(Option.NullChecks, false)]
        public void Invoke(TInput0 inArg)
        {
            switch (m_Mode)
            {
                case CallbackMode.NoArg:
                    Unsafe.FastCast<Action>(m_CallbackObject)();
                    break;

                case CallbackMode.NativeArg:
                    Unsafe.FastCast<Action<TInput0>>(m_CallbackObject)(inArg);
                    break;

                case CallbackMode.NativeArgRef:
                    Unsafe.FastCast<RefAction<TInput0>>(m_CallbackObject)(ref inArg);
                    break;

                case CallbackMode.CastedArg:
                    unsafe
                    {
                        m_CastedArgInvoker(m_CallbackObject, inArg);
                    }
                    break;

#if SUPPORTS_FUNCTION_POINTERS
                case CallbackMode.NoArg_Ptr:
                    unsafe
                    {
                        ((delegate*<void>) m_CallbackPtr)();
                    }
                    break;
                case CallbackMode.NativeArg_Ptr:
                    unsafe
                    {
                        ((delegate*<TInput0, void>) m_CallbackPtr)(inArg);
                    }
                    break;

                case CallbackMode.NativeArgRef_Ptr:
                    unsafe
                    {
                        ((delegate*<ref TInput0, void>) m_CallbackPtr)(ref inArg);
                    }
                    break;
#endif // SUPPORTS_FUNCTION_POINTERS

                case CallbackMode.Unassigned:
                default:
                    throw new InvalidOperationException("Callback has not been initialized, or has been disposed");
            }
        }

        [Il2CppSetOption(Option.NullChecks, false)]
        public void Invoke(ref TInput0 inArg)
        {
            switch (m_Mode)
            {
                case CallbackMode.NoArg:
                    Unsafe.FastCast<Action>(m_CallbackObject)();
                    break;

                case CallbackMode.NativeArg:
                    Unsafe.FastCast<Action<TInput0>>(m_CallbackObject)(inArg);
                    break;

                case CallbackMode.NativeArgRef:
                    Unsafe.FastCast<RefAction<TInput0>>(m_CallbackObject)(ref inArg);
                    break;

                case CallbackMode.CastedArg:
                    unsafe
                    {
                        m_CastedArgInvoker(m_CallbackObject, inArg);
                    }
                    break;

#if SUPPORTS_FUNCTION_POINTERS
                case CallbackMode.NoArg_Ptr:
                    unsafe
                    {
                        ((delegate*<void>) m_CallbackPtr)();
                    }
                    break;
                case CallbackMode.NativeArg_Ptr:
                    unsafe
                    {
                        ((delegate*<TInput0, void>) m_CallbackPtr)(inArg);
                    }
                    break;

                case CallbackMode.NativeArgRef_Ptr:
                    unsafe
                    {
                        ((delegate*<ref TInput0, void>) m_CallbackPtr)(ref inArg);
                    }
                    break;
#endif // SUPPORTS_FUNCTION_POINTERS

                case CallbackMode.Unassigned:
                default:
                    throw new InvalidOperationException("Callback has not been initialized, or has been disposed");
            }
        }

        #region Set

        public void Set(Action inAction)
        {
            if (inAction == null)
                throw new ArgumentNullException("inAction");

            m_Mode = CallbackMode.NoArg;
            unsafe
            {
                m_CastedArgInvoker = null;
                m_CallbackObject = inAction;
#if SUPPORTS_FUNCTION_POINTERS
                m_CallbackPtr = null;
#endif // SUPPORTS_FUNCTION_POINTERS
            }
        }

        public void Set(Action<TInput0> inAction)
        {
            if (inAction == null)
                throw new ArgumentNullException("inAction");

            m_Mode = CallbackMode.NativeArg;
            m_CallbackObject = inAction;
            unsafe
            {
                m_CastedArgInvoker = null;
#if SUPPORTS_FUNCTION_POINTERS
                m_CallbackPtr = null;
#endif // SUPPORTS_FUNCTION_POINTERS
            }
        }

        public void Set(RefAction<TInput0> inAction)
        {
            if (inAction == null)
                throw new ArgumentNullException("inAction");

            m_Mode = CallbackMode.NativeArgRef;
            m_CallbackObject = inAction;
            unsafe
            {
                m_CastedArgInvoker = null;
#if SUPPORTS_FUNCTION_POINTERS
                m_CallbackPtr = null;
#endif // SUPPORTS_FUNCTION_POINTERS
            }
        }

        public void Set<U>(Action<U> inAction)
        {
            if (inAction == null)
                throw new ArgumentNullException("inAction");

            m_Mode = CallbackMode.CastedArg;
            m_CallbackObject = inAction;
            unsafe
            {
#if SUPPORTS_FUNCTION_POINTERS
                m_CallbackPtr = null;
#endif // SUPPORTS_FUNCTION_POINTERS
                m_CastedArgInvoker = CastedActionInvoker<TInput0, U>.Invoker;
            }
        }

#if SUPPORTS_FUNCTION_POINTERS

        public unsafe void Set(delegate*<void> inPointer)
        {
            if (inPointer == null)
                throw new ArgumentNullException("inPointer");

            m_Mode = CallbackMode.NoArg_Ptr;
            m_CallbackObject = null;
            m_CallbackPtr = inPointer;
            m_CastedArgInvoker = null;
        }

        public unsafe void Set(delegate*<TInput0, void> inPointer)
        {
            if (inPointer == null)
                throw new ArgumentNullException("inPointer");

            m_Mode = CallbackMode.NativeArg_Ptr;
            m_CallbackObject = null;
            m_CallbackPtr = inPointer;
            m_CastedArgInvoker = null;
        }

        public unsafe void Set(delegate*<ref TInput0, void> inPointer)
        {
            if (inPointer == null)
                throw new ArgumentNullException("inPointer");

            m_Mode = CallbackMode.NativeArgRef_Ptr;
            m_CallbackObject = null;
            m_CallbackPtr = inPointer;
            m_CastedArgInvoker = null;
        }

#endif // SUPPORTS_FUNCTION_POINTERS

        #endregion // Set

        #region IDisposable

        public void Dispose()
        {
            m_Mode = CallbackMode.Unassigned;
            m_CallbackObject = null;
            unsafe
            {
#if SUPPORTS_FUNCTION_POINTERS
                m_CallbackPtr = null;
#endif // SUPPORTS_FUNCTION_POINTERS
                m_CastedArgInvoker = null;
            }
        }

        #endregion // IDisposable

        #region IEquatable

        public bool Equals(CastableAction<TInput0> other)
        {
            if (m_Mode != other.m_Mode)
                return false;

            if (m_Mode == CallbackMode.Unassigned)
                return true;

            unsafe
            {
#if SUPPORTS_FUNCTION_POINTERS
                return m_CallbackObject == other.m_CallbackObject && m_CallbackPtr == other.m_CallbackPtr;
#else
                return m_CallbackObject == other.m_CallbackObject;
#endif // SUPPORTS_FUNCTION_POINTERS
            }
        }

        public bool Equals(Action<TInput0> other)
        {
            if (other == null)
                return m_Mode == CallbackMode.Unassigned;

            return m_Mode == CallbackMode.NativeArg && m_CallbackObject == ((Delegate) other);
        }

        public bool Equals(RefAction<TInput0> other)
        {
            if (other == null)
                return m_Mode == CallbackMode.Unassigned;

            return m_Mode == CallbackMode.NativeArgRef && m_CallbackObject == ((Delegate) other);
        }

        public bool Equals(Action other)
        {
            if (other == null)
                return m_Mode == CallbackMode.Unassigned;

            return m_Mode == CallbackMode.NoArg && m_CallbackObject == ((Delegate) other);
        }

        public bool Equals(MulticastDelegate other)
        {
            if (other == null)
                return m_Mode == CallbackMode.Unassigned;

            return m_CallbackObject == ((Delegate) other);
        }

#if SUPPORTS_FUNCTION_POINTERS

        public unsafe bool Equals(delegate*<void> other)
        {
            if (other == null)
                return m_Mode == CallbackMode.Unassigned;

            return m_CallbackPtr == other;
        }

        public unsafe bool Equals(delegate*<TInput0, void> other)
        {
            if (other == null)
                return m_Mode == CallbackMode.Unassigned;

            return m_CallbackPtr == other;
        }

        public unsafe bool Equals(delegate*<ref TInput0, void> other)
        {
            if (other == null)
                return m_Mode == CallbackMode.Unassigned;

            return m_CallbackPtr == other;
        }

        public unsafe bool Equals(IntPtr other)
        {
            if (other == null)
                return m_Mode == CallbackMode.Unassigned;

            return m_CallbackPtr == other.ToPointer();
        }

#endif // SUPPORTS_FUNCTION_POINTERS

        #endregion // IEquatable

        #region Overrides

        public override unsafe int GetHashCode()
        {
            int hash = m_Mode.GetHashCode();
            if (m_CallbackObject != null)
                hash = (hash << 5) ^ m_CallbackObject.GetHashCode();
#if SUPPORTS_FUNCTION_POINTERS
            if (m_CallbackPtr != null)
                hash = (hash >> 3) ^ ((long) m_CallbackPtr).GetHashCode();
#endif // SUPPORTS_FUNCTION_POINTERS
            return hash;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return m_Mode == CallbackMode.Unassigned;

            if (obj is CastableAction<TInput0>)
            {
                return Equals((CastableAction<TInput0>) obj);
            }

            MulticastDelegate del = obj as MulticastDelegate;
            if (del == null)
                return false;

            return Equals(del);
        }

        #endregion // Overrides

        #region Create

        static public CastableAction<TInput0> Create(Action inAction)
        {
            return new CastableAction<TInput0>(inAction);
        }

        static public CastableAction<TInput0> Create(Action<TInput0> inAction)
        {
            return new CastableAction<TInput0>(inAction);
        }

        static public CastableAction<TInput0> Create(RefAction<TInput0> inAction)
        {
            return new CastableAction<TInput0>(inAction);
        }

        static public CastableAction<TInput0> Create<U>(Action<U> inAction)
        {
            unsafe
            {
                return new CastableAction<TInput0>(inAction, CastedActionInvoker<TInput0, U>.Invoker);
            }
        }

#if SUPPORTS_FUNCTION_POINTERS

        static public unsafe CastableAction<TInput0> Create(delegate*<void> inPointer)
        {
            return new CastableAction<TInput0>(inPointer);
        }

        static public unsafe CastableAction<TInput0> Create(delegate*<TInput0, void> inPointer)
        {
            return new CastableAction<TInput0>(inPointer);
        }

        static public unsafe CastableAction<TInput0> Create(delegate*<ref TInput0, void> inPointer)
        {
            return new CastableAction<TInput0>(inPointer);
        }

#endif // SUPPORTS_FUNCTION_POINTERS

        #endregion // Create

        #region Operators

        static public bool operator ==(CastableAction<TInput0> left, CastableAction<TInput0> right)
        {
            return left.Equals(right);
        }

        static public bool operator !=(CastableAction<TInput0> left, CastableAction<TInput0> right)
        {
            return !left.Equals(right);
        }

        #endregion // Operators
    }

    public delegate void RefAction<T>(ref T inValue);

#if SUPPORTS_FUNCTION_POINTERS
    internal static class CastedActionInvoker<TInput, TOutput>
    {
#if ENABLE_IL2CPP
        static internal unsafe readonly delegate*<Delegate, TInput, void> Invoker = &CastedInvoke;
#else
        static internal unsafe delegate*<Delegate, TInput, void> Invoker
        {
            get { return &CastedInvoke; }
        }
#endif // ENABLE_IL2CPP

        [Il2CppSetOption(Option.NullChecks, false)]
        static private void CastedInvoke(Delegate inDelegate, TInput inInput)
        {
            ((Action<TOutput>) inDelegate).Invoke(CastableArgument.Cast<TInput, TOutput>(inInput));
        }
    }
#else
    internal delegate void CastedAction<TInput>(Delegate inDelegate, TInput inInput);

    internal static class CastedActionInvoker<TInput, TOutput>
    {
        static internal CastedAction<TInput> Invoker
        {
            get
            {
                return s_Callback ?? (s_Callback = CastedInvoke);
            }
        }

        static private CastedAction<TInput> s_Callback;

        [Il2CppSetOption(Option.NullChecks, false)]
        static private void CastedInvoke(Delegate inDelegate, TInput inInput)
        {
            ((Action<TOutput>) inDelegate).Invoke(CastableArgument.Cast<TInput, TOutput>(inInput));
        }
    }
#endif // SUPPORTS_FUNCTION_POINTERS
}