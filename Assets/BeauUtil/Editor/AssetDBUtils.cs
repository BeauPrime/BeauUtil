/*
 * Copyright (C) 2017-2019. Filament Games, LLC. All rights reserved.
 * Author:  Alex Beauchesne
 * Date:    14 Oct 2019
 * 
 * File:    AssetDBUtils.cs
 * Purpose: Utility methods for dealing with AssetDatabase
 */

using System;
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
            T[] assets = new T[assetGuids.Length];
            for (int i = 0; i < assets.Length; ++i)
            {
                string path = AssetDatabase.GUIDToAssetPath(assetGuids[i]);
                assets[i] = AssetDatabase.LoadAssetAtPath<T>(path);
            }
            return assets;
        }

        /// <summary>
        /// Returns an array of all assets in the asset database that match the given type and optional name.
        /// </summary>
        static public UnityEngine.Object[] FindAssets(Type inType, string inName = null, string[] inSearchFolders = null)
        {
            string[] assetGuids = AssetDatabase.FindAssets(GenerateFilter(inType, inName), inSearchFolders);
            if (assetGuids == null)
                return null;
            UnityEngine.Object[] assets = new UnityEngine.Object[assetGuids.Length];
            for (int i = 0; i < assets.Length; ++i)
            {
                string path = AssetDatabase.GUIDToAssetPath(assetGuids[i]);
                assets[i] = AssetDatabase.LoadAssetAtPath(path, inType);
            }
            return assets;
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
            return AssetDatabase.LoadAssetAtPath(path, inType);
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
            if (fullname.StartsWith("UnityEngine."))
                fullname = fullname.Substring(12);
            return "t:" + fullname;
        }

        #endregion // Filters
    }
}