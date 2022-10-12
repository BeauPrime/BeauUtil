/*
 * Copyright (C) 2022. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    11 Oct 2022
 * 
 * File:    CastableArgument.cs
 * Purpose: Argument conversion for CastableAction and CastableFunc.
 */

using System;
using System.Runtime.CompilerServices;

namespace BeauUtil
{
    /// <summary>
    /// Argument casting utilities.
    /// </summary>
    static public class CastableArgument
    {
        static public void RegisterConverter<TInput, TOutput>(CastableArgumentConverter<TInput, TOutput> inConverterInstance)
        {
            Cache<TInput, TOutput>.ConverterInstance = inConverterInstance;
        }

        [MethodImpl(256)]
        static public TOutput Cast<TInput, TOutput>(TInput inInput)
        {
            var custom = Cache<TInput, TOutput>.ConverterInstance;
            if (custom != null)
                return custom(inInput);
            return (TOutput) (object) inInput; // brute force hack
        }

        static private class Cache<TInput, TOutput>
        {
            static internal CastableArgumentConverter<TInput, TOutput> ConverterInstance;
        }
    }

    public delegate TOutput CastableArgumentConverter<TInput, TOutput>(TInput inInput);
}