/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    4 Dec 2020
 * 
 * File:    CastableFunc.cs
 * Purpose: Function that supports three different signature types.
 */

using System;

namespace BeauUtil
{
    /// <summary>
    /// Basic function. Supports three signatures:
    /// no argument (Func<U>)
    /// object argument (Func<T, U>)
    /// casted object argument (Func<V, U>)
    /// </summary>
    public struct CastableFunc<TInput, TOutput> : IEquatable<CastableFunc<TInput, TOutput>>, IEquatable<Func<TOutput>>, IEquatable<Func<TInput, TOutput>>, IEquatable<MulticastDelegate>, IDisposable
    {
        private CallbackMode m_Mode;
        private Func<TOutput> m_CallbackNoArgs;
        private Func<TInput, TOutput> m_CallbackNativeArg;
        private MulticastDelegate m_CallbackWithCastedArg;
        private CastedFunc<TInput, TOutput> m_CastedArgInvoker;

        private CastableFunc(Func<TOutput> inFunc)
        {
            if (inFunc == null)
                throw new ArgumentNullException("inFunc");
            
            m_Mode = CallbackMode.NoArg;
            m_CallbackNativeArg = null;
            m_CallbackWithCastedArg = null;
            m_CastedArgInvoker = null;
            m_CallbackNoArgs = inFunc;
        }

        private CastableFunc(Func<TInput, TOutput> inFunc)
        {
            if (inFunc == null)
                throw new ArgumentNullException("inFunc");

            m_Mode = CallbackMode.NativeArg;
            m_CallbackNativeArg = inFunc;
            m_CallbackWithCastedArg = null;
            m_CastedArgInvoker = null;
            m_CallbackNoArgs = null;
        }

        private CastableFunc(MulticastDelegate inCastedDelegate, CastedFunc<TInput, TOutput> inCastedInvoker)
        {
            if (inCastedDelegate == null)
                throw new ArgumentNullException("inCastedDelegate");
            if (inCastedInvoker == null)
                throw new ArgumentNullException("inCastedInvoker");

            m_Mode = CallbackMode.CastedArg;
            m_CallbackNativeArg = null;
            m_CallbackWithCastedArg = inCastedDelegate;
            m_CastedArgInvoker = inCastedInvoker;
            m_CallbackNoArgs = null;
        }

        public bool IsEmpty {
            get { return m_Mode == CallbackMode.Unassigned; }
        }

        public TOutput Invoke(TInput inArg)
        {
            switch(m_Mode)
            {
                case CallbackMode.NoArg:
                    return m_CallbackNoArgs();

                case CallbackMode.NativeArg:
                    return m_CallbackNativeArg(inArg);

                case CallbackMode.CastedArg:
                    return m_CastedArgInvoker(m_CallbackWithCastedArg, inArg);

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
            m_CallbackNativeArg = null;
            m_CallbackWithCastedArg = null;
            m_CastedArgInvoker = null;
            m_CallbackNoArgs = inFunc;
        }

        public void Set(Func<TInput, TOutput> inFunc)
        {
            if (inFunc == null)
                throw new ArgumentNullException("inFunc");

            m_Mode = CallbackMode.NativeArg;
            m_CallbackNativeArg = inFunc;
            m_CallbackWithCastedArg = null;
            m_CastedArgInvoker = null;
            m_CallbackNoArgs = null;
        }

        public void Set<U>(Func<U, TOutput> inFunc)
        {
            if (inFunc == null)
                throw new ArgumentNullException("inFunc");

            m_Mode = CallbackMode.CastedArg;
            m_CallbackNativeArg = null;
            m_CallbackWithCastedArg = inFunc;
            m_CastedArgInvoker = CastedFuncInvoker<TInput, U, TOutput>.Invoker;
            m_CallbackNoArgs = null;
        }

        #endregion // Set

        #region IDisposable

        public void Dispose()
        {
            m_Mode = CallbackMode.Unassigned;
            m_CallbackNoArgs = null;
            m_CallbackNativeArg = null;
            m_CallbackWithCastedArg = null;
            m_CastedArgInvoker = null;
        }

        #endregion // IDisposable

        #region IEquatable

        public bool Equals(CastableFunc<TInput, TOutput> other)
        {
            if (m_Mode != other.m_Mode)
                return false;

            switch(m_Mode)
            {
                case CallbackMode.NoArg:
                    return m_CallbackNoArgs == other.m_CallbackNoArgs;

                case CallbackMode.NativeArg:
                    return m_CallbackNativeArg == other.m_CallbackNativeArg;

                case CallbackMode.CastedArg:
                    return (Delegate) m_CallbackWithCastedArg == other.m_CallbackWithCastedArg;

                default:
                    return true;
            }
        }

        public bool Equals(Func<TInput, TOutput> other)
        {
            if (other == null)
                return m_Mode == CallbackMode.Unassigned;

            return m_Mode == CallbackMode.NativeArg && m_CallbackNativeArg == other;
        }

        public bool Equals(Func<TOutput> other)
        {
            if (other == null)
                return m_Mode == CallbackMode.Unassigned;

            return m_Mode == CallbackMode.NoArg && m_CallbackNoArgs == other;
        }

        public bool Equals(MulticastDelegate other)
        {
            if (other == null)
                return m_Mode == CallbackMode.Unassigned;
            
            Func<TOutput> noArg = other as Func<TOutput>;
            if (noArg != null)
            {
                return Equals(noArg);
            }

            Func<TInput, TOutput> nativeArg = other as Func<TInput, TOutput>;
            if (nativeArg != null)
            {
                return Equals(nativeArg);
            }

            return m_Mode == CallbackMode.CastedArg && (Delegate) m_CallbackWithCastedArg == other;
        }

        #endregion // IEquatable

        #region Overrides

        public override int GetHashCode()
        {
            int hash = m_Mode.GetHashCode();
            switch(m_Mode)
            {
                case CallbackMode.NoArg:
                    return m_CallbackNoArgs.GetHashCode();

                case CallbackMode.NativeArg:
                    return m_CallbackNativeArg.GetHashCode();

                case CallbackMode.CastedArg:
                    return m_CallbackWithCastedArg.GetHashCode();
            }
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

        static public CastableFunc<TInput, TOutput> Create<U>(Func<U, TOutput> inFunc)
        {
            return new CastableFunc<TInput, TOutput>(inFunc, CastedFuncInvoker<TInput, U, TOutput>.Invoker);
        }

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
}