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
        
        #endregion // Bool

        #region Integers

        /// <summary>
        /// Attempts to parse a string slice into a byte.
        /// </summary>
        static public bool TryParseByte(StringSlice inSlice, out byte outByte)
        {
            inSlice = inSlice.Trim();

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

                if (!char.IsDigit(c))
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
        /// Attempts to parse a string slice into an sbyte.
        /// </summary>
        static public bool TryParseSByte(StringSlice inSlice, out sbyte outSByte)
        {
            inSlice = inSlice.Trim();

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

                if (!char.IsDigit(c))
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
        /// Attempts to parse a string slice into a ushort.
        /// </summary>
        static public bool TryParseUShort(StringSlice inSlice, out ushort outUShort)
        {
            inSlice = inSlice.Trim();

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

                if (!char.IsDigit(c))
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
        /// Attempts to parse a string slice into a short.
        /// </summary>
        static public bool TryParseShort(StringSlice inSlice, out short outShort)
        {
            inSlice = inSlice.Trim();

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

                if (!char.IsDigit(c))
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
        /// Attempts to parse a string slice into a uint.
        /// </summary>
        static public bool TryParseUInt(StringSlice inSlice, out uint outUInt)
        {
            inSlice = inSlice.Trim();

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

                if (!char.IsDigit(c))
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
        /// Attempts to parse a string slice into an int.
        /// </summary>
        static public bool TryParseInt(StringSlice inSlice, out int outInt)
        {
            inSlice = inSlice.Trim();

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

                if (!char.IsDigit(c))
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
        /// Attempts to parse a string slice into a ulong.
        /// </summary>
        static public bool TryParseULong(StringSlice inSlice, out ulong outULong)
        {
            inSlice = inSlice.Trim();

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

                if (!char.IsDigit(c))
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
        /// Attempts to parse a string slice into a long.
        /// </summary>
        static public bool TryParseLong(StringSlice inSlice, out long outLong)
        {
            inSlice = inSlice.Trim();

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

                if (!char.IsDigit(c))
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
                if (TryParseLong(inSlice, out l))
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
                if (TryParseLong(inSlice, out l))
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

        #endregion // Float

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

                if (!char.IsDigit(c))
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
            if (TooLong(inSlice, MaxDigits32))
                return ReadAsSystemFloat;

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

            return bHasDot ? ReadAsDecimalPlace : ReadAsInteger;
        }

        [MethodImpl(256)]
        static private bool IsDigit(char inChar)
        {
            return inChar >= '0' && inChar <= '9';
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
    }
}