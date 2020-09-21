/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    10 Sept 2020
 * 
 * File:    SceneBinding.cs
 * Purpose: Scene wrapper struct.
 */

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BeauUtil
{
    /// <summary>
    /// Wrapper for a scene.
    /// Can accomodate unloaded scenes.
    /// </summary>
    public struct SceneBinding : IEquatable<SceneBinding>, IComparable<SceneBinding>
    {
        public readonly StringHash Id;

        public readonly string  Name;
        public readonly string  Path;
        public readonly int     BuildIndex;

        private Scene m_CachedScene;

        public Scene Scene
        {
            get { return CacheScene(); }
        }

        public SceneBinding(Scene inScene)
        {
            Path = inScene.path;
            Name = inScene.name;
            BuildIndex = inScene.buildIndex;

            m_CachedScene = inScene;
            Id = Path;
        }

        public SceneBinding(int inBuildIndex, string inPath)
        {
            Path = inPath;
            Name = System.IO.Path.GetFileNameWithoutExtension(Path);
            BuildIndex = inBuildIndex;
            
            m_CachedScene = SceneManager.GetSceneByPath(Path);
            Id = Path;
        }

        private Scene CacheScene()
        {
            if (!m_CachedScene.IsValid() || !m_CachedScene.isLoaded)
            {
                #if UNITY_EDITOR
                if (!UnityEditor.EditorApplication.isPlaying)
                    m_CachedScene = UnityEditor.SceneManagement.EditorSceneManager.GetSceneByPath(Path);
                else
                #endif
                    m_CachedScene = SceneManager.GetSceneByPath(Path);
            }
            return m_CachedScene;
        }

        /// <summary>
        /// Returns if the scene is currently loaded.
        /// </summary>
        public bool IsLoaded()
        {
            return CacheScene().isLoaded;
        }

        /// <summary>
        /// Dispatches the OnLoaded callback.
        /// </summary>
        public void BroadcastLoaded(object inContext = null)
        {
            SceneHelper.OnLoaded(CacheScene(), inContext);
        }

        /// <summary>
        /// Dispatches the OnUnload callback.
        /// </summary>
        public void BroadcastUnload(object inContext = null)
        {
            SceneHelper.OnUnload(CacheScene(), inContext);
        }

        public bool IsValid()
        {
            return !string.IsNullOrEmpty(Path);
        }

        #region IEquatable

        public bool Equals(SceneBinding other)
        {
            return Id == other.Id;
        }

        #endregion // IEquatable

        #region IComparable

        public int CompareTo(SceneBinding other)
        {
            int buildIdxComp = BuildIndex - other.BuildIndex;
            if (buildIdxComp < 0)
                return -1;
            if (buildIdxComp > 0)
                return 1;
            
            return Id.CompareTo(other.Id);
        }

        #endregion // IComparable

        #region Overrides

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj is SceneBinding)
            {
                return Equals((SceneBinding) obj);
            }

            return false;
        }

        public override string ToString()
        {
            if (Id.IsEmpty)
                return "Null Scene";
            
            return string.Format("Scene {0} (path: '{1}', buildIndex: {2})", Id.ToString(), Path, BuildIndex);
        }

        #endregion // Overrides

        #region Operators

        static public bool operator==(SceneBinding left, SceneBinding right)
        {
            return left.Id == right.Id;
        }

        static public bool operator!=(SceneBinding left, SceneBinding right)
        {
            return left.Id != right.Id;
        }

        static public implicit operator SceneBinding(Scene inScene)
        {
            return new SceneBinding(inScene);
        }

        static public implicit operator Scene(SceneBinding inBinding)
        {
            return inBinding.Scene;
        }
    
        #endregion // Operators
    }
}