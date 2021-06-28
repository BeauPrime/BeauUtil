/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    16 Sept 2020
 * 
 * File:    VariantModification.cs
 * Purpose: Basic modification of a variant in a table
 */

using System;
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
        public bool Execute(IVariantResolver inResolver, object inContext = null, IMethodCache inInvoker = null)
        {
            Variant operandValue;
            if (Operand.TryResolve(inResolver, inContext, out operandValue, inInvoker))
            {
                return inResolver.TryModify(inContext, VariableKey, Operator, operandValue);
            }
            
            return false;
        }

        public string ToDebugString()
        {
            switch(Operator)
            {
                case VariantModifyOperator.Set:
                    return Log.Format("{0} = {1}", VariableKey, Operand);

                case VariantModifyOperator.Add:
                    return Log.Format("{0} + {1}", VariableKey, Operand);

                case VariantModifyOperator.Subtract:
                    return Log.Format("{0} - {1}", VariableKey, Operand);

                case VariantModifyOperator.Multiply:
                    return Log.Format("{0} * {1}", VariableKey, Operand);

                case VariantModifyOperator.Divide:
                    return Log.Format("{0} / {1}", VariableKey, Operand);
                
                default:
                    throw new InvalidOperationException("Unknown operator " + Operator.ToString());
            }
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