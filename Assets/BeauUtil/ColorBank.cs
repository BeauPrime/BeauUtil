// This file contains source code from MonoGame
// see MONOGAME-LICENSE.txt for licensing info

using System.Runtime.CompilerServices;
using UnityEngine;

namespace BeauUtil
{
    /// <summary>
    /// Bank of predefined colors.
    /// </summary>
    static public class ColorBank
    {
        #region Initialization

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static private Color DecodeABGR(uint inValue)
        {
            return Colors.ABGR(inValue);
        }

        static ColorBank()
        {
            TransparentBlack = DecodeABGR(0);
            Transparent = DecodeABGR(0);
            AliceBlue = DecodeABGR(0xfffff8f0);
            AntiqueWhite = DecodeABGR(0xffd7ebfa);
            Aqua = DecodeABGR(0xffffff00);
            Aquamarine = DecodeABGR(0xffd4ff7f);
            Azure = DecodeABGR(0xfffffff0);
            Beige = DecodeABGR(0xffdcf5f5);
            Bisque = DecodeABGR(0xffc4e4ff);
            Black = DecodeABGR(0xff000000);
            BlanchedAlmond = DecodeABGR(0xffcdebff);
            Blue = DecodeABGR(0xffff0000);
            BlueViolet = DecodeABGR(0xffe22b8a);
            Brown = DecodeABGR(0xff2a2aa5);
            BurlyWood = DecodeABGR(0xff87b8de);
            CadetBlue = DecodeABGR(0xffa09e5f);
            Chartreuse = DecodeABGR(0xff00ff7f);
            Chocolate = DecodeABGR(0xff1e69d2);
            Coral = DecodeABGR(0xff507fff);
            CornflowerBlue = DecodeABGR(0xffed9564);
            Cornsilk = DecodeABGR(0xffdcf8ff);
            Crimson = DecodeABGR(0xff3c14dc);
            Cyan = DecodeABGR(0xffffff00);
            DarkBlue = DecodeABGR(0xff8b0000);
            DarkCyan = DecodeABGR(0xff8b8b00);
            DarkGoldenrod = DecodeABGR(0xff0b86b8);
            DarkGray = DecodeABGR(0xffa9a9a9);
            DarkGreen = DecodeABGR(0xff006400);
            DarkKhaki = DecodeABGR(0xff6bb7bd);
            DarkMagenta = DecodeABGR(0xff8b008b);
            DarkOliveGreen = DecodeABGR(0xff2f6b55);
            DarkOrange = DecodeABGR(0xff008cff);
            DarkOrchid = DecodeABGR(0xffcc3299);
            DarkRed = DecodeABGR(0xff00008b);
            DarkSalmon = DecodeABGR(0xff7a96e9);
            DarkSeaGreen = DecodeABGR(0xff8bbc8f);
            DarkSlateBlue = DecodeABGR(0xff8b3d48);
            DarkSlateGray = DecodeABGR(0xff4f4f2f);
            DarkTurquoise = DecodeABGR(0xffd1ce00);
            DarkViolet = DecodeABGR(0xffd30094);
            DeepPink = DecodeABGR(0xff9314ff);
            DeepSkyBlue = DecodeABGR(0xffffbf00);
            DimGray = DecodeABGR(0xff696969);
            DodgerBlue = DecodeABGR(0xffff901e);
            Firebrick = DecodeABGR(0xff2222b2);
            FloralWhite = DecodeABGR(0xfff0faff);
            ForestGreen = DecodeABGR(0xff228b22);
            Fuchsia = DecodeABGR(0xffff00ff);
            Gainsboro = DecodeABGR(0xffdcdcdc);
            GhostWhite = DecodeABGR(0xfffff8f8);
            Gold = DecodeABGR(0xff00d7ff);
            Goldenrod = DecodeABGR(0xff20a5da);
            Gray = DecodeABGR(0xff808080);
            Green = DecodeABGR(0xff008000);
            GreenYellow = DecodeABGR(0xff2fffad);
            Honeydew = DecodeABGR(0xfff0fff0);
            HotPink = DecodeABGR(0xffb469ff);
            IndianRed = DecodeABGR(0xff5c5ccd);
            Indigo = DecodeABGR(0xff82004b);
            Ivory = DecodeABGR(0xfff0ffff);
            Khaki = DecodeABGR(0xff8ce6f0);
            Lavender = DecodeABGR(0xfffae6e6);
            LavenderBlush = DecodeABGR(0xfff5f0ff);
            LawnGreen = DecodeABGR(0xff00fc7c);
            LemonChiffon = DecodeABGR(0xffcdfaff);
            LightBlue = DecodeABGR(0xffe6d8ad);
            LightCoral = DecodeABGR(0xff8080f0);
            LightCyan = DecodeABGR(0xffffffe0);
            LightGoldenrodYellow = DecodeABGR(0xffd2fafa);
            LightGray = DecodeABGR(0xffd3d3d3);
            LightGreen = DecodeABGR(0xff90ee90);
            LightPink = DecodeABGR(0xffc1b6ff);
            LightSalmon = DecodeABGR(0xff7aa0ff);
            LightSeaGreen = DecodeABGR(0xffaab220);
            LightSkyBlue = DecodeABGR(0xffface87);
            LightSlateGray = DecodeABGR(0xff998877);
            LightSteelBlue = DecodeABGR(0xffdec4b0);
            LightYellow = DecodeABGR(0xffe0ffff);
            Lime = DecodeABGR(0xff00ff00);
            LimeGreen = DecodeABGR(0xff32cd32);
            Linen = DecodeABGR(0xffe6f0fa);
            Magenta = DecodeABGR(0xffff00ff);
            Maroon = DecodeABGR(0xff000080);
            MediumAquamarine = DecodeABGR(0xffaacd66);
            MediumBlue = DecodeABGR(0xffcd0000);
            MediumOrchid = DecodeABGR(0xffd355ba);
            MediumPurple = DecodeABGR(0xffdb7093);
            MediumSeaGreen = DecodeABGR(0xff71b33c);
            MediumSlateBlue = DecodeABGR(0xffee687b);
            MediumSpringGreen = DecodeABGR(0xff9afa00);
            MediumTurquoise = DecodeABGR(0xffccd148);
            MediumVioletRed = DecodeABGR(0xff8515c7);
            MidnightBlue = DecodeABGR(0xff701919);
            MintCream = DecodeABGR(0xfffafff5);
            MistyRose = DecodeABGR(0xffe1e4ff);
            Moccasin = DecodeABGR(0xffb5e4ff);
            MonoGameOrange = DecodeABGR(0xff003ce7);
            NavajoWhite = DecodeABGR(0xffaddeff);
            Navy = DecodeABGR(0xff800000);
            OldLace = DecodeABGR(0xffe6f5fd);
            Olive = DecodeABGR(0xff008080);
            OliveDrab = DecodeABGR(0xff238e6b);
            Orange = DecodeABGR(0xff00a5ff);
            OrangeRed = DecodeABGR(0xff0045ff);
            Orchid = DecodeABGR(0xffd670da);
            PaleGoldenrod = DecodeABGR(0xffaae8ee);
            PaleGreen = DecodeABGR(0xff98fb98);
            PaleTurquoise = DecodeABGR(0xffeeeeaf);
            PaleVioletRed = DecodeABGR(0xff9370db);
            PapayaWhip = DecodeABGR(0xffd5efff);
            PeachPuff = DecodeABGR(0xffb9daff);
            Peru = DecodeABGR(0xff3f85cd);
            Pink = DecodeABGR(0xffcbc0ff);
            Plum = DecodeABGR(0xffdda0dd);
            PowderBlue = DecodeABGR(0xffe6e0b0);
            Purple = DecodeABGR(0xff800080);
            Red = DecodeABGR(0xff0000ff);
            RosyBrown = DecodeABGR(0xff8f8fbc);
            RoyalBlue = DecodeABGR(0xffe16941);
            SaddleBrown = DecodeABGR(0xff13458b);
            Salmon = DecodeABGR(0xff7280fa);
            SandyBrown = DecodeABGR(0xff60a4f4);
            SeaGreen = DecodeABGR(0xff578b2e);
            SeaShell = DecodeABGR(0xffeef5ff);
            Sienna = DecodeABGR(0xff2d52a0);
            Silver = DecodeABGR(0xffc0c0c0);
            SkyBlue = DecodeABGR(0xffebce87);
            SlateBlue = DecodeABGR(0xffcd5a6a);
            SlateGray = DecodeABGR(0xff908070);
            Snow = DecodeABGR(0xfffafaff);
            SpringGreen = DecodeABGR(0xff7fff00);
            SteelBlue = DecodeABGR(0xffb48246);
            Tan = DecodeABGR(0xff8cb4d2);
            Teal = DecodeABGR(0xff808000);
            Thistle = DecodeABGR(0xffd8bfd8);
            Tomato = DecodeABGR(0xff4763ff);
            Turquoise = DecodeABGR(0xffd0e040);
            Violet = DecodeABGR(0xffee82ee);
            Wheat = DecodeABGR(0xffb3def5);
            White = DecodeABGR(uint.MaxValue);
            WhiteSmoke = DecodeABGR(0xfff5f5f5);
            Yellow = DecodeABGR(0xff00ffff);
            YellowGreen = DecodeABGR(0xff32cd9a);
        }

