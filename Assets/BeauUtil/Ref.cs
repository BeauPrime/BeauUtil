/*
 * Copyright (C) 2017-2019. Filament Games, LLC. All rights reserved.
 * Author:  Alex Beauchesne
 * Date:    4 April 2019
 * 
 * File:    Ref.cs
 * Purpose: Reference value wrapper and some reference utility functions.
 */

using System;
using System.Collections.Generic;

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

        /// <summary>
        /// Safetly disposes and switches a disposable object to another object.
        /// </summary>
        static public bool ReplaceDisposable<T>(ref T ioObject, T inReplace) where T : class, IDisposable
        {
            if (EqualityComparer<T>.Default.Equals(ioObject, inReplace))
                return false;

            if (ioObject != null)
                ioObject.Dispose();

            ioObject = inReplace;
            return true;
        }

        /// <summary>
        /// Replaces a reference to one object with a reference to another.
        /// </summary>
        static public bool Replace<T>(ref T ioObject, T inReplace)
        {
            if (EqualityComparer<T>.Default.Equals(ioObject, inReplace))
                return false;

            ioObject = inReplace;
            return true;
        }

        /// <summary>
        /// Swaps two references.
        /// </summary>
        static public void Swap<T>(ref T ioA, ref T ioB)
        {
            T temp = ioA;
            ioA = ioB;
            ioB = temp;
        }
    }
}