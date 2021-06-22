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
        public VariantOperand Left;
        public VariantCompareOperator Operator;
        public VariantOperand Right;

        /// <summary>
        /// Evaluates this comparison.
        /// </summary>
        public bool Evaluate(IVariantResolver inResolver, object inContext = null, IMethodCache inInvoker = null)
        {
            Variant leftValue, rightValue;
            bool bRetrievedLeft = Left.TryResolve(inResolver, inContext, out leftValue, inInvoker);
            bool bRetrievedRight = Right.TryResolve(inResolver, inContext, out rightValue, inInvoker);

            switch(Operator)
            {
                case VariantCompareOperator.LessThan:
                    return leftValue < rightValue;
                case VariantCompareOperator.LessThanOrEqualTo:
                    return leftValue <= rightValue;
                case VariantCompareOperator.EqualTo:
                    return leftValue == rightValue;
                case VariantCompareOperator.NotEqualTo:
                    return leftValue != rightValue;
                case VariantCompareOperator.GreaterThanOrEqualTo:
                    return leftValue >= rightValue;
                case VariantCompareOperator.GreaterThan:
                    return leftValue > rightValue;

                case VariantCompareOperator.Exists:
                    return bRetrievedLeft;
                case VariantCompareOperator.DoesNotExist:
                    return !bRetrievedLeft;
                case VariantCompareOperator.True:
                    return leftValue.AsBool();
                case VariantCompareOperator.False:
                    return !leftValue.AsBool();

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

            if (!VariantOperand.TryParse(idSlice, out outComparison.Left))
            {
                outComparison = default(VariantComparison);
                return false;
            }

            if (!bRequiresOperand)
            {
                outComparison.Right = default(VariantOperand);
                return true;
            }

            return VariantOperand.TryParse(operandSlice, out outComparison.Right);
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