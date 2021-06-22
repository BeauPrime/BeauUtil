/*
 * Copyright (C) 2017-2021. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    17 Feb 2021
 * 
 * File:    IMethodCache.cs
 * Purpose: Interface for invocation of methods.
 */

using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace BeauUtil
{
    /// <summary>
    /// Cache for object commands
    /// </summary>
    public interface IMethodCache
    {
        IStringConverter StringConverter { get; }
        
        void Load(Type inType);
        void LoadStatic();

        bool TryStaticInvoke(StringHash32 inId, StringSlice inArguments, out object outResult);
        bool TryInvoke(object inTarget, StringHash32 inId, StringSlice inArguments, out object outResult);
    }

    /// <summary>
    /// Extension methods for the method cache.
    /// </summary>
    static public class MethodCacheExtensions
    {
        static public bool TryStaticInvoke(this IMethodCache inCache, MethodCall inCall, out object outResult)
        {
            return inCache.TryStaticInvoke(inCall.Id, inCall.Args, out outResult);
        }

        static public bool TryInvoke(this IMethodCache inCache, object inTarget, MethodCall inCall, out object outResult)
        {
            return inCache.TryInvoke(inTarget, inCall.Id, inCall.Args, out outResult);
        }

        static public object StaticInvoke(this IMethodCache inCache, StringHash32 inId, StringSlice inArguments)
        {
            object result;
            inCache.TryStaticInvoke(inId, inArguments, out result);
            return result;
        }

        static public object Invoke(this IMethodCache inCache, object inTarget, StringHash32 inId, StringSlice inArguments)
        {
            object result;
            inCache.TryInvoke(inTarget, inId, inArguments, out result);
            return result;
        }

        static public object StaticInvoke(this IMethodCache inCache, MethodCall inCall)
        {
            object result;
            inCache.TryStaticInvoke(inCall.Id, inCall.Args, out result);
            return result;
        }

        static public object Invoke(this IMethodCache inCache, object inTarget, MethodCall inCall)
        {
            object result;
            inCache.TryInvoke(inTarget, inCall.Id, inCall.Args, out result);
            return result;
        }
    }
}