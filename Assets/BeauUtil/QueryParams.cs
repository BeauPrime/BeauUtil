/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    4 April 2019
 * 
 * File:    QueryParams.cs
 * Purpose: Query parameter parsing utility.
 */

#if UNITY_2018_3_OR_NEWER
#define USE_WEBREQUEST
#endif // UNITY_2018_3_OR_NEWER

#if CSHARP_7_3_OR_NEWER
#define EXPANDED_REFS
#endif // CSHARP_7_3_OR_NEWER

using System;
using System.Collections.Generic;
using System.Text;
using BeauUtil.Variants;

namespace BeauUtil
{
    /// <summary>
    /// Encodes and decodes query parameters.
    /// Keys are case-insensitive.
    /// </summary>
    public class QueryParams
    {
        private struct Parameter
        {
            public string Key;

            public string StringValue;
            public Variant VariantValue;

            public void Set(string inValue)
            {
                StringValue = inValue;
                VariantValue = Variant.Null;
            }

            public void Set(Variant inValue)
            {
                StringValue = null;
                VariantValue = inValue;
            }

            public override string ToString()
            {
                if (StringValue != null)
                {
                    return string.Format("{0}: \"{1}\"", Key, StringValue);
                }

                return string.Format("{0}: {1}", VariantValue.ToDebugString());
            }
        }

        private RingBuffer<Parameter> m_Parameters;

        #region Get/Set

        /// <summary>
        /// Returns if a parameter with the given key exists.
        /// </summary>
        public bool Contains(string inKey)
        {
            return GetParamIndexByKey(inKey, false) >= 0;
        }

        /// <summary>
        /// Sets a parameter with no value.
        /// </summary>
        public void Set(string inKey)
        {
            int idx = GetParamIndexByKey(inKey, true);
            #if EXPANDED_REFS
            ref Parameter parm = ref m_Parameters[idx];
            parm.Set(Variant.Null);
            #else
            Parameter parm = m_Parameters[idx];
            parm.Set(Variant.Null);
            m_Parameters[idx] = parm;
            #endif // EXPANDED_REFS
        }

        /// <summary>
        /// Sets a parameter with the given value.
        /// </summary>
        public void Set(string inKey, string inValue)
        {
            int idx = GetParamIndexByKey(inKey, true);
            #if EXPANDED_REFS
            ref Parameter parm = ref m_Parameters[idx];
            parm.Set(inValue);
            #else
            Parameter parm = m_Parameters[idx];
            parm.Set(inValue);
            m_Parameters[idx] = parm;
            #endif // EXPANDED_REFS
        }

        /// <summary>
        /// Sets a parameter with the given value.
        /// </summary>
        public void Set(string inKey, Variant inValue)
        {
            int idx = GetParamIndexByKey(inKey, true);
            #if EXPANDED_REFS
            ref Parameter parm = ref m_Parameters[idx];
            parm.Set(inValue);
            #else
            Parameter parm = m_Parameters[idx];
            parm.Set(inValue);
            m_Parameters[idx] = parm;
            #endif // EXPANDED_REFS
        }

        /// <summary>
        /// Removes the parameter with the given key.
        /// </summary>
        public void Remove(string inKey)
        {
            int idx = GetParamIndexByKey(inKey);
            if (idx >= 0)
                m_Parameters.FastRemoveAt(idx);
        }

        /// <summary>
        /// Returns the string value of the parameter with the given key.
        /// </summary>
        public string Get(string inKey)
        {
            int idx = GetParamIndexByKey(inKey);
            if (idx >= 0)
                return m_Parameters[idx].StringValue ?? m_Parameters[idx].VariantValue.ToString();
            return null;
        }

        /// <summary>
        /// Returns the variant value of the parameter with the given key.
        /// </summary>
        public Variant GetVariant(string inKey)
        {
            int idx = GetParamIndexByKey(inKey);
            if (idx >= 0)
                return m_Parameters[idx].VariantValue;
            return Variant.Null;
        }

        /// <summary>
        /// Clears all parameters.
        /// </summary>
        public void Clear()
        {
            if (m_Parameters != null)
                m_Parameters.Clear();
        }

        public string this[string inKey]
        {
            get { return Get(inKey); }
            set { Set(inKey, value); }
        }

        #endregion

        #region Encode/Decode

