/*
 * Copyright (C) 2017-2021. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    17 Feb 2021
 * 
 * File:    IMethodCache.cs
 * Purpose: Interface for invocation of methods.
 */

using System;
using System.Collections;
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
        void LoadStatic(IEnumerable<Assembly> inAssemblies);
        void LoadStaticFromSet(SerializedAttributeSet inAttributeSet);
        IEnumerator LoadStaticAsync();
        IEnumerator LoadStaticAsync(IEnumerable<Assembly> inAssemblies);
        IEnumerator LoadStaticFromSetAsync(SerializedAttributeSet inAttributeSet);

        bool Has(StringHash32 inId);
        bool HasStatic(StringHash32 inId);
        bool HasInstance(StringHash32 inId);

        bool TryStaticInvoke(StringHash32 inId, StringSlice inArguments, object inContext, out NonBoxedValue outResult);
        bool TryInvoke(object inTarget, StringHash32 inId, StringSlice inArguments, object inContext, out NonBoxedValue outResult);
    }

    /// <summary>
    /// Extension methods for the method cache.
    /// </summary>
    static public class MethodCacheExtensions
    {
        static public bool TryStaticInvoke(this IMethodCache inCache, MethodCall inCall, object inContext, out NonBoxedValue outResult)
        {
            return inCache.TryStaticInvoke(inCall.Id, inCall.Args, inContext, out outResult);
        }

        static public bool TryInvoke(this IMethodCache inCache, object inTarget, MethodCall inCall, object inContext, out NonBoxedValue outResult)
        {
            return inCache.TryInvoke(inTarget, inCall.Id, inCall.Args, inContext, out outResult);
        }

        static public NonBoxedValue StaticInvoke(this IMethodCache inCache, StringHash32 inId, StringSlice inArguments, object inContext)
        {
            NonBoxedValue result;
            inCache.TryStaticInvoke(inId, inArguments, inContext, out result);
            return result;
        }

        static public NonBoxedValue Invoke(this IMethodCache inCache, object inTarget, StringHash32 inId, StringSlice inArguments, object inContext)
        {
            NonBoxedValue result;
            inCache.TryInvoke(inTarget, inId, inArguments, inContext, out result);
            return result;
        }

        static public NonBoxedValue StaticInvoke(this IMethodCache inCache, MethodCall inCall, object inContext)
        {
            NonBoxedValue result;
            inCache.TryStaticInvoke(inCall.Id, inCall.Args, inContext, out result);
            return result;
        }

        static public NonBoxedValue Invoke(this IMethodCache inCache, object inTarget, MethodCall inCall, object inContext)
        {
            NonBoxedValue result;
            inCache.TryInvoke(inTarget, inCall.Id, inCall.Args, inContext, out result);
            return result;
        }
    }
}