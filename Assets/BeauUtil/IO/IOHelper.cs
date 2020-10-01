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
    }
}