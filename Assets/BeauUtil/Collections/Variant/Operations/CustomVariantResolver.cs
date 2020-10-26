/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    17 Sept 2020
 * 
 * File:    CustomVariantResolver.cs
 * Purpose: Customizable variant resolver.
 */

using System;
using System.Collections.Generic;

namespace BeauUtil.Variants
{
    /// <summary>
    /// Customizable variant lookup resolver.
    /// </summary>
    public class CustomVariantResolver : IVariantResolver
    {
        #region Types

        #region Variable Lookup

        // get var (assuming full id)
        public delegate Variant GetVarDelegate();
        public delegate Variant GetVarWithContextDelegate(object inContext);
        
        // get var (assuming table id)
        public delegate Variant GetVarWithIdDelegate(StringHash32 inId);
        public delegate Variant GetVarWithIdAndContextDelegate(StringHash32 inId, object inContext);
        
        // get var (fallback)
        public delegate Variant GetVarWithTableKeyDelegate(TableKeyPair inKey);
        public delegate Variant GetVarWithTableKeyAndContextDelegate(TableKeyPair inKey, object inContext);

        private class FullPathVarRule
        {
            private Variant m_ConstValue;
            private GetVarDelegate m_GetDelegate;
            private GetVarWithContextDelegate m_GetWithContextDelegate;

            public void SetConst(Variant inValue)
            {
                m_ConstValue = inValue;
                m_GetDelegate = null;
                m_GetWithContextDelegate = null;
            }

            public void SetDelegate(GetVarDelegate inGetDelegate)
            {
                m_ConstValue = default(Variant);
                m_GetDelegate = inGetDelegate;
                m_GetWithContextDelegate = null;
            }

            public void SetDelegate(GetVarWithContextDelegate inGetDelegate)
            {
                m_ConstValue = default(Variant);
                m_GetDelegate = null;
                m_GetWithContextDelegate = inGetDelegate;
            }

            public Variant Resolve(object inContext)
            {
                if (m_GetDelegate != null)
                    return m_GetDelegate();
                if (m_GetWithContextDelegate != null)
                    return m_GetWithContextDelegate(inContext);

                return m_ConstValue;
            }
        }

        private class TableVarRule
        {
            private GetVarWithIdDelegate m_GetDelegate;
            private GetVarWithIdAndContextDelegate m_GetWithContextDelegate;

            public void SetDelegate(GetVarWithIdDelegate inGetDelegate)
            {
                m_GetDelegate = inGetDelegate;
                m_GetWithContextDelegate = null;
            }

            public void SetDelegate(GetVarWithIdAndContextDelegate inGetDelegate)
            {
                m_GetDelegate = null;
                m_GetWithContextDelegate = inGetDelegate;
            }

            public Variant Resolve(StringHash32 inId, object inContext)
            {
                if (m_GetDelegate != null)
                    return m_GetDelegate(inId);
                if (m_GetWithContextDelegate != null)
                    return m_GetWithContextDelegate(inId, inContext);

                return Variant.Null;
            }
        }

        #endregion // Variable Lookup

        #region Table Lookup

        // get table (assuming table id)
        public delegate VariantTable GetTableDelegate();
        public delegate VariantTable GetTableWithContextDelegate(object inContext);

        // get table (fallback)
        public delegate VariantTable GetTableWithIdDelegate(StringHash32 inId);
        public delegate VariantTable GetTableWithIdAndContextDelegate(StringHash32 inId, object inContext);

        private class TableRule
        {
            private VariantTable m_Table;
            private GetTableDelegate m_TableDelegate;
            private GetTableWithContextDelegate m_TableWithContextDelegate;

            public void Clear()
            {
                m_Table = null;
                m_TableDelegate = null;
                m_TableWithContextDelegate = null;
            }

            public bool IsActive()
            {
                return m_Table != null || m_TableDelegate != null || m_TableWithContextDelegate != null;
            }

            public void SetConst(VariantTable inTable)
            {
                m_Table = inTable;
                m_TableDelegate = null;
                m_TableWithContextDelegate = null;
            }

            public void SetDelegate(GetTableDelegate inDelegate)
            {
                m_Table = null;
                m_TableDelegate = inDelegate;
                m_TableWithContextDelegate = null;
            }