        /// <summary>
        /// Encodes parameters into a url-encoded query string.
        /// </summary>
        public string Encode()
        {
            if (m_Parameters == null || m_Parameters.Count == 0)
                return string.Empty;

            StringBuilder builder = new StringBuilder(m_Parameters.Count * 32);

            builder.Append('?');
            for (int i = 0; i < m_Parameters.Count; ++i)
            {
                Parameter parm = m_Parameters[i];
                if (i > 0)
                    builder.Append('&');

                builder.Append(EscapeURL(parm.Key));
                if (parm.StringValue != null)
                {
                    builder.Append('=').Append(EscapeURL(parm.StringValue));
                }
                else if (parm.VariantValue != Variant.Null)
                {
                    builder.Append('=').Append(parm.VariantValue.ToString());
                }
            }

            return builder.ToString();
        }

        /// <summary>
        /// Decodes parameters from a query string.
        /// </summary>
        public bool TryParse(string inURL)
        {
            int queryStartIdx = inURL.LastIndexOf('?');
            if (queryStartIdx < 0 || queryStartIdx >= inURL.Length - 1)
            {
                if (m_Parameters != null)
                    m_Parameters.Clear();
                m_Parameters = null;
                return false;
            }

            StringSlice paramSection = new StringSlice(inURL, queryStartIdx + 1);
            StringSlice[] paramChunks = paramSection.Split(SplitChars, StringSplitOptions.RemoveEmptyEntries);

            if (m_Parameters == null)
            {
                m_Parameters = new RingBuffer<Parameter>(paramChunks.Length, RingBufferMode.Expand);
            }
            else
            {
                m_Parameters.Clear();
                if (m_Parameters.Capacity < paramChunks.Length)
                    m_Parameters.SetCapacity(paramChunks.Length);
            }

            for (int i = 0; i < paramChunks.Length; ++i)
            {
                StringSlice chunk = paramChunks[i];
                if (chunk.Length == 0 || chunk[0] == '=')
                    continue;

                Parameter parm = new Parameter();

                int equalIdx = chunk.IndexOf('=');
                if (equalIdx < 0)
                {
                    StringSlice key = UnEscapeURL(chunk);
                    parm.Key = key.ToString();
                }
                else
                {
                    StringSlice key = UnEscapeURL(chunk.Substring(0, equalIdx));
                    parm.Key = key.ToString();

                    StringSlice value = equalIdx >= chunk.Length - 1 ? string.Empty : UnEscapeURL(chunk.Substring(equalIdx + 1));
                    StringSlice strValue = value;

                    // ensure double-quotes are removed before writing the string value
                    if (strValue.Length >= 2 && strValue.StartsWith('"') && strValue.EndsWith('"')) {
                        strValue = strValue.Substring(1, strValue.Length - 1);
                    }
                    parm.StringValue = strValue.ToString();
                    Variant.TryParse(value, false, out parm.VariantValue);
                }

                m_Parameters.PushBack(parm);
            }

            return true;
        }

        #endregion

        #region Static

        /// <summary>
        /// Attempts to parse a set of QueryParams from the given url.
        /// </summary>
        static public bool TryParse(string inURL, out QueryParams outParams)
        {
            QueryParams parms = new QueryParams();
            bool bSuccess = parms.TryParse(inURL);
            if (bSuccess)
            {
                outParams = parms;
            }
            else
            {
                parms.Clear();
                outParams = null;
            }

            return bSuccess;
        }

        static private string EscapeURL(string inUrl)
        {
            #if USE_WEBREQUEST
            return UnityEngine.Networking.UnityWebRequest.EscapeURL(inUrl);
            #else
            return UnityEngine.WWW.EscapeURL(inUrl);
            #endif // USE_WEBREQUEST
        }

        static private StringSlice UnEscapeURL(StringSlice inUrl)
        {
            if (!inUrl.Contains('%') && !inUrl.Contains('+'))
                return inUrl;
            
            #if USE_WEBREQUEST
            return UnityEngine.Networking.UnityWebRequest.UnEscapeURL(inUrl.ToString());
            #else
            return UnityEngine.WWW.UnEscapeURL(inUrl.ToString());
            #endif // USE_WEBREQUEST
        }

        static private readonly char[] SplitChars = new char[] { '&' };

        #endregion

        private int GetParamIndexByKey(string inKey, bool inbCreate = false)
        {
            int parmIdx = -1;
            if (m_Parameters != null)
            {
                for (int i = 0; i < m_Parameters.Count; ++i)
                {
                    Parameter check = m_Parameters[i];
                    if (check.Key.Equals(inKey, StringComparison.OrdinalIgnoreCase))
                    {
                        parmIdx = i;
                        break;
                    }
                }
            }

            if (parmIdx == -1 && inbCreate)
            {
                if (m_Parameters == null)
                    m_Parameters = new RingBuffer<Parameter>();

                parmIdx = m_Parameters.Count;
                Parameter parm = new Parameter();

                parm = new Parameter();
                parm.Key = inKey;
                m_Parameters.PushBack(parm);
            }

            return parmIdx;
        }
    }
}