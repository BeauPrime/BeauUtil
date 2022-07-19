/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    28 August 2020
 * 
 * File:    StringParser.cs
 * Purpose: String parsing methods.
 */

using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using BeauUtil.Variants;
using UnityEngine;

namespace BeauUtil
{
    /// <summary>
    /// String parsing methods.
    /// </summary>
    static public class StringParser
    {
        private const char Dot = '.';
        private const char Negative = '-';
        private const char Positive = '+';

        private const int MaxDigits8 = 3;
        private const int MaxDigits16 = 5;
        private const int MaxDigits32 = 10;
        private const int MaxDigits64 = 20;

        private const int MaxDigitsHex64 = 16;

        private const int ReadAsInteger = 0;
        private const int ReadAsDecimalPlace = 1;
        private const int ReadAsSystemFloat = 2;
        private const int DoNotRead = -1;

        #region Bool

        /// <summary>
        /// Attempts to parse a string slice into a bool.
        /// </summary>
        static public bool TryParseBool(StringSlice inSlice, out bool outBool)
        {
            inSlice = inSlice.Trim();

            if (inSlice.Length == 0)
            {
                outBool = false;
                return false;
            }

            if (inSlice.Equals("true", true))
            {
                outBool = true;
                return true;
            }

            if (inSlice.Equals("false", true))
            {
                outBool = false;
                return true;
            }

            outBool = false;
            return false;
        }

        /// <summary>
        /// Parses a string slice into a bool.
        /// If unable to parse, the given default will be used instead.
        /// </summary>
        static public bool ParseBool(StringSlice inSlice, bool inbDefault = false)
        {
            bool result;
            if (!TryParseBool(inSlice, out result))
                result = inbDefault;
            return result;
        }
        
        #endregion // Bool

        #region Integers

        /// <summary>
        /// Attempts to parse a string slice into a byte.
        /// </summary>
        static public bool TryParseByte(StringSlice inSlice, out byte outByte)
        {
            inSlice = inSlice.Trim();

            if (inSlice.StartsWith("0x"))
            {
                ulong hex;
                if (TryParseHex(inSlice.Substring(2), 2, out hex))
                {
                    outByte = (byte) hex;
                    return true;
                }

                outByte = 0;
                return false;
            }

            if (inSlice.Length == 0 || TooLong(inSlice, MaxDigits8))
            {
                outByte = 0;
                return false;
            }

            int accum = 0;
            char c;
            for(int i = 0; i < inSlice.Length; ++i)
            {
                c = inSlice[i];

                if (c == Negative)
                {
                    outByte = 0;
                    return false;
                }
                if (c == Positive)
                {
                    if (i > 0)
                    {
                        outByte = 0;
                        return false;
                    }

                    continue;
                }

                if (!IsDigit(c))
                {
                    outByte = 0;
                    return false;
                }

                accum = (accum * 10) + (c - '0');
            }

            if (accum > byte.MaxValue)
            {
                outByte = 0;
                return false;
            }

            outByte = (byte) accum;
            return true;
        }

        /// <summary>
        /// Parses a string slice into a byte.
        /// If unable to parse, the given default will be used instead.
        /// </summary>
        static public byte ParseByte(StringSlice inSlice, byte inDefault = 0)
        {
            byte result;
            if (!TryParseByte(inSlice, out result))
                result = inDefault;
            return result;
        }

