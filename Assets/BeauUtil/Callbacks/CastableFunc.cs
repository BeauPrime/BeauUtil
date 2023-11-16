/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    4 Dec 2020
 * 
 * File:    CastableFunc.cs
 * Purpose: Function that supports three different signature types.
 */

#if UNITY_2021_2_OR_NEWER
#define SUPPORTS_FUNCTION_POINTERS
#endif // UNITY_2021_2_OR_NEWER

using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using Unity.IL2CPP.CompilerServices;

namespace BeauUtil
{
    /// <summary>
    /// Basic function. Supports three signatures:
    /// no argument (Func<U>)
    /// object argument (Func<T, U>)
    /// casted object argument (Func<V, U>)
    /// </summary>
    public struct CastableFunc<TInput, TOutput> : IEquatable<CastableFunc<TInput, TOutput>>, IEquatable<Func<TOutput>>, IEquatable<Func<TInput, TOutput>>, IEquatable<RefFunc<TInput, TOutput>>, IEquatable<MulticastDelegate>, IDisposable
    {
        private Delegate m_CallbackObject;
#if SUPPORTS_FUNCTION_POINTERS
        private unsafe delegate*<MulticastDelegate, TInput, TOutput> m_CastedArgInvoker;
        private unsafe void* m_CallbackPtr;
#else
        private CastedFunc<TInput, TOutput> m_CastedArgInvoker;
#endif // SUPPORTS_FUNCTION_POINTERS
        private CallbackMode m_Mode;

        private unsafe CastableFunc(Func<TOutput> inFunc)
        {
            if (inFunc == null)
                throw new ArgumentNullException("inFunc");

            m_Mode = CallbackMode.NoArg;
            m_CastedArgInvoker = null;
            m_CallbackObject = inFunc;
#if SUPPORTS_FUNCTION_POINTERS
            m_CallbackPtr = null;
#endif // SUPPORTS_FUNCTION_POINTERS
        }

        private unsafe CastableFunc(Func<TInput, TOutput> inFunc)
        {
            if (inFunc == null)
                throw new ArgumentNullException("inFunc");

            m_Mode = CallbackMode.NativeArg;
            m_CallbackObject = inFunc;
            m_CastedArgInvoker = null;
#if SUPPORTS_FUNCTION_POINTERS
            m_CallbackPtr = null;
#endif // SUPPORTS_FUNCTION_POINTERS
        }

        private unsafe CastableFunc(RefFunc<TInput, TOutput> inFunc)
        {
            if (inFunc == null)
                throw new ArgumentNullException("inFunc");

            m_Mode = CallbackMode.NativeArgRef;
            m_CallbackObject = inFunc;
            m_CastedArgInvoker = null;
#if SUPPORTS_FUNCTION_POINTERS
            m_CallbackPtr = null;
#endif // SUPPORTS_FUNCTION_POINTERS
        }

#if SUPPORTS_FUNCTION_POINTERS
        private unsafe CastableFunc(delegate*<TOutput> inPointer)
        {
            if (inPointer == null)
                throw new ArgumentNullException("inPointer");

            m_Mode = CallbackMode.NoArg_Ptr;
            m_CallbackPtr = inPointer;
            m_CallbackObject = null;
            m_CastedArgInvoker = null;
        }

        private unsafe CastableFunc(delegate*<TInput, TOutput> inPointer)
        {
            if (inPointer == null)
                throw new ArgumentNullException("inPointer");

            m_Mode = CallbackMode.NativeArg_Ptr;
            m_CallbackPtr = inPointer;
            m_CallbackObject = null;
            m_CastedArgInvoker = null;
        }

