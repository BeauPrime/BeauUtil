/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    21 Oct 2020
 * 
 * File:    CastableAction.cs
 * Purpose: Callback that supports three different signature types.
 */

using System;
using System.Reflection;
using Unity.IL2CPP.CompilerServices;

namespace BeauUtil
{
    /// <summary>
    /// Basic callback. Supports three signatures:
    /// no argument (Action)
    /// object argument (Action<T>)
    /// casted object argument (Action<U>)
    /// </summary>
    public struct CastableAction<TInput> : IEquatable<CastableAction<TInput>>, IEquatable<Action>, IEquatable<Action<TInput>>, IEquatable<RefAction<TInput>>, IEquatable<MulticastDelegate>, IDisposable
    {
        private Delegate m_CallbackObject;
        private CastedAction<TInput> m_CastedArgInvoker;
        private CallbackMode m_Mode;

        private CastableAction(Action inAction)
        {
            if (inAction == null)
                throw new ArgumentNullException("inAction");
            
            m_Mode = CallbackMode.NoArg;
            m_CastedArgInvoker = null;
            m_CallbackObject = inAction;
        }

        private CastableAction(Action<TInput> inAction)
        {
            if (inAction == null)
                throw new ArgumentNullException("inAction");

            m_Mode = CallbackMode.NativeArg;
            m_CallbackObject = inAction;
            m_CastedArgInvoker = null;
        }

        private CastableAction(RefAction<TInput> inAction)
        {
            if (inAction == null)
                throw new ArgumentNullException("inAction");

            m_Mode = CallbackMode.NativeArgRef;
            m_CallbackObject = inAction;
            m_CastedArgInvoker = null;
        }

        private CastableAction(MulticastDelegate inCastedDelegate, CastedAction<TInput> inCastedInvoker)
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
        public void Invoke(TInput inArg)
        {
            switch(m_Mode)
            {
                case CallbackMode.NoArg:
                    ((Action) m_CallbackObject)();
                    break;

                case CallbackMode.NativeArg:
                    ((Action<TInput>) m_CallbackObject)(inArg);
                    break;

                case CallbackMode.NativeArgRef:
                    ((RefAction<TInput>) m_CallbackObject)(ref inArg);
                    break;

                case CallbackMode.CastedArg:
                    m_CastedArgInvoker(m_CallbackObject, inArg);
                    break;

                case CallbackMode.Unassigned:
                default:
                    throw new InvalidOperationException("Callback has not been initialized, or has been disposed");
            }
        }

        [Il2CppSetOption(Option.NullChecks, false)]
        public void Invoke(ref TInput inArg)
        {
            switch(m_Mode)
            {
                case CallbackMode.NoArg:
                    ((Action) m_CallbackObject)();
                    break;

                case CallbackMode.NativeArg:
                    ((Action<TInput>) m_CallbackObject)(inArg);
                    break;

                case CallbackMode.NativeArgRef:
                    ((RefAction<TInput>) m_CallbackObject)(ref inArg);
                    break;

                case CallbackMode.CastedArg:
                    m_CastedArgInvoker(m_CallbackObject, inArg);
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
            m_CastedArgInvoker = null;
            m_CallbackObject = inAction;
        }

        public void Set(Action<TInput> inAction)
        {
            if (inAction == null)
                throw new ArgumentNullException("inAction");

            m_Mode = CallbackMode.NativeArg;
            m_CallbackObject = inAction;
            m_CastedArgInvoker = null;
        }

        public void Set(RefAction<TInput> inAction)
        {
            if (inAction == null)
                throw new ArgumentNullException("inAction");

            m_Mode = CallbackMode.NativeArgRef;
            m_CallbackObject = inAction;
            m_CastedArgInvoker = null;
        }

        public void Set<U>(Action<U> inAction)
        {
            if (inAction == null)
                throw new ArgumentNullException("inAction");

            m_Mode = CallbackMode.CastedArg;
            m_CallbackObject = inAction;
            m_CastedArgInvoker = CastedActionInvoker<TInput, U>.Invoker;
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

        public bool Equals(CastableAction<TInput> other)
        {
            if (m_Mode != other.m_Mode)
                return false;

            if (m_Mode == CallbackMode.Unassigned)
                return true;
            
            return m_CallbackObject == other.m_CallbackObject;
        }

        public bool Equals(Action<TInput> other)
        {
            if (other == null)
                return m_Mode == CallbackMode.Unassigned;

            return m_Mode == CallbackMode.NativeArg && m_CallbackObject == ((Delegate) other);
        }

        public bool Equals(RefAction<TInput> other)
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

            if (obj is CastableAction<TInput>)
            {
                return Equals((CastableAction<TInput>) obj);
            }

            MulticastDelegate del = obj as MulticastDelegate;
            if (del == null)
                return false;

            return Equals(del);
        }

        #endregion // Overrides

        #region Create
    
        static public CastableAction<TInput> Create(Action inAction)
        {
            return new CastableAction<TInput>(inAction);
        }

        static public CastableAction<TInput> Create(Action<TInput> inAction)
        {
            return new CastableAction<TInput>(inAction);
        }

        static public CastableAction<TInput> Create(RefAction<TInput> inAction)
        {
            return new CastableAction<TInput>(inAction);
        }

        static public CastableAction<TInput> Create<U>(Action<U> inAction)
        {
            return new CastableAction<TInput>(inAction, CastedActionInvoker<TInput, U>.Invoker);
        }

        #endregion // Create

        #region Operators

        static public bool operator ==(CastableAction<TInput> left, CastableAction<TInput> right)
        {
            return left.Equals(right);
        }

        static public bool operator !=(CastableAction<TInput> left, CastableAction<TInput> right)
        {
            return !left.Equals(right);
        }

        #endregion // Operators
    }

    public delegate void RefAction<T>(ref T inValue);

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

        static private void CastedInvoke(Delegate inDelegate, TInput inInput)
        {
            ((Action<TOutput>) inDelegate).Invoke(CastableArgument.Cast<TInput, TOutput>(inInput));
        }
    }
}