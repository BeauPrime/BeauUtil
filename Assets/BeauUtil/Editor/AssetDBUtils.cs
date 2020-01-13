/*
 * Copyright (C) 2017-2020. Filament Games, LLC. All rights reserved.
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
            string[] assetGuids = AssetDatabase.FindAssets(GenerateFilter(typeof(T), inName), inSearchFolders);
            if (assetGuids == null)
                return null;
            HashSet<T> assets = new HashSet<T>();
            for (int i = 0; i < assetGuids.Length; ++i)
            {
                string path = AssetDatabase.GUIDToAssetPath(assetGuids[i]);
                Filter<T>(path, inName, typeof(T), assets);
            }
            return GetArray(assets);
        }

        /// <summary>
        /// Returns an array of all assets in the asset database that match the given type and optional name.
        /// </summary>
        static public UnityEngine.Object[] FindAssets(Type inType, string inName = null, string[] inSearchFolders = null)
        {
            string[] assetGuids = AssetDatabase.FindAssets(GenerateFilter(inType, inName), inSearchFolders);
            if (assetGuids == null)
                return null;
            HashSet<UnityEngine.Object> assets = new HashSet<UnityEngine.Object>();
            for (int i = 0; i < assetGuids.Length; ++i)
            {
                string path = AssetDatabase.GUIDToAssetPath(assetGuids[i]);
                Filter<UnityEngine.Object>(path, inName, inType, assets);
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
            string[] assetGuids = AssetDatabase.FindAssets(GenerateFilter(inType, inName), inSearchFolders);
            if (assetGuids == null || assetGuids.Length <= 0)
                return null;
            string path = AssetDatabase.GUIDToAssetPath(assetGuids[0]);
            return Filter<UnityEngine.Object>(path, inName, inType);
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

            string fullname = inType.FullName;
            if (fullname.StartsWith("UnityEngine.") || fullname.StartsWith("UnityEditor."))
            {
                fullname = fullname.Substring(12);
            }
            return "t:" + fullname;
        }

        static private void Filter<T>(string inPath, string inName, Type inType, HashSet<T> outResults) where T : UnityEngine.Object
        {
            if (inType == typeof(UnityEngine.Object))
            {
                inType = null;
            }

            string lowerSearch = inName?.ToLowerInvariant();
            foreach (var obj in AssetDatabase.LoadAllAssetsAtPath(inPath))
            {
                if (inType == null || inType.IsAssignableFrom(obj.GetType()))
                {
                    if (!string.IsNullOrEmpty(lowerSearch) && !obj.name.ToLowerInvariant().Contains(lowerSearch))
                        continue;

                    outResults.Add((T) obj);
                }
            }
        }

        static private T Filter<T>(string inPath, string inName, Type inType) where T : UnityEngine.Object
        {
            if (inType == typeof(UnityEngine.Object))
            {
                inType = null;
            }

            string lowerSearch = inName?.ToLowerInvariant();
            foreach (var obj in AssetDatabase.LoadAllAssetsAtPath(inPath))
            {
                if (inType == null || inType.IsAssignableFrom(obj.GetType()))
                {
                    if (!string.IsNullOrEmpty(lowerSearch) && !obj.name.ToLowerInvariant().Contains(lowerSearch))
                        continue;

                    return (T) obj;
                }
            }

            return null;
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