        /// <summary>
        /// Attempts to parse a string slice into an sbyte.
        /// </summary>
        static public bool TryParseSByte(StringSlice inSlice, out sbyte outSByte)
        {
            inSlice = inSlice.Trim();
            
            if (inSlice.StartsWith("0x"))
            {
                ulong hex;
                if (TryParseHex(inSlice.Substring(2), 2, out hex))
                {
                    outSByte = (sbyte) hex;
                    return true;
                }

                outSByte = 0;
                return false;
            }

            if (inSlice.Length == 0 || TooLong(inSlice, MaxDigits8))
            {
                outSByte = 0;
                return false;
            }

            int accum = 0;
            char c;
            int sign = 1;
            for(int i = 0; i < inSlice.Length; ++i)
            {
                c = inSlice[i];

                if (c == Negative)
                {
                    if (i > 0)
                    {
                        outSByte = 0;
                        return false;
                    }

                    sign = -1;
                    continue;
                }
                else if (c == Positive)
                {
                    if (i > 0)
                    {
                        outSByte = 0;
                        return false;
                    }

                    continue;
                }

                if (!IsDigit(c))
                {
                    outSByte = 0;
                    return false;
                }

                accum = (accum * 10) + (c - '0');
            }

            accum *= sign;

            if (accum > sbyte.MaxValue || accum < sbyte.MinValue)
            {
                outSByte = 0;
                return false;
            }

            outSByte = (sbyte) accum;
            return true;
        }

        /// <summary>
        /// Parses a string slice into a sbyte.
        /// If unable to parse, the given default will be used instead.
        /// </summary>
        static public sbyte ParseSByte(StringSlice inSlice, sbyte inDefault = 0)
        {
            sbyte result;
            if (!TryParseSByte(inSlice, out result))
                result = inDefault;
            return result;
        }

        /// <summary>
        /// Attempts to parse a string slice into a ushort.
        /// </summary>
        static public bool TryParseUShort(StringSlice inSlice, out ushort outUShort)
        {
            inSlice = inSlice.Trim();

            if (inSlice.StartsWith("0x"))
            {
                ulong hex;
                if (TryParseHex(inSlice.Substring(2), 4, out hex))
                {
                    outUShort = (ushort) hex;
                    return true;
                }

                outUShort = 0;
                return false;
            }

            if (inSlice.Length == 0 || TooLong(inSlice, MaxDigits16))
            {
                outUShort = 0;
                return false;
            }

            int accum = 0;
            char c;
            for(int i = 0; i < inSlice.Length; ++i)
            {
                c = inSlice[i];

                if (c == Negative)
                {
                    outUShort = 0;
                    return false;
                }
                if (c == Positive)
                {
                    if (i > 0)
                    {
                        outUShort = 0;
                        return false;
                    }

                    continue;
                }

                if (!IsDigit(c))
                {
                    outUShort = 0;
                    return false;
                }

                accum = (accum * 10) + (c - '0');
            }

            if (accum > ushort.MaxValue)
            {
                outUShort = 0;
                return false;
            }

            outUShort = (ushort) accum;
            return true;
        }

        /// <summary>
        /// Parses a string slice into a ushort.
        /// If unable to parse, the given default will be used instead.
        /// </summary>
        static public ushort ParseUShort(StringSlice inSlice, ushort inDefault = 0)
        {
            ushort result;
            if (!TryParseUShort(inSlice, out result))
                result = inDefault;
            return result;
        }

        /// <summary>
        /// Attempts to parse a string slice into a short.
        /// </summary>
        static public bool TryParseShort(StringSlice inSlice, out short outShort)
        {
            inSlice = inSlice.Trim();

            if (inSlice.StartsWith("0x"))
            {
                ulong hex;
                if (TryParseHex(inSlice.Substring(2), 4, out hex))
                {
                    outShort = (short) hex;
                    return true;
                }

                outShort = 0;
                return false;
            }

            if (inSlice.Length == 0 || TooLong(inSlice, MaxDigits16))
            {
                outShort = 0;
                return false;
            }

            int accum = 0;
            char c;
            int sign = 1;
            for(int i = 0; i < inSlice.Length; ++i)
            {
                c = inSlice[i];

                if (c == Negative)
                {
                    if (i > 0)
                    {
                        outShort = 0;
                        return false;
                    }

                    sign = -1;
                    continue;
                }
                else if (c == Positive)
                {
                    if (i > 0)
                    {
                        outShort = 0;
                        return false;
                    }

                    continue;
                }

                if (!IsDigit(c))
                {
                    outShort = 0;
                    return false;
                }

                accum = (accum * 10) + (c - '0');
            }

            accum *= sign;

            if (accum > short.MaxValue || accum < short.MinValue)
            {
                outShort = 0;
                return false;
            }

            outShort = (short) accum;
            return true;
        }

