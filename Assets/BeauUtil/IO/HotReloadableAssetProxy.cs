/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    30 Sept 2020
 * 
 * File:    HotReloadableAssetProxy.cs
 * Purpose: Proxy for a reloadable asset.
 */

using System;

namespace BeauUtil.IO
{
    /// <summary>
    /// Proxy for a reloadable asset.
    /// </summary>
    public class HotReloadableAssetProxy<T> : IHotReloadable, IDisposable
        where T : UnityEngine.Object
    {
        static private readonly StringHash32 TypeTag = typeof(T).Name;

        public StringHash32 Id { get; private set; }
        public StringHash32 Tag { get; private set; }

        #if UNITY_EDITOR

        private T m_Asset;
        private long m_LastEditTime;
        private string m_AssetPath;
        private HotReloadAssetDelegate<T> m_OnReload;

        public HotReloadableAssetProxy(T inAsset, HotReloadAssetDelegate<T> inReload)
            : this(inAsset, TypeTag, inReload)
        {
        }

        public HotReloadableAssetProxy(T inAsset, StringHash32 inTag, HotReloadAssetDelegate<T> inReload)
        {
            m_Asset = inAsset;
            Tag = inTag;
            if (m_Asset)
            {
                m_LastEditTime = IOHelper.GetAssetModifyTimestamp(inAsset);
                m_AssetPath = UnityEditor.AssetDatabase.GetAssetPath(m_Asset);
                m_OnReload = inReload;
                Id = IOHelper.GetAssetIdentifier(inAsset);
            }
        }

        public void HotReload(HotReloadOperation inOperation)
        {
            if (m_Asset.IsReferenceNull())
                return;

            if (m_OnReload != null)
                m_OnReload.Invoke(m_Asset, inOperation);

            switch(inOperation)
            {
                case HotReloadOperation.Deleted:
                    {
                        Dispose();
                        break;
                    }

                case HotReloadOperation.Modified:
                    {
                        m_LastEditTime = IOHelper.GetAssetModifyTimestamp(m_Asset);
                        break;
                    }
            }
        }

        public HotReloadOperation NeedsReload()
        {
            if (m_Asset.IsReferenceNull())
                return HotReloadOperation.Unaffected;

            if (m_Asset.IsReferenceDestroyed())
            {
                m_Asset = UnityEditor.AssetDatabase.LoadAssetAtPath<T>(m_AssetPath);
                if (!m_Asset)
                    return HotReloadOperation.Deleted;
                Id = IOHelper.GetAssetIdentifier(m_Asset);
            }

            long fsEditTime = IOHelper.GetAssetModifyTimestamp(m_Asset);
            if (m_LastEditTime != fsEditTime)
                return fsEditTime == 0 ? HotReloadOperation.Deleted : HotReloadOperation.Modified;

            return HotReloadOperation.Unaffected;
        }

        public void Dispose()
        {
            Id = StringHash32.Null;
            m_Asset = null;
            m_LastEditTime = 0;
            m_OnReload = null;
        }

        #else

        public HotReloadableAssetProxy(T inAsset, HotReloadAssetDelegate<T> inReload)
            : this(inAsset, TypeTag, inReload)
        {
        }

        public HotReloadableAssetProxy(T inAsset, StringHash32 inTag, HotReloadAssetDelegate<T> inReload)
        {
            Tag = inTag;
            if (inAsset)
            {
                Id = IOHelper.GetAssetIdentifier(inAsset);
            }
        }

        public void HotReload(HotReloadOperation inOperation)
        {
        }

        public HotReloadOperation NeedsReload()
        {
            return HotReloadOperation.Unaffected;
        }

        public void Dispose()
        {
            Id = StringHash32.Null;
        }

        #endif // UNITY_EDITOR
    }

    public delegate void HotReloadAssetDelegate<T>(T inAsset, HotReloadOperation inReloadType) where T : UnityEngine.Object;
}