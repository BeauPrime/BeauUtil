/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    7 Sept 2020
 * 
 * File:    VariantTable.cs
 * Purpose: Collection of named variant values.
 */

#if CSHARP_7_3_OR_NEWER
#define EXPANDED_REFS
#endif // CSHARP_7_3_OR_NEWER

#if UNITY_2021_2_OR_NEWER && !BEAUUTIL_DISABLE_FUNCTION_POINTERS
#define SUPPORTS_FUNCTION_POINTERS
#endif // UNITY_2021_2_OR_NEWER

#if (UNITY_EDITOR && !IGNORE_UNITY_EDITOR) || DEVELOPMENT_BUILD || DEVELOPMENT
#define HAS_DEBUGGER
#endif // UNITY_EDITOR

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using BeauUtil.Debugger;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;

namespace BeauUtil.Variants
{
    /// <summary>
    /// Collection of named variant values.
    /// </summary>
    [Il2CppEagerStaticClassConstruction]
    [DebuggerDisplay("Count={Count}")]
#if HAS_DEBUGGER
    [DebuggerTypeProxy(typeof(VariantTable.DebugView))]
#endif // HAS_DEBUGGER
    public sealed class VariantTable : IDisposable, IDebugString
        #if USING_BEAUDATA
        , BeauData.ISerializedObject
        #endif // USING_BEAUDATA
    {
        private const int BinarySearchThreshold = 8;

        private NamedVariant[] m_Values;
        private int m_ValueCount;
        private bool m_Sorted = true;
        private StringHash32 m_Name;
        private VariantTable m_Base;

        public VariantTable()
        {
            m_Values = Array.Empty<NamedVariant>();
        }

        public VariantTable(int inCapacity)
        {
            if (inCapacity < 0)
            {
                throw new ArgumentOutOfRangeException("inCapacity");
            }
            m_Values = new NamedVariant[inCapacity];
        }

        public VariantTable(StringHash32 inName)
            : this()
        {
            m_Name = inName;
        }

        public VariantTable(StringHash32 inName, int inCapacity)
            : this(inCapacity)
        {
            m_Name = inName;
        }

        public VariantTable(StringHash32 inName, VariantTable inBase)
            : this(inName)
        {
            m_Base = inBase;
        }

        public VariantTable(StringHash32 inName, int inCapacity, VariantTable inBase)
            : this(inName, inCapacity)
        {
            m_Base = inBase;
        }

        /// <summary>
        /// Name of the variant table.
        /// </summary>
        public StringHash32 Name
        {
            get { return m_Name; }
            set { m_Name = value; }
        }

        /// <summary>
        /// Number of values in this table.
        /// </summary>
        public int Count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return m_ValueCount; }
        }

        /// <summary>
        /// Current value capacity.
        /// </summary>
        public int Capacity
        {
            [Il2CppSetOption(Option.NullChecks, false)]
            get { return m_Values.Length; }
            [Il2CppSetOption(Option.NullChecks, false)]
            set { Array.Resize(ref m_Values, value); }
        }

        /// <summary>
        /// Executed when a variant is updated.
        /// </summary>
        public event Action<NamedVariant> OnUpdated;

        /// <summary>
        /// Retrieves the value at the given index.
        /// </summary>
        public NamedVariant this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            [Il2CppSetOption(Option.NullChecks, false)]
            get { return m_Values[index]; }
        }

        /// <summary>
        /// Sets the value for the given id.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(StringHash32 inId, Variant inValue)
        {
            SetAt(IndexOf(inId), inId, inValue);
        }

        /// <summary>
        /// Retrieves the value on this table.
        /// Will not look into base tables.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Variant Get(StringHash32 inId)
        {
            return GetAt(IndexOf(inId));
        }

        /// <summary>
        /// Gets/sets the variants for the given id.
        /// </summary>
        public Variant this[StringHash32 inId]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return Get(inId); }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set { Set(inId, value); }
        }

        /// <summary>
        /// Returns if a value with the given id exists on this table.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Has(StringHash32 inId)
        {
            return IndexOf(inId) >= 0;
        }

        /// <summary>
        /// Attempts to delete the value with the given id.
        /// </summary>
        public bool Delete(StringHash32 inId)
        {
            int idx = IndexOf(inId);
            if (idx >= 0)
            {
                m_Values.FastRemoveAt(idx);
                OnUpdated?.Invoke(new NamedVariant(inId, Variant.Null));
                m_Sorted = false;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Clears all values from this table.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            m_ValueCount = 0;
            m_Sorted = true;
        }

        /// <summary>
        /// Clears values and base.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Reset()
        {
            Clear();
            m_Base = null;
        }

        /// <summary>
        /// Overwrites another table with this table's values.
        /// </summary>
        [Il2CppSetOption(Option.NullChecks, false)]
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        public void CopyTo(VariantTable inTarget)
        {
            Assert.NotNull(inTarget);
            Assert.True(inTarget != this);
            Array.Resize(ref inTarget.m_Values, m_Values.Length);
            inTarget.m_ValueCount = m_ValueCount;

            unsafe
            {
                fixed (NamedVariant* src = m_Values)
                {
                    fixed (NamedVariant* dst = inTarget.m_Values)
                    {
                        Unsafe.FastCopyArray(src, m_ValueCount, dst);
                    }
                }
            }

            inTarget.m_Sorted = m_Sorted;
        }

        /// <summary>
        /// The base VariantTable.
        /// Values that do not exist in this VariantTable will be looked up in the base.
        /// </summary>
        public VariantTable Base
        {
            get { return m_Base; }
            set
            {
                if (m_Base != value)
                {
                    if (value != null)
                    {
                        if (value == this || value.Base == this)
                            throw new InvalidOperationException("Provided parent would cause infinite loop");
                    }

                    m_Base = value;
                }
            }
        }

        /// <summary>
        /// Optimizes lookups.
        /// </summary>
        [Il2CppSetOption(Option.NullChecks, false)]
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        public void Optimize()
        {
            if (m_Sorted)
                return;

            if (m_ValueCount > BinarySearchThreshold)
            {
                unsafe
                {
                    fixed(NamedVariant* named = m_Values)
                    {
                        Unsafe.Quicksort(named, m_ValueCount, CompareEntriesPtr);
                    }
                }
            }
            m_Sorted = true;
        }

        /// <summary>
        /// Attempts to retrieve a value from the table.
        /// If not present in this table, it will look in the parent.
        /// </summary>
        public bool TryLookup(StringHash32 inId, out Variant outValue)
        {
            Optimize();

            int idx = IndexOf(inId);
            if (idx >= 0)
            {
                outValue = m_Values[idx].Value;
                return true;
            }

            if (m_Base != null)
            {
                return m_Base.TryLookup(inId, out outValue);
            }

            outValue = Variant.Null;
            return false;
        }

        /// <summary>
        /// Modify a value on the table.
        /// </summary>
        public void Modify(StringHash32 inId, VariantModifyOperator inOperator, Variant inOperand)
        {
            int idx = IndexOf(inId);
            switch(inOperator)
            {
                case VariantModifyOperator.Set:
                    {
                        SetAt(idx, inId, inOperand);
                        break;
                    }

                case VariantModifyOperator.Add:
                    {
                        Variant current = GetAt(idx);
                        SetAt(idx, inId, current + inOperand);
                        break;
                    }

                case VariantModifyOperator.Subtract:
                    {
                        Variant current = GetAt(idx);
                        SetAt(idx, inId, current - inOperand);
                        break;
                    }

                case VariantModifyOperator.Multiply:
                    {
                        Variant current = GetAt(idx);
                        SetAt(idx, inId, current * inOperand);
                        break;
                    }

                case VariantModifyOperator.Divide:
                    {
                        Variant current = GetAt(idx);
                        SetAt(idx, inId, current / inOperand);
                        break;
                    }
            }
        }

        [Il2CppSetOption(Option.NullChecks, false)]
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        private int IndexOf(StringHash32 inId)
        {
            if (m_Sorted && m_ValueCount > BinarySearchThreshold)
                return IndexOfBinarySearch(inId);

            for (int i = 0; i < m_ValueCount; i++)
            {
                if (m_Values[i].Id == inId)
                    return i;
            }

            return -1;
        }

        [Il2CppSetOption(Option.NullChecks, false)]
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        private int IndexOfBinarySearch(StringHash32 inSearchId)
        {
            int low = 0;
            int high = m_ValueCount - 1;

            while (low <= high)
            {
                int med = low + ((high - low) >> 1);
                StringHash32 entryId = m_Values[med].Id;
                if (entryId == inSearchId)
                    return med;
                if (inSearchId.HashValue > entryId.HashValue)
                    low = med + 1;
                else
                    high = med - 1;
            }

            return ~low;
        }

        [Il2CppSetOption(Option.NullChecks, false)]
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Variant GetAt(int inIdx)
        {
            return inIdx >= 0 ? m_Values[inIdx].Value : Variant.Null;
        }

        [Il2CppSetOption(Option.NullChecks, false)]
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        private void SetAt(int inIdx, StringHash32 inId, Variant inValue)
        {
            if (inIdx >= 0)
            {
                #if EXPANDED_REFS
                ref NamedVariant val = ref m_Values[inIdx];
                if (!val.Value.StrictEquals(inValue))
                {
                    val.Value = inValue;
                    OnUpdated?.Invoke(val);
                }
                #else
                NamedVariant val = m_Values[inIdx];
                if (!val.Value.StrictEquals(inValue))
                {
                    val.Value = inValue;
                    m_Values[inIdx] = val;
                    OnUpdated?.Invoke(val);
                }
                #endif // EXPANDED_VALUES
                return;
            }

            if (m_ValueCount + 1 > m_Values.Length)
            {
                Array.Resize(ref m_Values, BufferExpansionPolicy.CalculateNextGeometric(m_ValueCount, 0.5f, 32, 4));
            }

            NamedVariant newValue = new NamedVariant(inId, inValue);
            m_Values[m_ValueCount++] = newValue;
            m_Sorted = false;

            OnUpdated?.Invoke(newValue);
        }

        #region Output

        public string ToDebugString()
        {
            StringBuilder sb = new StringBuilder();
            ToDebugString(sb);
            return sb.ToString();
        }

        public void ToDebugString(StringBuilder inBuilder)
        {
            inBuilder.Append("Variant Table '");
            m_Name.ToDebugString(inBuilder);
            inBuilder.Append("', ").AppendNoAlloc(Count).Append(" entries");
            for (int i = 0; i < m_ValueCount; ++i)
            {
                inBuilder.Append("\n  ");
                m_Values[i].ToDebugString(inBuilder);
            }
        }

        #endregion // Output

        #region IDisposable

        void IDisposable.Dispose()
        {
            Reset();
        }

        #endregion // IDisposable

        #region Binary Search

#if SUPPORTS_FUNCTION_POINTERS
        static private unsafe readonly delegate*<NamedVariant, NamedVariant, int> CompareEntriesPtr = &CompareEntries;
#else
        static private readonly Comparison<NamedVariant> CompareEntriesPtr = CompareEntries;
#endif // SUPPORTS_FUNCTION_POINTERS

        static private int CompareEntries(NamedVariant a, NamedVariant b)
        {
            return a.Id.HashValue < b.Id.HashValue ? -1 : a.Id.HashValue > b.Id.HashValue ? 1 : 0;
        }

        #endregion // Binary Search

        #region ISerializedObject

#if USING_BEAUDATA

        public void Serialize(BeauData.Serializer ioSerializer)
        {
            ioSerializer.UInt32Proxy("name", ref m_Name);

            NamedVariant[] variantArray = null;
            if (ioSerializer.IsWriting)
            {
                if (m_ValueCount < m_Values.Length)
                {
                    variantArray = new NamedVariant[m_ValueCount];
                    Array.Copy(m_Values, variantArray, m_ValueCount);
                }
                else
                {
                    variantArray = m_Values;
                }
            }

            ioSerializer.ObjectArray("values", ref variantArray);

            if (ioSerializer.IsReading)
            {
                m_Values = variantArray;
                m_ValueCount = variantArray.Length;
                m_Sorted = false;
            }
        }

#endif // USING_BEAUDATA

        #endregion // ISerializedObject

#if HAS_DEBUGGER

        private class DebugView
        {
            private readonly VariantTable m_Data;

            public DebugView(VariantTable inData)
            {
                m_Data = inData;
            }

            [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
            public NamedVariant[] Items
            {
                get
                {
                    NamedVariant[] values = new NamedVariant[m_Data.m_ValueCount];
                    Array.Copy(m_Data.m_Values, values, m_Data.m_ValueCount);
                    return values;
                }
            }
        }

#endif // HAS_DEBUGGER
    }
}