        /// <summary>
        /// Parses a string slice into a short.
        /// If unable to parse, the given default will be used instead.
        /// </summary>
        static public short ParseShort(StringSlice inSlice, short inDefault = 0)
        {
            short result;
            if (!TryParseShort(inSlice, out result))
                result = inDefault;
            return result;
        }

        /// <summary>
        /// Attempts to parse a string slice into a uint.
        /// </summary>
        static public bool TryParseUInt(StringSlice inSlice, out uint outUInt)
        {
            inSlice = inSlice.Trim();

            if (inSlice.StartsWith("0x"))
            {
                ulong hex;
                if (TryParseHex(inSlice.Substring(2), 8, out hex))
                {
                    outUInt = (uint) hex;
                    return true;
                }

                outUInt = 0;
                return false;
            }

            if (inSlice.Length == 0 || TooLong(inSlice, MaxDigits32))
            {
                outUInt = 0;
                return false;
            }

            long accum = 0;
            char c;
            for(int i = 0; i < inSlice.Length; ++i)
            {
                c = inSlice[i];

                if (c == Negative)
                {
                    outUInt = 0;
                    return false;
                }
                if (c == Positive)
                {
                    if (i > 0)
                    {
                        outUInt = 0;
                        return false;
                    }

                    continue;
                }

                if (!IsDigit(c))
                {
                    outUInt = 0;
                    return false;
                }

                accum = (accum * 10) + (long) (c - '0');
            }

            if (accum > uint.MaxValue)
            {
                outUInt = 0;
                return false;
            }

            outUInt = (uint) accum;
            return true;
        }

        /// <summary>
        /// Parses a string slice into a uint.
        /// If unable to parse, the given default will be used instead.
        /// </summary>
        static public uint ParseUInt(StringSlice inSlice, uint inDefault = 0)
        {
            uint result;
            if (!TryParseUInt(inSlice, out result))
                result = inDefault;
            return result;
        }

        /// <summary>
        /// Attempts to parse a string slice into an int.
        /// </summary>
        static public bool TryParseInt(StringSlice inSlice, out int outInt)
        {
            inSlice = inSlice.Trim();

            if (inSlice.StartsWith("0x"))
            {
                ulong hex;
                if (TryParseHex(inSlice.Substring(2), 8, out hex))
                {
                    outInt = (int) hex;
                    return true;
                }

                outInt = 0;
                return false;
            }

            if (inSlice.Length == 0 || TooLong(inSlice, MaxDigits32))
            {
                outInt = 0;
                return false;
            }

            long accum = 0;
            char c;
            int sign = 1;
            for(int i = 0; i < inSlice.Length; ++i)
            {
                c = inSlice[i];

                if (c == Negative)
                {
                    if (i > 0)
                    {
                        outInt = 0;
                        return false;
                    }

                    sign = -1;
                    continue;
                }
                else if (c == Positive)
                {
                    if (i > 0)
                    {
                        outInt = 0;
                        return false;
                    }

                    continue;
                }

                if (!IsDigit(c))
                {
                    outInt = 0;
                    return false;
                }

                accum = (accum * 10) + (c - '0');
            }

            accum *= sign;

            if (accum > int.MaxValue || accum < int.MinValue)
            {
                outInt = 0;
                return false;
            }

            outInt = (int) accum;
            return true;
        }

