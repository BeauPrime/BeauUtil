/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    16 Sept 2020
 * 
 * File:    TableKeyPair.cs
 * Purpose: Key pair for looking up a variant from a specific table.
 */

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace BeauUtil.Variants
{
    /// <summary>
    /// Key pair for looking up a variant from a specific table.
    /// </summary>
    [DebuggerDisplay("{ToDebugString()}")]
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct TableKeyPair : IEquatable<TableKeyPair>
    {
        static public readonly string TableOperator = ":";
        static private readonly string StringFormat = "{0}" + TableOperator + "{1}";

        public StringHash TableId;
        public StringHash VariableId;

        public TableKeyPair(StringHash inVariantId)
        {
            TableId = StringHash.Null;
            VariableId = inVariantId;
        }

        public TableKeyPair(StringHash inTableId, StringHash inVariantId)
        {
            TableId = inTableId;
            VariableId = inVariantId;
        }

        public string ToDebugString()
        {
            if (VariableId.IsEmpty)
            {
                return "[null]";
            }

            if (TableId.IsEmpty)
            {
                return VariableId.ToDebugString();
            }

            return string.Format(StringFormat, TableId.ToDebugString(), VariableId.ToDebugString());
        }

        #region Parse

        /// <summary>
        /// Attempts to parse a StringSlice into a Variant Key.
        /// Valid formats are:
        /// <list type="bullet">
        /// <item>variableId</item>
        /// <item>tableId:variableId</item>
        /// <item>tableId[variableId]</item>
        /// </list>
        /// </summary>
        static public bool TryParse(StringSlice inSource, out TableKeyPair outKey)
        {
            if (inSource.IsEmpty)
            {
                outKey = default(TableKeyPair);
                return false;
            }

            inSource = inSource.Trim();

            int lBracketIdx = inSource.IndexOf('[');
            int rBracketIdx = inSource.IndexOf(']');

            if (lBracketIdx >= 0 && rBracketIdx >= 0)
            {
                if (rBracketIdx < lBracketIdx || lBracketIdx == 0)
                {
                    outKey = default(TableKeyPair);
                    return false;
                }

                int length = rBracketIdx - lBracketIdx;
                
                StringSlice tableId = inSource.Substring(0, lBracketIdx).TrimEnd();
                StringSlice variantId = inSource.Substring(lBracketIdx + 1, length).Trim();

                if (!VariantUtils.IsValidIdentifier(tableId) || !VariantUtils.IsValidIdentifier(variantId))
                {
                    outKey = default(TableKeyPair);
                    return false;
                }

                outKey = new TableKeyPair(tableId, variantId);
                return true;
            }

            int operatorIdx = inSource.IndexOf(TableOperator);
            if (operatorIdx >= 0)
            {
                int variantIdx = operatorIdx + TableOperator.Length;
                if (variantIdx >= inSource.Length)
                {
                    outKey = default(TableKeyPair);
                    return false;
                }

                StringSlice tableId = inSource.Substring(0, operatorIdx).TrimEnd();
                StringSlice variantId = inSource.Substring(variantIdx).TrimStart();

                if (!VariantUtils.IsValidIdentifier(tableId) || !VariantUtils.IsValidIdentifier(variantId))
                {
                    outKey = default(TableKeyPair);
                    return false;
                }

                outKey = new TableKeyPair(tableId, variantId);
                return true;
            }

            if (!VariantUtils.IsValidIdentifier(inSource))
            {
                outKey = default(TableKeyPair);
                return false;
            }
            
            outKey = new TableKeyPair(inSource);
            return true;
        }

        /// <summary>
        /// Parses the given data into a tablekeypair.
        /// </summary>
        static public TableKeyPair Parse(StringSlice inSource)
        {
            TableKeyPair pair;
            TryParse(inSource, out pair);
            return pair;
        }

        #endregion // Parse

        #region IEquatable

        public bool Equals(TableKeyPair other)
        {
            return TableId == other.TableId && VariableId == other.VariableId;
        }

        #endregion // IEquatable

        #region Overrides

        public override int GetHashCode()
        {
            return (TableId.GetHashCode() << 3) ^ VariableId.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj is TableKeyPair)
                return Equals((TableKeyPair) obj);

            return false;
        }

        public override string ToString()
        {
            if (VariableId.IsEmpty)
            {
                return string.Empty;
            }

            if (TableId.IsEmpty)
            {
                return VariableId.ToString();
            }

            return string.Format(StringFormat, TableId.ToString(), VariableId.ToString());
        }

        static public bool operator==(TableKeyPair left, TableKeyPair right)
        {
            return left.Equals(right);
        }

        static public bool operator!=(TableKeyPair left, TableKeyPair right)
        {
            return !left.Equals(right);
        }

        #endregion // Overrides
    }
}