        #endregion // Initialization

        #region Color Bank

        /// <summary>
        /// TransparentBlack color (R:0,G:0,B:0,A:0).
        /// </summary>
        public static Color TransparentBlack
        {
            get;
            private set;
        }

        /// <summary>
        /// Transparent color (R:0,G:0,B:0,A:0).
        /// </summary>
        public static Color Transparent
        {
            get;
            private set;
        }

        /// <summary>
        /// AliceBlue color (R:240,G:248,B:255,A:255).
        /// </summary>
        public static Color AliceBlue
        {
            get;
            private set;
        }

        /// <summary>
        /// AntiqueWhite color (R:250,G:235,B:215,A:255).
        /// </summary>
        public static Color AntiqueWhite
        {
            get;
            private set;
        }

        /// <summary>
        /// Aqua color (R:0,G:255,B:255,A:255).
        /// </summary>
        public static Color Aqua
        {
            get;
            private set;
        }

        /// <summary>
        /// Aquamarine color (R:127,G:255,B:212,A:255).
        /// </summary>
        public static Color Aquamarine
        {
            get;
            private set;
        }

        /// <summary>
        /// Azure color (R:240,G:255,B:255,A:255).
        /// </summary>
        public static Color Azure
        {
            get;
            private set;
        }

        /// <summary>
        /// Beige color (R:245,G:245,B:220,A:255).
        /// </summary>
        public static Color Beige
        {
            get;
            private set;
        }