        /// <summary>
        /// Parses a string slice into an int.
        /// If unable to parse, the given default will be used instead.
        /// </summary>
        static public int ParseInt(StringSlice inSlice, int inDefault = 0)
        {
            int result;
            if (!TryParseInt(inSlice, out result))
                result = inDefault;
            return result;
        }

        /// <summary>
        /// Attempts to parse a string slice into a ulong.
        /// </summary>
        static public bool TryParseULong(StringSlice inSlice, out ulong outULong)
        {
            inSlice = inSlice.Trim();

            if (inSlice.StartsWith("0x"))
            {
                return TryParseHex(inSlice.Substring(2), 16, out outULong);
            }

            if (inSlice.Length == 0 || TooLong(inSlice, MaxDigits64))
            {
                outULong = 0;
                return false;
            }

            decimal accum = 0;
            char c;
            for(int i = 0; i < inSlice.Length; ++i)
            {
                c = inSlice[i];

                if (c == Negative)
                {
                    outULong = 0;
                    return false;
                }
                if (c == Positive)
                {
                    if (i > 0)
                    {
                        outULong = 0;
                        return false;
                    }

                    continue;
                }

                if (!IsDigit(c))
                {
                    outULong = 0;
                    return false;
                }

                accum = (accum * 10) + (ulong) (c - '0');
            }

            if (accum > ulong.MaxValue)
            {
                outULong = 0;
                return false;
            }

            outULong = (uint) accum;
            return true;
        }

        /// <summary>
        /// Parses a string slice into a ulong.
        /// If unable to parse, the given default will be used instead.
        /// </summary>
        static public ulong ParseULong(StringSlice inSlice, ulong inDefault = 0)
        {
            ulong result;
            if (!TryParseULong(inSlice, out result))
                result = inDefault;
            return result;
        }

        /// <summary>
        /// Attempts to parse a string slice into a long.
        /// </summary>
        static public bool TryParseLong(StringSlice inSlice, out long outLong)
        {
            return TryParseLongInternal(inSlice, true, out outLong);
        }

        static private bool TryParseLongInternal(StringSlice inSlice, bool inbCheckHex, out long outLong)
        {
            inSlice = inSlice.Trim();

            if (inbCheckHex && inSlice.StartsWith("0x"))
            {
                ulong hex;
                if (TryParseHex(inSlice.Substring(2), 16, out hex))
                {
                    outLong = (long) hex;
                    return true;
                }

                outLong = 0;
                return false;
            }

            if (inSlice.Length == 0 || TooLong(inSlice, MaxDigits64))
            {
                outLong = 0;
                return false;
            }

            decimal accum = 0;
            char c;
            int sign = 1;
            for(int i = 0; i < inSlice.Length; ++i)
            {
                c = inSlice[i];

                if (c == Negative)
                {
                    if (i > 0)
                    {
                        outLong = 0;
                        return false;
                    }

                    sign = -1;
                    continue;
                }
                else if (c == Positive)
                {
                    if (i > 0)
                    {
                        outLong = 0;
                        return false;
                    }

                    continue;
                }

                if (!IsDigit(c))
                {
                    outLong = 0;
                    return false;
                }

                accum = (accum * 10) + (c - '0');
            }

            accum *= sign;

            if (accum > long.MaxValue || accum < long.MinValue)
            {
                outLong = 0;
                return false;
            }

            outLong = (long) accum;
            return true;
        }

        /// <summary>
        /// Parses a string slice into a long.
        /// If unable to parse, the given default will be used instead.
        /// </summary>
        static public long ParseLong(StringSlice inSlice, long inDefault = 0)
        {
            long result;
            if (!TryParseLong(inSlice, out result))
                result = inDefault;
            return result;
        }

        #endregion // Integers

        #region Float

