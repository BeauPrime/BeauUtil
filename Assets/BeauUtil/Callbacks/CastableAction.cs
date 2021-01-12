/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    21 Oct 2020
 * 
 * File:    CastableAction.cs
 * Purpose: Callback that supports three different signature types.
 */

using System;

namespace BeauUtil
{
    /// <summary>
    /// Basic callback. Supports three signatures:
    /// no argument (Action)
    /// object argument (Action<object>)
    /// casted object argument (Action<T>)
    /// </summary>
    public struct CastableAction<T> : IEquatable<CastableAction<T>>, IEquatable<Action>, IEquatable<Action<T>>, IEquatable<MulticastDelegate>, IDisposable
    {
        private CallbackMode m_Mode;
        private Action m_CallbackNoArgs;
        private Action<T> m_CallbackNativeArg;
        private MulticastDelegate m_CallbackWithCastedArg;
        private CastedAction m_CastedArgInvoker;

        private CastableAction(Action inAction)
        {
            if (inAction == null)
                throw new ArgumentNullException("inAction");
            
            m_Mode = CallbackMode.NoArg;
            m_CallbackNativeArg = null;
            m_CallbackWithCastedArg = null;
            m_CastedArgInvoker = null;
            m_CallbackNoArgs = inAction;
        }

        private CastableAction(Action<T> inAction)
        {
            if (inAction == null)
                throw new ArgumentNullException("inAction");

            m_Mode = CallbackMode.NativeArg;
            m_CallbackNativeArg = inAction;
            m_CallbackWithCastedArg = null;
            m_CastedArgInvoker = null;
            m_CallbackNoArgs = null;
        }

        private CastableAction(MulticastDelegate inCastedDelegate, CastedAction inCastedInvoker)
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

        public void Invoke(T inArg)
        {
            switch(m_Mode)
            {
                case CallbackMode.NoArg:
                    m_CallbackNoArgs();
                    break;

                case CallbackMode.NativeArg:
                    m_CallbackNativeArg(inArg);
                    break;

                case CallbackMode.CastedArg:
                    m_CastedArgInvoker(m_CallbackWithCastedArg, inArg);
                    break;

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
            m_CallbackNativeArg = null;
            m_CallbackWithCastedArg = null;
            m_CastedArgInvoker = null;
            m_CallbackNoArgs = inAction;
        }

        public void Set(Action<T> inAction)
        {
            if (inAction == null)
                throw new ArgumentNullException("inAction");

            m_Mode = CallbackMode.NativeArg;
            m_CallbackNativeArg = inAction;
            m_CallbackWithCastedArg = null;
            m_CastedArgInvoker = null;
            m_CallbackNoArgs = null;
        }

        public void Set<U>(Action<U> inAction)
        {
            if (inAction == null)
                throw new ArgumentNullException("inAction");

            m_Mode = CallbackMode.CastedArg;
            m_CallbackNativeArg = null;
            m_CallbackWithCastedArg = inAction;
            m_CastedArgInvoker = CastedActionInvoker<U>.Invoker;
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

        public bool Equals(CastableAction<T> other)
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
                    return m_CallbackWithCastedArg.Equals(other.m_CallbackWithCastedArg);

                default:
                    return true;
            }
        }

        public bool Equals(Action<T> other)
        {
            if (other == null)
                return m_Mode == CallbackMode.Unassigned;

            return m_Mode == CallbackMode.NativeArg && m_CallbackNativeArg == other;
        }

        public bool Equals(Action other)
        {
            if (other == null)
                return m_Mode == CallbackMode.Unassigned;

            return m_Mode == CallbackMode.NoArg && m_CallbackNoArgs == other;
        }

        public bool Equals(MulticastDelegate other)
        {
            if (other == null)
                return m_Mode == CallbackMode.Unassigned;
            
            Action noArg = other as Action;
            if (noArg != null)
            {
                return Equals(noArg);
            }

            Action<T> nativeArg = other as Action<T>;
            if (nativeArg != null)
            {
                return Equals(nativeArg);
            }

            return m_Mode == CallbackMode.CastedArg && m_CallbackWithCastedArg.Equals(other);
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

            if (obj is CastableAction<T>)
            {
                return Equals((CastableAction<T>) obj);
            }

            MulticastDelegate del = obj as MulticastDelegate;
            if (del == null)
                return false;

            return Equals(del);
        }

        #endregion // Overrides

        #region Create
    
        static public CastableAction<T> Create(Action inAction)
        {
            return new CastableAction<T>(inAction);
        }

        static public CastableAction<T> Create(Action<T> inAction)
        {
            return new CastableAction<T>(inAction);
        }

        static public CastableAction<T> Create<U>(Action<U> inAction)
        {
            return new CastableAction<T>(inAction, CastedActionInvoker<U>.Invoker);
        }

        #endregion // Create

        #region Operators

        static public bool operator ==(CastableAction<T> left, CastableAction<T> right)
        {
            return left.Equals(right);
        }

        static public bool operator !=(CastableAction<T> left, CastableAction<T> right)
        {
            return !left.Equals(right);
        }

        #endregion // Operators
    }

    internal delegate void CastedAction(MulticastDelegate inDelegate, object inInput);

    internal static class CastedActionInvoker<TOutput>
    {
        static internal CastedAction Invoker
        {
            get
            {
                return s_Callback ?? (s_Callback = CastedInvoke);
            }
        }

        static private CastedAction s_Callback;

        static private void CastedInvoke(MulticastDelegate inDelegate, object inInput)
        {
            ((Action<TOutput>) inDelegate).Invoke((TOutput) inInput);
        }
    }
}