        private unsafe CastableFunc(delegate*<ref TInput, TOutput> inPointer)
        {
            if (inPointer == null)
                throw new ArgumentNullException("inPointer");

            m_Mode = CallbackMode.NativeArgRef_Ptr;
            m_CallbackPtr = inPointer;
            m_CallbackObject = null;
            m_CastedArgInvoker = null;
        }
#endif // SUPPORTS_FUNCTION_POINTERS

#if SUPPORTS_FUNCTION_POINTERS
        private unsafe CastableFunc(MulticastDelegate inCastedDelegate, delegate*<MulticastDelegate, TInput, TOutput> inCastedInvoker)
#else
        private unsafe CastableFunc(MulticastDelegate inCastedDelegate, CastedFunc<TInput, TOutput> inCastedInvoker)
#endif // SUPPORTS_FUNCTION_POINTERS
        {
            if (inCastedDelegate == null)
                throw new ArgumentNullException("inCastedDelegate");
            if (inCastedInvoker == null)
                throw new ArgumentNullException("inCastedInvoker");

            m_Mode = CallbackMode.CastedArg;
            m_CallbackObject = inCastedDelegate;
            m_CastedArgInvoker = inCastedInvoker;
#if SUPPORTS_FUNCTION_POINTERS
            m_CallbackPtr = null;
#endif // SUPPORTS_FUNCTION_POINTERS
        }

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
        public TOutput Invoke(TInput inArg)
        {
            switch (m_Mode)
            {
                case CallbackMode.NoArg:
                    return ((Func<TOutput>) m_CallbackObject)();

                case CallbackMode.NativeArg:
                    return ((Func<TInput, TOutput>) m_CallbackObject)(inArg);

                case CallbackMode.NativeArgRef:
                    return ((RefFunc<TInput, TOutput>) m_CallbackObject)(ref inArg);

                case CallbackMode.CastedArg:
                    unsafe
                    {
                        return m_CastedArgInvoker((MulticastDelegate) m_CallbackObject, inArg);
                    }

#if SUPPORTS_FUNCTION_POINTERS
                case CallbackMode.NoArg_Ptr:
                    unsafe
                    {
                        return ((delegate*<TOutput>) m_CallbackPtr)();
                    }
                case CallbackMode.NativeArg_Ptr:
                    unsafe
                    {
                        return ((delegate*<TInput, TOutput>) m_CallbackPtr)(inArg);
                    }

                case CallbackMode.NativeArgRef_Ptr:
                    unsafe
                    {
                        return ((delegate*<ref TInput, TOutput>) m_CallbackPtr)(ref inArg);
                    }
#endif // SUPPORTS_FUNCTION_POINTERS

                case CallbackMode.Unassigned:
                default:
                    throw new InvalidOperationException("Function has not been initialized, or has been disposed");
            }
        }

        [Il2CppSetOption(Option.NullChecks, false)]
        public TOutput Invoke(ref TInput inArg)
        {
            switch (m_Mode)
            {
                case CallbackMode.NoArg:
                    return ((Func<TOutput>) m_CallbackObject)();

                case CallbackMode.NativeArg:
                    return ((Func<TInput, TOutput>) m_CallbackObject)(inArg);

                case CallbackMode.NativeArgRef:
                    return ((RefFunc<TInput, TOutput>) m_CallbackObject)(ref inArg);

                case CallbackMode.CastedArg:
                    unsafe
                    {
                        return m_CastedArgInvoker((MulticastDelegate) m_CallbackObject, inArg);
                    }

#if SUPPORTS_FUNCTION_POINTERS
                case CallbackMode.NoArg_Ptr:
                    unsafe
                    {
                        return ((delegate*<TOutput>) m_CallbackPtr)();
                    }
                case CallbackMode.NativeArg_Ptr:
                    unsafe
                    {
                        return ((delegate*<TInput, TOutput>) m_CallbackPtr)(inArg);
                    }

                case CallbackMode.NativeArgRef_Ptr:
                    unsafe
                    {
                        return ((delegate*<ref TInput, TOutput>) m_CallbackPtr)(ref inArg);
                    }
#endif // SUPPORTS_FUNCTION_POINTERS

                case CallbackMode.Unassigned:
                default:
                    throw new InvalidOperationException("Function has not been initialized, or has been disposed");
            }
        }

        #region Set

