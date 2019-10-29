/*
 * Copyright (C) 2017-2019. Filament Games, LLC. All rights reserved.
 * Author:  Alex Beauchesne
 * Date:    29 Oct 2019
 * 
 * File:    StringUtils.cs
 * Purpose: String utility methods.
 */

using System;
using System.Globalization;
using System.Text;

namespace BeauUtil
{
    /// <summary>
    /// String utility methods.
    /// </summary>
    static public class StringUtils
    {
        /// <summary>
        /// Custom escape definition.
        /// </summary>
        public interface ICustomEscapeEvaluator
        {
            /// <summary>
            /// Attempts to write an escaped sequence for the given character.
            /// Should return whether or not this evaluator was able to handle the character.
            /// </summary>
            /// <param name="inCharacter">Character to escape.</param>
            /// <param name="ioBuilder">Target for writing the escaped sequence.</param>
            bool TryEscape(char inCharacter, StringBuilder ioBuilder);
            
            /// <summary>
            /// Attempts to unescape the given character.
            /// Should return whether or not this evaluator was able to handle the character.
            /// </summary>
            /// <param name="inCharacter">Character to unescape.</param>
            /// <param name="inString">Source string.</param>
            /// <param name="inIndex">Current index.</param>
            /// <param name="outAdvance">Output for additional characters read.</param>
            /// <param name="ioBuilder">Target for writing the unescaped sequence.</param>
            bool TryUnescape(char inCharacter, string inString, int inIndex, out int outAdvance, StringBuilder ioBuilder);
        }

        #region Escape

        /// <summary>
        /// Escapes a string to the given string builder.
        /// </summary>
        static public void Escape(string inString, StringBuilder ioBuilder, ICustomEscapeEvaluator inCustomEscape = null)
        {
            if (inString == null)
                return;

            Escape(inString, 0, inString.Length, ioBuilder, inCustomEscape);
        }

        /// <summary>
        /// Escapes a string.
        /// </summary>
        static public string Escape(string inString, ICustomEscapeEvaluator inCustomEscape = null)
        {
            StringBuilder sb = new StringBuilder();
            Escape(inString, sb, inCustomEscape);
            return sb.ToString();
        }

        /// <summary>
        /// Escapes a substring to the given string builder.
        /// </summary>
        static public void Escape(string inString, int inStartIndex, StringBuilder ioBuilder, ICustomEscapeEvaluator inCustomEscape = null)
        {
            if (inString == null)
                return;

            Escape(inString, inStartIndex, inString.Length - inStartIndex, ioBuilder, inCustomEscape);
        }

        /// <summary>
        /// Escapes a substring.
        /// </summary>
        static public string Escape(string inString, int inStartIndex, ICustomEscapeEvaluator inCustomEscape = null)
        {
            StringBuilder sb = new StringBuilder();
            Escape(inString, inStartIndex, sb, inCustomEscape);
            return sb.ToString();
        }

        /// <summary>
        /// Escapes a substring to the given string builder.
        /// </summary>
        static public void Escape(string inString, int inStartIndex, int inLength, StringBuilder ioBuilder, ICustomEscapeEvaluator inCustomEscape = null)
        {
            if (inString == null || inLength <= 0)
                return;

            if (inStartIndex < 0 || inStartIndex >= inString.Length)
            {
                throw new ArgumentOutOfRangeException("Starting index is out of range");
            }
            if (inStartIndex + inLength > inString.Length)
            {
                throw new ArgumentOutOfRangeException("Length extends beyond end of string");
            }

            for (int idx = 0; idx < inLength; ++idx)
            {
                int realIdx = inStartIndex + idx;
                char c = inString[realIdx];
                if (inCustomEscape != null)
                {
                    if (inCustomEscape.TryEscape(c, ioBuilder))
                        continue;
                }

                switch (c)
                {
                    case '\\':
                        ioBuilder.Append("\\\\");
                        break;

                    case '\"':
                        ioBuilder.Append("\\\"");
                        break;

                    case '\0':
                        ioBuilder.Append("\\0");
                        break;

                    case '\a':
                        ioBuilder.Append("\\a");
                        break;

                    case '\v':
                        ioBuilder.Append("\\v");
                        break;

                    case '\t':
                        ioBuilder.Append("\\t");
                        break;

                    case '\b':
                        ioBuilder.Append("\\b");
                        break;

                    case '\f':
                        ioBuilder.Append("\\f");
                        break;

                    case '\n':
                        ioBuilder.Append("\\n");
                        break;

                    default:
                        ioBuilder.Append(c);
                        break;
                }
            }
        }

        /// <summary>
        /// Escapes a substring.
        /// </summary>
        static public string Escape(string inString, int inStartIndex, int inLength, ICustomEscapeEvaluator inCustomEscape = null)
        {
            StringBuilder sb = new StringBuilder(inLength);
            Escape(inString, inStartIndex, inLength, sb, inCustomEscape);
            return sb.ToString();
        }

        #endregion // Escape

        #region Unescape

        /// <summary>
        /// Unescapes a string to the given string builder.
        /// </summary>
        static public void Unescape(string inString, StringBuilder ioBuilder, ICustomEscapeEvaluator inCustomEscape = null)
        {
            if (inString == null)
                return;

            Unescape(inString, 0, inString.Length, ioBuilder, inCustomEscape);
        }

        /// <summary>
        /// Unescapes a string.
        /// </summary>
        static public string Unescape(string inString, ICustomEscapeEvaluator inCustomEscape = null)
        {
            StringBuilder sb = new StringBuilder();
            Unescape(inString, sb, inCustomEscape);
            return sb.ToString();
        }

