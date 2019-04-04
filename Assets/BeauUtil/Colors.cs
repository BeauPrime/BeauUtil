/*
 * Copyright (C) 2017-2019. Filament Games, LLC. All rights reserved.
 * Author:  Alex Beauchesne
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
	}
}

