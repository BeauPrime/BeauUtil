/*
 * Copyright (C) 2017-2019. Filament Games, LLC. All rights reserved.
 * Author:  Alex Beauchesne
 * Date:    4 April 2019
 * 
 * File:    ResourcePrefab.cs
 * Purpose: Links a MonoBehaviour class to a prefab in Resources.
 */

using System;
using System.Collections;
using UnityEngine;

namespace BeauUtil
{
    /// <summary>
    /// Type can be instantiated by loading a prefab from
    /// the Resources folder.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public sealed class ResourcePrefab : Prefab
    {
        /// <summary>
        /// Creates a ResourcePrefab directed
        /// towards the given resource path.
        /// </summary>
        public ResourcePrefab(string inPath, string inVariantName = null)
        {
            m_Path = inPath;
            m_VariantName = inVariantName == null ? string.Empty : inVariantName;
        }

        private string m_Path;
        private string m_VariantName;
        private MonoBehaviour m_Prefab;

        protected override string GetVariantName()
        {
            return m_VariantName;
        }

        protected override void EnsureLoaded<T>()
        {
            if (ReferenceEquals(m_Prefab, null))
            {
                m_Prefab = Resources.Load<T>(m_Path);
                if (m_Prefab == null)
                    throw new Exception("Unable to load resource of type " + typeof(T).FullName + " from path: " + m_Path);
            }
        }

        protected override IEnumerator EnsureLoadedAsync<T>()
        {
            if (ReferenceEquals(m_Prefab, null))
            {
                ResourceRequest request = Resources.LoadAsync<T>(m_Path);
                yield return request;
                m_Prefab = request.asset as T;
                if (m_Prefab == null)
                    throw new Exception("Unable to load resource of type " + typeof(T).FullName + " from path: " + m_Path);
            }
        }

        protected override bool GetLoaded<T>()
        {
            return !ReferenceEquals(m_Prefab, null);
        }

        protected override T GetPrefab<T>()
        {
            return (T)m_Prefab;
        }

        protected override void EnsureUnloaded<T>()
        {
            if (!ReferenceEquals(m_Prefab, null))
            {
                //Resources.UnloadAsset(m_Prefab);
                m_Prefab = null;
            }
        }

        protected override T Spawn<T>()
        {
            EnsureLoaded<T>();
            return MonoBehaviour.Instantiate<T>((T)m_Prefab);
        }
    }
}
