/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    17 Sept 2020
 * 
 * File:    VariantOperand.cs
 * Purpose: Key pair for looking up a variant from a specific table.
 */

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace BeauUtil.Variants
{
    /// <summary>
    /// Operand for a variant operation.
    /// </summary>
    [DebuggerDisplay("{ToDebugString()}")]
    public struct VariantOperand : IEquatable<VariantOperand>, IDebugString
    {
        #region Types

        internal enum VariantOperandType : byte
        {
            Variant,
            TableKey,
            Method
        }

        #endregion // Types

        private readonly VariantOperandType m_Type;
        private readonly Variant m_Value;
        private readonly TableKeyPair m_TableKey;
        private readonly MethodCall m_MethodCall;

        private VariantOperand(VariantOperandType inType)
        {
            m_Type = inType;
            m_Value = default(Variant);
            m_TableKey = default(TableKeyPair);
            m_MethodCall = default(MethodCall);
        }

        public VariantOperand(Variant inValue)
            : this(VariantOperandType.Variant)
        {
            m_Value = inValue;
        }

        public VariantOperand(TableKeyPair inKey)
            : this(VariantOperandType.TableKey)
        {
            m_TableKey = inKey;
        }

        public VariantOperand(MethodCall inMethodCall)
            : this(VariantOperandType.Method)
        {
            m_MethodCall = inMethodCall;
        }

        /// <summary>
        /// Attempts to resolve the value.
        /// </summary>
        public bool TryResolve(IVariantResolver inResolver, object inContext, out Variant outValue, IMethodCache inInvoker = null)
        {
            switch(m_Type)
            {
                case VariantOperandType.TableKey:
                    {
                        return inResolver.TryResolve(inContext, m_TableKey, out outValue);
                    }

                case VariantOperandType.Variant:
                    {
                        outValue = m_Value;
                        return true;
                    }

                case VariantOperandType.Method:
                    {
                        if (inInvoker == null)
                            throw new ArgumentNullException("inInvoker", "No IMethodCache provided - cannot invoke a method call operand");
                        
                        object obj;
                        if (!inInvoker.TryStaticInvoke(m_MethodCall, out obj) || !Variant.TryConvertFrom(obj, out outValue))
                        {
                            outValue = default(Variant);
                            return false;
                        }

                        return true;
                    }

                default:
                    throw new InvalidOperationException("Unknown operand type " + m_Type.ToString());
            }
        }
    
        #region IEquatable

        public bool Equals(VariantOperand inOther)
        {
            if (m_Type != inOther.m_Type)
                return false;

            switch(m_Type)
            {
                case VariantOperandType.TableKey:
                    return m_TableKey.Equals(inOther.m_TableKey);
                case VariantOperandType.Variant:
                    return m_Value.StrictEquals(inOther.m_Value);
            }

            return true;
        }

        #endregion // IEquatable

        #region Overrides

        public override int GetHashCode()
        {
            int hash = m_Type.GetHashCode();

            switch(m_Type)
            {
                case VariantOperandType.Variant:
                    hash = (hash << 5) ^ m_Value.GetHashCode();
                    break;

                case VariantOperandType.TableKey:
                    hash = (hash << 5) ^ m_TableKey.GetHashCode();
                    break;
            }

            return hash;
        }

        public override bool Equals(object obj)
        {
            if (obj is VariantOperand)
                return Equals((VariantOperand) obj);

            return false;
        }

        public override string ToString()
        {
            switch(m_Type)
            {
                case VariantOperandType.Variant:
                    return m_Value.ToString();

                case VariantOperandType.TableKey:
                    return m_TableKey.ToString();

                case VariantOperandType.Method:
                    return m_MethodCall.ToString();

                default:
                    throw new InvalidOperationException("Unknown variant operand type " + m_Type.ToString());
            }
        }

        public string ToDebugString()
        {
            switch(m_Type)
            {
                case VariantOperandType.Variant:
                    return m_Value.ToDebugString();

                case VariantOperandType.TableKey:
                    return m_TableKey.ToDebugString();

                case VariantOperandType.Method:
                    return m_MethodCall.ToDebugString();

                default:
                    throw new InvalidOperationException("Unknown variant operand type " + m_Type.ToString());
            }
        }

        #endregion // Overrides
    
        #region Parse

        /// <summary>
        /// Attempts to parse data into an operand.
        /// </summary>
        static public bool TryParse(StringSlice inData, out VariantOperand outOperand)
        {
            Variant value;
            if (Variant.TryParse(inData, out value))
            {
                outOperand = new VariantOperand(value);
                return true;
            }

            TableKeyPair tableKey;
            if (TableKeyPair.TryParse(inData, out tableKey))
            {
                outOperand = new VariantOperand(tableKey);
                return true;
            }

            MethodCall call;
            if (MethodCall.TryParse(inData, out call))
            {
                outOperand = new VariantOperand(call);
                return true;
            }

            outOperand = default(VariantOperand);
            return false;
        }

        #endregion // Parse
    }
}