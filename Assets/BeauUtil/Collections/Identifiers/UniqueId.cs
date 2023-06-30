/*
 * Copyright (C) 2023. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    29 June 2023
 * 
 * File:    UniqueId.cs
 * Purpose: Unique identifiers/handles.
*/

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace BeauUtil
{
    /// <summary>
    /// Unique 16-bit identifier.
    /// </summary>
    [DefaultEqualityComparer(typeof(UniqueId16.Comparer)), DefaultSorter(typeof(UniqueId16.Comparer))]
    public struct UniqueId16 : IEquatable<UniqueId16>, IComparable<UniqueId16>
    {
        private const int IndexBits = 10;
        public const int MaxIndex = (1 << IndexBits);
        internal const int IndexMask = MaxIndex - 1;
        private const int VersionBits = 6;
        internal const int VersionMask = (1 << VersionBits) - 1;

        /// <summary>
        /// Unique identifier.
        /// </summary>
        public readonly ushort Id;

        /// <summary>
        /// Index portion of the identifier.
        /// </summary>
        public int Index
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return Id & IndexMask; }
        }

        /// <summary>
        /// Version portion of the identifier.
        /// </summary>
        public int Version
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return (Id >> IndexBits) & VersionMask; }
        }

        /// <summary>
        /// Invalid identifier.
        /// </summary>
        static public readonly UniqueId16 Invalid = default(UniqueId16);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UniqueId16(ushort inId)
        {
            Id = inId;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UniqueId16(ushort inIndex, ushort inVersion)
        {
            Id = (ushort) ((inIndex & IndexMask) | ((inVersion & VersionMask) << IndexBits));
        }

        #region Interfaces

        public bool Equals(UniqueId16 other)
        {
            return Id == other.Id;
        }

        public int CompareTo(UniqueId16 other)
        {
            return Id - other.Id;
        }

        #endregion // Interfaces

        #region Overrides

        public override bool Equals(object obj)
        {
            if (obj is UniqueId16)
                return Equals((UniqueId16) obj);
            return false;
        }

        public override int GetHashCode()
        {
            return Id;
        }

        public override string ToString()
        {
            if (Id == 0)
                return "[Invalid]";
            return string.Format("[i{0}-v{1}]", Index, Version);
        }

        #endregion // Overrides

        #region Operators

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public bool operator==(UniqueId16 inA, UniqueId16 inB)
        {
            return inA.Id == inB.Id;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public bool operator !=(UniqueId16 inA, UniqueId16 inB)
        {
            return inA.Id != inB.Id;
        }

        static public bool operator <(UniqueId16 inA, UniqueId16 inB)
        {
            return inA.Id < inB.Id;
        }

        static public bool operator <=(UniqueId16 inA, UniqueId16 inB)
        {
            return inA.Id <= inB.Id;
        }

        static public bool operator >(UniqueId16 inA, UniqueId16 inB)
        {
            return inA.Id > inB.Id;
        }

        static public bool operator >=(UniqueId16 inA, UniqueId16 inB)
        {
            return inA.Id >= inB.Id;
        }

        #endregion // Operators

        #region Comparisons

        /// <summary>
        /// Default comparer.
        /// </summary>
        private sealed class Comparer : IEqualityComparer<UniqueId16>, IComparer<UniqueId16>
        {
            public int Compare(UniqueId16 x, UniqueId16 y)
            {
                return x.Id - y.Id;
            }

            public bool Equals(UniqueId16 x, UniqueId16 y)
            {
                return x.Id == y.Id;
            }

            public int GetHashCode(UniqueId16 obj)
            {
                return obj.Id;
            }
        }

        #endregion // Comparisons
    }

    /// <summary>
    /// Unique 32-bit identifier.
    /// </summary>
    [DefaultEqualityComparer(typeof(UniqueId32.Comparer)), DefaultSorter(typeof(UniqueId32.Comparer))]
    public struct UniqueId32 : IEquatable<UniqueId32>, IComparable<UniqueId32>
    {
        private const int IndexBits = 24;
        public const int MaxIndex = (1 << IndexBits);
        internal const int IndexMask = MaxIndex - 1;
        private const int VersionBits = 8;
        internal const int VersionMask = (1 << VersionBits) - 1;

        /// <summary>
        /// Unique identifier.
        /// </summary>
        public readonly uint Id;

        /// <summary>
        /// Index portion of the identifier.
        /// </summary>
        public int Index
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return (int) (Id & IndexMask); }
        }

        /// <summary>
        /// Version portion of the identifier.
        /// </summary>
        public int Version
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return (int) (Id >> IndexBits) & VersionMask; }
        }

        /// <summary>
        /// Invalid identifier.
        /// </summary>
        static public readonly UniqueId32 Invalid = default(UniqueId32);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UniqueId32(uint inId)
        {
            Id = inId;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UniqueId32(uint inIndex, uint inVersion)
        {
            Id = (inIndex & IndexMask) | ((inVersion & VersionMask) << IndexBits);
        }

        #region Interfaces

        public bool Equals(UniqueId32 other)
        {
            return Id == other.Id;
        }

        public int CompareTo(UniqueId32 other)
        {
            return Id.CompareTo(other.Id);
        }

        #endregion // Interfaces

        #region Overrides

        public override bool Equals(object obj)
        {
            if (obj is UniqueId32)
                return Equals((UniqueId32) obj);
            return false;
        }

        public override int GetHashCode()
        {
            return unchecked((int) Id);
        }

        public override string ToString()
        {
            if (Id == 0)
                return "[Invalid]";
            return string.Format("[i{0}-v{1}]", Index, Version);
        }

        #endregion // Overrides

        #region Operators

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public bool operator ==(UniqueId32 inA, UniqueId32 inB)
        {
            return inA.Id == inB.Id;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public bool operator !=(UniqueId32 inA, UniqueId32 inB)
        {
            return inA.Id != inB.Id;
        }

        static public bool operator <(UniqueId32 inA, UniqueId32 inB)
        {
            return inA.Id < inB.Id;
        }

        static public bool operator <=(UniqueId32 inA, UniqueId32 inB)
        {
            return inA.Id <= inB.Id;
        }

        static public bool operator >(UniqueId32 inA, UniqueId32 inB)
        {
            return inA.Id > inB.Id;
        }

        static public bool operator >=(UniqueId32 inA, UniqueId32 inB)
        {
            return inA.Id >= inB.Id;
        }

        #endregion // Operators

        #region Comparisons

        /// <summary>
        /// Default comparer.
        /// </summary>
        private sealed class Comparer : IEqualityComparer<UniqueId32>, IComparer<UniqueId32>
        {
            public int Compare(UniqueId32 x, UniqueId32 y)
            {
                return x.Id.CompareTo(y.Id);
            }

            public bool Equals(UniqueId32 x, UniqueId32 y)
            {
                return x.Id == y.Id;
            }

            public int GetHashCode(UniqueId32 obj)
            {
                return unchecked((int) obj.Id);
            }
        }

        #endregion // Comparisons
    }

    /// <summary>
    /// Unique 64-bit identifier.
    /// </summary>
    [DefaultEqualityComparer(typeof(UniqueId64.Comparer)), DefaultSorter(typeof(UniqueId64.Comparer))]
    public struct UniqueId64 : IEquatable<UniqueId64>, IComparable<UniqueId64>
    {
        private const int IndexBits = 54;
        public const ulong MaxIndex = (1UL << IndexBits);
        internal const ulong IndexMask = MaxIndex - 1;
        private const int VersionBits = 10;
        internal const int VersionMask = (1 << VersionBits) - 1;

        /// <summary>
        /// Unique identifier.
        /// </summary>
        public readonly ulong Id;

        /// <summary>
        /// Index portion of the identifier.
        /// </summary>
        public long Index
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return (long) (Id & IndexMask); }
        }

        /// <summary>
        /// Version portion of the identifier.
        /// </summary>
        public int Version
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return (int) ((Id >> IndexBits) & VersionMask); }
        }

        /// <summary>
        /// Invalid identifier.
        /// </summary>
        static public readonly UniqueId64 Invalid = default(UniqueId64);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UniqueId64(ulong inId)
        {
            Id = inId;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UniqueId64(ulong inIndex, uint inVersion)
        {
            Id = (inIndex & IndexMask) | ((inVersion & VersionMask) << IndexBits);
        }

        #region Interfaces

        public bool Equals(UniqueId64 other)
        {
            return Id == other.Id;
        }

        public int CompareTo(UniqueId64 other)
        {
            return Id.CompareTo(other.Id);
        }

        #endregion // Interfaces

        #region Overrides

        public override bool Equals(object obj)
        {
            if (obj is UniqueId64)
                return Equals((UniqueId64) obj);
            return false;
        }

        public override int GetHashCode()
        {
            return unchecked((int) ((Id >> 32) ^ (Id & 0xFFFF)));
        }

        public override string ToString()
        {
            if (Id == 0)
                return "[Invalid]";
            return string.Format("[i{0}-v{1}]", Index, Version);
        }

        #endregion // Overrides

        #region Operators

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public bool operator ==(UniqueId64 inA, UniqueId64 inB)
        {
            return inA.Id == inB.Id;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public bool operator !=(UniqueId64 inA, UniqueId64 inB)
        {
            return inA.Id != inB.Id;
        }

        static public bool operator <(UniqueId64 inA, UniqueId64 inB)
        {
            return inA.Id < inB.Id;
        }

        static public bool operator <=(UniqueId64 inA, UniqueId64 inB)
        {
            return inA.Id <= inB.Id;
        }

        static public bool operator >(UniqueId64 inA, UniqueId64 inB)
        {
            return inA.Id > inB.Id;
        }

        static public bool operator >=(UniqueId64 inA, UniqueId64 inB)
        {
            return inA.Id >= inB.Id;
        }

        #endregion // Operators

        #region Comparisons

        /// <summary>
        /// Default comparer.
        /// </summary>
        private sealed class Comparer : IEqualityComparer<UniqueId64>, IComparer<UniqueId64>
        {
            public int Compare(UniqueId64 x, UniqueId64 y)
            {
                return x.Id.CompareTo(y.Id);
            }

            public bool Equals(UniqueId64 x, UniqueId64 y)
            {
                return x.Id == y.Id;
            }

            public int GetHashCode(UniqueId64 obj)
            {
                return unchecked((int) ((obj.Id >> 32) ^ (obj.Id & 0xFFFF)));
            }
        }

        #endregion // Comparisons
    }
}