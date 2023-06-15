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
using BeauUtil.Variants;

namespace BeauUtil
{
    /// <summary>
    /// Argument casting utilities.
    /// </summary>
    static public class CastableArgument
    {
        static CastableArgument()
        {
            // default variant conversions
            RegisterConverter<Variant, StringHash32>((v) => v.AsStringHash());
            RegisterConverter<Variant, int>((v) => v.AsInt());
            RegisterConverter<Variant, float>((v) => v.AsFloat());
            RegisterConverter<Variant, uint>((v) => v.AsUInt());
            RegisterConverter<Variant, bool>((v) => v.AsBool());

            // default NonBoxedValue conversions
            RegisterConverter<NonBoxedValue, int>((v) => v.AsInt32());
            RegisterConverter<NonBoxedValue, bool>((v) => v.AsBool());
            RegisterConverter<NonBoxedValue, StringHash32>((v) => v.AsStringHash());
            RegisterConverter<NonBoxedValue, string>((v) => v.AsString());
            RegisterConverter<NonBoxedValue, StringSlice>((v) => v.AsStringSlice());
            RegisterConverter<NonBoxedValue, float>((v) => v.AsFloat());
            RegisterConverter<NonBoxedValue, uint>((v) => v.AsUInt32());
            RegisterConverter<NonBoxedValue, object>((v) => v.AsObject());
        }

        static public void RegisterConverter<TInput, TOutput>(CastableArgumentConverter<TInput, TOutput> inConverterInstance)
        {
            Cache<TInput, TOutput>.ConverterInstance = inConverterInstance;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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