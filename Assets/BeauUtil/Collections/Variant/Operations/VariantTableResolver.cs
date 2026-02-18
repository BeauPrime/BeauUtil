/*
 * Copyright (C) 2017-2026. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    17 Feb 2026
 * 
 * File:    VariantTableResolver.cs
 * Purpose: Simple variant table resolver.
 */

using System;
using System.Collections.Generic;
using BeauUtil.Debugger;

namespace BeauUtil.Variants
{
    /// <summary>
    /// Simple variant table resolver.
    /// </summary>
    public sealed class VariantTableResolver : IVariantTableResolver
    {
        #region Fields

        private VariantTable m_DefaultTable;
        private readonly Dictionary<StringHash32, VariantTable> m_TableLookup;
        private IVariantTableResolver m_Base;

        #endregion // Fields

        public VariantTableResolver(int inCapacity)
        {
            m_TableLookup = new Dictionary<StringHash32, VariantTable>(inCapacity, CompareUtils.DefaultEquals<StringHash32>());
        }

        /// <summary>
        /// Base variant resolver to use.
        /// If lookups fail on this resolver, it will retry with the base.
        /// </summary>
        public IVariantTableResolver Base
        {
            get { return m_Base; }
            set
            {
                if (m_Base != null)
                {
                    if (m_Base == this || (m_Base is VariantTableResolver && ((VariantTableResolver) m_Base).m_Base == this))
                        throw new InvalidOperationException("Provided base would create infinite loop");
                }
                m_Base = value;
            }
        }

        /// <summary>
        /// Clears all key remappings, table lookup, and variable lookup.
        /// </summary>
        public void Clear()
        {
            m_TableLookup.Clear();
            m_DefaultTable = null;
        }

        #region Table Lookup

        /// <summary>
        /// Sets the default table if no table id is provided.
        /// </summary>
        public VariantTableResolver SetDefaultTable(VariantTable inTable)
        {
            Assert.NotNull(inTable);
            m_DefaultTable = inTable;
            return this;
        }

        /// <summary>
        /// Clears default table lookup.
        /// </summary>
        public VariantTableResolver ClearDefaultTable()
        {
            m_DefaultTable = null;
            return this;
        }

        /// <summary>
        /// Adds a table to lookup, using the table's name as its id.
        /// </summary>
        public VariantTableResolver SetTable(VariantTable inTable)
        {
            Assert.NotNull(inTable);
            Assert.False(inTable.Name.IsEmpty, "Cannot bind an unnamed table without an explicit name");
            m_TableLookup[inTable.Name] = inTable;
            return this;
        }

        /// <summary>
        /// Sets the table associated with the given table id.
        /// </summary>
        public VariantTableResolver SetTable(StringHash32 inTableId, VariantTable inTable)
        {
            Assert.NotNull(inTable);
            m_TableLookup[inTableId] = inTable;
            return this;
        }

        /// <summary>
        /// Removes table lookup for the given table id.
        /// </summary>
        public VariantTableResolver ClearTable(StringHash32 inTableId)
        {
            m_TableLookup.Remove(inTableId);
            return this;
        }

        /// <summary>
        /// Returns all tables loaded into the resolver.
        /// </summary>
        public IEnumerable<VariantTable> AllTables()
        {
            if (m_DefaultTable != null)
                yield return m_DefaultTable;

            foreach(var lookup in m_TableLookup.Values)
            {
                yield return lookup;
            }
        }

        #endregion // Table Lookup

        #region VariantTableResolver

        public bool TryGetTable(StringHash32 inTableId, out VariantTable outTable)
        {
            if (inTableId.IsEmpty)
            {
                if (m_DefaultTable != null)
                {
                    outTable = m_DefaultTable;
                    return true;
                }
            }
            else if (m_TableLookup.TryGetValue(inTableId, out outTable))
            {
                return true;
            }

            if (m_Base != null)
            {
                return m_Base.TryGetTable(inTableId, out outTable);
            }

            outTable = null;
            return false;
        }

        #endregion // VariantTableResolver
    }
}