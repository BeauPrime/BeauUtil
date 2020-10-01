/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    30 Sept 2020
 * 
 * File:    HotReloadableFileProxy.cs
 * Purpose: Proxy for a reloadable file.
 */

using System;
using System.IO;

namespace BeauUtil.IO
{
    /// <summary>
    /// Proxy for a reloadable file.
    /// </summary>
    public class HotReloadableFileProxy : IHotReloadable, IDisposable
    {
        private string m_ResourcePath;
        private long m_LastEditTime;
        private HotReloadFileDelegate m_OnReload;

        public StringHash32 Id { get; private set; }
        public StringHash32 Tag { get; private set; }

        public HotReloadableFileProxy(string inFilePath, HotReloadFileDelegate inReload)
            : this(inFilePath, StringHash32.Null, inReload)
        { }

        public HotReloadableFileProxy(string inFilePath, StringHash32 inTag, HotReloadFileDelegate inReload)
        {
            m_ResourcePath = inFilePath;
            if (!String.IsNullOrEmpty(inFilePath))
            {
                m_LastEditTime = IOHelper.GetFileModifyTimestamp(inFilePath);
                m_OnReload = inReload;

                Id = inFilePath;
                Tag = inTag.IsEmpty ? Path.GetExtension(inFilePath).ToLowerInvariant() : inTag;
            }
        }

        public void HotReload(HotReloadOperation inAction)
        {
            if (string.IsNullOrEmpty(m_ResourcePath))
                return;

            if (m_OnReload != null)
                m_OnReload(m_ResourcePath, inAction);

            switch(inAction)
            {
                case HotReloadOperation.Deleted:
                    {
                        Dispose();
                        break;
                    }

                case HotReloadOperation.Modified:
                    {
                        m_LastEditTime = IOHelper.GetFileModifyTimestamp(m_ResourcePath);
                        break;
                    }
            }
        }

        public HotReloadOperation NeedsReload()
        {
            if (string.IsNullOrEmpty(m_ResourcePath))
                return HotReloadOperation.Unaffected;

            if (!File.Exists(m_ResourcePath))
                return HotReloadOperation.Deleted;

            long fsFileTime = IOHelper.GetFileModifyTimestamp(m_ResourcePath);
            if (m_LastEditTime != fsFileTime)
                return HotReloadOperation.Modified;

            return HotReloadOperation.Unaffected;
        }

        public void Dispose()
        {
            m_ResourcePath = null;
            m_LastEditTime = 0;
            m_OnReload = null;
            Id = StringHash32.Null;
        }
    }

    public delegate void HotReloadFileDelegate(string inFilePath, HotReloadOperation inReloadType);
}