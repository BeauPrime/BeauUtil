/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    16 Sept 2020
 * 
 * File:    VariantComparison.cs
 * Purpose: Basic comparison between a variant from a table and a value.
 */

using System;

namespace BeauUtil.Variants
{
    /// <summary>
    /// Comparison between a variable and a value.
    /// </summary>
    public struct VariantComparison
    {
        public TableKeyPair VariableKey;
        public VariantCompareOperator Operator;
        public VariantOperand Operand;

        /// <summary>
        /// Evaluates this comparison.
        /// </summary>
        public bool Evaluate(IVariantResolver inResolver, object inContext = null)
        {
            Variant variableValue, operandValue;
            bool bRetrievedVariable = inResolver.TryResolve(inContext, VariableKey, out variableValue);
            bool bRetrievedOperand = Operand.TryResolve(inResolver, inContext, out operandValue);

            switch(Operator)
            {
                case VariantCompareOperator.LessThan:
                    return variableValue < operandValue;
                case VariantCompareOperator.LessThanOrEqualTo:
                    return variableValue <= operandValue;
                case VariantCompareOperator.EqualTo:
                    return variableValue == operandValue;
                case VariantCompareOperator.NotEqualTo:
                    return variableValue != operandValue;
                case VariantCompareOperator.GreaterThanOrEqualTo:
                    return variableValue >= operandValue;
                case VariantCompareOperator.GreaterThan:
                    return variableValue > operandValue;

                case VariantCompareOperator.Exists:
                    return bRetrievedVariable;
                case VariantCompareOperator.DoesNotExist:
                    return !bRetrievedVariable;
                case VariantCompareOperator.True:
                    return variableValue.AsBool();
                case VariantCompareOperator.False:
                    return !variableValue.AsBool();

                default:
                    throw new InvalidOperationException("Unknown operator " + Operator.ToString());
            }
        }
    
        /// <summary>
        /// Attempts to parse a comparison.
        /// </summary>
        static public bool TryParse(StringSlice inData, out VariantComparison outComparison)
        {
            inData = inData.Trim();

            VariantCompareOperator op = VariantCompareOperator.True;
            int operatorIdx = -1;
            int operatorLength = 0;

            VariantUtils.TryFindOperator(inData, EqualsOperator, VariantCompareOperator.EqualTo, ref operatorIdx, ref op, ref operatorLength);
            VariantUtils.TryFindOperator(inData, NotEqualOperator, VariantCompareOperator.NotEqualTo, ref operatorIdx, ref op, ref operatorLength);
            VariantUtils.TryFindOperator(inData, GreaterThanOrEqualToOperator, VariantCompareOperator.GreaterThanOrEqualTo, ref operatorIdx, ref op, ref operatorLength);
            VariantUtils.TryFindOperator(inData, LessThanOrEqualToOperator, VariantCompareOperator.LessThanOrEqualTo, ref operatorIdx, ref op, ref operatorLength);
            VariantUtils.TryFindOperator(inData, GreaterThanOperator, VariantCompareOperator.GreaterThan, ref operatorIdx, ref op, ref operatorLength);
            VariantUtils.TryFindOperator(inData, LessThanOperator, VariantCompareOperator.LessThan, ref operatorIdx, ref op, ref operatorLength);
            VariantUtils.TryFindOperator(inData, NotOperator, VariantCompareOperator.False, ref operatorIdx, ref op, ref operatorLength);
            VariantUtils.TryFindOperator(inData, ShortEqualsOperator, VariantCompareOperator.EqualTo, ref operatorIdx, ref op, ref operatorLength);
            VariantUtils.TryFindOperator(inData, ExistsOperator, VariantCompareOperator.Exists, ref operatorIdx, ref op, ref operatorLength);
            VariantUtils.TryFindOperator(inData, DoesNotExistOperator, VariantCompareOperator.DoesNotExist, ref operatorIdx, ref op, ref operatorLength);

            if (operatorIdx < 0)
            {
                op = VariantCompareOperator.True;
            }

            outComparison.Operator = op;

            StringSlice idSlice = StringSlice.Empty;
            StringSlice operandSlice = StringSlice.Empty;
            bool bRequiresOperand = true;

            switch(op)
            {
                case VariantCompareOperator.False:
                    {
                        idSlice = inData.Substring(1).TrimStart();
                        bRequiresOperand = false;
                        break;
                    }

                case VariantCompareOperator.True:
                    {
                        idSlice = inData;
                        bRequiresOperand = false;
                        break;
                    }

                case VariantCompareOperator.Exists:
                    {
                        idSlice = inData.Substring(0, operatorIdx).TrimEnd();
                        bRequiresOperand = false;
                        break;
                    }

                case VariantCompareOperator.DoesNotExist:
                    {
                        idSlice = inData.Substring(0, operatorIdx).TrimEnd();
                        bRequiresOperand = false;
                        break;
                    }

                default:
                    {
                        idSlice = inData.Substring(0, operatorIdx).TrimEnd();
                        operandSlice = inData.Substring(operatorIdx + operatorLength).TrimStart();
                        break;
                    }
            }

            if (!TableKeyPair.TryParse(idSlice, out outComparison.VariableKey))
            {
                outComparison = default(VariantComparison);
                return false;
            }

            if (!bRequiresOperand)
            {
                outComparison.Operand = default(VariantOperand);
                return true;
            }

            return VariantOperand.TryParse(operandSlice, out outComparison.Operand);
        }

        private const string EqualsOperator = "==";
        private const char ShortEqualsOperator = '=';
        private const string NotEqualOperator = "!=";
        private const string GreaterThanOrEqualToOperator = ">=";
        private const string LessThanOrEqualToOperator = "<=";
        private const char GreaterThanOperator = '>';
        private const char LessThanOperator = '<';
        private const char NotOperator = '!';
        private const string ExistsOperator = "isDefined";
        private const string DoesNotExistOperator = "notDefined";
    }
}