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
#if UNITY_2020_2_OR_NEWER
using UnityEditor.AssetImporters;
#else
using UnityEditor.Experimental.AssetImporters;
#endif // UNITY_2020_2_OR_NEWER
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

        [NonSerialized] private Unsafe.PinnedArrayHandle<byte> m_PinHandle;
        [NonSerialized] private int m_PinCount;

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
        /// Returns the number of raw bytes.
        /// </summary>
        public int ByteLength()
        {
            return m_Bytes.Length;
        }

        /// <summary>
        /// Retrieves the source text and optionally caches the result.
        /// </summary>
        public string Source(bool inbCache = false)
        {
            if (inbCache)
            {
                return m_CachedString ?? (m_CachedString = Encoding.UTF8.GetString(m_Bytes));
            }
            else
            {
                return Encoding.UTF8.GetString(m_Bytes);
            }
        }

        private void Create(byte[] inBytes)
        {
            m_Bytes = inBytes;
            m_CachedString = null;
        }

        /// <summary>
        /// Pins the byte buffer and returns a pointer to it.
        /// </summary>
        public unsafe byte* PinBytes()
        {
            m_PinCount++;
            if (m_PinCount == 1)
            {
                m_PinHandle = Unsafe.PinArray(m_Bytes);
            }
            return (byte*) m_PinHandle.Address;
        }

        /// <summary>
        /// Releases the pinned byte buffer.
        /// </summary>
        public void ReleasePinnedBytes()
        {
            m_PinCount--;
            if (m_PinCount == 0)
            {
                m_PinHandle.Dispose();
                m_PinHandle = default;
            }
        }

        #endregion // Data

        #region Events

        protected virtual void OnDisable()
        {
            DisposeResources();
        }

        protected virtual void OnDestroy()
        {
            DisposeResources();
        }

        /// <summary>
        /// Disposes of any temporary resources this asset has claimed.
        /// </summary>
        public virtual void DisposeResources()
        {
            m_PinHandle.Dispose();
            m_CachedString = null;
            m_PinCount = 0;
        }

        #endregion // Events

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
            }
        }
        
        #endif // UNITY_EDITOR

        #endregion // Importer
    }
}