        public void Set(Func<TOutput> inFunc)
        {
            if (inFunc == null)
                throw new ArgumentNullException("inFunc");

            m_Mode = CallbackMode.NoArg;
            unsafe
            {
                m_CastedArgInvoker = null;
#if SUPPORTS_FUNCTION_POINTERS
                m_CallbackPtr = null;
#endif // SUPPORTS_FUNCTION_POINTERS
            }
            m_CallbackObject = inFunc;
        }

        public void Set(Func<TInput, TOutput> inFunc)
        {
            if (inFunc == null)
                throw new ArgumentNullException("inFunc");

            m_Mode = CallbackMode.NativeArg;
            m_CallbackObject = inFunc;
            unsafe
            {
                m_CastedArgInvoker = null;
#if SUPPORTS_FUNCTION_POINTERS
                m_CallbackPtr = null;
#endif // SUPPORTS_FUNCTION_POINTERS
            }
        }

        public void Set(RefFunc<TInput, TOutput> inFunc)
        {
            if (inFunc == null)
                throw new ArgumentNullException("inFunc");

            m_Mode = CallbackMode.NativeArgRef;
            m_CallbackObject = inFunc;
            unsafe
            {
                m_CastedArgInvoker = null;
#if SUPPORTS_FUNCTION_POINTERS
                m_CallbackPtr = null;
#endif // SUPPORTS_FUNCTION_POINTERS
            }
        }

        public void Set<U>(Func<U, TOutput> inFunc)
        {
            if (inFunc == null)
                throw new ArgumentNullException("inFunc");

            m_Mode = CallbackMode.CastedArg;
            m_CallbackObject = inFunc;
            unsafe
            {
                m_CastedArgInvoker = CastedFuncInvoker<TInput, U, TOutput>.Invoker;
#if SUPPORTS_FUNCTION_POINTERS
                m_CallbackPtr = null;
#endif // SUPPORTS_FUNCTION_POINTERS
            }
        }

#if SUPPORTS_FUNCTION_POINTERS

        public unsafe void Set(delegate*<TOutput> inPointer)
        {
            if (inPointer == null)
                throw new ArgumentNullException("inPointer");

            m_Mode = CallbackMode.CastedArg;
            m_CallbackObject = null;
            m_CallbackPtr = inPointer;
            m_CastedArgInvoker = null;
        }

        public unsafe void Set(delegate*<TInput, TOutput> inPointer)
        {
            if (inPointer == null)
                throw new ArgumentNullException("inPointer");

            m_Mode = CallbackMode.CastedArg;
            m_CallbackObject = null;
            m_CallbackPtr = inPointer;
            m_CastedArgInvoker = null;
        }

        public unsafe void Set(delegate*<ref TInput, TOutput> inPointer)
        {
            if (inPointer == null)
                throw new ArgumentNullException("inPointer");

            m_Mode = CallbackMode.CastedArg;
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
                m_CastedArgInvoker = null;
#if SUPPORTS_FUNCTION_POINTERS
                m_CallbackPtr = null;
#endif // SUPPORTS_FUNCTION_POINTERS
            }
        }

        #endregion // IDisposable

        #region IEquatable

        public bool Equals(CastableFunc<TInput, TOutput> other)
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

        public bool Equals(Func<TInput, TOutput> other)
        {
            if (other == null)
                return m_Mode == CallbackMode.Unassigned;

            return m_Mode == CallbackMode.NativeArg && m_CallbackObject == ((Delegate) other);
        }

        public bool Equals(RefFunc<TInput, TOutput> other)
        {
            if (other == null)
                return m_Mode == CallbackMode.Unassigned;

            return m_Mode == CallbackMode.NativeArgRef && m_CallbackObject == ((Delegate) other);
        }

        public bool Equals(Func<TOutput> other)
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

        public unsafe bool Equals(delegate*<TOutput> other)
        {
            if (other == null)
                return m_Mode == CallbackMode.Unassigned;

            return m_CallbackPtr == other;
        }

        public unsafe bool Equals(delegate*<TInput, TOutput> other)
        {
            if (other == null)
                return m_Mode == CallbackMode.Unassigned;

            return m_CallbackPtr == other;
        }

