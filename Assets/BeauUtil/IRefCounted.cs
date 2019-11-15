/*
 * Copyright (C) 2017-2019. Filament Games, LLC. All rights reserved.
 * Author:  Alex Beauchesne
 * Date:    31 Oct 2019
 * 
 * File:    IRefCounted.cs
 * Purpose: Reference counting utility.
 */

using System;

namespace BeauUtil
{
    /// <summary>
    /// Interface for a reference-counted object.
    /// </summary>
    public interface IRefCounted
    {
        /// <summary>
        /// Returns the reference count.
        /// </summary>
        int ReferenceCount { get; set; }

        /// <summary>
        /// Called when the object is first referenced.
        /// </summary>
        void OnReferenced();

        /// <summary>
        /// Called when the object has no more references.
        /// </summary>
        void OnReleased();
    }

    /// <summary>
    /// Reference to an IRefCounted object.
    /// </summary>
    public struct SharedRef<T> : IEquatable<T>, IEquatable<SharedRef<T>> where T : class, IRefCounted
    {
        private T m_Value;

        public SharedRef(T inValue)
        {
            m_Value = inValue;
            if (m_Value != null)
                m_Value.AcquireRef();
        }

        /// <summary>
        /// Gets/sets the currently referenced value.
        /// </summary>
        public T Value
        {
            get { return m_Value; }
            set { Set(value); }
        }

        /// <summary>
        /// Tries to set the reference to a different object.
        /// </summary>
        public bool Set(T inValue)
        {
            if (m_Value != inValue)
            {
                if (m_Value != null)
                {
                    m_Value.ReleaseRef();
                }

                m_Value = inValue;

                if (m_Value != null)
                {
                    m_Value.AcquireRef();
                }

                return true;
            }

            return false;
        }

        #region IEquatable

        public bool Equals(T obj)
        {
            return m_Value == obj;
        }

        public bool Equals(SharedRef<T> obj)
        {
            return m_Value == obj.m_Value;
        }

        #endregion // IEquatable

        #region Overrides

        public override bool Equals(object obj)
        {
            if (obj is T)
            {
                return Equals((T) obj);
            }
            else if (obj is SharedRef<T>)
            {
                return Equals((SharedRef<T>) obj);
            }
            else if (obj == null)
            {
                return m_Value == null;
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return m_Value == null ? 0 : m_Value.GetHashCode();
        }

        public override string ToString()
        {
            return m_Value == null ? "null" : m_Value.ToString();
        }

        #endregion // Overrides

        #region Operators

        static public implicit operator T(SharedRef<T> inRef)
        {
            return inRef.m_Value;
        }

        static public bool operator ==(SharedRef<T> inA, T inB)
        {
            return inA.m_Value == inB;
        }

        static public bool operator !=(SharedRef<T> inA, T inB)
        {
            return inA.m_Value != inB;
        }

        #endregion // Operators
    }

    static public class RefCount
    {
        /// <summary>
        /// Acquires a reference to the given object.
        /// </summary>
        static public void AcquireRef(this IRefCounted inRef, int inRefCount = 1)
        {
            if (inRefCount <= 0)
                return;

            inRef.ReferenceCount += inRefCount;
            if (inRef.ReferenceCount == inRefCount)
            {
                inRef.OnReferenced();
            }
        }

        /// <summary>
        /// Releases a reference to the given object.
        /// </summary>
        static public void ReleaseRef(this IRefCounted inRef, int inRefCount = 1)
        {
            if (inRefCount <= 0)
                return;

            inRef.ReferenceCount -= inRefCount;
            if (inRef.ReferenceCount == 0)
            {
                inRef.OnReleased();
            }
        }

        /// <summary>
        /// Returns if this object has active references.
        /// </summary>
        static public bool IsReferenced(this IRefCounted inRef)
        {
            return inRef.ReferenceCount > 0;
        }
    }
}