/*
 * Copyright (C) 2022. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    11 Oct 2022
 * 
 * File:    CastableArgument.cs
 * Purpose: Argument conversion for CastableAction and CastableFunc.
 */

#if UNITY_2021_2_OR_NEWER && !BEAUUTIL_DISABLE_FUNCTION_POINTERS
#define SUPPORTS_FUNCTION_POINTERS
#endif // UNITY_2021_2_OR_NEWER

using System.Runtime.CompilerServices;
using BeauUtil.Variants;
using Unity.IL2CPP.CompilerServices;

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

            // StringHash32 and SerializedHash32
            RegisterConverter<StringHash32, SerializedHash32>((v) => v);
            RegisterConverter<SerializedHash32, StringHash32>((v) => v);

            // String to StringHash32
            RegisterConverter<string, StringHash32>((v) => v);
            RegisterConverter<string, SerializedHash32>((v) => v);
            RegisterConverter<StringSlice, StringHash32>((v) => v);
        }

        /// <summary>
        /// Registers a custom argument converter from the given input type to the given output type.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public void RegisterConverter<TInput, TOutput>(CastableArgumentConverter<TInput, TOutput> inConverterDelegate)
        {
            Cache<TInput, TOutput>.Configure(inConverterDelegate);
        }

#if SUPPORTS_FUNCTION_POINTERS
        /// <summary>
        /// Registers a custom argument converter from the given input type to the given output type.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public unsafe void RegisterConverter<TInput, TOutput>(delegate*<TInput, TOutput> inConverterPtr)
        {
            Cache<TInput, TOutput>.Configure(inConverterPtr);
        }
#endif // SUPPORTS_FUNCTION_POINTERS

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Il2CppSetOption(Option.NullChecks, false)]
        static public TOutput Cast<TInput, TOutput>(TInput inInput)
        {
#if SUPPORTS_FUNCTION_POINTERS
            unsafe {
                return Cache<TInput, TOutput>.ConverterPtr(inInput);
            }
#else
            return Cache<TInput, TOutput>.ConverterDelegate(inInput);
#endif // SUPPORTS_FUNCTION_POINTERS
        }

        static private unsafe class Cache<TInput, TOutput>
        {
            static Cache()
            {
#if SUPPORTS_FUNCTION_POINTERS
                ConverterPtr = &DefaultCast;
#else
                ConverterDelegate = DefaultCast;
#endif // SUPPORTS_FUNCTION_POINTERS
            }

#if SUPPORTS_FUNCTION_POINTERS
            static internal delegate*<TInput, TOutput> ConverterPtr;

            [Il2CppSetOption(Option.NullChecks, false)]
            static private TOutput DelegateCast(TInput inInput)
            {
                return ConverterDelegate(inInput);
            }

            static internal void Configure(delegate*<TInput, TOutput> inPtr)
            {
                ConverterDelegate = null;
                ConverterPtr = inPtr != null ? inPtr : &DefaultCast;
            }
#endif // SUPPORTS_FUNCTION_POINTERS

            static internal CastableArgumentConverter<TInput, TOutput> ConverterDelegate;

            static internal void Configure(CastableArgumentConverter<TInput, TOutput> inDelegate)
            {
#if SUPPORTS_FUNCTION_POINTERS
                if (inDelegate != null)
                {
                    ConverterPtr = &DelegateCast;
                    ConverterDelegate = inDelegate;
                } else
                {
                    ConverterPtr = &DefaultCast;
                    ConverterDelegate = null;
                }
#else
                ConverterDelegate = inDelegate ?? DefaultCast;
#endif // SUPPORTS_FUNCTION_POINTERS
            }

            static private TOutput DefaultCast(TInput inInput)
            {
                return (TOutput) (object) inInput; // brute force hack
            }
        }
    }

    public delegate TOutput CastableArgumentConverter<TInput, TOutput>(TInput inInput);
}