        public unsafe bool Equals(delegate*<ref TInput, TOutput> other)
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

            if (obj is CastableFunc<TInput, TOutput>)
            {
                return Equals((CastableFunc<TInput, TOutput>) obj);
            }

            MulticastDelegate del = obj as MulticastDelegate;
            if (del == null)
                return false;

            return Equals(del);
        }

        #endregion // Overrides

        #region Create

        static public CastableFunc<TInput, TOutput> Create(Func<TOutput> inFunc)
        {
            return new CastableFunc<TInput, TOutput>(inFunc);
        }

        static public CastableFunc<TInput, TOutput> Create(Func<TInput, TOutput> inFunc)
        {
            return new CastableFunc<TInput, TOutput>(inFunc);
        }

        static public CastableFunc<TInput, TOutput> Create(RefFunc<TInput, TOutput> inFunc)
        {
            return new CastableFunc<TInput, TOutput>(inFunc);
        }

        static public CastableFunc<TInput, TOutput> Create<U>(Func<U, TOutput> inFunc)
        {
            unsafe
            {
                return new CastableFunc<TInput, TOutput>(inFunc, CastedFuncInvoker<TInput, U, TOutput>.Invoker);
            }
        }

#if SUPPORTS_FUNCTION_POINTERS

        static public unsafe CastableFunc<TInput, TOutput> Create(delegate*<TOutput> inPointer)
        {
            return new CastableFunc<TInput, TOutput>(inPointer);
        }

        static public unsafe CastableFunc<TInput, TOutput> Create(delegate*<TInput, TOutput> inPointer)
        {
            return new CastableFunc<TInput, TOutput>(inPointer);
        }

        static public unsafe CastableFunc<TInput, TOutput> Create(delegate*<ref TInput, TOutput> inPointer)
        {
            return new CastableFunc<TInput, TOutput>(inPointer);
        }

#endif // SUPPORTS_FUNCTION_POINTERS

        #endregion // Create

        #region Operators

        static public bool operator ==(CastableFunc<TInput, TOutput> left, CastableFunc<TInput, TOutput> right)
        {
            return left.Equals(right);
        }

        static public bool operator !=(CastableFunc<TInput, TOutput> left, CastableFunc<TInput, TOutput> right)
        {
            return !left.Equals(right);
        }

        #endregion // Operators
    }

    public delegate TOutput RefFunc<TInput, out TOutput>(ref TInput inValue);

#if SUPPORTS_FUNCTION_POINTERS
    internal static class CastedFuncInvoker<TInput, TInputCasted, TOutput>
    {
#if ENABLE_IL2CPP
        static internal unsafe readonly delegate*<MulticastDelegate, TInput, TOutput> Invoker = &CastedInvoke;
#else
        static internal unsafe delegate*<MulticastDelegate, TInput, TOutput> Invoker
        {
            get { return &CastedInvoke; }
        }
#endif // ENABLE_IL2CPP

        static private TOutput CastedInvoke(MulticastDelegate inDelegate, TInput inInput)
        {
            return ((Func<TInputCasted, TOutput>) inDelegate).Invoke(CastableArgument.Cast<TInput, TInputCasted>(inInput));
        }
    }
#else
    internal delegate TOutput CastedFunc<TInput, TOutput>(MulticastDelegate inDelegate, TInput inInput);

    internal static class CastedFuncInvoker<TInput, TInputCasted, TOutput>
    {
        static internal CastedFunc<TInput, TOutput> Invoker
        {
            get
            {
                return s_Callback ?? (s_Callback = CastedInvoke);
            }
        }

        static private CastedFunc<TInput, TOutput> s_Callback;

        static private TOutput CastedInvoke(MulticastDelegate inDelegate, TInput inInput)
        {
            return ((Func<TInputCasted, TOutput>) inDelegate).Invoke(CastableArgument.Cast<TInput, TInputCasted>(inInput));
        }
    }
#endif // SUPPORTS_FUNCTION_POINTERS
}