/*
 * Copyright (C) 2023. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    22 June 2023
 * 
 * File:    BitSet.cs
 * Purpose: Various bit sets.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using BeauUtil.Debugger;
using UnityEngine;

namespace BeauUtil
{
    /// <summary>
    /// Interface for a bit set.
    /// </summary>
    public interface IBitSet
    {
        int Capacity { get; }
        int Count { get; }
        bool IsEmpty { get; }

        bool IsSet(int inIndex);
        void Set(int inIndex);
        void Set(int inIndex, bool inValue);
        void Unset(int inIndex);
        void Clear();

        bool this[int inIndex] { get; set; }
    }

    /// <summary>
    /// 32-bit mask.
    /// </summary>
    [Serializable]
    public struct BitSet32 : IBitSet, IEquatable<BitSet32>, IEnumerable<int>
    {
        [SerializeField] private unsafe uint m_Bits;

        public BitSet32(uint inData)
        {
            m_Bits = inData;
        }

        public int Capacity { get { return 32; } }

        /// <summary>
        /// Number of set bits.
        /// </summary>
        public int Count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return Bits.Count(m_Bits); }
        }

        /// <summary>
        /// If the set is empty.
        /// </summary>
        public bool IsEmpty {
            get { return m_Bits == 0; }
        }

        /// <summary>
        /// Gets/sets the value of the bit at the given index.
        /// </summary>
        public bool this[int inIndex]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return IsSet(inIndex); }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set { Set(inIndex, value); }
        }

        /// <summary>
        /// Returns if the bit at the given index is set.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsSet(int inIndex)
        {
            Assert.True(inIndex >= 0 && inIndex < Capacity, "Bit index {0} is outside range 0-{1}", inIndex, Capacity - 1);
            return Bits.Contains(m_Bits, inIndex);
        }

        /// <summary>
        /// Sets the bit at the given index.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(int inIndex)
        {
            Assert.True(inIndex >= 0 && inIndex < Capacity, "Bit index {0} is outside range 0-{1}", inIndex, Capacity - 1);
            Bits.Add(ref m_Bits, inIndex);
        }

        /// <summary>
        /// Unsets the bit at the given index.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Unset(int inIndex)
        {
            Assert.True(inIndex >= 0 && inIndex < Capacity, "Bit index {0} is outside range 0-{1}", inIndex, Capacity - 1);
            Bits.Remove(ref m_Bits, inIndex);
        }

        /// <summary>
        /// Sets the bit at the given index to the given value.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(int inIndex, bool inValue)
        {
            if (inValue)
            {
                Set(inIndex);
            }
            else
            {
                Unset(inIndex);
            }
        }

        /// <summary>
        /// Clears all set bits.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            m_Bits = 0;
        }

        /// <summary>
        /// Unpacks the bit data.
        /// </summary>
        public void Unpack(out uint outBits)
        {
            outBits = m_Bits;
        }

        #region Enumerable

        public BitEnumerator32 GetEnumerator()
        {
            return new BitEnumerator32(m_Bits);
        }

        IEnumerator<int> IEnumerable<int>.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion // Enumerable

        #region Operators

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(BitSet32 other)
        {
            return m_Bits == other.m_Bits;
        }

        public override bool Equals(object obj)
        {
            if (obj is BitSet32)
                return Equals((BitSet32) obj);
            return false;
        }

        public override int GetHashCode()
        {
            return unchecked((int) m_Bits);
        }

        public override string ToString()
        {
            unsafe
            {
                fixed(uint* bits = &m_Bits)
                {
                    return Unsafe.DumpMemory(bits, 4, ' ', 4);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public BitSet32 operator |(in BitSet32 inA, in BitSet32 inB)
        {
            return new BitSet32(inA.m_Bits | inB.m_Bits);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public BitSet32 operator &(in BitSet32 inA, in BitSet32 inB)
        {
            return new BitSet32(inA.m_Bits & inB.m_Bits);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public BitSet32 operator ^(in BitSet32 inA, in BitSet32 inB)
        {
            return new BitSet32(inA.m_Bits ^ inB.m_Bits);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public BitSet32 operator ~(in BitSet32 inA)
        {
            return new BitSet32(~inA.m_Bits);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public bool operator ==(in BitSet32 inA, in BitSet32 inB)
        {
            return inA.m_Bits == inB.m_Bits;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public bool operator !=(in BitSet32 inA, in BitSet32 inB)
        {
            return inA.m_Bits != inB.m_Bits;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public implicit operator bool(in BitSet32 inA)
        {
            return inA.m_Bits != 0;
        }

        #endregion // Operators
    }

    /// <summary>
    /// 64-bit mask.
    /// </summary>
    [Serializable]
    public struct BitSet64 : IBitSet, IEquatable<BitSet64>, IEnumerable<int>
    {
        [SerializeField] private unsafe ulong m_Bits;

        public BitSet64(ulong inData)
        {
            m_Bits = inData;
        }

        public int Capacity { get { return 64; } }

        /// <summary>
        /// Number of set bits.
        /// </summary>
        public int Count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return Bits.Count(m_Bits); }
        }

        /// <summary>
        /// If the set is empty.
        /// </summary>
        public bool IsEmpty {
            get { return m_Bits == 0; }
        }

        /// <summary>
        /// Gets/sets the value of the bit at the given index.
        /// </summary>
        public bool this[int inIndex]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return IsSet(inIndex); }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set { Set(inIndex, value); }
        }

        /// <summary>
        /// Returns if the bit at the given index is set.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsSet(int inIndex)
        {
            Assert.True(inIndex >= 0 && inIndex < Capacity, "Bit index {0} is outside range 0-{1}", inIndex, Capacity - 1);
            return Bits.Contains(m_Bits, inIndex);
        }

        /// <summary>
        /// Sets the bit at the given index.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(int inIndex)
        {
            Assert.True(inIndex >= 0 && inIndex < Capacity, "Bit index {0} is outside range 0-{1}", inIndex, Capacity - 1);
            Bits.Add(ref m_Bits, inIndex);
        }

        /// <summary>
        /// Unsets the bit at the given index.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Unset(int inIndex)
        {
            Assert.True(inIndex >= 0 && inIndex < Capacity, "Bit index {0} is outside range 0-{1}", inIndex, Capacity - 1);
            Bits.Remove(ref m_Bits, inIndex);
        }

        /// <summary>
        /// Sets the bit at the given index to the given value.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(int inIndex, bool inValue)
        {
            Assert.True(inIndex >= 0 && inIndex < Capacity, "Bit index {0} is outside range 0-{1}", inIndex, Capacity - 1);
            Bits.Set(ref m_Bits, inIndex, inValue);
        }

        /// <summary>
        /// Clears all set bits.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            m_Bits = 0;
        }

        /// <summary>
        /// Unpacks the bit data.
        /// </summary>
        public void Unpack(out ulong outBits)
        {
            outBits = m_Bits;
        }

        #region Enumerable

        public BitEnumerator64 GetEnumerator()
        {
            return new BitEnumerator64(m_Bits);
        }

        IEnumerator<int> IEnumerable<int>.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion // Enumerable

        #region Operators

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(BitSet64 other)
        {
            return m_Bits == other.m_Bits;
        }

        public override bool Equals(object obj)
        {
            if (obj is BitSet64)
                return Equals((BitSet64) obj);
            return false;
        }

        public override int GetHashCode()
        {
            return unchecked((int) m_Bits ^ (int) m_Bits >> 32);
        }

        public override string ToString()
        {
            unsafe
            {
                fixed (ulong* bits = &m_Bits)
                {
                    return Unsafe.DumpMemory(bits, 8, ' ', 4);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public BitSet64 operator |(in BitSet64 inA, in BitSet64 inB)
        {
            return new BitSet64(inA.m_Bits | inB.m_Bits);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public BitSet64 operator &(in BitSet64 inA, in BitSet64 inB)
        {
            return new BitSet64(inA.m_Bits & inB.m_Bits);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public BitSet64 operator ^(in BitSet64 inA, in BitSet64 inB)
        {
            return new BitSet64(inA.m_Bits ^ inB.m_Bits);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public BitSet64 operator ~(in BitSet64 inA)
        {
            return new BitSet64(~inA.m_Bits);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public bool operator ==(in BitSet64 inA, in BitSet64 inB)
        {
            return inA.m_Bits == inB.m_Bits;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public bool operator !=(in BitSet64 inA, in BitSet64 inB)
        {
            return inA.m_Bits != inB.m_Bits;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public implicit operator bool(in BitSet64 inA)
        {
            return inA.m_Bits != 0;
        }

        #endregion // Operators
    }

    /// <summary>
    /// 256-bit mask.
    /// </summary>
    [Serializable]
    public struct BitSet128 : IBitSet, IEquatable<BitSet128>
    {
        [SerializeField] private unsafe ulong m_Bits0;
        [SerializeField] private unsafe ulong m_Bits1;

        public BitSet128(ulong inData0, ulong inData1)
        {
            m_Bits0 = inData0;
            m_Bits1 = inData1;
        }

        public int Capacity { get { return 128; } }

        /// <summary>
        /// Number of set bits.
        /// </summary>
        public int Count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return Bits.Count(m_Bits0) + Bits.Count(m_Bits1); }
        }

        /// <summary>
        /// If the set is empty.
        /// </summary>
        public bool IsEmpty {
            get { return m_Bits0 == 0 && m_Bits1 == 0; }
        }

        /// <summary>
        /// Gets/sets the value of the bit at the given index.
        /// </summary>
        public bool this[int inIndex]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return IsSet(inIndex); }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set { Set(inIndex, value); }
        }

        /// <summary>
        /// Returns if the bit at the given index is set.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsSet(int inIndex)
        {
            Assert.True(inIndex >= 0 && inIndex < Capacity, "Bit index {0} is outside range 0-{1}", inIndex, Capacity - 1);
            unsafe
            {
                fixed(ulong* bits = &m_Bits0)
                {
                    return Bits.Contains(bits[inIndex >> 6], inIndex & 0x3F);
                }
            }
        }

        /// <summary>
        /// Sets the bit at the given index.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(int inIndex)
        {
            Assert.True(inIndex >= 0 && inIndex < Capacity, "Bit index {0} is outside range 0-{1}", inIndex, Capacity - 1);
            unsafe
            {
                fixed (ulong* bits = &m_Bits0)
                {
                    Bits.Add(ref bits[inIndex >> 6], inIndex & 0x3F);
                }
            }
        }

        /// <summary>
        /// Unsets the bit at the given index.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Unset(int inIndex)
        {
            Assert.True(inIndex >= 0 && inIndex < Capacity, "Bit index {0} is outside range 0-{1}", inIndex, Capacity - 1);
            unsafe
            {
                fixed (ulong* bits = &m_Bits0)
                {
                    Bits.Remove(ref bits[inIndex >> 6], inIndex & 0x3F);
                }
            }
        }

        /// <summary>
        /// Sets the bit at the given index to the given value.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(int inIndex, bool inValue)
        {
            if (inValue)
            {
                Set(inIndex);
            }
            else
            {
                Unset(inIndex);
            }
        }

        /// <summary>
        /// Clears all set bits.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            m_Bits0 = m_Bits1 = 0;
        }

        /// <summary>
        /// Unpacks the bit data.
        /// </summary>
        public void Unpack(out ulong outBits0, out ulong outBits1)
        {
            outBits0 = m_Bits0;
            outBits1 = m_Bits1;
        }

        #region Operators

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(BitSet128 other)
        {
            return m_Bits0 == other.m_Bits0 && m_Bits1 == other.m_Bits1;
        }

        public override bool Equals(object obj)
        {
            if (obj is BitSet128)
                return Equals((BitSet128) obj);
            return false;
        }

        public override int GetHashCode()
        {
            return m_Bits0.GetHashCode() ^ m_Bits1.GetHashCode();
        }

        public override string ToString()
        {
            unsafe
            {
                fixed (ulong* bits = &m_Bits0)
                {
                    return Unsafe.DumpMemory(bits, 16, ' ', 4);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public BitSet128 operator |(in BitSet128 inA, in BitSet128 inB)
        {
            return new BitSet128(inA.m_Bits0 | inB.m_Bits0, inA.m_Bits1 | inB.m_Bits1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public BitSet128 operator &(in BitSet128 inA, in BitSet128 inB)
        {
            return new BitSet128(inA.m_Bits0 & inB.m_Bits0, inA.m_Bits1 & inB.m_Bits1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public BitSet128 operator ^(in BitSet128 inA, in BitSet128 inB)
        {
            return new BitSet128(inA.m_Bits0 ^ inB.m_Bits0, inA.m_Bits1 ^ inB.m_Bits1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public BitSet128 operator ~(in BitSet128 inA)
        {
            return new BitSet128(~inA.m_Bits0, ~inA.m_Bits1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public bool operator ==(in BitSet128 inA, in BitSet128 inB)
        {
            return inA.m_Bits0 == inB.m_Bits0 && inA.m_Bits1 == inB.m_Bits1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public bool operator !=(in BitSet128 inA, in BitSet128 inB)
        {
            return inA.m_Bits0 != inB.m_Bits0 || inA.m_Bits1 != inB.m_Bits1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public implicit operator bool(in BitSet128 inA)
        {
            return inA.m_Bits0 != 0 || inA.m_Bits1 != 0;
        }

        #endregion // Operators
    }

    /// <summary>
    /// 256-bit mask.
    /// </summary>
    [Serializable]
    public struct BitSet256 : IBitSet, IEquatable<BitSet256>
    {
        [SerializeField] private unsafe ulong m_Bits0;
        [SerializeField] private unsafe ulong m_Bits1;
        [SerializeField] private unsafe ulong m_Bits2;
        [SerializeField] private unsafe ulong m_Bits3;

        private BitSet256(ulong inData0, ulong inData1, ulong inData2, ulong inData3)
        {
            m_Bits0 = inData0;
            m_Bits1 = inData1;
            m_Bits2 = inData2;
            m_Bits3 = inData3;
        }

        public int Capacity { get { return 256; } }

        /// <summary>
        /// Number of set bits.
        /// </summary>
        public int Count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return Bits.Count(m_Bits0) + Bits.Count(m_Bits1)
                    + Bits.Count(m_Bits2) + Bits.Count(m_Bits3);
            }
        }

        /// <summary>
        /// If the set is empty.
        /// </summary>
        public bool IsEmpty {
            get { return m_Bits0 == 0 && m_Bits1 == 0 && m_Bits2 == 0 && m_Bits3 == 0; }
        }

        /// <summary>
        /// Gets/sets the value of the bit at the given index.
        /// </summary>
        public bool this[int inIndex]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return IsSet(inIndex); }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set { Set(inIndex, value); }
        }

        /// <summary>
        /// Returns if the bit at the given index is set.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsSet(int inIndex)
        {
            Assert.True(inIndex >= 0 && inIndex < Capacity, "Bit index {0} is outside range 0-{1}", inIndex, Capacity - 1);
            unsafe
            {
                fixed (ulong* bits = &m_Bits0)
                {
                    return Bits.Contains(bits[inIndex >> 6], inIndex & 0x3F);
                }
            }
        }

        /// <summary>
        /// Sets the bit at the given index.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(int inIndex)
        {
            Assert.True(inIndex >= 0 && inIndex < Capacity, "Bit index {0} is outside range 0-{1}", inIndex, Capacity - 1);
            unsafe
            {
                fixed (ulong* bits = &m_Bits0)
                {
                    Bits.Add(ref bits[inIndex >> 6], inIndex & 0x3F);
                }
            }
        }

        /// <summary>
        /// Unsets the bit at the given index.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Unset(int inIndex)
        {
            Assert.True(inIndex >= 0 && inIndex < Capacity, "Bit index {0} is outside range 0-{1}", inIndex, Capacity - 1);
            unsafe
            {
                fixed (ulong* bits = &m_Bits0)
                {
                    Bits.Remove(ref bits[inIndex >> 6], inIndex & 0x3F);
                }
            }
        }

        /// <summary>
        /// Sets the bit at the given index to the given value.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(int inIndex, bool inValue)
        {
            if (inValue)
            {
                Set(inIndex);
            }
            else
            {
                Unset(inIndex);
            }
        }

        /// <summary>
        /// Clears all set bits.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            m_Bits0 = m_Bits1 = m_Bits2 = m_Bits3 = 0;
        }

        #region Operators

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(BitSet256 other)
        {
            return m_Bits0 == other.m_Bits0 && m_Bits1 == other.m_Bits1
                && m_Bits2 == other.m_Bits2 && m_Bits3 == other.m_Bits3;
        }

        public override bool Equals(object obj)
        {
            if (obj is BitSet256)
                return Equals((BitSet256) obj);
            return false;
        }

        public override int GetHashCode()
        {
            return m_Bits0.GetHashCode() ^ m_Bits1.GetHashCode() ^ m_Bits2.GetHashCode() ^ m_Bits3.GetHashCode();
        }

        public override string ToString()
        {
            unsafe
            {
                fixed (ulong* bits = &m_Bits0)
                {
                    return Unsafe.DumpMemory(bits, 32, ' ', 4);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public BitSet256 operator |(in BitSet256 inA, in BitSet256 inB)
        {
            return new BitSet256(inA.m_Bits0 | inB.m_Bits0, inA.m_Bits1 | inB.m_Bits1,
                inA.m_Bits2 | inB.m_Bits2, inA.m_Bits3 | inA.m_Bits3);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public BitSet256 operator &(in BitSet256 inA, in BitSet256 inB)
        {
            return new BitSet256(inA.m_Bits0 & inB.m_Bits0, inA.m_Bits1 & inB.m_Bits1,
                inA.m_Bits2 & inB.m_Bits2, inA.m_Bits3 & inA.m_Bits3);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public BitSet256 operator ^(in BitSet256 inA, in BitSet256 inB)
        {
            return new BitSet256(inA.m_Bits0 ^ inB.m_Bits0, inA.m_Bits1 ^ inB.m_Bits1,
                inA.m_Bits2 ^ inB.m_Bits2, inA.m_Bits3 ^ inA.m_Bits3);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public BitSet256 operator ~(in BitSet256 inA)
        {
            return new BitSet256(~inA.m_Bits0, ~inA.m_Bits1, ~inA.m_Bits2, ~inA.m_Bits3);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public bool operator ==(in BitSet256 inA, in BitSet256 inB)
        {
            return inA.m_Bits0 == inB.m_Bits0 && inA.m_Bits1 == inB.m_Bits1
                && inA.m_Bits2 == inB.m_Bits2 && inA.m_Bits3 == inB.m_Bits3;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public bool operator !=(in BitSet256 inA, in BitSet256 inB)
        {
            return inA.m_Bits0 != inB.m_Bits0 || inA.m_Bits1 != inB.m_Bits1
                || inA.m_Bits2 != inB.m_Bits2 || inA.m_Bits3 != inB.m_Bits3;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public implicit operator bool(in BitSet256 inA)
        {
            return inA.m_Bits0 != 0 || inA.m_Bits1 != 0 || inA.m_Bits2 != 0 || inA.m_Bits3 != 0;
        }

        #endregion // Operators
    }

    /// <summary>
    /// 512-bit mask.
    /// </summary>
    [Serializable]
    public struct BitSet512 : IBitSet, IEquatable<BitSet512>
    {
        [SerializeField] private unsafe BitSet256 m_Bits0;
        [SerializeField] private unsafe BitSet256 m_Bits1;

        private BitSet512(in BitSet256 inData0, in BitSet256 inData1)
        {
            m_Bits0 = inData0;
            m_Bits1 = inData1;
        }

        public int Capacity { get { return 512; } }

        /// <summary>
        /// Number of set bits.
        /// </summary>
        public int Count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return m_Bits0.Count + m_Bits1.Count;
            }
        }

        /// <summary>
        /// If the set is empty.
        /// </summary>
        public bool IsEmpty {
            get { return m_Bits0.IsEmpty && m_Bits1.IsEmpty; }
        }

        /// <summary>
        /// Gets/sets the value of the bit at the given index.
        /// </summary>
        public bool this[int inIndex]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return IsSet(inIndex); }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set { Set(inIndex, value); }
        }

        /// <summary>
        /// Returns if the bit at the given index is set.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsSet(int inIndex)
        {
            Assert.True(inIndex >= 0 && inIndex < Capacity, "Bit index {0} is outside range 0-{1}", inIndex, Capacity - 1);
            unsafe
            {
                fixed (BitSet256* bits = &m_Bits0)
                {
                    return bits[inIndex >> 8].IsSet(inIndex & 0xFF);
                }
            }
        }

        /// <summary>
        /// Sets the bit at the given index.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(int inIndex)
        {
            Assert.True(inIndex >= 0 && inIndex < Capacity, "Bit index {0} is outside range 0-{1}", inIndex, Capacity - 1);
            unsafe
            {
                fixed (BitSet256* bits = &m_Bits0)
                {
                    bits[inIndex >> 8].Set(inIndex & 0xFF);
                }
            }
        }

        /// <summary>
        /// Unsets the bit at the given index.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Unset(int inIndex)
        {
            Assert.True(inIndex >= 0 && inIndex < Capacity, "Bit index {0} is outside range 0-{1}", inIndex, Capacity - 1);
            unsafe
            {
                fixed (BitSet256* bits = &m_Bits0)
                {
                    bits[inIndex >> 8].Unset(inIndex & 0xFF);
                }
            }
        }

        /// <summary>
        /// Sets the bit at the given index to the given value.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(int inIndex, bool inValue)
        {
            if (inValue)
            {
                Set(inIndex);
            }
            else
            {
                Unset(inIndex);
            }
        }

        /// <summary>
        /// Clears all set bits.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            m_Bits0.Clear();
            m_Bits1.Clear();
        }

        #region Operators

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(BitSet512 other)
        {
            return m_Bits0 == other.m_Bits0 && m_Bits1 == other.m_Bits1;
        }

        public override bool Equals(object obj)
        {
            if (obj is BitSet512)
                return Equals((BitSet512) obj);
            return false;
        }

        public override int GetHashCode()
        {
            return m_Bits0.GetHashCode() ^ m_Bits1.GetHashCode();
        }

        public override string ToString()
        {
            unsafe
            {
                fixed (void* bits = &m_Bits0)
                {
                    return Unsafe.DumpMemory(bits, 64, ' ', 4);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public BitSet512 operator |(in BitSet512 inA, in BitSet512 inB)
        {
            return new BitSet512(inA.m_Bits0 | inB.m_Bits0, inA.m_Bits1 | inB.m_Bits1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public BitSet512 operator &(in BitSet512 inA, in BitSet512 inB)
        {
            return new BitSet512(inA.m_Bits0 & inB.m_Bits0, inA.m_Bits1 & inB.m_Bits1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public BitSet512 operator ^(in BitSet512 inA, in BitSet512 inB)
        {
            return new BitSet512(inA.m_Bits0 ^ inB.m_Bits0, inA.m_Bits1 ^ inB.m_Bits1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public BitSet512 operator ~(in BitSet512 inA)
        {
            return new BitSet512(~inA.m_Bits0, ~inA.m_Bits1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public bool operator ==(in BitSet512 inA, in BitSet512 inB)
        {
            return inA.m_Bits0 == inB.m_Bits0 && inA.m_Bits1 == inB.m_Bits1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public bool operator !=(in BitSet512 inA, in BitSet512 inB)
        {
            return inA.m_Bits0 != inB.m_Bits0 || inA.m_Bits1 != inB.m_Bits1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public implicit operator bool(in BitSet512 inA)
        {
            return (bool) inA.m_Bits0 || (bool) inA.m_Bits1;
        }

        #endregion // Operators
    }

    /// <summary>
    /// N-bit mask.
    /// NOTE: this is a class, not a struct, as its size cannot be determined at compile-time.
    /// </summary>
    [Serializable]
    public class BitSetN : IBitSet, ISerializationCallbackReceiver
    {
        [SerializeField] private uint[] m_Bits;
        [NonSerialized] private int m_Capacity;
        [NonSerialized] private int m_ChunkCount;

        public BitSetN(int inCapacity)
        {
            if (inCapacity < 64)
                throw new ArgumentOutOfRangeException("inCapacity", "BitSetN capacity must be at least 64");

            inCapacity = Unsafe.AlignUp32(inCapacity);
            m_Capacity = inCapacity;
            m_ChunkCount = inCapacity >> 5;
            m_Bits = new uint[m_ChunkCount];
        }

        public int Capacity { get { return m_Capacity; } }

        /// <summary>
        /// Resizes the bit set to hold the given number of bits.
        /// </summary>
        public void Resize(int inCapacity)
        {
            inCapacity = Unsafe.AlignUp32(inCapacity);
            m_Capacity = inCapacity;
            m_ChunkCount = inCapacity >> 5;
            Array.Resize(ref m_Bits, m_ChunkCount);
        }

        /// <summary>
        /// Number of set bits.
        /// </summary>
        public int Count
        {
            get
            {
                int count = 0;
                for (int i = 0; i < m_ChunkCount; i++)
                    count += Bits.Count(m_Bits[i]);
                return count;
            }
        }

        /// <summary>
        /// If the set is empty.
        /// </summary>
        public bool IsEmpty {
            get {
                for(int i = 0; i < m_ChunkCount; i++) {
                    if (m_Bits[i] != 0) {
                        return false;
                    }
                }
                return true;
            }
        }

        /// <summary>
        /// Gets/sets the value of the bit at the given index.
        /// </summary>
        public bool this[int inIndex]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return IsSet(inIndex); }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set { Set(inIndex, value); }
        }

        /// <summary>
        /// Returns if the bit at the given index is set.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsSet(int inIndex)
        {
            Assert.True(inIndex >= 0 && inIndex < Capacity, "Bit index {0} is outside range 0-{1}", inIndex, Capacity - 1);
            return Bits.Contains(m_Bits[inIndex >> 5], inIndex & 0x1F);
        }

        /// <summary>
        /// Sets the bit at the given index.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(int inIndex)
        {
            Assert.True(inIndex >= 0 && inIndex < Capacity, "Bit index {0} is outside range 0-{1}", inIndex, Capacity - 1);
            Bits.Add(ref m_Bits[inIndex >> 5], inIndex & 0x1F);
        }

        /// <summary>
        /// Unsets the bit at the given index.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Unset(int inIndex)
        {
            Assert.True(inIndex >= 0 && inIndex < Capacity, "Bit index {0} is outside range 0-{1}", inIndex, Capacity - 1);
            Bits.Remove(ref m_Bits[inIndex >> 5], inIndex & 0x1F);
        }

        /// <summary>
        /// Sets the bit at the given index to the given value.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(int inIndex, bool inValue)
        {
            if (inValue)
            {
                Set(inIndex);
            }
            else
            {
                Unset(inIndex);
            }
        }

        /// <summary>
        /// Clears all set bits.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            Array.Clear(m_Bits, 0, m_ChunkCount);
        }

        /// <summary>
        /// Unpacks the bit data.
        /// </summary>
        public void Unpack(out uint[] outBits)
        {
            outBits = m_Bits;
        }

        #region Operators

        public override string ToString()
        {
            unsafe
            {
                fixed (uint* bits = m_Bits)
                {
                    return Unsafe.DumpMemory(bits, m_ChunkCount * 4, ' ', 4);
                }
            }
        }

        #endregion // Operators

        #region Serialization

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            m_ChunkCount = m_Bits.Length;
            m_Capacity = m_ChunkCount << 5;
        }

        #endregion // Serialization
    }
}