        /// <summary>
        /// Bisque color (R:255,G:228,B:196,A:255).
        /// </summary>
        public static Color Bisque
        {
            get;
            private set;
        }

        /// <summary>
        /// Black color (R:0,G:0,B:0,A:255).
        /// </summary>
        public static Color Black
        {
            get;
            private set;
        }

        /// <summary>
        /// BlanchedAlmond color (R:255,G:235,B:205,A:255).
        /// </summary>
        public static Color BlanchedAlmond
        {
            get;
            private set;
        }

        /// <summary>
        /// Blue color (R:0,G:0,B:255,A:255).
        /// </summary>
        public static Color Blue
        {
            get;
            private set;
        }

        /// <summary>
        /// BlueViolet color (R:138,G:43,B:226,A:255).
        /// </summary>
        public static Color BlueViolet
        {
            get;
            private set;
        }

        /// <summary>
        /// Brown color (R:165,G:42,B:42,A:255).
        /// </summary>
        public static Color Brown
        {
            get;
            private set;
        }

        /// <summary>
        /// BurlyWood color (R:222,G:184,B:135,A:255).
        /// </summary>
        public static Color BurlyWood
        {
            get;
            private set;
        }

        /// <summary>
        /// CadetBlue color (R:95,G:158,B:160,A:255).
        /// </summary>
        public static Color CadetBlue
        {
            get;
            private set;
        }

        /// <summary>
        /// Chartreuse color (R:127,G:255,B:0,A:255).
        /// </summary>
        public static Color Chartreuse
        {
            get;
            private set;
        }

        /// <summary>
        /// Chocolate color (R:210,G:105,B:30,A:255).
        /// </summary>
        public static Color Chocolate
        {
            get;
            private set;
        }

        /// <summary>
        /// Coral color (R:255,G:127,B:80,A:255).
        /// </summary>
        public static Color Coral
        {
            get;
            private set;
        }

        /// <summary>
        /// CornflowerBlue color (R:100,G:149,B:237,A:255).
        /// </summary>
        public static Color CornflowerBlue
        {
            get;
            private set;
        }

