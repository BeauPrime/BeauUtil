/*
 * Copyright (C) 2017-2019. Filament Games, LLC. All rights reserved.
 * Author:  Alex Beauchesne
 * Date:    4 April 2019
 * 
 * File:    Ref.cs
 * Purpose: Reference value wrapper and some reference utility functions.
*/

using System;

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
        static public void Dispose<T>(ref T inObjectToDispose) where T : class, IDisposable
        {
            if (inObjectToDispose != null)
            {
                inObjectToDispose.Dispose();
                inObjectToDispose = null;
            }
        }

        /// <summary>
        /// Safetly disposes and switches a disposable object to another object.
        /// </summary>
        static public void Replace<T>(ref T inObject, T inReplace) where T : class, IDisposable
        {
            if (inObject != null && inObject != inReplace)
                inObject.Dispose();
            inObject = inReplace;
        }

        /// <summary>
        /// Swaps two references.
        /// </summary>
        static public void Swap<T>(ref T inA, ref T inB)
        {
            T temp = inA;
            inA = inB;
            inB = temp;
        }
    }
}
