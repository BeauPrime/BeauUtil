/*
 * Copyright (C) 2023. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    26 June 2023
 * 
 * File:    OffsetLength.cs
 * Purpose: Simple pair of offset and length.
 */

using System;
using System.Runtime.CompilerServices;

namespace BeauUtil
{
    /// <summary>
    /// Represents a range in an array, with a 32-bit offset and length.
    /// </summary>
    public struct OffsetLength32 : IEquatable<OffsetLength32>, IComparable<OffsetLength32>
    {
        public int Offset;
        public int Length;

        public int End
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return Offset + Length; }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set { Length = value - Offset; }
        }

        public OffsetLength32(int inOffset, int inLength)
        {
            Offset = inOffset;
            Length = inLength;
        }

        #region Checks

        /// <summary>
        /// Returns if this range overlaps with the given range.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Overlaps(OffsetLength32 inOther)
        {
            return Offset < inOther.End && inOther.Offset < End;
        }

        /// <summary>
        /// Returns if this range contains the other range.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(OffsetLength32 inOther)
        {
            return inOther.Offset >= Offset && inOther.End <= End;
        }

        #endregion // Checks

        #region Interfaces

        public bool Equals(OffsetLength32 other)
        {
            return Offset == other.Offset && Length == other.Length;
        }

        public int CompareTo(OffsetLength32 other)
        {
            return Offset.CompareTo(other.Offset);
        }

        #endregion // Interfaces

        #region Operators

        static public bool operator== (OffsetLength32 inA, OffsetLength32 inB)
        {
            return inA.Offset == inB.Offset && inA.Length == inB.Length;
        }

        static public bool operator !=(OffsetLength32 inA, OffsetLength32 inB)
        {
            return inA.Offset != inB.Offset || inA.Length != inB.Length;
        }

        static public implicit operator OffsetLength32(OffsetLength16 inOther)
        {
            return new OffsetLength32(inOther.Offset, inOther.Length);
        }

        static public implicit operator OffsetLength32(OffsetLengthU16 inOther)
        {
            return new OffsetLength32(inOther.Offset, inOther.Length);
        }

        static public explicit operator OffsetLength32(OffsetLengthU32 inOther)
        {
            return new OffsetLength32((int) inOther.Offset, (int) inOther.Length);
        }

        #endregion // Operators

        #region Overrides

        public override bool Equals(object obj)
        {
            if (obj is OffsetLength32)
                return Equals((OffsetLength32) obj);
            return false;
        }

        public override int GetHashCode()
        {
            return Offset.GetHashCode() << 7 ^ Length.GetHashCode();
        }

        public override string ToString()
        {
            return string.Format("[{0}({1})]", Offset.ToStringLookup(), Length.ToStringLookup());
        }

        #endregion // Overrides
    }

    /// <summary>
    /// Represents a range in an array, with an unsigned 32-bit offset and length.
    /// </summary>
    public struct OffsetLengthU32 : IEquatable<OffsetLengthU32>, IComparable<OffsetLengthU32>
    {
        public uint Offset;
        public uint Length;

        public uint End
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return Offset + Length; }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set { Length = value - Offset; }
        }

        public OffsetLengthU32(uint inOffset, uint inLength)
        {
            Offset = inOffset;
            Length = inLength;
        }

        #region Checks

        /// <summary>
        /// Returns if this range overlaps with the given range.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Overlaps(OffsetLengthU32 inOther)
        {
            return Offset < inOther.End && inOther.Offset < End;
        }

        /// <summary>
        /// Returns if this range contains the other range.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(OffsetLengthU32 inOther)
        {
            return inOther.Offset >= Offset && inOther.End <= End;
        }

        #endregion // Checks

        #region Interfaces

        public bool Equals(OffsetLengthU32 other)
        {
            return Offset == other.Offset && Length == other.Length;
        }

        public int CompareTo(OffsetLengthU32 other)
        {
            return Offset.CompareTo(other.Offset);
        }

        #endregion // Interfaces

        #region Operators

        static public bool operator ==(OffsetLengthU32 inA, OffsetLengthU32 inB)
        {
            return inA.Offset == inB.Offset && inA.Length == inB.Length;
        }

        static public bool operator !=(OffsetLengthU32 inA, OffsetLengthU32 inB)
        {
            return inA.Offset != inB.Offset || inA.Length != inB.Length;
        }

        static public explicit operator OffsetLengthU32(OffsetLength16 inOther)
        {
            return new OffsetLengthU32((uint) inOther.Offset, (uint) inOther.Length);
        }

        static public implicit operator OffsetLengthU32(OffsetLengthU16 inOther)
        {
            return new OffsetLengthU32(inOther.Offset, inOther.Length);
        }

        static public explicit operator OffsetLengthU32(OffsetLength32 inOther)
        {
            return new OffsetLengthU32((uint) inOther.Offset, (uint) inOther.Length);
        }

        #endregion // Operators

        #region Overrides

        public override bool Equals(object obj)
        {
            if (obj is OffsetLengthU32)
                return Equals((OffsetLengthU32) obj);
            return false;
        }

        public override int GetHashCode()
        {
            return Offset.GetHashCode() << 7 ^ Length.GetHashCode();
        }

        public override string ToString()
        {
            return string.Format("[{0}({1})]", Offset.ToStringLookup(), Length.ToStringLookup());
        }

        #endregion // Overrides
    }

    /// <summary>
    /// Represents a range in an array, with a 16-bit offset and length.
    /// </summary>
    public struct OffsetLength16 : IEquatable<OffsetLength16>, IComparable<OffsetLength16>
    {
        public short Offset;
        public short Length;

        public short End
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return (short) (Offset + Length); }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set { Length = (short) (value - Offset); }
        }

        public OffsetLength16(short inOffset, short inLength)
        {
            Offset = inOffset;
            Length = inLength;
        }

        #region Checks

        /// <summary>
        /// Returns if this range overlaps with the given range.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Overlaps(OffsetLength16 inOther)
        {
            return Offset < inOther.End && inOther.Offset < End;
        }

        /// <summary>
        /// Returns if this range contains the other range.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(OffsetLength16 inOther)
        {
            return inOther.Offset >= Offset && inOther.End <= End;
        }

        #endregion // Checks

        #region Interfaces

        public bool Equals(OffsetLength16 other)
        {
            return Offset == other.Offset && Length == other.Length;
        }

        public int CompareTo(OffsetLength16 other)
        {
            return Offset.CompareTo(other.Offset);
        }

        #endregion // Interfaces

        #region Operators

        static public bool operator ==(OffsetLength16 inA, OffsetLength16 inB)
        {
            return inA.Offset == inB.Offset && inA.Length == inB.Length;
        }

        static public bool operator !=(OffsetLength16 inA, OffsetLength16 inB)
        {
            return inA.Offset != inB.Offset || inA.Length != inB.Length;
        }

        static public explicit operator OffsetLength16(OffsetLengthU16 inOther)
        {
            return new OffsetLength16((short) inOther.Offset, (short) inOther.Length);
        }

        static public explicit operator OffsetLength16(OffsetLength32 inOther)
        {
            return new OffsetLength16((short) inOther.Offset, (short) inOther.Length);
        }

        static public explicit operator OffsetLength16(OffsetLengthU32 inOther)
        {
            return new OffsetLength16((short) inOther.Offset, (short) inOther.Length);
        }

        #endregion // Operators

        #region Overrides

        public override bool Equals(object obj)
        {
            if (obj is OffsetLength16)
                return Equals((OffsetLength16) obj);
            return false;
        }

        public override int GetHashCode()
        {
            return Offset.GetHashCode() << 7 ^ Length.GetHashCode();
        }

        public override string ToString()
        {
            return string.Format("[{0}({1})]", ((int) Offset).ToStringLookup(), ((int) Length).ToStringLookup());
        }

        #endregion // Overrides
    }

    /// <summary>
    /// Represents a range in an array, with an unsigned 16-bit offset and length.
    /// </summary>
    public struct OffsetLengthU16 : IEquatable<OffsetLengthU16>, IComparable<OffsetLengthU16>
    {
        public ushort Offset;
        public ushort Length;

        public ushort End
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return (ushort) (Offset + Length); }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set { Length = (ushort) (value - Offset); }
        }

        public OffsetLengthU16(ushort inOffset, ushort inLength)
        {
            Offset = inOffset;
            Length = inLength;
        }

        #region Checks

        /// <summary>
        /// Returns if this range overlaps with the given range.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Overlaps(OffsetLengthU16 inOther)
        {
            return Offset < inOther.End && inOther.Offset < End;
        }

        /// <summary>
        /// Returns if this range contains the other range.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(OffsetLengthU16 inOther)
        {
            return inOther.Offset >= Offset && inOther.End <= End;
        }

        #endregion // Checks

        #region Interfaces

        public bool Equals(OffsetLengthU16 other)
        {
            return Offset == other.Offset && Length == other.Length;
        }

        public int CompareTo(OffsetLengthU16 other)
        {
            return Offset.CompareTo(other.Offset);
        }

        #endregion // Interfaces

        #region Operators

        static public bool operator ==(OffsetLengthU16 inA, OffsetLengthU16 inB)
        {
            return inA.Offset == inB.Offset && inA.Length == inB.Length;
        }

        static public bool operator !=(OffsetLengthU16 inA, OffsetLengthU16 inB)
        {
            return inA.Offset != inB.Offset || inA.Length != inB.Length;
        }

        static public explicit operator OffsetLengthU16(OffsetLength16 inOther)
        {
            return new OffsetLengthU16((ushort) inOther.Offset, (ushort) inOther.Length);
        }

        static public explicit operator OffsetLengthU16(OffsetLength32 inOther)
        {
            return new OffsetLengthU16((ushort) inOther.Offset, (ushort) inOther.Length);
        }

        static public explicit operator OffsetLengthU16(OffsetLengthU32 inOther)
        {
            return new OffsetLengthU16((ushort) inOther.Offset, (ushort) inOther.Length);
        }

        #endregion // Operators

        #region Overrides

        public override bool Equals(object obj)
        {
            if (obj is OffsetLengthU16)
                return Equals((OffsetLengthU16) obj);
            return false;
        }

        public override int GetHashCode()
        {
            return Offset.GetHashCode() << 7 ^ Length.GetHashCode();
        }

        public override string ToString()
        {
            return string.Format("[{0}({1})]", ((int) Offset).ToStringLookup(), ((int) Length).ToStringLookup());
        }

        #endregion // Overrides
    }

    /// <summary>
    /// OffsetLength struct utility methods.
    /// </summary>
    static public class OffsetLength
    {
        #region Merging

        /// <summary>
        /// Attempts to merge the two offset lengths into one.
        /// </summary
        static public bool TryMerge(ref OffsetLength32 ioDest, OffsetLength32 inSource)
        {
            if (ioDest.Overlaps(inSource))
            {
                int min = Math.Min(ioDest.Offset, inSource.Offset);
                int max = Math.Max(ioDest.End, inSource.End);
                ioDest.Offset = min;
                ioDest.Length = max - min;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Merges the two offset lengths into one.
        /// </summary
        static public void Merge(ref OffsetLength32 ioDest, OffsetLength32 inSource)
        {
            int min = Math.Min(ioDest.Offset, inSource.Offset);
            int max = Math.Max(ioDest.End, inSource.End);
            ioDest.Offset = min;
            ioDest.Length = max - min;
        }

        /// <summary>
        /// Merges the two offset lengths into one.
        /// </summary
        static public OffsetLength32 Merge(OffsetLength32 inA, OffsetLength32 inB)
        {
            int min = Math.Min(inA.Offset, inB.Offset);
            int max = Math.Max(inA.End, inB.End);
            return new OffsetLength32(min, max - min);
        }

        /// <summary>
        /// Attempts to merge the two offset lengths into one.
        /// </summary
        static public bool TryMerge(ref OffsetLengthU32 ioDest, OffsetLengthU32 inSource)
        {
            if (ioDest.Overlaps(inSource))
            {
                uint min = Math.Min(ioDest.Offset, inSource.Offset);
                uint max = Math.Max(ioDest.End, inSource.End);
                ioDest.Offset = min;
                ioDest.Length = max - min;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Merges the two offset lengths into one.
        /// </summary
        static public void Merge(ref OffsetLengthU32 ioDest, OffsetLengthU32 inSource)
        {
            uint min = Math.Min(ioDest.Offset, inSource.Offset);
            uint max = Math.Max(ioDest.End, inSource.End);
            ioDest.Offset = min;
            ioDest.Length = max - min;
        }

        /// <summary>
        /// Merges the two offset lengths into one.
        /// </summary
        static public OffsetLengthU32 Merge(OffsetLengthU32 inA, OffsetLengthU32 inB)
        {
            uint min = Math.Min(inA.Offset, inB.Offset);
            uint max = Math.Max(inA.End, inB.End);
            return new OffsetLengthU32(min, max - min);
        }

        /// <summary>
        /// Attempts to merge the two offset lengths into one.
        /// </summary
        static public bool TryMerge(ref OffsetLengthU16 ioDest, OffsetLengthU16 inSource)
        {
            if (ioDest.Overlaps(inSource))
            {
                ushort min = Math.Min(ioDest.Offset, inSource.Offset);
                ushort max = Math.Max(ioDest.End, inSource.End);
                ioDest.Offset = min;
                ioDest.Length = (ushort) (max - min);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Merges the two offset lengths into one.
        /// </summary
        static public void Merge(ref OffsetLengthU16 ioDest, OffsetLengthU16 inSource)
        {
            ushort min = Math.Min(ioDest.Offset, inSource.Offset);
            ushort max = Math.Max(ioDest.End, inSource.End);
            ioDest.Offset = min;
            ioDest.Length = (ushort) (max - min);
        }

        /// <summary>
        /// Merges the two offset lengths into one.
        /// </summary
        static public OffsetLengthU16 Merge(OffsetLengthU16 inA, OffsetLengthU16 inB)
        {
            ushort min = Math.Min(inA.Offset, inB.Offset);
            ushort max = Math.Max(inA.End, inB.End);
            return new OffsetLengthU16(min, (ushort) (max - min));
        }

        /// <summary>
        /// Attempts to merge the two offset lengths into one.
        /// </summary
        static public bool TryMerge(ref OffsetLength16 ioDest, OffsetLength16 inSource)
        {
            if (ioDest.Overlaps(inSource))
            {
                short min = Math.Min(ioDest.Offset, inSource.Offset);
                short max = Math.Max(ioDest.End, inSource.End);
                ioDest.Offset = min;
                ioDest.Length = (short) (max - min);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Merges the two offset lengths into one.
        /// </summary
        static public void Merge(ref OffsetLength16 ioDest, OffsetLength16 inSource)
        {
            short min = Math.Min(ioDest.Offset, inSource.Offset);
            short max = Math.Max(ioDest.End, inSource.End);
            ioDest.Offset = min;
            ioDest.Length = (short) (max - min);
        }

        /// <summary>
        /// Merges the two offset lengths into one.
        /// </summary
        static public OffsetLength16 Merge(OffsetLength16 inA, OffsetLength16 inB)
        {
            short min = Math.Min(inA.Offset, inB.Offset);
            short max = Math.Max(inA.End, inB.End);
            return new OffsetLength16(min, (short) (max - min));
        }

        #endregion // Merging

        #region Collapsing

        /// <summary>
        /// Merges a subrange of pre-sorted array of intervals.
        /// Returns the new length of subrange.
        /// </summary>
        static public int MergePreSorted(OffsetLength32[] inArray, int inArrayOffset, int inArrayLength)
        {
            if (inArrayLength <= 1)
            {
                return inArrayLength;
            }

            int modificationIndex = inArrayOffset;
            for (int i = inArrayOffset + 1, end = inArrayOffset + inArrayLength; i < end; i++)
            {
                if (inArray[modificationIndex].End >= inArray[i].Offset)
                {
                    Merge(ref inArray[modificationIndex], inArray[i]);
                }
                else
                {
                    inArray[++modificationIndex] = inArray[i];
                }
            }

            return modificationIndex - inArrayOffset + 1;
        }

        /// <summary>
        /// Merges a subrange of pre-sorted array of intervals.
        /// Returns the new length of subrange.
        /// </summary>
        static public int MergePreSorted(RingBuffer<OffsetLength32> inBuffer, int inBufferOffset, int inBufferLength)
        {
            if (inBufferLength <= 1)
            {
                return inBufferLength;
            }

            int modificationIndex = inBufferOffset;
            for (int i = inBufferOffset + 1, end = inBufferOffset + inBufferLength; i < end; i++)
            {
                if (inBuffer[modificationIndex].End >= inBuffer[i].Offset)
                {
                    Merge(ref inBuffer[modificationIndex], inBuffer[i]);
                }
                else
                {
                    inBuffer[++modificationIndex] = inBuffer[i];
                }
            }

            return modificationIndex - inBufferOffset + 1;
        }

        /// <summary>
        /// Merges a subrange of pre-sorted array of intervals.
        /// Returns the new length of subrange.
        /// </summary>
        static public unsafe int MergePreSorted(OffsetLength32* inBuffer, int inBufferOffset, int inBufferLength)
        {
            if (inBufferLength <= 1)
            {
                return inBufferLength;
            }

            int modificationIndex = inBufferOffset;
            for (int i = inBufferOffset + 1, end = inBufferOffset + inBufferLength; i < end; i++)
            {
                if (inBuffer[modificationIndex].End >= inBuffer[i].Offset)
                {
                    Merge(ref inBuffer[modificationIndex], inBuffer[i]);
                }
                else
                {
                    inBuffer[++modificationIndex] = inBuffer[i];
                }
            }

            return modificationIndex - inBufferOffset + 1;
        }

        /// <summary>
        /// Merges a subrange of pre-sorted array of intervals.
        /// Returns the new length of subrange.
        /// </summary>
        static public int MergePreSorted(OffsetLengthU32[] inArray, int inArrayOffset, int inArrayLength)
        {
            if (inArrayLength <= 1)
            {
                return inArrayLength;
            }

            int modificationIndex = inArrayOffset;
            for (int i = inArrayOffset + 1, end = inArrayOffset + inArrayLength; i < end; i++)
            {
                if (inArray[modificationIndex].End >= inArray[i].Offset)
                {
                    Merge(ref inArray[modificationIndex], inArray[i]);
                }
                else
                {
                    inArray[++modificationIndex] = inArray[i];
                }
            }

            return modificationIndex - inArrayOffset + 1;
        }

        /// <summary>
        /// Merges a subrange of pre-sorted array of intervals.
        /// Returns the new length of subrange.
        /// </summary>
        static public int MergePreSorted(RingBuffer<OffsetLengthU32> inBuffer, int inBufferOffset, int inBufferLength)
        {
            if (inBufferLength <= 1)
            {
                return inBufferLength;
            }

            int modificationIndex = inBufferOffset;
            for (int i = inBufferOffset + 1, end = inBufferOffset + inBufferLength; i < end; i++)
            {
                if (inBuffer[modificationIndex].End >= inBuffer[i].Offset)
                {
                    Merge(ref inBuffer[modificationIndex], inBuffer[i]);
                }
                else
                {
                    inBuffer[++modificationIndex] = inBuffer[i];
                }
            }

            return modificationIndex - inBufferOffset + 1;
        }

        /// <summary>
        /// Merges a subrange of pre-sorted array of intervals.
        /// Returns the new length of subrange.
        /// </summary>
        static public unsafe int MergePreSorted(OffsetLengthU32* inBuffer, int inBufferOffset, int inBufferLength)
        {
            if (inBufferLength <= 1)
            {
                return inBufferLength;
            }

            int modificationIndex = inBufferOffset;
            for (int i = inBufferOffset + 1, end = inBufferOffset + inBufferLength; i < end; i++)
            {
                if (inBuffer[modificationIndex].End >= inBuffer[i].Offset)
                {
                    Merge(ref inBuffer[modificationIndex], inBuffer[i]);
                }
                else
                {
                    inBuffer[++modificationIndex] = inBuffer[i];
                }
            }

            return modificationIndex - inBufferOffset + 1;
        }

        /// <summary>
        /// Merges a subrange of pre-sorted array of intervals.
        /// Returns the new length of subrange.
        /// </summary>
        static public int MergePreSorted(OffsetLength16[] inArray, int inArrayOffset, int inArrayLength)
        {
            if (inArrayLength <= 1)
            {
                return inArrayLength;
            }

            int modificationIndex = inArrayOffset;
            for (int i = inArrayOffset + 1, end = inArrayOffset + inArrayLength; i < end; i++)
            {
                if (inArray[modificationIndex].End >= inArray[i].Offset)
                {
                    Merge(ref inArray[modificationIndex], inArray[i]);
                }
                else
                {
                    inArray[++modificationIndex] = inArray[i];
                }
            }

            return modificationIndex - inArrayOffset + 1;
        }

        /// <summary>
        /// Merges a subrange of pre-sorted array of intervals.
        /// Returns the new length of subrange.
        /// </summary>
        static public int MergePreSorted(RingBuffer<OffsetLength16> inBuffer, int inBufferOffset, int inBufferLength)
        {
            if (inBufferLength <= 1)
            {
                return inBufferLength;
            }

            int modificationIndex = inBufferOffset;
            for (int i = inBufferOffset + 1, end = inBufferOffset + inBufferLength; i < end; i++)
            {
                if (inBuffer[modificationIndex].End >= inBuffer[i].Offset)
                {
                    Merge(ref inBuffer[modificationIndex], inBuffer[i]);
                }
                else
                {
                    inBuffer[++modificationIndex] = inBuffer[i];
                }
            }

            return modificationIndex - inBufferOffset + 1;
        }

        /// <summary>
        /// Merges a subrange of pre-sorted array of intervals.
        /// Returns the new length of subrange.
        /// </summary>
        static public unsafe int MergePreSorted(OffsetLength16* inBuffer, int inBufferOffset, int inBufferLength)
        {
            if (inBufferLength <= 1)
            {
                return inBufferLength;
            }

            int modificationIndex = inBufferOffset;
            for (int i = inBufferOffset + 1, end = inBufferOffset + inBufferLength; i < end; i++)
            {
                if (inBuffer[modificationIndex].End >= inBuffer[i].Offset)
                {
                    Merge(ref inBuffer[modificationIndex], inBuffer[i]);
                }
                else
                {
                    inBuffer[++modificationIndex] = inBuffer[i];
                }
            }

            return modificationIndex - inBufferOffset + 1;
        }

        /// <summary>
        /// Merges a subrange of pre-sorted array of intervals.
        /// Returns the new length of subrange.
        /// </summary>
        static public int MergePreSorted(OffsetLengthU16[] inArray, int inArrayOffset, int inArrayLength)
        {
            if (inArrayLength <= 1)
            {
                return inArrayLength;
            }

            int modificationIndex = inArrayOffset;
            for (int i = inArrayOffset + 1, end = inArrayOffset + inArrayLength; i < end; i++)
            {
                if (inArray[modificationIndex].End >= inArray[i].Offset)
                {
                    Merge(ref inArray[modificationIndex], inArray[i]);
                }
                else
                {
                    inArray[++modificationIndex] = inArray[i];
                }
            }

            return modificationIndex - inArrayOffset + 1;
        }

        /// <summary>
        /// Merges a subrange of pre-sorted array of intervals.
        /// Returns the new length of subrange.
        /// </summary>
        static public int MergePreSorted(RingBuffer<OffsetLengthU16> inBuffer, int inBufferOffset, int inBufferLength)
        {
            if (inBufferLength <= 1)
            {
                return inBufferLength;
            }

            int modificationIndex = inBufferOffset;
            for (int i = inBufferOffset + 1, end = inBufferOffset + inBufferLength; i < end; i++)
            {
                if (inBuffer[modificationIndex].End >= inBuffer[i].Offset)
                {
                    Merge(ref inBuffer[modificationIndex], inBuffer[i]);
                }
                else
                {
                    inBuffer[++modificationIndex] = inBuffer[i];
                }
            }

            return modificationIndex - inBufferOffset + 1;
        }

        /// <summary>
        /// Merges a subrange of pre-sorted array of intervals.
        /// Returns the new length of subrange.
        /// </summary>
        static public unsafe int MergePreSorted(OffsetLengthU16* inBuffer, int inBufferOffset, int inBufferLength)
        {
            if (inBufferLength <= 1)
            {
                return inBufferLength;
            }

            int modificationIndex = inBufferOffset;
            for (int i = inBufferOffset + 1, end = inBufferOffset + inBufferLength; i < end; i++)
            {
                if (inBuffer[modificationIndex].End >= inBuffer[i].Offset)
                {
                    Merge(ref inBuffer[modificationIndex], inBuffer[i]);
                }
                else
                {
                    inBuffer[++modificationIndex] = inBuffer[i];
                }
            }

            return modificationIndex - inBufferOffset + 1;
        }

        #endregion // Collapsing
    }
}