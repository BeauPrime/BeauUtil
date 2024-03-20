/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    4 April 2019
 * 
 * File:    Colors.cs
 * Purpose: Color utility methods.
 */

using Unity.IL2CPP.CompilerServices;
using UnityEngine;

namespace BeauUtil
{
    /// <summary>
    /// Color utilities.
    /// </summary>
    static public class Colors
    {
        /// <summary>
        /// Returns a color derived from a hex code.
        /// </summary>
        static public Color Hex(string inHexCode)
        {
            return Hex(inHexCode, Color.magenta);
        }

        /// <summary>
        /// Returns a color derived from a hex code,
        /// or a default if unable to be parsed.
        /// </summary>
        [Il2CppSetOption(Option.NullChecks, false)]
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        static public Color Hex(string inHexCode, Color inDefault)
        {
            StringSlice str = inHexCode;
            if (str.StartsWith("#"))
                str = str.Substring(1);
            else if (str.StartsWith("0x"))
                str = str.Substring(2);

            if (!StringParser.IsHex(str))
                return inDefault;

            Color32 c = default;
            if (str.Length == 3 || str.Length == 4)
            {
                c.r = (byte) (16 * StringParser.FromHex(str[0])); c.r += c.r;
                c.g = (byte) (16 * StringParser.FromHex(str[1])); c.g += c.g;
                c.b = (byte) (16 * StringParser.FromHex(str[3])); c.b += c.b;
                if (str.Length == 4)
                {
                    c.a = (byte) (16 * StringParser.FromHex(str[4])); c.a += c.a;
                }
                else
                {
                    c.a = 255;
                }
            }
            if (str.Length == 6 || str.Length == 8)
            {
                c.r = (byte) (16 * StringParser.FromHex(str[0]) + StringParser.FromHex(str[1]));
                c.g = (byte) (16 * StringParser.FromHex(str[2]) + StringParser.FromHex(str[3]));
                c.b = (byte) (16 * StringParser.FromHex(str[4]) + StringParser.FromHex(str[5]));
                if (str.Length == 8)
                {
                    c.a = (byte) (16 * StringParser.FromHex(str[6]) + StringParser.FromHex(str[7]));
                }
                else
                {
                    c.a = 255;
                }
            }

            return (Color) c;
        }

        /// <summary>
        /// Returns a color derived from a hex code,
        /// or a default if unable to be parsed.
        /// </summary>
        [Il2CppSetOption(Option.NullChecks, false)]
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        static public Color Hex(StringSlice inHexCode, Color inDefault) {
            StringSlice str = inHexCode;
            if (str.StartsWith("#"))
                str = str.Substring(1);
            else if (str.StartsWith("0x"))
                str = str.Substring(2);

            if (!StringParser.IsHex(str))
                return inDefault;

            Color32 c = default;
            if (str.Length == 3 || str.Length == 4) {
                c.r = (byte) (16 * StringParser.FromHex(str[0]));
                c.r += c.r;
                c.g = (byte) (16 * StringParser.FromHex(str[1]));
                c.g += c.g;
                c.b = (byte) (16 * StringParser.FromHex(str[3]));
                c.b += c.b;
                if (str.Length == 4) {
                    c.a = (byte) (16 * StringParser.FromHex(str[4]));
                    c.a += c.a;
                } else {
                    c.a = 255;
                }
            }
            if (str.Length == 6 || str.Length == 8) {
                c.r = (byte) (16 * StringParser.FromHex(str[0]) + StringParser.FromHex(str[1]));
                c.g = (byte) (16 * StringParser.FromHex(str[2]) + StringParser.FromHex(str[3]));
                c.b = (byte) (16 * StringParser.FromHex(str[4]) + StringParser.FromHex(str[5]));
                if (str.Length == 8) {
                    c.a = (byte) (16 * StringParser.FromHex(str[6]) + StringParser.FromHex(str[7]));
                } else {
                    c.a = 255;
                }
            }

            return (Color) c;
        }

        /// <summary>
        /// Returns a color derived from an HTML color.
        /// </summary>
        static public Color HTML(string inHTML)
        {
            return HTML(inHTML, Color.magenta);
        }

        /// <summary>
        /// Returns a color derived from an HTML color,
        /// or a default if unable to be parsed.
        /// </summary>
        static public Color HTML(string inHTML, Color inDefault)
        {
            Color newColor;
            if (!ColorUtility.TryParseHtmlString(inHTML, out newColor))
                newColor = inDefault;
            return newColor;
        }

        /// <summary>
        /// Returns the color defined by the given RGBA-encoded unsigned integer.
        /// </summary>
        static public Color RGBA(uint inRGBA)
        {
            Color color = default(Color);
            color.a = (byte) ((inRGBA) & 0xFF) / 255f;
            color.b = (byte) ((inRGBA >> 8) & 0xFF) / 255f;
            color.g = (byte) ((inRGBA >> 16) & 0xFF) / 255f;
            color.r = (byte) ((inRGBA >> 24) & 0xFF) / 255f;
            return color;
        }

