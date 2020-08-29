/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    28 July 2020
 * 
 * File:    SceneHelper.cs
 * Purpose: Scene utility methods.
 */

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BeauUtil
{
    /// <summary>
    /// Scene utility methods.
    /// </summary>
    static public class SceneHelper
    {
        static SceneHelper()
        {
            s_IgnoredSceneFilters = new List<SceneFilter>(4);
            foreach(var sceneName in s_DefaultIgnoreSceneNames)
            {
                s_IgnoredSceneFilters.Add(new SceneFilter(sceneName, null));
            }
        }

        #region Load/Unload Events

        /// <summary>
        /// Delegate to execute when a scene is loaded/unloaded.
        /// </summary>
        public delegate void SceneLoadAction(Scene inScene, object inContext);

        /// <summary>
        /// Event invoked when a scene indicates it is finished loading
        /// by invoking its OnLoaded method.
        /// </summary>
        static public event SceneLoadAction OnSceneLoaded;

        /// <summary>
        /// Event invoked when a scene indicates it is preparing to unload
        /// by invoking its OnUnloadMethod.
        /// </summary>
        static public event SceneLoadAction OnSceneUnload;
        
        /// <summary>
        /// Indicates that this scene is loaded.
        /// Will dispatch to all ISceneLoadHandler components in the scene
        /// and invoke SceneHelper.OnSceneLoaded
        /// </summary>
        static public void OnLoaded(this Scene inScene, object inContext = null)
        {
            inScene.ForEachComponent<ISceneLoadHandler>(true, inContext, s_DispatchSceneLoad);
            if (OnSceneLoaded != null)
            {
                OnSceneLoaded(inScene, inContext);
            }
        }

        /// <summary>
        /// Indicates that this scene is preparing to unload.
        /// Will dispatch to all ISceneUnloadHandler components in the scene
        /// and invoke SceneHelper.OnSceneUnload
        /// </summary>
        static public void OnUnload(this Scene inScene, object inContext = null)
        {
            inScene.ForEachComponent<ISceneUnloadHandler>(true, inContext, s_DispatchSceneUnload);
            if (OnSceneUnload != null)
            {
                OnSceneUnload(inScene, inContext);
            }
        }

        static private readonly SceneComponentContextAction<ISceneLoadHandler> s_DispatchSceneLoad = (scene, handler, context) => handler.OnSceneLoad(scene, context);
        static private readonly SceneComponentContextAction<ISceneUnloadHandler> s_DispatchSceneUnload = (scene, handler, context) => handler.OnSceneUnload(scene, context);

        #endregion // Load/Unload Events

        #region Scene Filtering

        private struct SceneFilter
        {
            public string Name;
            public string Path;

            public SceneFilter(string inNameFilter, string inPathFilter)
            {
                Name = inNameFilter;
                Path = inPathFilter;
            }

            public bool Match(Scene inScene)
            {
                if (!string.IsNullOrEmpty(Name))
                {
                    return StringUtils.WildcardMatch(inScene.name, Name);
                }

                if (!string.IsNullOrEmpty(Path))
                {
                    return StringUtils.WildcardMatch(inScene.path, Path);
                }

                return false;
            }
        }

        static private readonly string[] s_DefaultIgnoreSceneNames = new string[] { "DontDestroyOnLoad" };
        static private readonly List<SceneFilter> s_IgnoredSceneFilters;

        static public void IgnoreSceneByName(string inNameFilter)
        {
            if (string.IsNullOrEmpty(inNameFilter))
                return;

            for(int i = s_IgnoredSceneFilters.Count - 1; i >= 0; --i)
            {
                if (StringComparer.Ordinal.Equals(s_IgnoredSceneFilters[i].Name, inNameFilter))
                {
                    return;
                }
            }

            s_IgnoredSceneFilters.Add(new SceneFilter(inNameFilter, null));
        }

        static public void IgnoreSceneByPath(string inPathFilter)
        {
            if (string.IsNullOrEmpty(inPathFilter))
                return;

            for(int i = s_IgnoredSceneFilters.Count - 1; i >= 0; --i)
            {
                if (StringComparer.Ordinal.Equals(s_IgnoredSceneFilters[i].Path, inPathFilter))
                {
                    return;
                }
            }

            s_IgnoredSceneFilters.Add(new SceneFilter(null, inPathFilter));
        }

        static public void AllowSceneByName(string inNameFilter)
        {
            if (string.IsNullOrEmpty(inNameFilter))
                return;

            for(int i = s_IgnoredSceneFilters.Count - 1; i >= 0; --i)
            {
                if (StringComparer.Ordinal.Equals(s_IgnoredSceneFilters[i].Name, inNameFilter))
                {
                    ListUtils.FastRemoveAt(s_IgnoredSceneFilters, i);
                    return;
                }
            }
        }

        static public void AllowSceneByPath(string inPathFilter)
        {
            if (string.IsNullOrEmpty(inPathFilter))
                return;

            for(int i = s_IgnoredSceneFilters.Count - 1; i >= 0; --i)
            {
                if (StringComparer.Ordinal.Equals(s_IgnoredSceneFilters[i].Path, inPathFilter))
                {
                    ListUtils.FastRemoveAt(s_IgnoredSceneFilters, i);
                    return;
                }
            }
        }

        /// <summary>
        /// Returns if the given scene is ignored by filters.
        /// </summary>
        static public bool IsIgnored(Scene inScene)
        {
            for(int i = 0; i < s_IgnoredSceneFilters.Count; ++i)
            {
                if (s_IgnoredSceneFilters[i].Match(inScene))
                    return true;
            }

            return false;
        }

        #endregion // Scene Filtering

        #region Scene List

        /// <summary>
        /// Finds scenes, filtering by category.
        /// </summary>
        static public IEnumerable<Scene> FindScenes(SceneCategories inCategories)
        {
            bool bBuild = (inCategories & SceneCategories.Build) == SceneCategories.Build;
            bool bLoaded = (inCategories & SceneCategories.Loaded) == SceneCategories.Loaded;
            bool bActive = (inCategories & SceneCategories.ActiveOnly) == SceneCategories.ActiveOnly;
            bool bIncludeIgnored = (inCategories & SceneCategories.IncludeIgnored) == SceneCategories.IncludeIgnored;

            if (bActive)
            {
                Scene active = SceneManager.GetActiveScene();
                if (!bIncludeIgnored && IsIgnored(active))
                    yield break;

                if (bBuild && active.buildIndex < 0)
                    yield break;

                if (bLoaded && !active.isLoaded)
                    yield break;

                yield return active;
            }
            else if (bBuild)
            {
                foreach(var scene in AllBuildScenes(bIncludeIgnored))
                {
                    if (bLoaded && !scene.isLoaded)
                        continue;

                    yield return scene;
                }
            }
            else if (bLoaded)
            {
                foreach(var scene in AllLoadedScenes(bIncludeIgnored))
                {
                    yield return scene;
                }
            }
        }

        /// <summary>
        /// Finds a scene, filtering by category.
        /// </summary>
        static public Scene FindScene(SceneCategories inCategories)
        {
            foreach(var scene in FindScenes(inCategories))
                return scene;

            return default(Scene);
        }

        /// <summary>
        /// Finds scenes, filtering by name and category.
        /// </summary>
        static public IEnumerable<Scene> FindScenesByName(string inNameFilter, SceneCategories inCategories)
        {
            foreach(var scene in FindScenes(inCategories))
            {
                if (StringUtils.WildcardMatch(scene.name, inNameFilter))
                    yield return scene;
            }
        }

        /// <summary>
        /// Finds a scene, filtering by name and category.
        /// </summary>
        static public Scene FindSceneByName(string inNameFilter, SceneCategories inCategories)
        {
            foreach(var scene in FindScenes(inCategories))
            {
                if (StringUtils.WildcardMatch(scene.name, inNameFilter))
                    return scene;
            }

            return default(Scene);
        }

        /// <summary>
        /// Finds scenes, filtering by path and category.
        /// </summary>
        static public IEnumerable<Scene> FindScenesByPath(string inPathFilter, SceneCategories inCategories = SceneCategories.Loaded)
        {
            foreach(var scene in FindScenes(inCategories))
            {
                if (StringUtils.WildcardMatch(scene.path, inPathFilter))
                    yield return scene;
            }
        }

        /// <summary>
        /// Returns all scenes in the build.
        /// </summary>
        static public IEnumerable<Scene> AllBuildScenes(bool inbIncludeIgnored = false)
        {
            int buildSceneCount = SceneManager.sceneCountInBuildSettings;
            for(int i = 0; i < buildSceneCount; ++i)
            {
                Scene scene = SceneManager.GetSceneByBuildIndex(i);
                if (inbIncludeIgnored || !IsIgnored(scene))
                    yield return scene;
            }
        }

        /// <summary>
        /// Gathers all scenes in the build.
        /// </summary>
        static public int AllBuildScenes(ICollection<Scene> outScenes, bool inbIncludeIgnored = false)
        {
            int buildSceneCount = SceneManager.sceneCountInBuildSettings;

            int returnedCount = 0;
            for(int i = 0; i < buildSceneCount; ++i)
            {
                Scene scene = SceneManager.GetSceneByBuildIndex(i);
                if (inbIncludeIgnored || !IsIgnored(scene))
                {
                    outScenes.Add(scene);
                    ++returnedCount;
                }
            }
            return returnedCount;
        }

        /// <summary>
        /// Returns all active scenes.
        /// </summary>
        static public IEnumerable<Scene> AllLoadedScenes(bool inbIncludeIgnored = false)
        {
            int sceneCount = SceneManager.sceneCount;
            for(int i = 0; i < sceneCount; ++i)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if (inbIncludeIgnored || !IsIgnored(scene))
                    yield return scene;
            }
        }

        /// <summary>
        /// Returns all active scenes.
        /// </summary>
        static public int AllLoadedScenes(ICollection<Scene> outScenes, bool inbIncludeIgnored = false)
        {
            int sceneCount = SceneManager.sceneCount;

            int returnedCount = 0;
            for(int i = 0; i < sceneCount; ++i)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if (inbIncludeIgnored || !IsIgnored(scene))
                {
                    outScenes.Add(scene);
                    ++returnedCount;
                }
            }

            return returnedCount;
        }

        #endregion // Scene List

        #region GetAllComponents

        static private readonly List<GameObject> s_CachedRootGOs = new List<GameObject>(64);

        /// <summary>
        /// Retrieves all components in a scene.
        /// </summary>
        static public void GetAllComponents<T>(this Scene inScene, List<T> outList)
        {
            GetAllComponents<T>(inScene, false, outList);
        }

        /// <summary>
        /// Retrieves all components of a type in a scene.
        /// </summary>
        static public void GetAllComponents<T>(this Scene inScene, bool inbIncludeInactive, List<T> outList)
        {
            List<T> cache = null;
            GetAllComponents(inScene, inbIncludeInactive, outList, ref cache, true);
        }

        // Retrieves all components of from the given scene
        static private void GetAllComponents<T>(Scene inScene, bool inbIncludeInactive, List<T> outList, ref List<T> ioCache, bool inbClearList)
        {
            if (outList == null)
                throw new ArgumentNullException("outList");

            if (inbClearList)
                outList.Clear();

            if (!inScene.IsValid() || inScene.rootCount <= 0)
                return;

            inScene.GetRootGameObjects(s_CachedRootGOs);
            ListUtils.EnsureCapacity(ref ioCache, inScene.rootCount * 4);
            ioCache.Clear();
            foreach(var go in s_CachedRootGOs)
            {
                if (!inbIncludeInactive && !go.activeSelf)
                    continue;
                
                go.GetComponentsInChildren<T>(inbIncludeInactive, ioCache);
                ListUtils.EnsureCapacity(ref outList, Mathf.NextPowerOfTwo(outList.Count + ioCache.Count));
                outList.AddRange(ioCache);
                ioCache.Clear();
            }

            s_CachedRootGOs.Clear();
        }

        #endregion // GetAllComponents

        #region ForEachComponent

        /// <summary>
        /// Delegate to execute on components in a scene.
        /// </summary>
        public delegate void SceneComponentAction<T>(Scene inScene, T inComponent);

        /// <summary>
        /// Delegate to execute on components in a scene.
        /// </summary>
        public delegate void SceneComponentContextAction<T>(Scene inScene, T inComponent, object inContext);

        /// <summary>
        /// Executes an action on each active component of the given type in the scene.
        /// </summary>
        static public void ForEachComponent<T>(this Scene inScene, SceneComponentAction<T> inAction)
        {
            ForEachComponent<T>(inScene, false, inAction);
        }

        /// <summary>
        /// Executes an action on each component of the given type in the scene.
        /// </summary>
        static public void ForEachComponent<T>(this Scene inScene, bool inbIncludeInteractive, SceneComponentAction<T> inAction)
        {
            List<T> allComponents = new List<T>();
            GetAllComponents<T>(inScene, inbIncludeInteractive, allComponents);
            for(int i = 0, count = allComponents.Count; i < count; ++i)
            {
                inAction(inScene, allComponents[i]);
            }
        }

        /// <summary>
        /// Executes an action on each active component of the given type in the scene, with a context.
        /// </summary>
        static public void ForEachComponent<T>(this Scene inScene, object inContext, SceneComponentContextAction<T> inAction)
        {
            ForEachComponent<T>(inScene, false, inContext, inAction);
        }

        /// <summary>
        /// Executes an action on each component of the given type in the scene, with a context.
        /// </summary>
        static public void ForEachComponent<T>(this Scene inScene, bool inbIncludeInteractive, object inContext, SceneComponentContextAction<T> inAction)
        {
            List<T> allComponents = new List<T>();
            GetAllComponents<T>(inScene, inbIncludeInteractive, allComponents);
            for(int i = 0, count = allComponents.Count; i < count; ++i)
            {
                inAction(inScene, allComponents[i], inContext);
            }
        }

        #endregion // ForEachComponent
    
        #region Load Wrappers

        /// <summary>
        /// Loads a scene asynchronously, dispatching the appropriate load and unload events.
        /// </summary>
        static public AsyncOperation LoadAsync(Scene inScene, object inContext = null, LoadSceneMode inMode = LoadSceneMode.Single)
        {
            if (!inScene.IsValid())
            {
                Debug.LogErrorFormat("Cannot load invalid scene '{0}'", inScene.path);
                return null;
            }
            
            if (inMode == LoadSceneMode.Single)
            {
                foreach(var scene in FindScenes(SceneCategories.Loaded))
                    scene.OnUnload(inContext);
            }

            AsyncOperation loadOp = SceneManager.LoadSceneAsync(inScene.path, inMode);
            loadOp.completed += (AsyncOperation op) =>
            {
                inScene.OnLoaded(inContext);
            };
            return loadOp;
        }

        /// <summary>
        /// Unloads a scene asynchronously, dispatching the appropriate unload events.
        /// </summary>
        static public AsyncOperation UnloadAsync(Scene inScene, object inContext = null, UnloadSceneOptions inOptions = UnloadSceneOptions.None)
        {
            if (!inScene.IsValid())
            {
                Debug.LogErrorFormat("Cannot unload invalid scene '{0}'", inScene.path);
                return null;
            }

            inScene.OnUnload(inContext);
            return SceneManager.UnloadSceneAsync(inScene, inOptions);
        }

        #endregion // Load Wrappers
    }
}