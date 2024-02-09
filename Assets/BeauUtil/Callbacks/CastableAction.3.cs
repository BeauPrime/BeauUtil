/*
 * Copyright (C) 2023. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    16 Dec 2023
 * 
 * File:    CastableAction.3.cs
 * Purpose: Callback that supports two different signature types.
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
    /// Basic callback. Supports two signatures:
    /// no argument (Action)
    /// object argument (Action<T0, T1, T2>)
    /// </summary>
    public struct CastableAction<TInput0, TInput1, TInput2> : IEquatable<CastableAction<TInput0, TInput1, TInput2>>, IEquatable<Action>, IEquatable<Action<TInput0, TInput1, TInput2>>, IEquatable<RefAction<TInput0, TInput1, TInput2>>, IEquatable<MulticastDelegate>, IDisposable
    {
        private Delegate m_CallbackObject;
#if SUPPORTS_FUNCTION_POINTERS
        private unsafe void* m_CallbackPtr;
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
            m_CallbackObject = inAction;
        }

        private unsafe CastableAction(Action<TInput0, TInput1, TInput2> inAction)
        {
            if (inAction == null)
                throw new ArgumentNullException("inAction");

            m_Mode = CallbackMode.NativeArg;
#if SUPPORTS_FUNCTION_POINTERS
            m_CallbackPtr = null;
#endif // SUPPORTS_FUNCTION_POINTERS
            m_CallbackObject = inAction;
        }

        private unsafe CastableAction(RefAction<TInput0, TInput1, TInput2> inAction)
        {
            if (inAction == null)
                throw new ArgumentNullException("inAction");

            m_Mode = CallbackMode.NativeArgRef;
#if SUPPORTS_FUNCTION_POINTERS
            m_CallbackPtr = null;
#endif // SUPPORTS_FUNCTION_POINTERS
            m_CallbackObject = inAction;
        }

#if SUPPORTS_FUNCTION_POINTERS
        private unsafe CastableAction(delegate*<void> inPointer)
        {
            if (inPointer == null)
                throw new ArgumentNullException("inPointer");

            m_Mode = CallbackMode.NoArg_Ptr;
            m_CallbackPtr = inPointer;
            m_CallbackObject = null;
        }

        private unsafe CastableAction(delegate*<TInput0, TInput1, TInput2, void> inPointer)
        {
            if (inPointer == null)
                throw new ArgumentNullException("inPointer");

            m_Mode = CallbackMode.NativeArg_Ptr;
            m_CallbackPtr = inPointer;
            m_CallbackObject = null;
        }

        private unsafe CastableAction(delegate*<ref TInput0, ref TInput1, ref TInput2, void> inPointer)
        {
            if (inPointer == null)
                throw new ArgumentNullException("inPointer");

            m_Mode = CallbackMode.NativeArgRef_Ptr;
            m_CallbackPtr = inPointer;
            m_CallbackObject = null;
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
        public void Invoke(TInput0 inArg0, TInput1 inArg1, TInput2 inArg2)
        {
            switch (m_Mode)
            {
                case CallbackMode.NoArg:
                    ((Action) m_CallbackObject)();
                    break;

                case CallbackMode.NativeArg:
                    ((Action<TInput0, TInput1, TInput2>) m_CallbackObject)(inArg0, inArg1, inArg2);
                    break;

                case CallbackMode.NativeArgRef:
                    ((RefAction<TInput0, TInput1, TInput2>) m_CallbackObject)(ref inArg0, ref inArg1, ref inArg2);
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
                        ((delegate*<TInput0, TInput1, TInput2, void>) m_CallbackPtr)(inArg0, inArg1, inArg2);
                    }
                    break;

                case CallbackMode.NativeArgRef_Ptr:
                    unsafe
                    {
                        ((delegate*<ref TInput0, ref TInput1, ref TInput2, void>) m_CallbackPtr)(ref inArg0, ref inArg1, ref inArg2);
                    }
                    break;
#endif // SUPPORTS_FUNCTION_POINTERS

                case CallbackMode.Unassigned:
                default:
                    throw new InvalidOperationException("Callback has not been initialized, or has been disposed");
            }
        }

        [Il2CppSetOption(Option.NullChecks, false)]
        public void Invoke(ref TInput0 inArg0, ref TInput1 inArg1, ref TInput2 inArg2)
        {
            switch (m_Mode)
            {
                case CallbackMode.NoArg:
                    ((Action) m_CallbackObject)();
                    break;

                case CallbackMode.NativeArg:
                    ((Action<TInput0, TInput1, TInput2>) m_CallbackObject)(inArg0, inArg1, inArg2);
                    break;

                case CallbackMode.NativeArgRef:
                    ((RefAction<TInput0, TInput1, TInput2>) m_CallbackObject)(ref inArg0, ref inArg1, ref inArg2);
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
                        ((delegate*<TInput0, TInput1, TInput2, void>) m_CallbackPtr)(inArg0, inArg1, inArg2);
                    }
                    break;

                case CallbackMode.NativeArgRef_Ptr:
                    unsafe
                    {
                        ((delegate*<ref TInput0, ref TInput1, ref TInput2, void>) m_CallbackPtr)(ref inArg0, ref inArg1, ref inArg2);
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
                m_CallbackObject = inAction;
#if SUPPORTS_FUNCTION_POINTERS
                m_CallbackPtr = null;
#endif // SUPPORTS_FUNCTION_POINTERS
            }
        }

        public void Set(Action<TInput0, TInput1, TInput2> inAction)
        {
            if (inAction == null)
                throw new ArgumentNullException("inAction");

            m_Mode = CallbackMode.NativeArg;
            m_CallbackObject = inAction;
            unsafe
            {
#if SUPPORTS_FUNCTION_POINTERS
                m_CallbackPtr = null;
#endif // SUPPORTS_FUNCTION_POINTERS
            }
        }

        public void Set(RefAction<TInput0, TInput1, TInput2> inAction)
        {
            if (inAction == null)
                throw new ArgumentNullException("inAction");

            m_Mode = CallbackMode.NativeArgRef;
            m_CallbackObject = inAction;
            unsafe
            {
#if SUPPORTS_FUNCTION_POINTERS
                m_CallbackPtr = null;
#endif // SUPPORTS_FUNCTION_POINTERS
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
        }

        public unsafe void Set(delegate*<TInput0, TInput1, TInput2, void> inPointer)
        {
            if (inPointer == null)
                throw new ArgumentNullException("inPointer");

            m_Mode = CallbackMode.NativeArg_Ptr;
            m_CallbackObject = null;
            m_CallbackPtr = inPointer;
        }

        public unsafe void Set(delegate*<ref TInput0, ref TInput1, ref TInput2, void> inPointer)
        {
            if (inPointer == null)
                throw new ArgumentNullException("inPointer");

            m_Mode = CallbackMode.NativeArgRef_Ptr;
            m_CallbackObject = null;
            m_CallbackPtr = inPointer;
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
            }
        }

        #endregion // IDisposable

        #region IEquatable

        public bool Equals(CastableAction<TInput0, TInput1, TInput2> other)
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

        public bool Equals(Action<TInput0, TInput1, TInput2> other)
        {
            if (other == null)
                return m_Mode == CallbackMode.Unassigned;

            return m_Mode == CallbackMode.NativeArg && m_CallbackObject == ((Delegate) other);
        }

        public bool Equals(RefAction<TInput0, TInput1, TInput2> other)
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

        public unsafe bool Equals(delegate*<TInput0, TInput1, TInput2, void> other)
        {
            if (other == null)
                return m_Mode == CallbackMode.Unassigned;

            return m_CallbackPtr == other;
        }

        public unsafe bool Equals(delegate*<ref TInput0, ref TInput1, ref TInput2, void> other)
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

            if (obj is CastableAction<TInput0, TInput1, TInput2>)
            {
                return Equals((CastableAction<TInput0, TInput1, TInput2>) obj);
            }

            MulticastDelegate del = obj as MulticastDelegate;
            if (del == null)
                return false;

            return Equals(del);
        }

        #endregion // Overrides

        #region Create

        static public CastableAction<TInput0, TInput1, TInput2> Create(Action inAction)
        {
            return new CastableAction<TInput0, TInput1, TInput2>(inAction);
        }

        static public CastableAction<TInput0, TInput1, TInput2> Create(Action<TInput0, TInput1, TInput2> inAction)
        {
            return new CastableAction<TInput0, TInput1, TInput2>(inAction);
        }

        static public CastableAction<TInput0, TInput1, TInput2> Create(RefAction<TInput0, TInput1, TInput2> inAction)
        {
            return new CastableAction<TInput0, TInput1, TInput2>(inAction);
        }

#if SUPPORTS_FUNCTION_POINTERS

        static public unsafe CastableAction<TInput0, TInput1, TInput2> Create(delegate*<void> inPointer)
        {
            return new CastableAction<TInput0, TInput1, TInput2>(inPointer);
        }

        static public unsafe CastableAction<TInput0, TInput1, TInput2> Create(delegate*<TInput0, TInput1, TInput2, void> inPointer)
        {
            return new CastableAction<TInput0, TInput1, TInput2>(inPointer);
        }

        static public unsafe CastableAction<TInput0, TInput1, TInput2> Create(delegate*<ref TInput0, ref TInput1, ref TInput2, void> inPointer)
        {
            return new CastableAction<TInput0, TInput1, TInput2>(inPointer);
        }

#endif // SUPPORTS_FUNCTION_POINTERS

        #endregion // Create

        #region Operators

        static public bool operator ==(CastableAction<TInput0, TInput1, TInput2> left, CastableAction<TInput0, TInput1, TInput2> right)
        {
            return left.Equals(right);
        }

        static public bool operator !=(CastableAction<TInput0, TInput1, TInput2> left, CastableAction<TInput0, TInput1, TInput2> right)
        {
            return !left.Equals(right);
        }

        #endregion // Operators
    }

    public delegate void RefAction<T0, T1, T2>(ref T0 inValue0, ref T1 inValue1, ref T2 inValue2);
}