        /// <summary>
        /// Cornsilk color (R:255,G:248,B:220,A:255).
        /// </summary>
        public static Color Cornsilk
        {
            get;
            private set;
        }

        /// <summary>
        /// Crimson color (R:220,G:20,B:60,A:255).
        /// </summary>
        public static Color Crimson
        {
            get;
            private set;
        }

        /// <summary>
        /// Cyan color (R:0,G:255,B:255,A:255).
        /// </summary>
        public static Color Cyan
        {
            get;
            private set;
        }

        /// <summary>
        /// DarkBlue color (R:0,G:0,B:139,A:255).
        /// </summary>
        public static Color DarkBlue
        {
            get;
            private set;
        }

        /// <summary>
        /// DarkCyan color (R:0,G:139,B:139,A:255).
        /// </summary>
        public static Color DarkCyan
        {
            get;
            private set;
        }

        /// <summary>
        /// DarkGoldenrod color (R:184,G:134,B:11,A:255).
        /// </summary>
        public static Color DarkGoldenrod
        {
            get;
            private set;
        }

        /// <summary>
        /// DarkGray color (R:169,G:169,B:169,A:255).
        /// </summary>
        public static Color DarkGray
        {
            get;
            private set;
        }

        /// <summary>
        /// DarkGreen color (R:0,G:100,B:0,A:255).
        /// </summary>
        public static Color DarkGreen
        {
            get;
            private set;
        }

        /// <summary>
        /// DarkKhaki color (R:189,G:183,B:107,A:255).
        /// </summary>
        public static Color DarkKhaki
        {
            get;
            private set;
        }

        /// <summary>
        /// DarkMagenta color (R:139,G:0,B:139,A:255).
        /// </summary>
        public static Color DarkMagenta
        {
            get;
            private set;
        }

        /// <summary>
        /// DarkOliveGreen color (R:85,G:107,B:47,A:255).
        /// </summary>
        public static Color DarkOliveGreen
        {
            get;
            private set;
        }

        /// <summary>
        /// DarkOrange color (R:255,G:140,B:0,A:255).
        /// </summary>
        public static Color DarkOrange
        {
            get;
            private set;
        }

        /// <summary>
        /// DarkOrchid color (R:153,G:50,B:204,A:255).
        /// </summary>
        public static Color DarkOrchid
        {
            get;
            private set;
        }

        /// <summary>
        /// DarkRed color (R:139,G:0,B:0,A:255).
        /// </summary>
        public static Color DarkRed
        {
            get;
            private set;
        }

        /// <summary>
        /// DarkSalmon color (R:233,G:150,B:122,A:255).
        /// </summary>
        public static Color DarkSalmon
        {
            get;
            private set;
        }

        /// <summary>
        /// DarkSeaGreen color (R:143,G:188,B:139,A:255).
        /// </summary>
        public static Color DarkSeaGreen
        {
            get;
            private set;
        }

        /// <summary>
        /// DarkSlateBlue color (R:72,G:61,B:139,A:255).
        /// </summary>
        public static Color DarkSlateBlue
        {
            get;
            private set;
        }

        /// <summary>
        /// DarkSlateGray color (R:47,G:79,B:79,A:255).
        /// </summary>
        public static Color DarkSlateGray
        {
            get;
            private set;
        }

        /// <summary>
        /// DarkTurquoise color (R:0,G:206,B:209,A:255).
        /// </summary>
        public static Color DarkTurquoise
        {
            get;
            private set;
        }

        /// <summary>
        /// DarkViolet color (R:148,G:0,B:211,A:255).
        /// </summary>
        public static Color DarkViolet
        {
            get;
            private set;
        }

        /// <summary>
        /// DeepPink color (R:255,G:20,B:147,A:255).
        /// </summary>
        public static Color DeepPink
        {
            get;
            private set;
        }

        /// <summary>
        /// DeepSkyBlue color (R:0,G:191,B:255,A:255).
        /// </summary>
        public static Color DeepSkyBlue
        {
            get;
            private set;
        }

        /// <summary>
        /// DimGray color (R:105,G:105,B:105,A:255).
        /// </summary>
        public static Color DimGray
        {
            get;
            private set;
        }

        /// <summary>
        /// DodgerBlue color (R:30,G:144,B:255,A:255).
        /// </summary>
        public static Color DodgerBlue
        {
            get;
            private set;
        }

