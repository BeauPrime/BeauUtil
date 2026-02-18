/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    16 Sept 2020
 * 
 * File:    VariantModification.cs
 * Purpose: Basic modification of a variant in a table
 */

using System;
using System.Text;
using BeauUtil.Debugger;

namespace BeauUtil.Variants
{
    /// <summary>
    /// Modification of a variant in a table
    /// </summary>
    public struct VariantModification : IDebugString
    {
        public TableKeyPair VariableKey;
        public VariantModifyOperator Operator;
        public VariantOperand Operand;

        /// <summary>
        /// Executes this modification.
        /// </summary>
        public bool Execute(IVariantTableResolver inResolver, object inContext = null, IMethodCache inInvoker = null)
        {
            Variant operandValue;
            if (Operand.TryResolve(inResolver, inContext, out operandValue, inInvoker))
            {
                return inResolver.TryModify(VariableKey, Operator, operandValue);
            }
            
            return false;
        }

        public string ToDebugString()
        {
            switch(Operator)
            {
                case VariantModifyOperator.Set:
                    return string.Format("{0} = {1}", VariableKey.ToDebugString(), Operand.ToDebugString());

                case VariantModifyOperator.Add:
                    return string.Format("{0} + {1}", VariableKey.ToDebugString(), Operand.ToDebugString());

                case VariantModifyOperator.Subtract:
                    return string.Format("{0} - {1}", VariableKey.ToDebugString(), Operand.ToDebugString());

                case VariantModifyOperator.Multiply:
                    return string.Format("{0} * {1}", VariableKey.ToDebugString(), Operand.ToDebugString());

                case VariantModifyOperator.Divide:
                    return string.Format("{0} / {1}", VariableKey.ToDebugString(), Operand.ToDebugString());
                
                default:
                    throw new InvalidOperationException("Unknown operator " + Operator.ToString());
            }
        }

        public void ToDebugString(StringBuilder inBuilder)
        {
            VariableKey.ToDebugString(inBuilder);
            inBuilder.Append(' ');
            switch (Operator)
            {
                case VariantModifyOperator.Set:
                    inBuilder.Append('=');
                    break;

                case VariantModifyOperator.Add:
                    inBuilder.Append('+');
                    break;

                case VariantModifyOperator.Subtract:
                    inBuilder.Append('-');
                    break;

                case VariantModifyOperator.Multiply:
                    inBuilder.Append('*');
                    break;

                case VariantModifyOperator.Divide:
                    inBuilder.Append('/');
                    break;

                default:
                    throw new InvalidOperationException("Unknown operator " + Operator.ToString());
            }
            inBuilder.Append(' ');
            Operand.ToDebugString(inBuilder);
        }

        /// <summary>
        /// Attempts to parse a modification.
        /// </summary>
        static public bool TryParse(StringSlice inData, out VariantModification outModification)
        {
            inData = inData.Trim();

            VariantModifyOperator op = default(VariantModifyOperator);
            int operatorIdx = -1;
            int operatorLength = 0;

            VariantUtils.TryFindOperator(inData, '=', VariantModifyOperator.Set, ref operatorIdx, ref op, ref operatorLength);
            VariantUtils.TryFindOperator(inData, '+', VariantModifyOperator.Add, ref operatorIdx, ref op, ref operatorLength);
            VariantUtils.TryFindOperator(inData, '-', VariantModifyOperator.Subtract, ref operatorIdx, ref op, ref operatorLength);
            VariantUtils.TryFindOperator(inData, '*', VariantModifyOperator.Multiply, ref operatorIdx, ref op, ref operatorLength);
            VariantUtils.TryFindOperator(inData, '/', VariantModifyOperator.Divide, ref operatorIdx, ref op, ref operatorLength);

            if (operatorIdx < 0)
            {
                outModification = default(VariantModification);
                return false;
            }

            outModification.Operator = op;

            StringSlice key = inData.Substring(0, operatorIdx);
            StringSlice operand = inData.Substring(operatorIdx + 1);

            if (!TableKeyPair.TryParse(key, out outModification.VariableKey))
            {
                outModification = default(VariantModification);
                return false;
            }

            return VariantOperand.TryParse(operand, out outModification.Operand);
        }
    }
}