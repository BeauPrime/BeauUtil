/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    16 Sept 2020
 * 
 * File:    IVariantResolver.cs
 * Purpose: Resolution for variants.
 */

using System;

namespace BeauUtil.Variants
{
    public interface IVariantTableResolver
    {
        /// <summary>
        /// Attempts to get a variant table from the given table id.
        /// </summary>
        bool TryGetTable(StringHash32 inTableId, out VariantTable outTable);
    }

    static public class VariantResolverExtensions
    {
        /// <summary>
        /// Attempts to resolve a variable.
        /// </summary>
        static public bool TryResolve(this IVariantTableResolver inResolver, TableKeyPair inKey, out Variant outVariant)
        {
            VariantTable table;
            bool bFoundTable = inResolver.TryGetTable(inKey.TableId, out table);
            if (!bFoundTable || table == null)
            {
                UnityEngine.Debug.LogErrorFormat("[IVariantResolver] Unable to retrieve table with id '{0}'", inKey.TableId.ToDebugString());
                outVariant = Variant.Null;
                return false;
            }

            return table.TryLookup(inKey.VariableId, out outVariant);
        }

        /// <summary>
        /// Attempts to apply a modification.
        /// </summary>
        static public bool TryModify(this IVariantTableResolver inResolver, TableKeyPair inKey, VariantModifyOperator inOperator, Variant inVariant)
        {
            VariantTable table;
            bool bRetrieved = inResolver.TryGetTable(inKey.TableId, out table);
            if (!bRetrieved || table == null)
            {
                UnityEngine.Debug.LogErrorFormat("[IVariantResolver] Unable to retrieve table with id '{0}'", inKey.TableId.ToDebugString());
                return false;
            }

            table.Modify(inKey.VariableId, inOperator, inVariant);
            return true;
        }

        /// <summary>
        /// Attempts to apply one or more modifications, described by the given string.
        /// </summary>
        static public bool TryModify(this IVariantTableResolver inResolver, object inContext, StringSlice inModifyData, IMethodCache inInvoker = null)
        {
            if (inModifyData.IsWhitespace)
                return true;
            
            StringSlice.ISplitter splitter = StringUtils.ArgsList.Splitter.Instance;
            bool bSuccess = true;
            VariantModification mod;
            foreach(var group in inModifyData.EnumeratedSplit(splitter, StringSplitOptions.RemoveEmptyEntries))
            {
                if (!VariantModification.TryParse(group, out mod) || !mod.Execute(inResolver, inContext, inInvoker))
                    bSuccess = false;
            }

            return bSuccess;
        }

        /// <summary>
        /// Attempts to evaluate if all conditions described by the given string are true.
        /// </summary>
        static public bool TryEvaluate(this IVariantTableResolver inResolver, object inContext, StringSlice inEvalData, IMethodCache inInvoker = null)
        {
            if (inEvalData.IsWhitespace)
                return true;
            
            StringSlice.ISplitter splitter = StringUtils.ArgsList.Splitter.Instance;
            VariantComparison comp;
            foreach(var group in inEvalData.EnumeratedSplit(splitter, StringSplitOptions.RemoveEmptyEntries))
            {
                if (!VariantComparison.TryParse(group, out comp) || !comp.Evaluate(inResolver, inContext, inInvoker))
                    return false;
            }

            return true;
        }
    }
}