        /// <summary>
        /// Firebrick color (R:178,G:34,B:34,A:255).
        /// </summary>
        public static Color Firebrick
        {
            get;
            private set;
        }

        /// <summary>
        /// FloralWhite color (R:255,G:250,B:240,A:255).
        /// </summary>
        public static Color FloralWhite
        {
            get;
            private set;
        }

        /// <summary>
        /// ForestGreen color (R:34,G:139,B:34,A:255).
        /// </summary>
        public static Color ForestGreen
        {
            get;
            private set;
        }

        /// <summary>
        /// Fuchsia color (R:255,G:0,B:255,A:255).
        /// </summary>
        public static Color Fuchsia
        {
            get;
            private set;
        }

        /// <summary>
        /// Gainsboro color (R:220,G:220,B:220,A:255).
        /// </summary>
        public static Color Gainsboro
        {
            get;
            private set;
        }

        /// <summary>
        /// GhostWhite color (R:248,G:248,B:255,A:255).
        /// </summary>
        public static Color GhostWhite
        {
            get;
            private set;
        }
        /// <summary>
        /// Gold color (R:255,G:215,B:0,A:255).
        /// </summary>
        public static Color Gold
        {
            get;
            private set;
        }

        /// <summary>
        /// Goldenrod color (R:218,G:165,B:32,A:255).
        /// </summary>
        public static Color Goldenrod
        {
            get;
            private set;
        }

        /// <summary>
        /// Gray color (R:128,G:128,B:128,A:255).
        /// </summary>
        public static Color Gray
        {
            get;
            private set;
        }

        /// <summary>
        /// Green color (R:0,G:128,B:0,A:255).
        /// </summary>
        public static Color Green
        {
            get;
            private set;
        }

        /// <summary>
        /// GreenYellow color (R:173,G:255,B:47,A:255).
        /// </summary>
        public static Color GreenYellow
        {
            get;
            private set;
        }

        /// <summary>
        /// Honeydew color (R:240,G:255,B:240,A:255).
        /// </summary>
        public static Color Honeydew
        {
            get;
            private set;
        }

        /// <summary>
        /// HotPink color (R:255,G:105,B:180,A:255).
        /// </summary>
        public static Color HotPink
        {
            get;
            private set;
        }

        /// <summary>
        /// IndianRed color (R:205,G:92,B:92,A:255).
        /// </summary>
        public static Color IndianRed
        {
            get;
            private set;
        }

        /// <summary>
        /// Indigo color (R:75,G:0,B:130,A:255).
        /// </summary>
        public static Color Indigo
        {
            get;
            private set;
        }

        /// <summary>
        /// Ivory color (R:255,G:255,B:240,A:255).
        /// </summary>
        public static Color Ivory
        {
            get;
            private set;
        }

        /// <summary>
        /// Khaki color (R:240,G:230,B:140,A:255).
        /// </summary>
        public static Color Khaki
        {
            get;
            private set;
        }

        /// <summary>
        /// Lavender color (R:230,G:230,B:250,A:255).
        /// </summary>
        public static Color Lavender
        {
            get;
            private set;
        }

        /// <summary>
        /// LavenderBlush color (R:255,G:240,B:245,A:255).
        /// </summary>
        public static Color LavenderBlush
        {
            get;
            private set;
        }

        /// <summary>
        /// LawnGreen color (R:124,G:252,B:0,A:255).
        /// </summary>
        public static Color LawnGreen
        {
            get;
            private set;
        }

        /// <summary>
        /// LemonChiffon color (R:255,G:250,B:205,A:255).
        /// </summary>
        public static Color LemonChiffon
        {
            get;
            private set;
        }

        /// <summary>
        /// LightBlue color (R:173,G:216,B:230,A:255).
        /// </summary>
        public static Color LightBlue
        {
            get;
            private set;
        }

        /// <summary>
        /// LightCoral color (R:240,G:128,B:128,A:255).
        /// </summary>
        public static Color LightCoral
        {
            get;
            private set;
        }

        /// <summary>
        /// LightCyan color (R:224,G:255,B:255,A:255).
        /// </summary>
        public static Color LightCyan
        {
            get;
            private set;
        }

        /// <summary>
        /// LightGoldenrodYellow color (R:250,G:250,B:210,A:255).
        /// </summary>
        public static Color LightGoldenrodYellow
        {
            get;
            private set;
        }

