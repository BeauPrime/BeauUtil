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

using System;
using System.Collections;
using System.Collections.Generic;

namespace BeauUtil
{
    /// <summary>
    /// Collection of named variant values.
    /// </summary>
    public class VariantTable : IEnumerable<NamedVariant>
    {
        private RingBuffer<NamedVariant> m_Values;
        private VariantTable m_Base;

        public VariantTable()
        {
            m_Values = new RingBuffer<NamedVariant>();
            m_Base = null;
        }

        public VariantTable(VariantTable inBase)
        {
            m_Values = new RingBuffer<NamedVariant>();
            m_Base = inBase;
        }

        /// <summary>
        /// Attempts to retrieve a value from the table.
        /// If not present in this table, it will look in the parent.
        /// </summary>
        public bool TryLookup(StringHash inId, out Variant outValue)
        {
            for(int i = 0, count = m_Values.Count; i < count; ++i)
            {
                #if EXPANDED_REFS
                ref NamedVariant val = ref m_Values[i];
                #else
                NamedVariant val = m_Values[i];
                #endif // EXPANDED_REFS
                if (val.Id == inId)
                {
                    outValue = val.Value;
                    return true;
                }
            }

            if (m_Base != null)
            {
                return m_Base.TryLookup(inId, out outValue);
            }

            outValue = Variant.Null;
            return false;
        }

        /// <summary>
        /// Attempts to delete the value with the given id.
        /// </summary>
        public bool Delete(StringHash inId)
        {
            for(int i = 0, count = m_Values.Count; i < count; ++i)
            {
                if (m_Values[i].Id == inId)
                {
                    m_Values.FastRemoveAt(i);
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Sets the value for the given id.
        /// </summary>
        public void Set(StringHash inId, Variant inValue)
        {
            for(int i = 0, count = m_Values.Count; i < count; ++i)
            {
                #if EXPANDED_REFS
                ref NamedVariant val = ref m_Values[i];
                #else
                NamedVariant val = m_Values[i];
                #endif // EXPANDED_REFS
                if (val.Id == inId)
                {
                    val.Value = inValue;
                    return;
                }
            }

            m_Values.PushBack(new NamedVariant(inId, inValue));
        }

        /// <summary>
        /// Retrieves the value on this table.
        /// Will not look into base tables.
        /// </summary>
        public Variant Get(StringHash inId)
        {
            for(int i = 0, count = m_Values.Count; i < count; ++i)
            {
                #if EXPANDED_REFS
                ref NamedVariant val = ref m_Values[i];
                #else
                NamedVariant val = m_Values[i];
                #endif // EXPANDED_REFS
                if (val.Id == inId)
                {
                    return val.Value;
                }
            }

            return Variant.Null;
        }

        /// <summary>
        /// Returns if a value with the given id exists on this table.
        /// </summary>
        public bool Has(StringHash inId)
        {
            for(int i = 0, count = m_Values.Count; i < count; ++i)
            {
                if (m_Values[i].Id == inId)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Clears all values from this table.
        /// </summary>
        public void Clear()
        {
            m_Values.Clear();
        }

        /// <summary>
        /// Number of values in this table.
        /// </summary>
        public int Count
        {
            get { return m_Values.Count; }
        }

        /// <summary>
        /// The base VariantTable.
        /// Values that do not exist in this VariantTable
        /// will be looked up in the base.
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
                        if (value == this || value.m_Base == this)
                            throw new InvalidOperationException("Provided parent would cause infinite loop");
                    }

                    m_Base = value;
                }
            }
        }

        /// <summary>
        /// Gets/sets the variants for the given id.
        /// </summary>
        public Variant this[StringHash inId]
        {
            get { return Get(inId); }
            set { Set(inId, value); }
        }
        
        #region IEnumerable

        public IEnumerator<NamedVariant> GetEnumerator()
        {
            return m_Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return m_Values.GetEnumerator();
        }

        #endregion // IEnumerable
    }
}