            public void SetDelegate(GetTableWithContextDelegate inDelegate)
            {
                m_Table = null;
                m_TableDelegate = null;
                m_TableWithContextDelegate = inDelegate;
            }

            public VariantTable Resolve(object inContext)
            {
                if (m_TableDelegate != null)
                    return m_TableDelegate();
                if (m_TableWithContextDelegate != null)
                    return m_TableWithContextDelegate(inContext);

                return m_Table;
            }
        }

        #endregion // Table Lookup

        #endregion // Types

        #region Fields

        // parent
        private IVariantResolver m_Base;

        // remap
        private Dictionary<TableKeyPair, TableKeyPair> m_FullKeyRemap;
        private Dictionary<StringHash32, StringHash32> m_TableIdRemap;
        private Dictionary<StringHash32, StringHash32> m_VariableIdRemap;
        private bool m_HasRemaps;

        // table lookup
        private Dictionary<StringHash32, TableRule> m_TableLookup;
        private TableRule m_DefaultTable;
        private GetTableWithIdDelegate m_GetTableFallback;
        private GetTableWithIdAndContextDelegate m_GetTableWithContextFallback;

        // var lookup
        private Dictionary<TableKeyPair, FullPathVarRule> m_FullPathVarLookup;
        private Dictionary<StringHash32, TableVarRule> m_TableVarLookup;
        private GetVarWithTableKeyDelegate m_GetVarFallback;
        private GetVarWithTableKeyAndContextDelegate m_GetVarWithContextFallback;
        private bool m_HasSpecialVariantLookups;

        #endregion // Fields