        /// <summary>
        /// Attempts to parse a string slice into a float.
        /// </summary>
        static public bool TryParseFloat(StringSlice inSlice, out float outFloat)
        {
            inSlice = inSlice.Trim();

            if (inSlice.Length == 0)
            {
                outFloat = 0;
                return false;
            }

            if (inSlice.Equals("Infinity"))
            {
                outFloat = float.PositiveInfinity;
                return true;
            }

            if (inSlice.Equals("-Infinity"))
            {
                outFloat = float.NegativeInfinity;
                return true;
            }

            if (inSlice.Equals("NaN"))
            {
                outFloat = float.NaN;
                return true;
            }

            int evalMode = EvaluateFloatMode(inSlice);
            if (evalMode == DoNotRead)
            {
                outFloat = 0;
                return false;
            }
            else if (evalMode == ReadAsInteger)
            {
                long l;
                if (TryParseLongInternal(inSlice, false, out l))
                {
                    outFloat = (float) l;
                    return true;
                }
            }
            else if (evalMode == ReadAsDecimalPlace)
            {
                double d;
                if (TryParseLongDouble(inSlice, out d))
                {
                    outFloat = (float) d;
                    return true;
                }
            }

            return float.TryParse(inSlice.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out outFloat);
        }

        /// <summary>
        /// Parses a string slice into a float.
        /// If unable to parse, the given default will be used instead.
        /// </summary>
        static public float ParseFloat(StringSlice inSlice, float inDefault = 0)
        {
            float result;
            if (!TryParseFloat(inSlice, out result))
                result = inDefault;
            return result;
        }

        /// <summary>
        /// Attempts to parse a string slice into a double.
        /// </summary>
        static public bool TryParseDouble(StringSlice inSlice, out double outDouble)
        {
            inSlice = inSlice.Trim();

            if (inSlice.Length == 0)
            {
                outDouble = 0;
                return false;
            }

            if (inSlice.Equals("Infinity"))
            {
                outDouble = double.PositiveInfinity;
                return true;
            }

            if (inSlice.Equals("-Infinity"))
            {
                outDouble = double.NegativeInfinity;
                return true;
            }

            if (inSlice.Equals("NaN"))
            {
                outDouble = double.NaN;
                return true;
            }

            int evalMode = EvaluateFloatMode(inSlice);
            if (evalMode == DoNotRead)
            {
                outDouble = 0;
                return false;
            }
            else if (evalMode == ReadAsInteger)
            {
                long l;
                if (TryParseLongInternal(inSlice, false, out l))
                {
                    outDouble = (double) l;
                    return true;
                }
            }
            else if (evalMode == ReadAsDecimalPlace)
            {
                if (TryParseLongDouble(inSlice, out outDouble))
                {
                    return true;
                }
            }
            
            return double.TryParse(inSlice.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out outDouble);
        }

        /// <summary>
        /// Parses a string slice into a double.
        /// If unable to parse, the given default will be used instead.
        /// </summary>
        static public double ParseDouble(StringSlice inSlice, double inDefault = 0)
        {
            double result;
            if (!TryParseDouble(inSlice, out result))
                result = inDefault;
            return result;
        }

        #endregion // Float

        #region Colors

        /// <summary>
        /// Attempts to parse the string slice into a color.
        /// </summary>
        static public bool TryParseColor(StringSlice inSlice, out Color outColor)
        {
            StringSlice colorData = inSlice;
            StringSlice alphaData = StringSlice.Empty;

            int dotIdx = inSlice.IndexOf('.');
            if (dotIdx >= 0)
            {
                colorData = inSlice.Substring(0, dotIdx);
                alphaData = inSlice.Substring(dotIdx + 1);
            }

            Color color = default(Color);
            bool bParsed = false;

            if (colorData.StartsWith('#'))
            {
                ulong hex;
                StringSlice hexString = colorData.Substring(1);
                if (hexString.Length <= 6 && TryParseHex(colorData, 6, out hex))
                {
                    color = Colors.RGBA((uint) hex << 8);
                    bParsed = true;
                }
                else if (TryParseHex(colorData, 8, out hex))
                {
                    color = Colors.RGBA((uint) hex);
                    bParsed = true;
                }
            }

            if (!bParsed)
            {
                bParsed = ColorUtility.TryParseHtmlString(colorData.ToString(), out color);
                if (!bParsed)
                {
                    outColor = default(Color);
                    return false;
                }
            }

            if (!alphaData.IsEmpty)
            {
                float alphaMult;
                if (!TryParseFloat(alphaData, out alphaMult))
                {
                    outColor = default(Color);
                    return false;
                }

                color.a *= alphaMult / 100f;
            }

            outColor = color;
            return bParsed;
        }

