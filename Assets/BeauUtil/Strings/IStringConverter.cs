/*
 * Copyright (C) 2017-2021. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    17 Feb 2021
 * 
 * File:    IStringConverter.cs
 * Purpose: Interface for facilitating conversions between string and a type.
 */

using System;
using BeauUtil.Variants;

namespace BeauUtil {
    /// <summary>
    /// Interface for facilitating conversions between strings and objects.
    /// </summary>
    public interface IStringConverter
    {
        bool CanConvertTo(Type inType);
        bool TryConvertTo(StringSlice inData, Type inType, object inContext, out NonBoxedValue outObject);
        bool TryConvertToVariant(StringSlice inData, object inContext, out Variant outVariant);
    }

    /// <summary>
    /// Default string conversion interface.
    /// Wrapper for StringParser
    /// </summary>
    public class DefaultStringConverter : IStringConverter
    {
        static public readonly DefaultStringConverter Instance = new DefaultStringConverter();

        public bool CanConvertTo(Type inType)
        {
            return StringParser.CanConvertTo(inType);
        }

        public bool TryConvertTo(StringSlice inData, Type inType, object inContext, out NonBoxedValue outObject)
        {
            return StringParser.TryConvertTo(inData, inType, out outObject);
        }

        public bool TryConvertToVariant(StringSlice inData, object inContext, out Variant outVariant)
        {
            return Variant.TryParse(inData, true, out outVariant);
        }
    }
}