        /// <summary>
        /// LightGray color (R:211,G:211,B:211,A:255).
        /// </summary>
        public static Color LightGray
        {
            get;
            private set;
        }

        /// <summary>
        /// LightGreen color (R:144,G:238,B:144,A:255).
        /// </summary>
        public static Color LightGreen
        {
            get;
            private set;
        }

        /// <summary>
        /// LightPink color (R:255,G:182,B:193,A:255).
        /// </summary>
        public static Color LightPink
        {
            get;
            private set;
        }

        /// <summary>
        /// LightSalmon color (R:255,G:160,B:122,A:255).
        /// </summary>
        public static Color LightSalmon
        {
            get;
            private set;
        }

        /// <summary>
        /// LightSeaGreen color (R:32,G:178,B:170,A:255).
        /// </summary>
        public static Color LightSeaGreen
        {
            get;
            private set;
        }

        /// <summary>
        /// LightSkyBlue color (R:135,G:206,B:250,A:255).
        /// </summary>
        public static Color LightSkyBlue
        {
            get;
            private set;
        }

        /// <summary>
        /// LightSlateGray color (R:119,G:136,B:153,A:255).
        /// </summary>
        public static Color LightSlateGray
        {
            get;
            private set;
        }

        /// <summary>
        /// LightSteelBlue color (R:176,G:196,B:222,A:255).
        /// </summary>
        public static Color LightSteelBlue
        {
            get;
            private set;
        }

        /// <summary>
        /// LightYellow color (R:255,G:255,B:224,A:255).
        /// </summary>
        public static Color LightYellow
        {
            get;
            private set;
        }

        /// <summary>
        /// Lime color (R:0,G:255,B:0,A:255).
        /// </summary>
        public static Color Lime
        {
            get;
            private set;
        }

        /// <summary>
        /// LimeGreen color (R:50,G:205,B:50,A:255).
        /// </summary>
        public static Color LimeGreen
        {
            get;
            private set;
        }

        /// <summary>
        /// Linen color (R:250,G:240,B:230,A:255).
        /// </summary>
        public static Color Linen
        {
            get;
            private set;
        }

        /// <summary>
        /// Magenta color (R:255,G:0,B:255,A:255).
        /// </summary>
        public static Color Magenta
        {
            get;
            private set;
        }

        /// <summary>
        /// Maroon color (R:128,G:0,B:0,A:255).
        /// </summary>
        public static Color Maroon
        {
            get;
            private set;
        }

        /// <summary>
        /// MediumAquamarine color (R:102,G:205,B:170,A:255).
        /// </summary>
        public static Color MediumAquamarine
        {
            get;
            private set;
        }

        /// <summary>
        /// MediumBlue color (R:0,G:0,B:205,A:255).
        /// </summary>
        public static Color MediumBlue
        {
            get;
            private set;
        }

        /// <summary>
        /// MediumOrchid color (R:186,G:85,B:211,A:255).
        /// </summary>
        public static Color MediumOrchid
        {
            get;
            private set;
        }

        /// <summary>
        /// MediumPurple color (R:147,G:112,B:219,A:255).
        /// </summary>
        public static Color MediumPurple
        {
            get;
            private set;
        }

        /// <summary>
        /// MediumSeaGreen color (R:60,G:179,B:113,A:255).
        /// </summary>
        public static Color MediumSeaGreen
        {
            get;
            private set;
        }

        /// <summary>
        /// MediumSlateBlue color (R:123,G:104,B:238,A:255).
        /// </summary>
        public static Color MediumSlateBlue
        {
            get;
            private set;
        }

        /// <summary>
        /// MediumSpringGreen color (R:0,G:250,B:154,A:255).
        /// </summary>
        public static Color MediumSpringGreen
        {
            get;
            private set;
        }

        /// <summary>
        /// MediumTurquoise color (R:72,G:209,B:204,A:255).
        /// </summary>
        public static Color MediumTurquoise
        {
            get;
            private set;
        }

        /// <summary>
        /// MediumVioletRed color (R:199,G:21,B:133,A:255).
        /// </summary>
        public static Color MediumVioletRed
        {
            get;
            private set;
        }

        /// <summary>
        /// MidnightBlue color (R:25,G:25,B:112,A:255).
        /// </summary>
        public static Color MidnightBlue
        {
            get;
            private set;
        }