        /// <summary>
        /// Parses a string slice into a color.
        /// If unable to parse, the given default will be used instead.
        /// </summary>
        static public Color ParseColor(StringSlice inSlice, Color inDefault = default(Color))
        {
            Color result;
            if (TryParseColor(inSlice, out result))
                result = inDefault;
            return result;
        }

        #endregion // Colors

        #region Convert

        /// <summary>
        /// Attempts to convert a string slice into a value of the given type.
        /// </summary>
        static public bool TryConvertTo(StringSlice inSlice, Type inType, out object outValue)
        {
            if (inType.IsEnum)
            {
                try
                {
                    outValue = Enum.Parse(inType, inSlice.ToString(), false);
                    return true;
                }
                catch
                {
                    outValue = null;
                    return false;
                }
            }

            TypeCode tc = Type.GetTypeCode(inType);

            switch(tc)
            {
                case TypeCode.Boolean:
                    {
                        bool b;
                        if (TryParseBool(inSlice, out b))
                        {
                            outValue = b;
                            return true;
                        }

                        break;
                    }

                case TypeCode.Byte:
                    {
                        byte b;
                        if (TryParseByte(inSlice, out b))
                        {
                            outValue = b;
                            return true;
                        }

                        break;
                    }

                case TypeCode.Char:
                    {
                        if (inSlice.Length > 0)
                        {
                            outValue = inSlice[0];
                            return true;
                        }

                        break;
                    }

                case TypeCode.DateTime:
                    {
                        try
                        {
                            outValue = Convert.ToDateTime(inSlice.ToString());
                            return true;
                        }
                        catch
                        {
                            outValue = null;
                            return false;
                        }
                    }

                case TypeCode.Decimal:
                    {
                        double d;
                        if (TryParseDouble(inSlice, out d))
                        {
                            outValue = (decimal) d;
                            return true;
                        }

                        break;
                    }
                
                case TypeCode.Double:
                    {
                        double d;
                        if (TryParseDouble(inSlice, out d))
                        {
                            outValue = d;
                            return true;
                        }

                        break;
                    }

                case TypeCode.Int16:
                    {
                        short s;
                        if (TryParseShort(inSlice, out s))
                        {
                            outValue = s;
                            return true;
                        }

                        break;
                    }

                case TypeCode.Int32:
                    {
                        int i;
                        if (TryParseInt(inSlice, out i))
                        {
                            outValue = i;
                            return true;
                        }

                        break;
                    }

                case TypeCode.Int64:
                    {
                        long l;
                        if (TryParseLong(inSlice, out l))
                        {
                            outValue = l;
                            return true;
                        }

                        break;
                    }

                case TypeCode.Object:
                    {
                        if (inType == typeof(StringSlice) || inType == typeof(object))
                        {
                            outValue = inSlice;
                            return true;
                        }
                        if (inType == typeof(StringHash32))
                        {
                            StringHash32 hash;
                            if (StringHash32.TryParse(inSlice, out hash))
                            {
                                outValue = hash;
                                return true;
                            }
                        }
                        if (inType == typeof(StringHash64))
                        {
                            StringHash64 hash;
                            if (StringHash64.TryParse(inSlice, out hash))
                            {
                                outValue = hash;
                                return true;
                            }
                        }
                        else if (inType == typeof(Variant))
                        {
                            Variant v;
                            if (Variant.TryParse(inSlice, true, out v))
                            {
                                outValue = v;
                                return true;
                            }
                        }

                        break;
                    }

                case TypeCode.SByte:
                    {
                        sbyte s;
                        if (TryParseSByte(inSlice, out s))
                        {
                            outValue = s;
                            return true;
                        }

                        break;
                    }

                case TypeCode.Single:
                    {
                        float f;
                        if (TryParseFloat(inSlice, out f))
                        {
                            outValue = f;
                            return true;
                        }

                        break;
                    }

                case TypeCode.String:
                    {
                        outValue = inSlice.ToString();
                        return true;
                    }

                case TypeCode.UInt16:
                    {
                        ushort u;
                        if (TryParseUShort(inSlice, out u))
                        {
                            outValue = u;
                            return true;
                        }

                        break;
                    }

                case TypeCode.UInt32:
                    {
                        uint u;
                        if (TryParseUInt(inSlice, out u))
                        {
                            outValue = u;
                            return true;
                        }

                        break;
                    }

                case TypeCode.UInt64:
                    {
                        ulong u;
                        if (TryParseULong(inSlice, out u))
                        {
                            outValue = u;
                            return true;
                        }

                        break;
                    }
            }

            outValue = null;
            return false;
        }