        /// <summary>
        /// Base variant resolver to use.
        /// If lookups fail on this resolver, it will retry with the base.
        /// </summary>
        public IVariantResolver Base
        {
            get { return m_Base; }
            set
            {
                if (m_Base != null)
                {
                    if (m_Base == this || (m_Base is CustomVariantResolver && ((CustomVariantResolver) m_Base).m_Base == this))
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
            ClearRemapping();
            ClearTableLookup();
            ClearVarLookup();
        }

        #region Remapping

        /// <summary>
        /// Clears all key remapping.
        /// </summary>
        public void ClearRemapping()
        {
            m_HasRemaps = false;
            
            if (m_FullKeyRemap != null)
                m_FullKeyRemap.Clear();

            if (m_TableIdRemap != null)
                m_TableIdRemap.Clear();

            if (m_VariableIdRemap != null)
                m_VariableIdRemap.Clear();
        }

        /// <summary>
        /// Sets up a remap between two full table keys.
        /// </summary>
        public CustomVariantResolver RemapFullKey(TableKeyPair inKey, TableKeyPair inRemap)
        {
            m_HasRemaps = true;
            if (m_FullKeyRemap == null)
                m_FullKeyRemap = new Dictionary<TableKeyPair, TableKeyPair>();

            m_FullKeyRemap[inKey] = inRemap;
            return this;
        }

        /// <summary>
        /// Sets up a remap between two table ids.
        /// </summary>
        public CustomVariantResolver RemapTableId(StringHash32 inId, StringHash32 inRemap)
        {
            m_HasRemaps = true;
            if (m_TableIdRemap == null)
                m_TableIdRemap = new Dictionary<StringHash32, StringHash32>();

            m_TableIdRemap[inId] = inRemap;
            return this;
        }

        /// <summary>
        /// Sets up a remap between two variable ids.
        /// </summary>
        public CustomVariantResolver RemapVarId(StringHash32 inId, StringHash32 inRemap)
        {
            m_HasRemaps = true;
            if (m_VariableIdRemap == null)
                m_VariableIdRemap = new Dictionary<StringHash32, StringHash32>();

            m_VariableIdRemap[inId] = inRemap;
            return this;
        }

        #endregion // Remapping

        #region Table Lookup

        /// <summary>
        /// Clears table lookup.
        /// </summary>
        public void ClearTableLookup()
        {
            if (m_TableLookup != null)
                m_TableLookup.Clear();
            if (m_DefaultTable != null)
                m_DefaultTable.Clear();
            
            m_GetTableFallback = null;
            m_GetTableWithContextFallback = null;
        }

        /// <summary>
        /// Sets the default table if no table id is provided.
        /// </summary>
        public CustomVariantResolver SetDefaultTable(VariantTable inTable)
        {
            if (inTable == null)
                throw new ArgumentNullException("inTable");
            
            if (m_DefaultTable == null)
                m_DefaultTable = new TableRule();
            m_DefaultTable.SetConst(inTable);
            return this;
        }

        /// <summary>
        /// Uses a delegate to return a default table if no table id is provided.
        /// </summary>
        public CustomVariantResolver SetDefaultTable(GetTableDelegate inGetter)
        {
            if (inGetter == null)
                throw new ArgumentNullException("inGetter");

            if (m_DefaultTable == null)
                m_DefaultTable = new TableRule();
            m_DefaultTable.SetDelegate(inGetter);
            return this;
        }

        /// <summary>
        /// Uses a delegate to return a default table if no table id is provided.
        /// </summary>
        public CustomVariantResolver SetDefaultTable(GetTableWithContextDelegate inGetter)
        {
            if (inGetter == null)
                throw new ArgumentNullException("inGetter");

            if (m_DefaultTable == null)
                m_DefaultTable = new TableRule();
            m_DefaultTable.SetDelegate(inGetter);
            return this;
        }

        /// <summary>
        /// Clears default table lookup.
        /// </summary>
        public CustomVariantResolver ClearDefaultTable()
        {
            if (m_DefaultTable != null)
                m_DefaultTable.Clear();
            return this;
        }

        /// <summary>
        /// Adds a table to lookup, using the table's name as its id.
        /// </summary>
        public CustomVariantResolver SetTable(VariantTable inTable)
        {
            if (inTable == null)
                throw new ArgumentNullException("inTable");

            return SetTable(inTable.Name, inTable);
        }

        /// <summary>
        /// Sets the table associated with the given table id.
        /// </summary>
        public CustomVariantResolver SetTable(StringHash32 inTableId, VariantTable inTable)
        {
            if (inTable == null)
                throw new ArgumentNullException("inTable");

            MakeTableRule(inTableId).SetConst(inTable);
            return this;
        }

        /// <summary>
        /// Uses a delegate to return a table for the given table id.
        /// </summary>
        public CustomVariantResolver SetTable(StringHash32 inTableId, GetTableDelegate inGetter)
        {
            if (inGetter == null)
                throw new ArgumentNullException("inGetter");

            MakeTableRule(inTableId).SetDelegate(inGetter);
            return this;
        }

        /// <summary>
        /// Uses a delegate to return a table for the given table id.
        /// </summary>
        public CustomVariantResolver SetTable(StringHash32 inTableId, GetTableWithContextDelegate inGetter)
        {
            if (inGetter == null)
                throw new ArgumentNullException("inGetter");

            MakeTableRule(inTableId).SetDelegate(inGetter);
            return this;
        }

        /// <summary>
        /// Removes table lookup for the given table id.
        /// </summary>
        public CustomVariantResolver ClearTable(StringHash32 inTableId)
        {
            if (m_TableLookup != null)
                m_TableLookup.Remove(inTableId);
            return this;
        }

        /// <summary>
        /// Sets a fallback delegate for returning a table from an arbitrary id.
        /// </summary>
        public CustomVariantResolver SetTableFallback(GetTableWithIdDelegate inFallback)
        {
            m_GetTableFallback = inFallback;
            m_GetTableWithContextFallback = null;
            return this;
        }

        /// <summary>
        /// Sets a fallback delegate for returning a table from an arbitrary id.
        /// </summary>
        public CustomVariantResolver SetTableFallback(GetTableWithIdAndContextDelegate inFallback)
        {
            m_GetTableFallback = null;
            m_GetTableWithContextFallback = inFallback;
            return this;
        }

        private TableRule MakeTableRule(StringHash32 inTableId)
        {
            TableRule rule;
            if (m_TableLookup == null)
            {
                m_TableLookup = new Dictionary<StringHash32, TableRule>();
                rule = null;
            }
            else
            {
                m_TableLookup.TryGetValue(inTableId, out rule);
            }

            if (rule == null)
            {
                rule = new TableRule();
                m_TableLookup.Add(inTableId, rule);
            }
            
            return rule;
        }

        /// <summary>
        /// Returns all tables loaded into the resolver.
        /// </summary>
        public IEnumerable<VariantTable> AllTables()
        {
            if (m_DefaultTable != null)
                yield return m_DefaultTable.Resolve(null);

            if (m_TableLookup != null)
            {
                foreach(var lookup in m_TableLookup.Values)
                {
                    yield return lookup.Resolve(null);
                }
            }
        }

        #endregion // Table Lookup

        #region Var Lookup

        /// <summary>
        /// Clears variable lookup.
        /// </summary>
        public void ClearVarLookup()
        {
            m_HasSpecialVariantLookups = false;

            if (m_TableVarLookup != null)
                m_TableVarLookup.Clear();

            if (m_FullPathVarLookup != null)
                m_FullPathVarLookup.Clear();

            m_GetVarFallback = null;
            m_GetVarWithContextFallback = null;
        }

        /// <summary>
        /// Sets the value associated with the given key.
        /// </summary>
        public CustomVariantResolver SetVar(TableKeyPair inKey, Variant inValue)
        {
            MakeFullPathVarRule(inKey).SetConst(inValue);
            return this;
        }

        /// <summary>
        /// Uses a delegate to return a value for the given key.
        /// </summary>
        public CustomVariantResolver SetVar(TableKeyPair inKey, GetVarDelegate inGetter)
        {
            if (inGetter == null)
                throw new ArgumentNullException("inGetter");

            MakeFullPathVarRule(inKey).SetDelegate(inGetter);
            return this;
        }

        /// <summary>
        /// Uses a delegate to return a value for the given key.
        /// </summary>
        public CustomVariantResolver SetVar(TableKeyPair inKey, GetVarWithContextDelegate inGetter)
        {
            if (inGetter == null)
                throw new ArgumentNullException("inGetter");

            MakeFullPathVarRule(inKey).SetDelegate(inGetter);
            return this;
        }

        /// <summary>
        /// Uses a delegate to return a value for the given table id.
        /// </summary>
        public CustomVariantResolver SetTableVar(StringHash32 inTableId, GetVarWithIdDelegate inGetter)
        {
            if (inGetter == null)
                throw new ArgumentNullException("inGetter");

            MakeTableVarRule(inTableId).SetDelegate(inGetter);
            return this;
        }

        /// <summary>
        /// Uses a delegate to return a value for the given table id
        /// </summary>
        public CustomVariantResolver SetTableVar(StringHash32 inTableId, GetVarWithIdAndContextDelegate inGetter)
        {
            if (inGetter == null)
                throw new ArgumentNullException("inGetter");

            MakeTableVarRule(inTableId).SetDelegate(inGetter);
            return this;
        }

        /// <summary>
        /// Removes variable lookup for the given table id.
        /// </summary>
        public CustomVariantResolver ClearTableVars(StringHash32 inTableId)
        {
            if (m_TableVarLookup != null)
            {
                m_TableVarLookup.Remove(inTableId);
                m_HasSpecialVariantLookups = CalculateHasSpecialVariants();
            }
            return this;
        }

        /// <summary>
        /// Removes variable lookup for the given key.
        /// </summary>
        public CustomVariantResolver ClearVar(TableKeyPair inKey)
        {
            if (m_FullPathVarLookup != null)
            {
                m_FullPathVarLookup.Remove(inKey);
                m_HasSpecialVariantLookups = CalculateHasSpecialVariants();
            }
            return this;
        }

        /// <summary>
        /// Sets a fallback delegate for returning a variable from an arbitrary id.
        /// </summary>
        public CustomVariantResolver SetVariableFallback(GetVarWithTableKeyDelegate inFallback)
        {
            m_GetVarFallback = inFallback;
            m_GetVarWithContextFallback = null;
            m_HasSpecialVariantLookups = CalculateHasSpecialVariants();
            return this;
        }

        /// <summary>
        /// Sets a fallback delegate for returning a variable from an arbitrary id.
        /// </summary>
        public CustomVariantResolver SetVariableFallback(GetVarWithTableKeyAndContextDelegate inFallback)
        {
            m_GetVarFallback = null;
            m_GetVarWithContextFallback = inFallback;
            m_HasSpecialVariantLookups = CalculateHasSpecialVariants();
            return this;
        }

        private bool CalculateHasSpecialVariants()
        {
            return (m_FullPathVarLookup != null && m_FullPathVarLookup.Count > 0)
                || (m_TableVarLookup != null && m_TableVarLookup.Count > 0)
                || m_GetVarFallback != null || m_GetVarWithContextFallback != null;
        }

        private FullPathVarRule MakeFullPathVarRule(TableKeyPair inKey)
        {
            FullPathVarRule rule;
            if (m_FullPathVarLookup == null)
            {
                m_FullPathVarLookup = new Dictionary<TableKeyPair, FullPathVarRule>();
                rule = null;
                m_HasSpecialVariantLookups = true;
            }
            else
            {
                m_FullPathVarLookup.TryGetValue(inKey, out rule);
            }

            if (rule == null)
            {
                rule = new FullPathVarRule();
                m_FullPathVarLookup.Add(inKey, rule);
            }
            
            return rule;
        }

        private TableVarRule MakeTableVarRule(StringHash32 inTableId)
        {
            TableVarRule rule;
            if (m_TableVarLookup == null)
            {
                m_TableVarLookup = new Dictionary<StringHash32, TableVarRule>();
                rule = null;
                m_HasSpecialVariantLookups = true;
            }
            else
            {
                m_TableVarLookup.TryGetValue(inTableId, out rule);
            }

            if (rule == null)
            {
                rule = new TableVarRule();
                m_TableVarLookup.Add(inTableId, rule);
            }
            
            return rule;
        }

        #endregion // Var Lookup

        #region IVariantResolver

        public void RemapKey(ref TableKeyPair ioKey)
        {
            if (m_HasRemaps)
            {
                if (m_FullKeyRemap != null)
                {
                    TableKeyPair fullRemap;
                    if (m_FullKeyRemap.TryGetValue(ioKey, out fullRemap))
                    {
                        ioKey = fullRemap;
                        return;
                    }
                }

                if (m_TableIdRemap != null)
                {
                    StringHash32 tableRemap;
                    if (m_TableIdRemap.TryGetValue(ioKey.TableId, out tableRemap))
                    {
                        ioKey.TableId = tableRemap;
                    }
                }

                if (m_VariableIdRemap != null)
                {
                    StringHash32 varRemap;
                    if (m_VariableIdRemap.TryGetValue(ioKey.TableId, out varRemap))
                    {
                        ioKey.VariableId = varRemap;
                    }
                }
            }

            if (m_Base != null)
            {
                m_Base.RemapKey(ref ioKey);
            }
        }

        public bool TryGetTable(object inContext, StringHash32 inTableId, out VariantTable outTable)
        {
            if (inTableId.IsEmpty && m_DefaultTable != null && m_DefaultTable.IsActive())
            {
                outTable = m_DefaultTable.Resolve(inContext);
                return true;
            }

            if (m_TableLookup != null)
            {
                TableRule tableRule;
                if (m_TableLookup.TryGetValue(inTableId, out tableRule))
                {
                    outTable = tableRule.Resolve(inContext);
                    return true;
                }
            }

            if (m_GetTableWithContextFallback != null)
            {
                outTable = m_GetTableWithContextFallback(inTableId, inContext);
                if (outTable != null)
                    return true;
            }

            if (m_GetTableFallback != null)
            {
                outTable = m_GetTableFallback(inTableId);
                if (outTable != null)
                    return true;
            }

            if (m_Base != null)
            {
                return m_Base.TryGetTable(inContext, inTableId, out outTable);
            }

            outTable = null;
            return false;
        }

        public bool TryGetVariant(object inContext, TableKeyPair inKey, out Variant outVariant)
        {
            if (m_HasSpecialVariantLookups)
            {
                if (m_FullPathVarLookup != null)
                {
                    FullPathVarRule rule;
                    if (m_FullPathVarLookup.TryGetValue(inKey, out rule))
                    {
                        outVariant = rule.Resolve(inContext);
                        return true;
                    }
                }

                if (m_TableVarLookup != null)
                {
                    TableVarRule rule;
                    if (m_TableVarLookup.TryGetValue(inKey.TableId, out rule))
                    {
                        outVariant = rule.Resolve(inKey.VariableId, inContext);
                        return true;
                    }
                }

                if (m_GetVarWithContextFallback != null)
                {
                    outVariant = m_GetVarWithContextFallback(inKey, inContext);
                    return true;
                }

                if (m_GetVarFallback != null)
                {
                    outVariant = m_GetVarFallback(inKey);
                    return true;
                }
            }

            if (m_Base != null)
            {
                return m_Base.TryGetVariant(inContext, inKey, out outVariant);
            }
            
            outVariant = Variant.Null;
            return false;
        }

        #endregion // IVariantResolver
    }
}