        /// <summary>
        /// Unescapes a substring to the given string builder.
        /// </summary>
        static public void Unescape(string inString, int inStartIndex, StringBuilder ioBuilder, ICustomEscapeEvaluator inCustomEscape = null)
        {
            if (inString == null)
                return;

            Unescape(inString, inStartIndex, inString.Length - inStartIndex, ioBuilder, inCustomEscape);
        }

        /// <summary>
        /// Unescapes a substring.
        /// </summary>
        static public string Unescape(string inString, int inStartIndex, ICustomEscapeEvaluator inCustomEscape = null)
        {
            StringBuilder sb = new StringBuilder();
            Unescape(inString, inStartIndex, sb, inCustomEscape);
            return sb.ToString();
        }

        /// <summary>
        /// Unescapes a substring to the given string builder.
        /// </summary>
        static public void Unescape(string inString, int inStartIndex, int inLength, StringBuilder ioBuilder, ICustomEscapeEvaluator inCustomEscape = null)
        {
            if (inString == null || inLength <= 0)
                return;

            if (inStartIndex < 0 || inStartIndex >= inString.Length)
            {
                throw new ArgumentOutOfRangeException("Starting index is out of range");
            }
            if (inStartIndex + inLength > inString.Length)
            {
                throw new ArgumentOutOfRangeException("Length extends beyond end of string");
            }

            for (int idx = 0; idx < inLength; ++idx)
            {
                int realIdx = inStartIndex + idx;
                char c = inString[realIdx];

                if (inCustomEscape != null)
                {
                    int advance = 0;
                    if (inCustomEscape.TryUnescape(c, inString, realIdx, out advance, ioBuilder))
                    {
                        idx += advance;
                        continue;
                    }
                }

                if (c == '\\')
                {
                    ++idx;
                    ++realIdx;
                    c = inString[realIdx];
                    switch (c)
                    {
                        case '0':
                            ioBuilder.Append('\0');
                            break;

                        case 'a':
                            ioBuilder.Append('\a');
                            break;

                        case 'v':
                            ioBuilder.Append('\v');
                            break;

                        case 't':
                            ioBuilder.Append('\t');
                            break;

                        case 'r':
                            ioBuilder.Append('\r');
                            break;

                        case 'n':
                            ioBuilder.Append('\n');
                            break;

                        case 'b':
                            ioBuilder.Append('\b');
                            break;

                        case 'f':
                            ioBuilder.Append('\f');
                            break;

                        case 'u':
                            {
                                string unicode = inString.Substring(realIdx + 1, 4);
                                char code = (char) int.Parse(unicode, NumberStyles.AllowHexSpecifier);
                                ioBuilder.Append(code);
                                idx += 4;
                                break;
                            }

                        default:
                            ioBuilder.Append(c);
                            break;
                    }
                }
                else
                {
                    ioBuilder.Append(c);
                }
            }
        }

        /// <summary>
        /// Unescapes a substring.
        /// </summary>
        static public string Unescape(string inString, int inStartIndex, int inLength, ICustomEscapeEvaluator inCustomEscape = null)
        {
            StringBuilder sb = new StringBuilder(inLength);
            Unescape(inString, inStartIndex, inLength, sb, inCustomEscape);
            return sb.ToString();
        }

        #endregion // Unescape

        /// <summary>
        /// CSV utils.
        /// </summary>
        static public class CSV
        {
            /// <summary>
            /// String splitter for CSV.
            /// </summary>
            public sealed class Splitter : StringSlice.ISplitter
            {
                private readonly bool m_Unescape;
                private bool m_QuoteMode;

                public Splitter(bool inbUnescape = false)
                {
                    m_Unescape = inbUnescape;
                }

                public bool Evaluate(string inString, int inIndex, out int outAdvance)
                {
                    outAdvance = 0;
                    char c = inString[inIndex];
                    if (c == ',')
                    {
                        return !m_QuoteMode;
                    }
                    if (c == '"')
                    {
                        if (m_QuoteMode)
                        {
                            if (inIndex < inString.Length - 1 && inString[inIndex + 1] == '"')
                            {
                                outAdvance = 1;
                            }
                            else
                            {
                                m_QuoteMode = !m_QuoteMode;
                            }
                        }
                        else
                        {
                            m_QuoteMode = true;
                        }
                    }

                    return false;
                }

                public StringSlice Process(StringSlice inSlice)
                {
                    StringSlice slice = inSlice.TrimStart().Trim(QuoteTrim);

                    // if this contains escaped CSV sequences, unescape it here
                    if (m_Unescape && (slice.Contains("\\") || slice.Contains("\"\"")))
                    {
                        return slice.Unescape(Escaper.Instance);
                    }
                    return slice;
                }

                static private readonly char[] QuoteTrim = { '"' };
            }

            /// <summary>
            /// Escape/unescape for CSV.
            /// </summary>
            public sealed class Escaper : ICustomEscapeEvaluator
            {
                static public readonly Escaper Instance = new Escaper();

                public bool TryEscape(char inCharacter, StringBuilder ioBuilder)
                {
                    if (inCharacter == '"')
                    {
                        ioBuilder.Append("\"\"");
                        return true;
                    }

                    return false;
                }

                public bool TryUnescape(char inCharacter, string inString, int inIndex, out int outAdvance, StringBuilder ioBuilder)
                {
                    if (inCharacter == '"')
                    {
                        if (inIndex < inString.Length - 1 && inString[inIndex + 1] == '"')
                        {
                            ioBuilder.Append('"');
                            outAdvance = 1;
                            return true;
                        }
                    }

                    outAdvance = 0;
                    return false;
                }
            }
        }
    }
}