        /// <summary>
        /// Parses a string slice into a ulong.
        /// If unable to parse, the given default will be used instead.
        /// </summary>
        static public object ConvertTo(StringSlice inSlice, Type inType, object inDefault = null)
        {
            object result;
            if (!TryConvertTo(inSlice, inType, out result))
                result = inDefault;
            return result;
        }

        /// <summary>
        /// Parses a string slice into a ulong.
        /// If unable to parse, the given default will be used instead.
        /// </summary>
        static public T ConvertTo<T>(StringSlice inSlice, T inDefault = default(T))
        {
            object result;
            if (!TryConvertTo(inSlice, typeof(T), out result))
                return inDefault;
            return (T) result;
        }

        /// <summary>
        /// Returns if a string is convertible to the given type.
        /// </summary>
        static public bool CanConvertTo(Type inType)
        {
            if (inType.IsEnum)
                return true;

            TypeCode tc = Type.GetTypeCode(inType);

            if (tc == TypeCode.Object)
                return Array.IndexOf(ValidConversionTypes, inType) >= 0;

            return Array.IndexOf(ValidConversionTypeCodes, tc) >= 0;
        }

        /// <summary>
        /// Returns if a string is convertible to the given type.
        /// </summary>
        static public bool CanConvertTo<T>()
        {
            return CanConvertTo(typeof(T));
        }

        static private readonly TypeCode[] ValidConversionTypeCodes = new TypeCode[]
        {
            TypeCode.Boolean, TypeCode.Byte, TypeCode.Char, TypeCode.DateTime, TypeCode.Decimal, TypeCode.Double,
            TypeCode.Int16, TypeCode.Int32, TypeCode.Int64, TypeCode.SByte, TypeCode.Single, TypeCode.String,
            TypeCode.UInt16, TypeCode.UInt32, TypeCode.UInt64
        };

        static private readonly Type[] ValidConversionTypes = new Type[]
        {
            typeof(object), typeof(StringSlice), typeof(StringHash32), typeof(Variant), typeof(UnityEngine.Color)
        };

        #endregion // Convert

        #region Internal

        /// <summary>
        /// Returns the integer value associated with the given hex character.
        /// </summary>
        [MethodImpl(256)]
        static public int FromHex(char inChar)
        {
            inChar = char.ToUpperInvariant(inChar);
            if (IsDigit(inChar))
                return inChar - '0';
            else if (IsHexAlpha(inChar))
                return 10 + inChar - 'A';
            else
                return -1;
        }

