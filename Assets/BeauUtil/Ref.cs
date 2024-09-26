/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    4 April 2019
 * 
 * File:    Ref.cs
 * Purpose: Reference value wrapper and some reference utility functions.
 */

#if NET_4_6 || CSHARP_7_OR_LATER
#define HAS_ENUM_CONSTRAINT
#endif // NET_4_6 || CSHARP_7_OR_LATER

#if CSHARP_7_3_OR_NEWER
#define UNMANAGED_CONSTRAINT
#endif // CSHARP_7_3_OR_NEWER

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace BeauUtil
{
    /// <summary>
    /// Hacky reference for when you can't use
    /// C# refs in parameters 
    /// </summary>
    public class Ref<T>
    {
        public T Value;

        public Ref()
        {
            Value = default(T);
        }

        public Ref(T inValue)
        {
            Value = inValue;
        }

        static public implicit operator T(Ref<T> inRef)
        {
            return inRef.Value;
        }

        static public implicit operator Ref<T>(T inValue)
        {
            return new Ref<T>(inValue);
        }
    }

    /// <summary>
    /// Helper functions operating on references.
    /// </summary>
    static public class Ref
    {
        /// <summary>
        /// Safely disposes of an object, if it's an IDisposable, and sets its reference to null.
        /// </summary>
        static public bool TryDispose<T>(ref T ioObjectToDispose) where T : class
        {
            if (ioObjectToDispose != null)
            {
                IDisposable disposable = ioObjectToDispose as IDisposable;
                if (disposable != null)
                {
                    disposable.Dispose();
                }
                ioObjectToDispose = null;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Safely disposes of an object and sets its reference to null.
        /// </summary>
        static public bool Dispose<T>(ref T ioObjectToDispose) where T : class, IDisposable
        {
            if (ioObjectToDispose != null)
            {
                ioObjectToDispose.Dispose();
                ioObjectToDispose = null;
                return true;
            }

            return false;
        }

        #region Interfaces

        /// <summary>
        /// Safetly disposes and switches a disposable object to another object.
        /// </summary>
        static public bool ReplaceDisposable<T>(ref T ioObject, T inReplace) where T : class, IDisposable
        {
            if (CompareUtils.Equals(ioObject, inReplace))
                return false;

            if (ioObject != null)
                ioObject.Dispose();

            ioObject = inReplace;
            return true;
        }

        /// <summary>
        /// Safetly releases and switches a reference-counted object to another object.
        /// </summary>
        static public bool ReplaceRefCounted<T>(ref T ioObject, T inReplace, int inRefCount = 1) where T : class, IRefCounted
        {
            if (CompareUtils.Equals(ioObject, inReplace))
                return false;

            if (ioObject != null)
                ioObject.ReleaseRef(inRefCount);

            ioObject = inReplace;

            if (ioObject != null)
                ioObject.AcquireRef(inRefCount);

            return true;
        }

        #endregion // Interfaces

        #region Replace

        /// <summary>
        /// Replaces a reference to one object with a reference to another.
        /// </summary>
        static public bool Replace<T>(ref T ioObject, T inReplace)
        {
            if (CompareUtils.Equals(ioObject, inReplace))
                return false;

            ioObject = inReplace;
            return true;
        }

        /// <summary>
        /// Replaces one integral with another.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public bool Replace(ref byte ioObject, byte inReplace)
        {
            if (ioObject == inReplace)
                return false;

            ioObject = inReplace;
            return true;
        }

        /// <summary>
        /// Replaces one integral with another.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public bool Replace(ref sbyte ioObject, sbyte inReplace)
        {
            if (ioObject == inReplace)
                return false;

            ioObject = inReplace;
            return true;
        }

        /// <summary>
        /// Replaces one integral with another.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public bool Replace(ref short ioObject, short inReplace)
        {
            if (ioObject == inReplace)
                return false;

            ioObject = inReplace;
            return true;
        }

        /// <summary>
        /// Replaces one integral with another.
        /// </summary>
        static public bool Replace(ref ushort ioObject, ushort inReplace)
        {
            if (ioObject == inReplace)
                return false;

            ioObject = inReplace;
            return true;
        }

        /// <summary>
        /// Replaces one integral with another.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public bool Replace(ref int ioObject, int inReplace)
        {
            if (ioObject == inReplace)
                return false;

            ioObject = inReplace;
            return true;
        }

        /// <summary>
        /// Replaces one integral with another.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public bool Replace(ref uint ioObject, uint inReplace)
        {
            if (ioObject == inReplace)
                return false;

            ioObject = inReplace;
            return true;
        }

        /// <summary>
        /// Replaces one integral with another.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public bool Replace(ref long ioObject, long inReplace)
        {
            if (ioObject == inReplace)
                return false;

            ioObject = inReplace;
            return true;
        }

        /// <summary>
        /// Replaces one integral with another.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public bool Replace(ref ulong ioObject, ulong inReplace)
        {
            if (ioObject == inReplace)
                return false;

            ioObject = inReplace;
            return true;
        }

        /// <summary>
        /// Replaces one integral with another.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public bool Replace(ref char ioObject, char inReplace)
        {
            if (ioObject == inReplace)
                return false;

            ioObject = inReplace;
            return true;
        }

        /// <summary>
        /// Replaces one boolean with another.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public bool Replace(ref bool ioObject, bool inReplace)
        {
            if (ioObject == inReplace)
                return false;

            ioObject = inReplace;
            return true;
        }

        /// <summary>
        /// Replaces one enum with another.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public bool ReplaceEnum<T>(ref T ioObject, T inReplace)
#if UNMANAGED_CONSTRAINT
            where T : unmanaged, Enum 
#elif HAS_ENUM_CONSTRAINT
            where T : struct, Enum
#else
            where T : struct, IConvertible
#endif // HAS_ENUM_CONSTRAINT
        {
            if (Enums.AreEqual(ioObject, inReplace))
                return false;

            ioObject = inReplace;
            return true;
        }

        #endregion // Replace

        #region CompareExchange

        /// <summary>
        /// Replaces a reference to one object with a reference to another.
        /// </summary>
        static public bool CompareExchange<T>(ref T ioObject, T inExpected, T inReplace)
        {
            if (!CompareUtils.Equals(ioObject, inExpected))
                return false;

            ioObject = inReplace;
            return true;
        }

        #endregion // CompareExchange

        /// <summary>
        /// Swaps two references.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public void Swap<T>(ref T ioA, ref T ioB)
        {
            T temp = ioA;
            ioA = ioB;
            ioB = temp;
        }
    }
}