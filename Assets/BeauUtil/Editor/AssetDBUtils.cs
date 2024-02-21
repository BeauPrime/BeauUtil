/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    14 Oct 2019
 * 
 * File:    AssetDBUtils.cs
 * Purpose: Utility methods for dealing with AssetDatabase
 */

using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace BeauUtil.Editor
{
    /// <summary>
    /// Contains utility functions for dealing with the asset database.
    /// </summary>
    static public class AssetDBUtils
    {
        #region Finding Assets

        /// <summary>
        /// Returns an array of all assets in the asset database that match the given type and optional name.
        /// </summary>
        static public T[] FindAssets<T>(string inName = null, string[] inSearchFolders = null) where T : UnityEngine.Object
        {
            WildcardMatch match = WildcardMatch.Compile(inName, '*', true);
            string[] assetGuids = AssetDatabase.FindAssets(GenerateFilter(typeof(T), match.Pattern), inSearchFolders);
            if (assetGuids == null)
                return null;
            HashSet<T> assets = new HashSet<T>();
            for (int i = 0; i < assetGuids.Length; ++i)
            {
                string path = AssetDatabase.GUIDToAssetPath(assetGuids[i]);
                Filter<T>(path, match, typeof(T), assets);
            }
            return GetArray(assets);
        }

        /// <summary>
        /// Returns an array of all assets in the asset database that match the given type and optional name.
        /// </summary>
        static public UnityEngine.Object[] FindAssets(Type inType, string inName = null, string[] inSearchFolders = null)
        {
            WildcardMatch match = WildcardMatch.Compile(inName, '*', true);
            string[] assetGuids = AssetDatabase.FindAssets(GenerateFilter(inType, match.Pattern), inSearchFolders);
            if (assetGuids == null)
                return null;
            HashSet<UnityEngine.Object> assets = new HashSet<UnityEngine.Object>();
            for (int i = 0; i < assetGuids.Length; ++i)
            {
                string path = AssetDatabase.GUIDToAssetPath(assetGuids[i]);
                Filter<UnityEngine.Object>(path, match, inType, assets);
            }
            return GetArray(assets);
        }

        /// <summary>
        /// Returns the first asset in the asset database that matches the given type and optional name.
        /// </summary>
        static public T FindAsset<T>(string inName = null, string[] inSearchFolders = null) where T : UnityEngine.Object
        {
            return (T) FindAsset(typeof(T), inName, inSearchFolders);
        }

        /// <summary>
        /// Returns the first asset in the asset database that matches the given type and optional name.
        /// </summary>
        static public UnityEngine.Object FindAsset(Type inType, string inName = null, string[] inSearchFolders = null)
        {
            WildcardMatch match = WildcardMatch.Compile(inName, '*', false);
            string[] assetGuids = AssetDatabase.FindAssets(GenerateFilter(inType, match.Pattern), inSearchFolders);
            if (assetGuids == null || assetGuids.Length <= 0)
                return null;
            for (int i = 0; i < assetGuids.Length; ++i)
            {
                string path = AssetDatabase.GUIDToAssetPath(assetGuids[i]);
                if (TryFilter<UnityEngine.Object>(path, match, inType, out UnityEngine.Object obj))
                    return obj;
            }

            return null;
        }

        #endregion // Finding Assets

        #region Filters

        static private string GenerateFilter(Type inType, string inName)
        {
            StringBuilder sb = new StringBuilder();
            if (inType != null)
            {
                sb.Append(GenerateFilter(inType));
            }
            if (!string.IsNullOrEmpty(inName))
            {
                if (sb.Length > 0)
                    sb.Append(' ');
                sb.Append(inName);
            }
            return sb.ToString();
        }

        static private string GenerateFilter(Type inType)
        {
            if (inType == null)
                return string.Empty;

            // if we're looking for a component, then search for prefabs
            if (typeof(UnityEngine.Component).IsAssignableFrom(inType))
                return "t:Prefab";

            string fullname = inType.FullName;
            if (fullname.StartsWith("UnityEngine.") || fullname.StartsWith("UnityEditor."))
            {
                fullname = fullname.Substring(12);
            }
            return "t:" + fullname;
        }

        static private void Filter<T>(string inPath, WildcardMatch inName, Type inType, HashSet<T> outResults) where T : UnityEngine.Object
        {
            if (inType == typeof(UnityEngine.Object))
            {
                inType = null;
            }

            if (inType == typeof(UnityEditor.SceneAsset))
            {
                SceneAsset asset = AssetDatabase.LoadAssetAtPath<SceneAsset>(inPath);
                if (asset)
                {
                    outResults.Add((T) (object) asset);
                }
                return;
            }

            if (inType != null && typeof(Component).IsAssignableFrom(inType))
            {
                GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(inPath);
                if (!go)
                    return;
                if (!inName.Match(go.name))
                    return;
                Component c = go.GetComponent(inType);
                if (c != null)
                {
                    outResults.Add((T) (object) c);
                }
                return;
            }

            foreach (var obj in AssetDatabase.LoadAllAssetsAtPath(inPath))
            {
                if (!obj)
                    continue;
                if (inType == null || inType.IsAssignableFrom(obj.GetType()))
                {
                    if (!inName.Match(obj.name))
                        continue;

                    outResults.Add((T) obj);
                }
            }
        }

        static private bool TryFilter<T>(string inPath, WildcardMatch inName, Type inType, out T outObject) where T : UnityEngine.Object
        {
            if (inType == typeof(UnityEngine.Object))
            {
                inType = null;
            }

            if (inType == typeof(UnityEditor.SceneAsset))
            {
                outObject = ((T) (object) AssetDatabase.LoadAssetAtPath<SceneAsset>(inPath));
                return outObject != null;
            }

            if (inType != null && typeof(Component).IsAssignableFrom(inType))
            {
                GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(inPath);
                if (!go)
                {
                    outObject = null;
                    return false;
                }
                if (!inName.Match(go.name))
                {
                    outObject = null;
                    return false;
                }
                Component c = go.GetComponent(inType);
                if (c != null)
                {
                    outObject = (T) (object) c;
                    return true;
                }

                outObject = null;
                return false;
            }

            foreach (var obj in AssetDatabase.LoadAllAssetsAtPath(inPath))
            {
                if (!obj)
                    continue;

                if (inType == null || inType.IsAssignableFrom(obj.GetType()))
                {
                    if (!inName.Match(obj.name))
                        continue;

                    outObject = (T) obj;
                    return true;
                }
            }

            outObject = null;
            return false;
        }

        static private T[] GetArray<T>(HashSet<T> inSet)
        {
            if (inSet == null)
                return null;

            T[] array = new T[inSet.Count];
            inSet.CopyTo(array);
            return array;
        }

        #endregion // Filters
    }
}