        static internal bool IsHex(StringSlice inSlice)
        {
            char c;
            for(int i = 0; i < inSlice.Length; ++i)
            {
                c = char.ToUpperInvariant(inSlice[i]);

                if (!IsDigit(c) && !IsHexAlpha(c))
                {
                    return false;
                }
            }

            return true;
        }

        static internal bool TryParseHex(StringSlice inSlice, int inMaxChars, out ulong outULong)
        {
            if (inSlice.Length <= 0 || inSlice.Length > inMaxChars)
            {
                outULong = 0;
                return false;
            }

            ulong accum = 0;
            char c;
            for(int i = 0; i < inSlice.Length; ++i)
            {
                c = char.ToUpperInvariant(inSlice[i]);

                if (IsDigit(c))
                {
                    accum = (accum << 4) + (ulong) (c - '0');
                }
                else if (IsHexAlpha(c))
                {
                    accum = (accum << 4) + (ulong) (10 + c - 'A');
                }
                else
                {
                    outULong = 0;
                    return false;
                }
            }

            outULong = accum;
            return true;
        }

        static private bool TryParseLongDouble(StringSlice inSlice, out double outDouble)
        {
            decimal accum = 0;
            char c;
            int sign = 1;
            int decimalPoints = 0;
            for(int i = 0; i < inSlice.Length; ++i)
            {
                c = inSlice[i];

                if (c == Negative)
                {
                    if (i > 0)
                    {
                        outDouble = 0;
                        return false;
                    }

                    sign = -1;
                    continue;
                }
                else if (c == Positive)
                {
                    if (i > 0)
                    {
                        outDouble = 0;
                        return false;
                    }

                    continue;
                }
                
                if (c == Dot)
                {
                    if (decimalPoints == 0)
                    {
                        decimalPoints = inSlice.Length - 1 - i;
                        continue;
                    }

                    outDouble = 0;
                    return false;
                }

                if (!IsDigit(c))
                {
                    outDouble = 0;
                    return false;
                }

                accum = (accum * 10) + (c - '0');
            }

            while(--decimalPoints >= 0)
                accum /= 10;

            outDouble = (double) accum * sign;
            return true;
        }

        static private int EvaluateFloatMode(StringSlice inSlice)
        {
            if (inSlice.StartsWith("0x"))
                return ReadAsInteger;

            bool bHasDot = false;
            bool bHasE = false;

            for(int i = 0; i < inSlice.Length; ++i)
            {
                char c = inSlice[i];
                if (c == '.')
                {
                    if (bHasDot)
                        return DoNotRead;

                    bHasDot = true;
                }
                else if (c == 'e' || c == 'E')
                {
                    if (bHasE)
                        return DoNotRead;
                    
                    bHasE = true;
                }
                else
                {
                    if (c == Positive || c == Negative)
                    {
                        if (i > 0)
                            return DoNotRead;

                        continue;
                    }

                    if (!IsDigit(c))
                        return DoNotRead;
                }

                if (bHasDot && bHasE)
                    break;
            }

            if (bHasE)
                return ReadAsSystemFloat;

            if (TooLong(inSlice, MaxDigits32))
                return ReadAsSystemFloat;

            return bHasDot ? ReadAsDecimalPlace : ReadAsInteger;
        }

        [MethodImpl(256)]
        static private bool IsDigit(char inChar)
        {
            return inChar >= '0' && inChar <= '9';
        }

        [MethodImpl(256)]
        static private bool IsHexAlpha(char inChar)
        {
            return inChar >= 'A' && inChar <= 'F';
        }

        [MethodImpl(256)]
        static private bool IsSign(char inChar)
        {
            return inChar == Positive || inChar == Negative;
        }

        static private bool TooLong(StringSlice inSlice, int inMaxLength)
        {
            if (inSlice.Length > 0 && IsSign(inSlice[0]))
                ++inMaxLength;

            return inSlice.Length > inMaxLength;
        }

        #endregion // Internal
    }
}