        /// <summary>
        /// MintCream color (R:245,G:255,B:250,A:255).
        /// </summary>
        public static Color MintCream
        {
            get;
            private set;
        }

        /// <summary>
        /// MistyRose color (R:255,G:228,B:225,A:255).
        /// </summary>
        public static Color MistyRose
        {
            get;
            private set;
        }

        /// <summary>
        /// Moccasin color (R:255,G:228,B:181,A:255).
        /// </summary>
        public static Color Moccasin
        {
            get;
            private set;
        }

        /// <summary>
        /// MonoGame orange theme color (R:231,G:60,B:0,A:255).
        /// </summary>
        public static Color MonoGameOrange
        {
            get;
            private set;
        }

        /// <summary>
        /// NavajoWhite color (R:255,G:222,B:173,A:255).
        /// </summary>
        public static Color NavajoWhite
        {
            get;
            private set;
        }

        /// <summary>
        /// Navy color (R:0,G:0,B:128,A:255).
        /// </summary>
        public static Color Navy
        {
            get;
            private set;
        }

        /// <summary>
        /// OldLace color (R:253,G:245,B:230,A:255).
        /// </summary>
        public static Color OldLace
        {
            get;
            private set;
        }

        /// <summary>
        /// Olive color (R:128,G:128,B:0,A:255).
        /// </summary>
        public static Color Olive
        {
            get;
            private set;
        }

        /// <summary>
        /// OliveDrab color (R:107,G:142,B:35,A:255).
        /// </summary>
        public static Color OliveDrab
        {
            get;
            private set;
        }

        /// <summary>
        /// Orange color (R:255,G:165,B:0,A:255).
        /// </summary>
        public static Color Orange
        {
            get;
            private set;
        }

        /// <summary>
        /// OrangeRed color (R:255,G:69,B:0,A:255).
        /// </summary>
        public static Color OrangeRed
        {
            get;
            private set;
        }

        /// <summary>
        /// Orchid color (R:218,G:112,B:214,A:255).
        /// </summary>
        public static Color Orchid
        {
            get;
            private set;
        }

        /// <summary>
        /// PaleGoldenrod color (R:238,G:232,B:170,A:255).
        /// </summary>
        public static Color PaleGoldenrod
        {
            get;
            private set;
        }

        /// <summary>
        /// PaleGreen color (R:152,G:251,B:152,A:255).
        /// </summary>
        public static Color PaleGreen
        {
            get;
            private set;
        }

        /// <summary>
        /// PaleTurquoise color (R:175,G:238,B:238,A:255).
        /// </summary>
        public static Color PaleTurquoise
        {
            get;
            private set;
        }
        /// <summary>
        /// PaleVioletRed color (R:219,G:112,B:147,A:255).
        /// </summary>
        public static Color PaleVioletRed
        {
            get;
            private set;
        }

        /// <summary>
        /// PapayaWhip color (R:255,G:239,B:213,A:255).
        /// </summary>
        public static Color PapayaWhip
        {
            get;
            private set;
        }

        /// <summary>
        /// PeachPuff color (R:255,G:218,B:185,A:255).
        /// </summary>
        public static Color PeachPuff
        {
            get;
            private set;
        }

        /// <summary>
        /// Peru color (R:205,G:133,B:63,A:255).
        /// </summary>
        public static Color Peru
        {
            get;
            private set;
        }

        /// <summary>
        /// Pink color (R:255,G:192,B:203,A:255).
        /// </summary>
        public static Color Pink
        {
            get;
            private set;
        }

        /// <summary>
        /// Plum color (R:221,G:160,B:221,A:255).
        /// </summary>
        public static Color Plum
        {
            get;
            private set;
        }

        /// <summary>
        /// PowderBlue color (R:176,G:224,B:230,A:255).
        /// </summary>
        public static Color PowderBlue
        {
            get;
            private set;
        }

        /// <summary>
        ///  Purple color (R:128,G:0,B:128,A:255).
        /// </summary>
        public static Color Purple
        {
            get;
            private set;
        }

        /// <summary>
        /// Red color (R:255,G:0,B:0,A:255).
        /// </summary>
        public static Color Red
        {
            get;
            private set;
        }

        /// <summary>
        /// RosyBrown color (R:188,G:143,B:143,A:255).
        /// </summary>
        public static Color RosyBrown
        {
            get;
            private set;
        }

