/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    4 Dec 2020
 * 
 * File:    CastableFunc.cs
 * Purpose: Function that supports three different signature types.
 */

using System;
using System.Reflection;
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
        private CastedFunc<TInput, TOutput> m_CastedArgInvoker;
        private CallbackMode m_Mode;

        private CastableFunc(Func<TOutput> inFunc)
        {
            if (inFunc == null)
                throw new ArgumentNullException("inFunc");
            
            m_Mode = CallbackMode.NoArg;
            m_CastedArgInvoker = null;
            m_CallbackObject = inFunc;
        }

        private CastableFunc(Func<TInput, TOutput> inFunc)
        {
            if (inFunc == null)
                throw new ArgumentNullException("inFunc");

            m_Mode = CallbackMode.NativeArg;
            m_CallbackObject = inFunc;
            m_CastedArgInvoker = null;
        }

        private CastableFunc(RefFunc<TInput, TOutput> inFunc)
        {
            if (inFunc == null)
                throw new ArgumentNullException("inFunc");

            m_Mode = CallbackMode.NativeArgRef;
            m_CallbackObject = inFunc;
            m_CastedArgInvoker = null;
        }

        private CastableFunc(MulticastDelegate inCastedDelegate, CastedFunc<TInput, TOutput> inCastedInvoker)
        {
            if (inCastedDelegate == null)
                throw new ArgumentNullException("inCastedDelegate");
            if (inCastedInvoker == null)
                throw new ArgumentNullException("inCastedInvoker");

            m_Mode = CallbackMode.CastedArg;
            m_CallbackObject = inCastedDelegate;
            m_CastedArgInvoker = inCastedInvoker;
        }

        public bool IsEmpty
        {
            get { return m_Mode == CallbackMode.Unassigned; }
        }

        public object Target
        {
            get { return m_CallbackObject?.Target; }
        }

        public MethodInfo Method
        {
            get { return m_CallbackObject?.Method;}
        }

        [Il2CppSetOption(Option.NullChecks, false)]
        public TOutput Invoke(TInput inArg)
        {
            switch(m_Mode)
            {
                case CallbackMode.NoArg:
                    return ((Func<TOutput>) m_CallbackObject)();

                case CallbackMode.NativeArg:
                    return ((Func<TInput, TOutput>) m_CallbackObject)(inArg);

                case CallbackMode.NativeArgRef:
                    return ((RefFunc<TInput, TOutput>) m_CallbackObject)(ref inArg);

                case CallbackMode.CastedArg:
                    return m_CastedArgInvoker((MulticastDelegate) m_CallbackObject, inArg);

                case CallbackMode.Unassigned:
                default:
                    throw new InvalidOperationException("Function has not been initialized, or has been disposed");
            }
        }

        [Il2CppSetOption(Option.NullChecks, false)]
        public TOutput Invoke(ref TInput inArg)
        {
            switch(m_Mode)
            {
                case CallbackMode.NoArg:
                    return ((Func<TOutput>) m_CallbackObject)();

                case CallbackMode.NativeArg:
                    return ((Func<TInput, TOutput>) m_CallbackObject)(inArg);

                case CallbackMode.NativeArgRef:
                    return ((RefFunc<TInput, TOutput>) m_CallbackObject)(ref inArg);

                case CallbackMode.CastedArg:
                    return m_CastedArgInvoker((MulticastDelegate) m_CallbackObject, inArg);

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
            m_CastedArgInvoker = null;
            m_CallbackObject = inFunc;
        }

        public void Set(Func<TInput, TOutput> inFunc)
        {
            if (inFunc == null)
                throw new ArgumentNullException("inFunc");

            m_Mode = CallbackMode.NativeArg;
            m_CallbackObject = inFunc;
            m_CastedArgInvoker = null;
        }

        public void Set(RefFunc<TInput, TOutput> inFunc)
        {
            if (inFunc == null)
                throw new ArgumentNullException("inFunc");

            m_Mode = CallbackMode.NativeArgRef;
            m_CallbackObject = inFunc;
            m_CastedArgInvoker = null;
        }

        public void Set<U>(Func<U, TOutput> inFunc)
        {
            if (inFunc == null)
                throw new ArgumentNullException("inFunc");

            m_Mode = CallbackMode.CastedArg;
            m_CallbackObject = inFunc;
            m_CastedArgInvoker = CastedFuncInvoker<TInput, U, TOutput>.Invoker;
        }

        #endregion // Set

        #region IDisposable

        public void Dispose()
        {
            m_Mode = CallbackMode.Unassigned;
            m_CallbackObject = null;
            m_CastedArgInvoker = null;
        }

        #endregion // IDisposable

        #region IEquatable

        public bool Equals(CastableFunc<TInput, TOutput> other)
        {
            if (m_Mode != other.m_Mode)
                return false;

            if (m_Mode == CallbackMode.Unassigned)
                return true;
            
            return m_CallbackObject == other.m_CallbackObject;
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

        #endregion // IEquatable

        #region Overrides

        public override int GetHashCode()
        {
            int hash = m_Mode.GetHashCode();
            if (m_CallbackObject != null)
                hash = (hash << 5) ^ m_CallbackObject.GetHashCode();
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

    public delegate TOutput RefFunc<TInput, out TOutput>(ref TInput inValue);

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