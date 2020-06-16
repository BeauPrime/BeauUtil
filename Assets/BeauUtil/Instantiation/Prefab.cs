/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    4 April 2019
 * 
 * File:    Prefab.cs
 * Purpose: Base class for the prefab attribute.
            Contains helper methods for loading/unloading prefabs.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BeauUtil
{
    /// <summary>
    /// Allows for instantiation of prefabs by referencing their types.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public abstract class Prefab : Attribute
    {
        #region Abstract

        /// <summary>
        /// Returns the name of the given variant.
        /// </summary>
        protected abstract string GetVariantName();

        /// <summary>
        /// Ensures the prefab is loaded. Blocking call.
        /// </summary>
        protected abstract void EnsureLoaded<T>() where T : MonoBehaviour;

        /// <summary>
        /// Ensures the prefab is loaded. Asynchronous call.
        /// </summary>
        protected abstract IEnumerator EnsureLoadedAsync<T>() where T : MonoBehaviour;

        /// <summary>
        /// Ensures a prefab is unloaded.
        /// </summary>
        protected abstract void EnsureUnloaded<T>() where T : MonoBehaviour;

        /// <summary>
        /// Returns if the prefab is loaded.
        /// </summary>
        protected abstract bool GetLoaded<T>() where T : MonoBehaviour;

        /// <summary>
        /// Returns the prefab itself.
        /// </summary>
        protected abstract T GetPrefab<T>() where T : MonoBehaviour;

        /// <summary>
        /// Spawns an instance of the prefab.
        /// </summary>
        protected abstract T Spawn<T>() where T : MonoBehaviour;

        #endregion

        #region Loading/Unloading

        /// <summary>
        /// Loads the prefab for the given type.
        /// </summary>
        static public void Load<T>(string inVariant = null) where T : MonoBehaviour
        {
            Prefab prefab = GetPrefab(typeof(T), inVariant, true);
            if (prefab != null)
                prefab.EnsureLoaded<T>();
            else
                Debug.LogError("[Prefab] No prefab associated with the given type " + typeof(T).Name);
        }

        /// <summary>
        /// Asynchronously loads the prefab for the given type.
        /// </summary>
        static public IEnumerator LoadAsync<T>(string inVariant = null) where T : MonoBehaviour
        {
            Prefab prefab = GetPrefab(typeof(T), inVariant, true);
            if (prefab != null)
                return prefab.EnsureLoadedAsync<T>();
            Debug.LogError("[Prefab] No prefab associated with the given type " + typeof(T).Name);
            return null;
        }

        /// <summary>
        /// Returns if the prefab for the given type is loaded.
        /// </summary>
        static public bool IsLoaded<T>(string inVariant = null) where T : MonoBehaviour
        {
            Prefab prefab = GetPrefab(typeof(T), inVariant, false);
            return prefab != null && prefab.GetLoaded<T>();
        }

        /// <summary>
        /// Returns the prefab for the given type.
        /// </summary>
        static public T Get<T>(string inVariant = null) where T : MonoBehaviour
        {
            Prefab prefab = GetPrefab(typeof(T), inVariant, false);
            return prefab == null ? null : prefab.GetPrefab<T>();
        }

        /// <summary>
        /// Unloads the prefab for the given type.
        /// </summary>
        static public void Unload<T>(string inVariant = null) where T : MonoBehaviour
        {
            Prefab prefab = GetPrefab(typeof(T), inVariant, false);
            if (prefab != null)
                prefab.EnsureUnloaded<T>();
        }

        #endregion

        #region Instantiation

        /// <summary>
        /// Instantiates the prefab for the given type.
        /// </summary>
        static public T Instantiate<T>(string inVariant = null) where T : MonoBehaviour
        {
            Prefab prefab = GetPrefab(typeof(T), inVariant, true);
            if (prefab != null)
                return prefab.Spawn<T>();
            Debug.LogError("[Prefab] No prefab associated with the given type " + typeof(T).Name);
            return null;
        }

        /// <summary>
        /// Instantiates the prefab for the given type
        /// at the given world position.
        /// </summary>
        static public T Instantiate<T>(Vector3 inPosition) where T : MonoBehaviour
        {
            Prefab prefab = GetPrefab(typeof(T), string.Empty, true);
            if (prefab != null)
            {
                T spawned = prefab.Spawn<T>();
                spawned.transform.position = inPosition;
                return spawned;
            }
            Debug.LogError("[Prefab] No prefab associated with the given type " + typeof(T).Name);
            return null;
        }

        /// <summary>
        /// Instantiates the prefab for the given type
        /// at the given world position.
        /// </summary>
        static public T Instantiate<T>(string inVariant, Vector3 inPosition) where T : MonoBehaviour
        {
            Prefab prefab = GetPrefab(typeof(T), inVariant, true);
            if (prefab != null)
            {
                T spawned = prefab.Spawn<T>();
                spawned.transform.position = inPosition;
                return spawned;
            }
            Debug.LogError("[Prefab] No prefab associated with the given type " + typeof(T).Name);
            return null;
        }

        #endregion

        #region Cache

        private sealed class PrefabGroup
        {
            private Dictionary<string, Prefab> m_Prefabs;
            private Prefab m_Default;

            public PrefabGroup(Prefab[] inPrefabs)
            {
                m_Prefabs = new Dictionary<string, Prefab>(inPrefabs.Length);
                for(int i = 0; i < inPrefabs.Length; ++i)
                {
                    string variantName = inPrefabs[i].GetVariantName();
                    if (m_Default == null && string.IsNullOrEmpty(variantName))
                        m_Default = inPrefabs[i];

                    m_Prefabs[variantName] = inPrefabs[i];
                }

                // If none of the variants are defined as default,
                // just take the first one
                if (m_Default == null)
                    m_Default = inPrefabs[0];
            }

            public bool TryGetValue(string inVariantName, out Prefab outPrefab)
            {
                if (String.IsNullOrEmpty(inVariantName))
                {
                    outPrefab = m_Default;
                    return true;
                }

                if (!m_Prefabs.TryGetValue(inVariantName, out outPrefab))
                {
                    Debug.LogError("[Prefab] Unable to find variant with given name. Using default prefab.");
                    outPrefab = m_Default;
                    return false;
                }
                return true;
            }
        }

        static private Dictionary<Int64, PrefabGroup> s_CachedPrefabs = new Dictionary<Int64, PrefabGroup>();

        // Returns the prefab attribute for the given type.
        static private Prefab GetPrefab(Type inType, string inVariantName, bool inbLoad)
        {
            if (inVariantName == null)
                inVariantName = string.Empty;

            Prefab prefab = null;
            PrefabGroup prefabGroup;

            if (!s_CachedPrefabs.TryGetValue(inType.TypeHandle.Value.ToInt64(), out prefabGroup) && inbLoad)
            {
                Prefab[] prefabs = (Prefab[])Attribute.GetCustomAttributes(inType, typeof(Prefab), false);
                if (prefabs != null && prefabs.Length > 0)
                {
                    prefabGroup = new PrefabGroup(prefabs);
                    s_CachedPrefabs.Add(inType.TypeHandle.Value.ToInt64(), prefabGroup);
                }
            }

            if (prefabGroup != null)
                prefabGroup.TryGetValue(inVariantName, out prefab);

            return prefab;
        }

        #endregion
    }
}
