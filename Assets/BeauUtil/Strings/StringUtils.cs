/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    29 Oct 2019
 * 
 * File:    StringUtils.cs
 * Purpose: String utility methods.
 */

using System;
using System.Collections.Generic;
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
                                char code = (char)int.Parse(unicode, NumberStyles.AllowHexSpecifier);
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

        #region Equals

        /// <summary>
        /// Determines if two strings are equal,
        /// treating null and string.empty as equivalent.
        /// </summary>
        static public bool NullEquivalentEquals(string inStringA, string inStringB)
        {
            inStringA = inStringA ?? string.Empty;
            inStringB = inStringB ?? string.Empty;
            return inStringA.Equals(inStringB);
        }

        /// <summary>
        /// Determines if two strings are equal,
        /// treating null and string.empty as equivalent.
        /// </summary>
        static public bool NullEquivalentEquals(string inStringA, string inStringB, StringComparison inComparison)
        {
            inStringA = inStringA ?? string.Empty;
            inStringB = inStringB ?? string.Empty;
            return inStringA.Equals(inStringB, inComparison);
        }

        #endregion // Equals

        #region Pattern Matching

        /// <summary>
        /// Determines if a string matches any of the given filters.
        /// Wildcard characters are supported at the start and end of the filter.
        /// </summary>
        /// <remarks>
        /// This does not yet support wildcards in the middle of the filter.
        /// </remarks>
        static public bool WildcardMatch(StringSlice inString, string[] inFilters, char inWildcard = '*', bool inbIgnoreCase = false)
        {
            if (inFilters.Length == 0)
                return false;

            for (int i = 0; i < inFilters.Length; ++i)
            {
                if (WildcardMatch(inString, inFilters[i], inWildcard, inbIgnoreCase))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Determines if a string matches any of the given filters.
        /// Wildcard characters are supported at the start and end of the filter.
        /// </summary>
        /// <remarks>
        /// This does not yet support wildcards in the middle of the filter.
        /// </remarks>
        static public bool WildcardMatch(StringSlice inString, ICollection<string> inFilters, char inWildcard = '*', bool inbIgnoreCase = false)
        {
            if (inFilters.Count == 0)
                return false;

            foreach (var filter in inFilters)
            {
                if (WildcardMatch(inString, filter, inWildcard, inbIgnoreCase))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Determines if a string matches the given filter.
        /// Wildcard characters are supported at the start and end of the filter.
        /// </summary>
        /// <remarks>
        /// This does not yet support wildcards in the middle of the filter.
        /// </remarks>
        static public bool WildcardMatch(StringSlice inString, string inFilter, char inWildcard = '*', bool inbIgnoreCase = false)
        {
            if (string.IsNullOrEmpty(inFilter))
            {
                return inString.IsEmpty;
            }

            int filterLength = inFilter.Length;
            if (filterLength == 1 && inFilter[0] == inWildcard)
                return true;

            if (filterLength == 2 && inFilter[0] == inWildcard && inFilter[1] == inWildcard)
                return true;

            bool bStart = inFilter[0] == inWildcard;
            bool bEnd = inFilter[filterLength - 1] == inWildcard;
            if (bStart || bEnd)
            {
                string filterStr = inFilter;
                int startIdx = 0;
                if (bStart)
                {
                    ++startIdx;
                    --filterLength;
                }
                if (bEnd)
                {
                    --filterLength;
                }

                filterStr = filterStr.Substring(startIdx, filterLength);
                if (bStart && bEnd)
                {
                    return inString.Contains(filterStr, inbIgnoreCase);
                }
                if (bStart)
                {
                    return inString.EndsWith(filterStr, inbIgnoreCase);
                }
                return inString.StartsWith(filterStr, inbIgnoreCase);
            }

            return inString.Equals(inFilter, inbIgnoreCase);
        }

        /// <summary>
        /// Returns if the given string contains the given string at the given index.
        /// </summary>
        static public bool AttemptMatch(this string inString, int inIndex, string inMatch, bool inbIgnoreCase = false)
        {
            if (string.IsNullOrEmpty(inString))
                return false;

            if (string.IsNullOrEmpty(inMatch))
                return false;

            if (inIndex < 0 || inIndex + inMatch.Length > inString.Length)
                return false;

            for (int i = 0; i < inMatch.Length; ++i)
            {
                char a = inString[inIndex + i];
                char b = inMatch[i];
                if (!inbIgnoreCase)
                {
                    if (a != b)
                        return false;
                }
                else
                {
                    if (char.ToLowerInvariant(a) != char.ToLowerInvariant(b))
                        return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Returns if the given StringSlice contains the given string at the given index.
        /// </summary>
        static public bool AttemptMatch(this StringSlice inString, int inIndex, string inMatch, bool inbIgnoreCase = false)
        {
            if (inString.IsEmpty)
                return false;

            if (string.IsNullOrEmpty(inMatch))
                return false;

            if (inIndex < 0 || inIndex + inMatch.Length > inString.Length)
                return false;

            for (int i = 0; i < inMatch.Length; ++i)
            {
                char a = inString[inIndex + i];
                char b = inMatch[i];
                if (!inbIgnoreCase)
                {
                    if (a != b)
                        return false;
                }
                else
                {
                    if (char.ToLowerInvariant(a) != char.ToLowerInvariant(b))
                        return false;
                }
            }

            return true;
        }

        #endregion // Pattern Matching

        #region StringBuilder

        /// <summary>
        /// Retrieves the string from the StringBuilder
        /// and clears the StringBuilder's state.
        /// </summary>
        static public string Flush(this StringBuilder ioBuilder)
        {
            if (ioBuilder == null)
                throw new ArgumentNullException("ioBuilder");

            if (ioBuilder.Length <= 0)
                return string.Empty;

            string str = ioBuilder.ToString();
            ioBuilder.Length = 0;
            return str;
        }

        /// <summary>
        /// Appends a StringSlice to the given StringBuilder.
        /// </summary>
        static public StringBuilder AppendSlice(this StringBuilder ioBuilder, StringSlice inSlice)
        {
            if (ioBuilder == null)
                throw new ArgumentNullException("ioBuilder");

            inSlice.AppendTo(ioBuilder);
            return ioBuilder;
        }

        /// <summary>
        /// Returns if the given StringBuilder contains the given string at the given index.
        /// </summary>
        static public bool AttemptMatch(this StringBuilder inBuilder, int inIndex, string inMatch, bool inbIgnoreCase = false)
        {
            if (inBuilder == null)
                throw new ArgumentNullException("inBuilder");

            if (string.IsNullOrEmpty(inMatch))
                return false;

            if (inIndex < 0 || inIndex + inMatch.Length > inBuilder.Length)
                return false;

            for (int i = 0; i < inMatch.Length; ++i)
            {
                char a = inBuilder[inIndex + i];
                char b = inMatch[i];
                if (!inbIgnoreCase)
                {
                    if (a != b)
                        return false;
                }
                else
                {
                    if (char.ToLowerInvariant(a) != char.ToLowerInvariant(b))
                        return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Trims characters from the end of a StringBuilder.
        /// </summary>
        static public StringBuilder TrimEnd(this StringBuilder ioBuilder, char[] inTrimChars)
        {
            if (ioBuilder == null)
                throw new ArgumentNullException("ioBuilder");
            if (inTrimChars == null)
                throw new ArgumentException("inTrimChars");

            if (inTrimChars.Length == 0)
                return ioBuilder;

            int length = ioBuilder.Length;
            char c;
            bool bFound;
            int toRemove = 0;
            while(length > 0)
            {
                c = ioBuilder[length - 1];
                bFound = false;
                for(int i = 0; !bFound && i < inTrimChars.Length; ++i)
                {
                    bFound = c == inTrimChars[i];
                }

                if (bFound)
                {
                    --length;
                    ++toRemove;
                }
                else
                {
                    break;
                }
            }

            if (toRemove > 0)
            {
                ioBuilder.Length -= toRemove;
            }

            return ioBuilder;
        }

        #endregion // StringBuilder

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

                public void Reset()
                {
                    m_QuoteMode = false;
                }

                public bool Evaluate(string inString, int inIndex, out int outAdvance)
                {
                    outAdvance = 0;
                    char c = inString[inIndex];
                    if (c == ',')
                    {
                        return !m_QuoteMode;
                    }
                    else if (c == '"')
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

        /// <summary>
        /// Delimiter-separated argument list utils.
        /// </summary>
        static public class ArgsList
        {
            /// <summary>
            /// Returns if the given string contains at least two delimiter-separated arguments.
            /// </summary>
            static public bool IsList(StringSlice inString, char inDelimiter = 'c')
            {
                bool quote = false;
                int group = 0;

                char c;
                for(int i = 0; i < inString.Length; i++)
                {
                    c = inString[i];

                    if (c == inDelimiter)
                    {
                        if (!quote && group <= 0)
                            return true;
                    }

                    switch(c)
                    {
                        case '(':
                        case '[':
                        case '{':
                            if (!quote)
                            {
                                group++;
                            }
                            break;

                        case ')':
                        case ']':
                        case '}':
                            if (!quote)
                            {
                                --group;
                            }
                            break;

                        case '"':
                            if (quote)
                            {
                                if (i > 0 && inString[i - 1] == '\\')
                                {
                                    i++;
                                }
                                else
                                {
                                    quote = !quote;
                                }
                            }
                            else
                            {
                                quote = true;
                            }
                            break;
                    }
                }
                return false;
            }

            /// <summary>
            /// String splitter for arg lists.
            /// </summary>
            public sealed class Splitter : StringSlice.ISplitter
            {
                [ThreadStatic] static private Splitter s_Instance;

                static public Splitter Instance
                {
                    get { return s_Instance ?? (s_Instance = new Splitter()); }
                }

                private readonly bool m_Unescape;
                private readonly char m_Delimiter;
                private bool m_QuoteMode;
                private int m_GroupingDepth;

                public Splitter(bool inbUnescape = false)
                {
                    m_Delimiter = ',';
                    m_Unescape = inbUnescape;
                }

                public Splitter(char inDelimiter, bool inbUnescape = false)
                {
                    m_Delimiter = inDelimiter;
                    m_Unescape = inbUnescape;
                }

                public void Reset()
                {
                    m_QuoteMode = false;
                }

                public bool Evaluate(string inString, int inIndex, out int outAdvance)
                {
                    outAdvance = 0;
                    char c = inString[inIndex];

                    if (c == m_Delimiter)
                        return !m_QuoteMode && m_GroupingDepth <= 0;

                    switch(c)
                    {
                        case '(':
                        case '[':
                        case '{':
                            if (!m_QuoteMode)
                            {
                                m_GroupingDepth++;
                            }
                            break;

                        case ')':
                        case ']':
                        case '}':
                            if (!m_QuoteMode)
                            {
                                --m_GroupingDepth;
                            }
                            break;

                        case '"':
                            if (inIndex > 0 && inString[inIndex - 1] == '\\')
                            {
                                break;
                            }
                            else
                            {
                                m_QuoteMode = !m_QuoteMode;
                            }
                            break;
                    }

                    return false;
                }

                public StringSlice Process(StringSlice inSlice)
                {
                    StringSlice slice = inSlice.Trim();

                    if (slice.Length >= 2 && slice.StartsWith('"') && slice.EndsWith('"'))
                    {
                        slice = slice.Substring(1, slice.Length - 2);
                    }

                    // if this contains escaped CSV sequences, unescape it here
                    if (m_Unescape && (slice.Contains("\\") || slice.Contains("\\\"")))
                    {
                        return slice.Unescape(Escaper.Instance);
                    }
                    return slice;
                }

                static private readonly char[] QuoteTrim = { '"' };
            }

            /// <summary>
            /// Escape/unescape for arg lists.
            /// </summary>
            public sealed class Escaper : ICustomEscapeEvaluator
            {
                static public readonly Escaper Instance = new Escaper();

                public bool TryEscape(char inCharacter, StringBuilder ioBuilder)
                {
                    if (inCharacter == '"')
                    {
                        ioBuilder.Append("\\\"");
                        return true;
                    }

                    return false;
                }

                public bool TryUnescape(char inCharacter, string inString, int inIndex, out int outAdvance, StringBuilder ioBuilder)
                {
                    // This is covered by the standard unescaper
                    
                    // if (inCharacter == '\\')
                    // {
                    //     if (inIndex < inString.Length - 1 && inString[inIndex + 1] == '"')
                    //     {
                    //         ioBuilder.Append('"');
                    //         outAdvance = 1;
                    //         return true;
                    //     }
                    // }

                    outAdvance = 0;
                    return false;
                }
            }
        }

        /// <summary>
        /// Rich text utils.
        /// </summary>
        static public class RichText
        {
            static private readonly string[] s_KnownRichTags = new string[]
            {
                "align", "alpha", "color", "b", "i", "cspace", "font", "indent",
                "line-height", "line-indent", "link", "lowercase", "uppercase", "smallcaps",
                "margin", "mark", "mspace", "noparse", "nobr", "page", "pos",
                "size", "space", "sprite", "s", "u", "style", "sub", "sup",
                "voffset", "width", "material", "quad"
            };

            static private readonly string[] s_VisibleRichTags = new string[]
            {
                "sprite", "quad"
            };

            /// <summary>
            /// Returns if this tag generates something visible.
            /// </summary>
            static public bool GeneratesVisibleCharacter(string inRichTag)
            {
                return Array.IndexOf(s_VisibleRichTags, inRichTag) >= 0;
            }

            /// <summary>
            /// List of all recognized rich tags.
            /// This includes Unity's Rich Tags and TextMesh Pro's rich tags.
            /// </summary>
            static public IReadOnlyList<string> RecognizedRichTags { get { return s_KnownRichTags; } }
        }

        /// <summary>
        /// Default newline characters
        /// </summary>
        static public readonly char[] DefaultNewLineChars = new char[] { '\n', '\r' };
    }
}