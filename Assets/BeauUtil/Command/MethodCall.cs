/*
 * Copyright (C) 2017-2021. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    21 June 2021
 * 
 * File:    MethodCall.cs
 * Purpose: Struct representing a method call.
 */

using System;
using System.Diagnostics;

namespace BeauUtil
{
    /// <summary>
    /// Method call arguments for a IMethodCache.
    /// </summary>
    [DebuggerDisplay("{ToDebugString()}")]
    public struct MethodCall : IEquatable<MethodCall>, IDebugString
    {
        public StringHash32 Id;
        public StringSlice Args;

        #region Interfaces

        public bool Equals(MethodCall other)
        {
            return Id == other.Id && Args == other.Args;
        }

        public string ToDebugString()
        {
            return string.Format("{0}({1})", Id.ToDebugString(), Args);
        }

        #endregion // Interfaces

        #region Overrides

        public override bool Equals(object obj)
        {
            if (obj is MethodCall)
                return Equals((MethodCall) obj);
            
            return false;
        }

        public override string ToString()
        {
            return string.Format("{0}({1})", Id.ToString(), Args);
        }

        public override int GetHashCode()
        {
            return (Id.GetHashCode() << 5) ^ (Args.GetHashCode());
        }

        #endregion // Overrides
    
        /// <summary>
        /// Attempts to parse a method call with format MethodId(Args)
        /// </summary>
        static public bool TryParse(StringSlice inData, out MethodCall outMethodCall)
        {
            int openParenIdx = inData.IndexOf('(');
            int closeParenIdx = inData.LastIndexOf(')');

            if (openParenIdx <= 0 || closeParenIdx <= 0 || closeParenIdx <= openParenIdx)
            {
                outMethodCall = default(MethodCall);
                return false;
            }

            StringSlice methodSlice = inData.Substring(0, openParenIdx).TrimEnd();
            if (methodSlice.Length == 0)
            {
                outMethodCall = default(MethodCall);
                return false;
            }

            StringSlice afterMethod = inData.Substring(closeParenIdx + 1);
            if (!afterMethod.IsWhitespace)
            {
                outMethodCall = default(MethodCall);
                return false;
            }

            int argsLength = closeParenIdx - 1 - openParenIdx;

            outMethodCall.Id = methodSlice.Hash32();
            outMethodCall.Args = inData.Substring(openParenIdx + 1, argsLength).Trim();
            return true;
        }
    }
}