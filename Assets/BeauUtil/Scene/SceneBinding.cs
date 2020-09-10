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
    public struct SceneBinding
    {
        public readonly StringHash Id;

        public readonly string  Name;
        public readonly string  Path;
        public readonly int     BuildIndex;

        public Scene Scene { get; private set; }

        public SceneBinding(Scene inScene)
        {
            Path = inScene.path;
            Name = inScene.name;
            BuildIndex = inScene.buildIndex;

            Scene = inScene;
            Id = Path;
        }

        public SceneBinding(int inBuildIndex, string inPath)
        {
            Path = inPath;
            Name = System.IO.Path.GetFileNameWithoutExtension(Path);
            BuildIndex = inBuildIndex;
            
            Scene = SceneManager.GetSceneByPath(Path);
            Id = Path;
        }

        public void Refresh()
        {
            if (!Scene.IsValid() || !Scene.isLoaded)
                Scene = SceneManager.GetSceneByPath(Path);
        }

        public bool IsLoaded()
        {
            Refresh();
            return Scene.isLoaded;
        }

        public void OnLoaded(object inContext = null)
        {
            Refresh();
            SceneHelper.OnLoaded(Scene, inContext);
        }

        public void OnUnload(object inContext = null)
        {
            Refresh();
            SceneHelper.OnUnload(Scene, inContext);
        }

        public bool IsValid()
        {
            return !string.IsNullOrEmpty(Path);
        }

        static public implicit operator SceneBinding(Scene inScene)
        {
            return new SceneBinding(inScene);
        }

        static public implicit operator Scene(SceneBinding inBinding)
        {
            return SceneManager.GetSceneByPath(inBinding.Path);
        }
    }
}