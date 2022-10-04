/*
 * Copyright (C) 2022. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    29 Sept 2022
 * 
 * File:    TextHelper.cs
 * Purpose: Text display utilities.
 */

using UnityEngine;
using System;

#if ENABLE_TEXTMESHPRO
using TMPro;
#endif // ENABLE_TEXTMESHPRO

namespace BeauUtil.UI
{
    /// <summary>
    /// Text display utilities.
    /// </summary>
    static public class TextHelper
    {
        private const int DefaultBufferSize = 256;

        static private int s_CurrentCharBufferSize = DefaultBufferSize;
        static private char[] s_CurrentCharBuffer = null;

        /// <summary>
        /// Size of the internal char buffer.
        /// Used when displaying text from an unsafe buffer.
        /// </summary>
        static public int BufferSize
        {
            get { return s_CurrentCharBufferSize; }
            set
            {
                if (value == s_CurrentCharBufferSize)
                    return;

                if (value != 0 && (value < 16 || value > ushort.MaxValue + 1))
                    throw new ArgumentOutOfRangeException("value", "Buffer size must be between 16 and 655356");

                if (value > 0 && !Mathf.IsPowerOfTwo(value))
                    throw new ArgumentException("Buffer size must be a power of 2", "value");

                s_CurrentCharBufferSize = value;
                Array.Resize(ref s_CurrentCharBuffer, value);
            }
        }

        #region TextMeshPro

        #if ENABLE_TEXTMESHPRO

        /// <summary>
        /// Sets text on the given TextMeshPro element from an unsafe char buffer.
        /// If the text length exceeds the current <c>TextUtil.BufferSize</c>, then a string will be allocated.
        /// Otherwise this will not allocate any extra string memory.
        /// </summary>
        /// <remarks>
        /// Note:   In the editor, TextMeshPro will automatically allocate a string internally,
        ///         so it can be displayed in the inspector. This does not occur in builds.
        /// </remarks>
        static public unsafe void SetText(this TMP_Text inTextMeshPro, char* inCharBuffer, int inCharBufferLength)
        {
            if (inCharBuffer == null || inCharBufferLength <= 0)
            {
                inTextMeshPro.SetText(string.Empty);
                return;
            }

            if (inCharBufferLength > s_CurrentCharBufferSize)
            {
                Debug.LogWarningFormat("[TextUtils] Input text of length {0} exceeded buffer size {1} - consider adjusting buffer size", inCharBufferLength.ToString(), s_CurrentCharBufferSize.ToString());
                inTextMeshPro.SetText(new string(inCharBuffer, 0, inCharBufferLength));
                return;
            }

            if (s_CurrentCharBuffer == null)
            {
                s_CurrentCharBuffer = new char[s_CurrentCharBufferSize];
            }

            Unsafe.CopyArray(inCharBuffer, inCharBufferLength, s_CurrentCharBuffer);
            inTextMeshPro.SetText(s_CurrentCharBuffer, 0, inCharBufferLength);
        }

        #endif // ENABLE_TEXTMESHPRO

        #endregion // TextMeshPro
    }
}