        /// <summary>
        /// Returns the color defined by the given ARGB-encoded unsigned integer.
        /// </summary>
        static public Color ARGB(uint inARGB)
        {
            Color color = default(Color);
            color.b = (byte) ((inARGB) & 0xFF) / 255f;
            color.g = (byte) ((inARGB >> 8) & 0xFF) / 255f;
            color.r = (byte) ((inARGB >> 16) & 0xFF) / 255f;
            color.a = (byte) ((inARGB >> 24) & 0xFF) / 255f;
            return color;
        }

        /// <summary>
        /// Returns the color defined by the given ABGR-encoded unsigned integer.
        /// </summary>
        static public Color ABGR(uint inARGB)
        {
            Color color = default(Color);
            color.r = (byte) ((inARGB) & 0xFF) / 255f;
            color.g = (byte) ((inARGB >> 8) & 0xFF) / 255f;
            color.b = (byte) ((inARGB >> 16) & 0xFF) / 255f;
            color.a = (byte) ((inARGB >> 24) & 0xFF) / 255f;
            return color;
        }

        /// <summary>
        /// Returns the RGBA unsigned integer value for the color.
        /// </summary>
        static public uint ToRGBA(this Color inColor)
        {
            uint c = (uint) ((byte) (inColor.r * 255) << 24) |
                (uint) ((byte) (inColor.g * 255) << 16) |
                (uint) ((byte) (inColor.b * 255) << 8) |
                (uint) ((byte) (inColor.a * 255));
            return c;
        }

        /// <summary>
        /// Returns the ARGB unsigned integer value for the color.
        /// </summary>
        static public uint ToARGB(this Color inColor)
        {
            uint c = (uint) ((byte) (inColor.a * 255) << 24) |
                (uint) ((byte) (inColor.r * 255) << 16) |
                (uint) ((byte) (inColor.g * 255) << 8) |
                (uint) ((byte) (inColor.b * 255));
            return c;
        }

        /// <summary>
        /// Unpacks the given color into its separate float components.
        /// </summary>
        static public void Unpack(this Color inColor, out float outR, out float outG, out float outB, out float outA)
        {
            outR = inColor.r;
            outG = inColor.g;
            outB = inColor.b;
            outA = inColor.a;
        }

        /// <summary>
        /// Unpacks the given color into its separate byte components.
        /// </summary>
        static public void Unpack(this Color inColor, out byte outR, out byte outG, out byte outB, out byte outA)
        {
            outR = (byte) (inColor.r * 255);
            outG = (byte) (inColor.g * 255);
            outB = (byte) (inColor.b * 255);
            outA = (byte) (inColor.a * 255);
        }

        /// <summary>
        /// Unpacks the given color into its separate byte components.
        /// </summary>
        static public void Unpack(this Color32 inColor, out byte outR, out byte outG, out byte outB, out byte outA)
        {
            outR = inColor.r;
            outG = inColor.g;
            outB = inColor.b;
            outA = inColor.a;
        }

        /// <summary>
        /// Changes the alpha of the given color and returns it.
        /// </summary>
        static public Color WithAlpha(this Color inColor, float inAlpha)
        {
            inColor.a = inAlpha;
            return inColor;
        }

        /// <summary>
        /// Changes the alpha of the given color and returns it.
        /// </summary>
        static public Color32 WithAlpha(this Color32 inColor, byte inAlpha)
        {
            inColor.a = inAlpha;
            return inColor;
        }

        /// <summary>
        /// Returns the inverted color, preserving the alpha channel.
        /// </summary>
        static public Color Invert(this Color inColor)
        {
            return new Color(1 - inColor.r, 1 - inColor.g, 1 - inColor.b, inColor.a);
        }

        /// <summary>
        /// Returns the inverted color, preserving the alpha channel.
        /// </summary>
        static public Color32 Invert(this Color32 inColor)
        {
            return new Color32((byte)(255 - inColor.r), (byte)(255 - inColor.g), (byte)(255 - inColor.b), inColor.a);
        }

        /// <summary>
        /// Returns if two Color32s are equal.
        /// </summary>
        static public bool Equals32(this Color32 inColor32, Color32 inOther)
        {
            unsafe
            {
                uint a = *((uint*) (&inColor32.r));
                uint b = *((uint*) (&inOther.r));
                return a == b;
            }
        }

        /// <summary>
        /// Multiplies two Color32s together.
        /// </summary>
        static public Color32 Multiply(Color32 inA, Color32 inB)
        {
            Color32 result = default(Color32);
            result.r = (byte) (inA.r * inB.r / 255f);
            result.g = (byte) (inA.g * inB.g / 255f);
            result.b = (byte) (inA.b * inB.b / 255f);
            result.a = (byte) (inA.a * inB.a / 255f);
            return result;
        }

        /// <summary>
        /// Returns if the Color32 is different from its maximum value.
        /// </summary>
        static public bool IsNotWhite(this Color32 inColor32)
        {
            return inColor32.r < 255 || inColor32.g < 255 || inColor32.b < 255 || inColor32.a < 255;
        }
    }
}