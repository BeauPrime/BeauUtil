/*
 * Copyright (C) 2017-2021. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    1 Feb 2021
 * 
 * File:    CustomTextAsset.cs
 * Purpose: Custom text asset.
 */

using UnityEngine;
using System.IO;
using System.Text;
using System;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Experimental.AssetImporters;
#endif // UNITY_EDITOR  

namespace BeauUtil.Blocks
{
    /// <summary>
    /// Custom text asset wrapper.
    /// </summary>
    public abstract class CustomTextAsset : ScriptableObject
    {
        #region Inspector

        [SerializeField, HideInInspector] private byte[] m_Bytes = null;

        #endregion // Inspector

        [NonSerialized] private string m_CachedString = null;
        [NonSerialized] private StringHash32 m_CachedNameHash = null;

        /// <summary>
        /// Hashed string name.
        /// </summary>
        public StringHash32 NameHash { get { return m_CachedNameHash.IsEmpty ? (m_CachedNameHash = name) : m_CachedNameHash; } }

        #region Data

        /// <summary>
        /// Raw bytes making up the source text.
        /// </summary>
        public byte[] Bytes()
        {
            return m_Bytes;
        }

        /// <summary>
        /// Returns the source text.
        /// </summary>
        public string Source()
        {
            return m_CachedString ?? (m_CachedString = Encoding.UTF8.GetString(m_Bytes));
        }

        private void Create(byte[] inBytes)
        {
            m_Bytes = inBytes;
            m_CachedString = null;
        }

        #endregion // Data

        #region Importer

        #if UNITY_EDITOR

        protected class ScriptedExtensionAttribute : ScriptedImporterAttribute
        {
            public ScriptedExtensionAttribute(int inVersion, string inExtension)
                : base(inVersion, inExtension)
            {}
        }

        protected abstract class ImporterBase<TAsset> : ScriptedImporter
            where TAsset : CustomTextAsset
        {
            public override void OnImportAsset(AssetImportContext ctx)
            {
                TAsset asset = ScriptableObject.CreateInstance<TAsset>();
                
                byte[] sourceBytes = File.ReadAllBytes(ctx.assetPath);
                asset.Create(sourceBytes);

                EditorUtility.SetDirty(asset);

                ctx.AddObjectToAsset("Text", asset);
                ctx.SetMainObject(asset);

                AssetDatabase.SaveAssets();
            }
        }
        
        #endif // UNITY_EDITOR

        #endregion // Importer
    }
}