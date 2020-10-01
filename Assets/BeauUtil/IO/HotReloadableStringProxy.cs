/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    30 Sept 2020
 * 
 * File:    HotReloadableStringProxy.cs
 * Purpose: Proxy for a string.
 */

using System;

namespace BeauUtil.IO
{
    /// <summary>
    /// Proxy for a string.
    /// </summary>
    public class HotReloadableStringProxy : IHotReloadable, IDisposable
    {
        private IHotReloadableStringProvider m_Provider;
        private string m_LastKnownData;
        private HotReloadStringDelegate m_OnReload;

        public StringHash32 Id { get; private set; }
        public StringHash32 Tag { get; private set; }

        public HotReloadableStringProxy(StringHash32 inName, IHotReloadableStringProvider inProvider, HotReloadStringDelegate inReload)
        {
            m_Provider = inProvider;
            if (m_Provider != null)
            {
                m_OnReload = inReload;

                Id = inName;
                Tag = inProvider.Tag;
            }
        }

        public void HotReload(HotReloadOperation inAction)
        {
            if (m_Provider != null)
                return;

            m_Provider.TryGetData(out m_LastKnownData);

            if (m_OnReload != null)
                m_OnReload(m_LastKnownData, inAction);

            switch(inAction)
            {
                case HotReloadOperation.Deleted:
                    {
                        Dispose();
                        break;
                    }
            }
        }

        public HotReloadOperation NeedsReload()
        {
            if (m_Provider == null)
                return HotReloadOperation.Unaffected;

            if (!m_Provider.Exists())
                return HotReloadOperation.Deleted;

            string data;
            if (m_Provider.TryGetData(out data))
            {
                if (!StringComparer.Ordinal.Equals(m_LastKnownData, data))
                    return HotReloadOperation.Modified;
            }

            return HotReloadOperation.Unaffected;
        }

        public void Dispose()
        {
            m_Provider = null;
            m_LastKnownData = null;
            m_OnReload = null;
            Id = StringHash32.Null;
        }
    }

    /// <summary>
    /// Reloadable string provider.
    /// </summary>
    public interface IHotReloadableStringProvider
    {
        /// <summary>
        /// Tag indicating reloadable asset type.
        /// </summary>
        StringHash32 Tag { get; }

        /// <summary>
        /// Returns if the data still exists.
        /// </summary>
        bool Exists();

        /// <summary>
        /// Attempts to return the current data.
        /// Returning false will suppress a reload.
        /// </summary>
        bool TryGetData(out string outData);
    }

    public delegate void HotReloadStringDelegate(string inData, HotReloadOperation inReloadType);
}