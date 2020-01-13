/*
 * Copyright (C) 2017-2020. Filament Games, LLC. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    4 April 2019
 * 
 * File:    Colors.cs
 * Purpose: Color utility methods.
 */

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
        static public Color Hex(string inHexCode, Color inDefault)
        {
            string hexString = inHexCode;
            if (hexString[0] != '#')
                hexString = "#" + hexString;
            return HTML(inHexCode, inDefault);
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
    }
}