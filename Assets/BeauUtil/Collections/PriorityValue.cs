/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    24 August 2020
 * 
 * File:    PriorityValue.cs
 * Purpose: Item with a priority.
 */

#if CSHARP_7_3_OR_NEWER
#define EXPANDED_REFS
#endif // CSHARP_7_3_OR_NEWER

using System;
using System.Collections.Generic;

namespace BeauUtil
{
    /// <summary>
    /// Value with an associated priority
    /// </summary>
    [Serializable]
    public struct PriorityValue<T> : IComparable<PriorityValue<T>>, IEquatable<PriorityValue<T>>
    {
        public T Value;
        public float Priority;

        #if EXPANDED_REFS
        public PriorityValue(in T inValue, float inPriority = 0)
        #else
        public PriorityValue(T inValue, float inPriority = 0)
        #endif // EXPANDED_REFS
        {
            Value = inValue;
            Priority = inPriority;
        }

        #region Interfaces

        public int CompareTo(PriorityValue<T> other)
        {
            float comp = Priority - other.Priority;
            if (comp > 0)
                return -1;
            if (comp < 0)
                return 1;
            return 0;
        }

        public bool Equals(PriorityValue<T> other)
        {
            return Priority == other.Priority && EqualityComparer<T>.Default.Equals(Value, other.Value);
        }

        #endregion // Interfaces

        #region Operators

        public override bool Equals(object obj)
        {
            if (obj is PriorityValue<T>)
                return Equals((PriorityValue<T>) obj);
            return false;
        }

        public override int GetHashCode()
        {
            int hash = EqualityComparer<T>.Default.GetHashCode(Value);
            hash = (hash << 2) ^ Priority.GetHashCode();
            return hash;
        }

        public override string ToString()
        {
            if (Value == null)
                return string.Format("null:{0}", Priority);
            if (typeof(T) == typeof(string))
                return string.Format("\"{0}\":{1}", Value, Priority);
            return string.Format("{0}:{1}", Value, Priority);
        }

        static public implicit operator T(PriorityValue<T> inValue)
        {
            return inValue.Value;
        }

        static public bool operator ==(PriorityValue<T> inLeft, PriorityValue<T> inRight)
        {
            return inLeft.Equals(inRight);
        }

        static public bool operator !=(PriorityValue<T> inLeft, PriorityValue<T> inRight)
        {
            return !inLeft.Equals(inRight);
        }

        #endregion // Operators
    }
}