        /// <summary>
        /// RoyalBlue color (R:65,G:105,B:225,A:255).
        /// </summary>
        public static Color RoyalBlue
        {
            get;
            private set;
        }

        /// <summary>
        /// SaddleBrown color (R:139,G:69,B:19,A:255).
        /// </summary>
        public static Color SaddleBrown
        {
            get;
            private set;
        }

        /// <summary>
        /// Salmon color (R:250,G:128,B:114,A:255).
        /// </summary>
        public static Color Salmon
        {
            get;
            private set;
        }

        /// <summary>
        /// SandyBrown color (R:244,G:164,B:96,A:255).
        /// </summary>
        public static Color SandyBrown
        {
            get;
            private set;
        }

        /// <summary>
        /// SeaGreen color (R:46,G:139,B:87,A:255).
        /// </summary>
        public static Color SeaGreen
        {
            get;
            private set;
        }

        /// <summary>
        /// SeaShell color (R:255,G:245,B:238,A:255).
        /// </summary>
        public static Color SeaShell
        {
            get;
            private set;
        }

        /// <summary>
        /// Sienna color (R:160,G:82,B:45,A:255).
        /// </summary>
        public static Color Sienna
        {
            get;
            private set;
        }

        /// <summary>
        /// Silver color (R:192,G:192,B:192,A:255).
        /// </summary>
        public static Color Silver
        {
            get;
            private set;
        }

        /// <summary>
        /// SkyBlue color (R:135,G:206,B:235,A:255).
        /// </summary>
        public static Color SkyBlue
        {
            get;
            private set;
        }

        /// <summary>
        /// SlateBlue color (R:106,G:90,B:205,A:255).
        /// </summary>
        public static Color SlateBlue
        {
            get;
            private set;
        }

        /// <summary>
        /// SlateGray color (R:112,G:128,B:144,A:255).
        /// </summary>
        public static Color SlateGray
        {
            get;
            private set;
        }

        /// <summary>
        /// Snow color (R:255,G:250,B:250,A:255).
        /// </summary>
        public static Color Snow
        {
            get;
            private set;
        }

        /// <summary>
        /// SpringGreen color (R:0,G:255,B:127,A:255).
        /// </summary>
        public static Color SpringGreen
        {
            get;
            private set;
        }

        /// <summary>
        /// SteelBlue color (R:70,G:130,B:180,A:255).
        /// </summary>
        public static Color SteelBlue
        {
            get;
            private set;
        }

        /// <summary>
        /// Tan color (R:210,G:180,B:140,A:255).
        /// </summary>
        public static Color Tan
        {
            get;
            private set;
        }

        /// <summary>
        /// Teal color (R:0,G:128,B:128,A:255).
        /// </summary>
        public static Color Teal
        {
            get;
            private set;
        }

        /// <summary>
        /// Thistle color (R:216,G:191,B:216,A:255).
        /// </summary>
        public static Color Thistle
        {
            get;
            private set;
        }

        /// <summary>
        /// Tomato color (R:255,G:99,B:71,A:255).
        /// </summary>
        public static Color Tomato
        {
            get;
            private set;
        }

        /// <summary>
        /// Turquoise color (R:64,G:224,B:208,A:255).
        /// </summary>
        public static Color Turquoise
        {
            get;
            private set;
        }

        /// <summary>
        /// Violet color (R:238,G:130,B:238,A:255).
        /// </summary>
        public static Color Violet
        {
            get;
            private set;
        }

        /// <summary>
        /// Wheat color (R:245,G:222,B:179,A:255).
        /// </summary>
        public static Color Wheat
        {
            get;
            private set;
        }

        /// <summary>
        /// White color (R:255,G:255,B:255,A:255).
        /// </summary>
        public static Color White
        {
            get;
            private set;
        }

        /// <summary>
        /// WhiteSmoke color (R:245,G:245,B:245,A:255).
        /// </summary>
        public static Color WhiteSmoke
        {
            get;
            private set;
        }

        /// <summary>
        /// Yellow color (R:255,G:255,B:0,A:255).
        /// </summary>
        public static Color Yellow
        {
            get;
            private set;
        }

        /// <summary>
        /// YellowGreen color (R:154,G:205,B:50,A:255).
        /// </summary>
        public static Color YellowGreen
        {
            get;
            private set;
        }

        #endregion
    }
}