/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    30 Sept 2020
 * 
 * File:    IOHelper.cs
 * Purpose: Utility methods for IO.
 */

using System;
using System.IO;
using System.Runtime.CompilerServices;

#if UNITY_EDITOR
using UnityEditor;
#endif // UNITY_EDITOR

namespace BeauUtil.IO
{
    /// <summary>
    /// Input-output utility methods.
    /// </summary>
    static public class IOHelper
    {
        /// <summary>
        /// Returns the last modified time for the given file.
        /// </summary>
        static public long GetFileModifyTimestamp(string inFilePath)
        {
            try
            {
                return File.GetLastWriteTimeUtc(inFilePath).ToFileTimeUtc();
            }
            catch(Exception e)
            {
                UnityEngine.Debug.LogException(e);
                return 0;
            }
        }

        /// <summary>
        /// Returns the last modified time for the given asset.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public long GetAssetModifyTimestamp(UnityEngine.Object inObject)
        {
            #if UNITY_EDITOR
            if (inObject.IsReferenceNull())
                return 0;

            string assetPath = AssetDatabase.GetAssetPath(inObject);
            return GetFileModifyTimestamp(assetPath);
            #else
            return 0;
            #endif // UNITY_EDITOR
        }

        /// <summary>
        /// Returns an identifier for the given asset.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public StringHash32 GetAssetIdentifier(UnityEngine.Object inObject)
        {
            if (inObject.IsReferenceNull())
                return StringHash32.Null;

            #if UNITY_EDITOR
            string assetPath = AssetDatabase.GetAssetPath(inObject);
            if (AssetDatabase.IsMainAsset(inObject))
                return string.Format("{0}::{1}", inObject.GetType().Name, assetPath);
        
            return string.Format("{0}::{1}->{2}", inObject.GetType().Name, assetPath, inObject.name);
            #else
            return string.Format("{0}::{1}({2})", inObject.GetType().Name, inObject.name, inObject.GetInstanceID());
            #endif // UNITY_EDITOR
        }

        /// <summary>
        /// Returns a relative path.
        /// </summary>
        static public string GetRelativePath(string inRelativeTo, string inPath)
        {
            inPath = inPath.Replace('\\', '/');
            inRelativeTo = inRelativeTo.Replace('\\', '/');

            if (Path.IsPathRooted(inPath) && inPath.StartsWith(inRelativeTo))
            {
                return inPath.Substring(inRelativeTo.Length + 1);
            }

            return inPath;
        }

#if UNITY_EDITOR
        static private readonly string s_CachedDataPathParent = Directory.GetParent(UnityEngine.Application.dataPath).FullName;
#endif // UNITY_EDITOR

        [Obsolete("IOHelper.GetRelativePath(string) renamed to GetLogicalPath", false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public string GetRelativePath(string inPath)
        {
            return GetLogicalPath(inPath);
        }

        /// <summary>
        /// Returns the relative path to the current working directory, or project directory if in the editor.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public string GetLogicalPath(string inPath)
        {
#if UNITY_EDITOR
#if UNITY_2021_2_OR_NEWER
            return FileUtil.GetLogicalPath(inPath);
#else
            return GetRelativePath(s_CachedDataPathParent, inPath);
#endif // UNITY_2021_2_OR_NEWER
#else
            return GetRelativePath(Environment.CurrentDirectory, inPath);
#endif // UNITY_EDITOR
        }

        /// <summary>
        /// Returns the absolute path to the given logical path.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public string GetPhysicalPath(string inPath)
        {
#if UNITY_EDITOR
#if UNITY_2021_2_OR_NEWER
            return FileUtil.GetPhysicalPath(inPath);
#else
            return Path.Combine(s_CachedDataPathParent, inPath);
#endif // UNITY_2021_2_OR_NEWER
#else
            return Path.Combine(Environment.CurrentDirectory, inPath);
#endif // UNITY_EDITOR
        }
    }
}