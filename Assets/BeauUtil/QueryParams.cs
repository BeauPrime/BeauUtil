/*
 * Copyright (C) 2017-2019. Filament Games, LLC. All rights reserved.
 * Author:  Alex Beauchesne
 * Date:    4 April 2019
 * 
 * File:    QueryParams.cs
 * Purpose: Query parameter parsing utility.
 */

#if UNITY_2018_3_OR_NEWER
#define USE_WEBREQUEST
#endif // UNITY_2018_3_OR_NEWER

using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace BeauUtil
{
    /// <summary>
    /// Encodes and decodes query parameters.
    /// Keys are case-insensitive.
    /// </summary>
    public class QueryParams
    {
        private class Parameter
        {
            public string Key;
            public string Value;

            public void Create(string inKeyValue)
            {
                Key = inKeyValue;
                Value = null;
            }

            public void Create(string inKey, string inValue)
            {
                Key = inKey;
                Value = inValue;
            }
        }

        private List<Parameter> m_Parameters;

        #region Get/Set

        /// <summary>
        /// Returns if a parameter with the given key exists.
        /// </summary>
        public bool Contains(string inKey)
        {
            return GetParamByKey(inKey) != null;
        }

        /// <summary>
        /// Sets a parameter with no value.
        /// </summary>
        public void Set(string inKey)
        {
            Parameter parm = GetParamByKey(inKey, true);
            parm.Value = null;
        }

        /// <summary>
        /// Sets a parameter with the given value.
        /// </summary>
        public void Set(string inKey, string inValue)
        {
            Parameter parm = GetParamByKey(inKey, true);
            parm.Value = inValue;
        }

        /// <summary>
        /// Removes the parameter with the given key.
        /// </summary>
        public void Remove(string inKey)
        {
            Parameter parm = GetParamByKey(inKey, false);
            if (parm != null)
                m_Parameters.Remove(parm);
        }

        /// <summary>
        /// Returns the value of the parameter with the given key.
        /// </summary>
        public string Get(string inKey)
        {
            Parameter parm = GetParamByKey(inKey, false);
            return parm == null ? null : parm.Value;
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

            using(PooledStringBuilder pooledBuilder = PooledStringBuilder.Create())
            {
                StringBuilder builder = pooledBuilder.Builder;

                builder.Append('?');
                for (int i = 0; i < m_Parameters.Count; ++i)
                {
                    Parameter parm = m_Parameters[i];
                    if (i > 0)
                        builder.Append('&');

                    builder.Append(EscapeURL(parm.Key));
                    if (parm.Value != null)
                    {
                        builder.Append('=').Append(EscapeURL(parm.Value));
                    }
                }

                return builder.ToString();
            }
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

            string paramSection = inURL.Substring(queryStartIdx + 1);
            string[] paramChunks = paramSection.Split('&');

            if (m_Parameters == null)
            {
                m_Parameters = new List<Parameter>(paramChunks.Length);
            }
            else
            {
                m_Parameters.Clear();
                if (m_Parameters.Capacity < paramChunks.Length)
                    m_Parameters.Capacity = paramChunks.Length;
            }

            for (int i = 0; i < paramChunks.Length; ++i)
            {
                string chunk = paramChunks[i];
                if (chunk.Length == 0 || chunk[0] == '=')
                    continue;

                Parameter parm = new Parameter();

                int equalIdx = chunk.IndexOf('=');
                if (equalIdx < 0)
                {
                    string unescaped = UnEscapeURL(chunk);
                    parm.Create(unescaped);
                }
                else
                {
                    string key = UnEscapeURL(chunk.Substring(0, equalIdx));
                    string value = equalIdx >= chunk.Length - 1 ? string.Empty : UnEscapeURL(chunk.Substring(equalIdx + 1));
                    if (value.Equals("null", StringComparison.Ordinal))
                        value = null;

                    parm.Create(key, value);
                }

                m_Parameters.Add(parm);
            }

            return true;
        }

        #endregion

        #region Static

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
            return UnityEngine.Networking.UnityWebRequest.UnEscapeURL(inUrl);
            #else
            return WWW.UnEscapeURL(inUrl);
            #endif // USE_WEBREQUEST
        }

        static private string UnEscapeURL(string inUrl)
        {
            #if USE_WEBREQUEST
            return UnityEngine.Networking.UnityWebRequest.EscapeURL(inUrl);
            #else
            return WWW.EscapeURL(inUrl);
            #endif // USE_WEBREQUEST
        }

        #endregion

        private Parameter GetParamByKey(string inKey, bool inbCreate = false)
        {
            Parameter parm = null;
            if (m_Parameters != null)
            {
                for (int i = 0; i < m_Parameters.Count; ++i)
                {
                    Parameter check = m_Parameters[i];
                    if (check.Key.Equals(inKey, StringComparison.OrdinalIgnoreCase))
                    {
                        parm = check;
                        break;
                    }
                }
            }

            if (parm == null && inbCreate)
            {
                if (m_Parameters == null)
                    m_Parameters = new List<Parameter>();

                parm = new Parameter();
                parm.Key = inKey;
                m_Parameters.Add(parm);
            }

            return parm;
        }
    }
}