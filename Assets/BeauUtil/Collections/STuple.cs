/*
 * Copyright (C) 2024. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    16 Sept 2024
 * 
 * File:    STuple.cs
 * Purpose: Struct tuple.
 */

using System;
using System.Collections;

namespace BeauUtil
{
    public interface ISTuple
    {
        int Length { get; }
    }

    /// <summary>
    /// Readonly 1-element tuple, as a struct.
    /// </summary>
    public readonly struct STuple<T1> : IEquatable<STuple<T1>>, ISTuple
    {
        public readonly T1 Item1;

        public STuple(in T1 inItem1)
        {
            Item1 = inItem1;
        }

        public int Length
        {
            get { return 1; }
        }

        public bool Equals(STuple<T1> other)
        {
            return CompareUtils.Equals(Item1, other.Item1);
        }

        public override bool Equals(object obj)
        {
            if (obj is STuple<T1>)
            {
                return Equals((STuple<T1>) obj);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return CompareUtils.GetHashCode(Item1);
        }

        public override string ToString()
        {
            return string.Format("({0})", Item1);
        }
    }

    /// <summary>
    /// Readonly 2-element tuple, as a struct.
    /// </summary>
    public readonly struct STuple<T1, T2> : IEquatable<STuple<T1, T2>>, ISTuple
    {
        public readonly T1 Item1;
        public readonly T2 Item2;

        public STuple(in T1 inItem1, in T2 inItem2)
        {
            Item1 = inItem1;
            Item2 = inItem2;
        }

        public int Length
        {
            get { return 2; }
        }

        public bool Equals(STuple<T1, T2> other)
        {
            return CompareUtils.Equals(Item1, other.Item1)
                && CompareUtils.Equals(Item2, other.Item2);
        }

        public override bool Equals(object obj)
        {
            if (obj is STuple<T1, T2>)
            {
                return Equals((STuple<T1, T2>) obj);
            }
            return false;
        }

        public override int GetHashCode()
        {
            int hash = CompareUtils.GetHashCode(Item1);
            hash = (hash << 5) ^ CompareUtils.GetHashCode(Item2);
            return hash;
        }

        public override string ToString()
        {
            return string.Format("({0}, {1})", Item1, Item2);
        }
    }

    /// <summary>
    /// Readonly 3-element tuple, as a struct.
    /// </summary>
    public readonly struct STuple<T1, T2, T3> : IEquatable<STuple<T1, T2, T3>>, ISTuple
    {
        public readonly T1 Item1;
        public readonly T2 Item2;
        public readonly T3 Item3;

        public STuple(in T1 inItem1, in T2 inItem2, in T3 inItem3)
        {
            Item1 = inItem1;
            Item2 = inItem2;
            Item3 = inItem3;
        }

        public int Length
        {
            get { return 3; }
        }

        public bool Equals(STuple<T1, T2, T3> other)
        {
            return CompareUtils.Equals(Item1, other.Item1)
                && CompareUtils.Equals(Item2, other.Item2)
                && CompareUtils.Equals(Item3, other.Item3);
        }

        public override bool Equals(object obj)
        {
            if (obj is STuple<T1, T2, T3>)
            {
                return Equals((STuple<T1, T2, T3>) obj);
            }
            return false;
        }

        public override int GetHashCode()
        {
            int hash = CompareUtils.GetHashCode(Item1);
            hash = (hash << 5) ^ CompareUtils.GetHashCode(Item2);
            hash = (hash >> 3) ^ CompareUtils.GetHashCode(Item3);
            return hash;
        }

        public override string ToString()
        {
            return string.Format("({0}, {1}, {2})", Item1, Item2, Item3);
        }
    }

    /// <summary>
    /// Readonly 4-element tuple, as a struct.
    /// </summary>
    public readonly struct STuple<T1, T2, T3, T4> : IEquatable<STuple<T1, T2, T3, T4>>, ISTuple
    {
        public readonly T1 Item1;
        public readonly T2 Item2;
        public readonly T3 Item3;
        public readonly T4 Item4;

        public STuple(in T1 inItem1, in T2 inItem2, in T3 inItem3, in T4 inItem4)
        {
            Item1 = inItem1;
            Item2 = inItem2;
            Item3 = inItem3;
            Item4 = inItem4;
        }

        public int Length
        {
            get { return 4; }
        }

        public bool Equals(STuple<T1, T2, T3, T4> other)
        {
            return CompareUtils.Equals(Item1, other.Item1)
                && CompareUtils.Equals(Item2, other.Item2)
                && CompareUtils.Equals(Item3, other.Item3)
                && CompareUtils.Equals(Item4, other.Item4);
        }

        public override bool Equals(object obj)
        {
            if (obj is STuple<T1, T2, T3, T4>)
            {
                return Equals((STuple<T1, T2, T3, T4>) obj);
            }
            return false;
        }

        public override int GetHashCode()
        {
            int hash = CompareUtils.GetHashCode(Item1);
            hash = (hash << 5) ^ CompareUtils.GetHashCode(Item2);
            hash = (hash >> 3) ^ CompareUtils.GetHashCode(Item3);
            hash = (hash << 5) ^ CompareUtils.GetHashCode(Item4);
            return hash;
        }

        public override string ToString()
        {
            return string.Format("({0}, {1}, {2}, {3})", Item1, Item2, Item3, Item4);
        }
    }
}