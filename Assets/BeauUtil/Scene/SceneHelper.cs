/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    28 July 2020
 * 
 * File:    SceneHelper.cs
 * Purpose: Scene utility methods.
 */

#if !USING_TINYIL || (!UNITY_EDITOR && !ENABLE_IL2CPP)
#define RESTRICT_INTERNAL_CALLS
#endif // !USING_TINYIL || (!UNITY_EDITOR && !ENABLE_IL2CPP)

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
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
            foreach (var sceneName in s_DefaultIgnoreSceneNames)
            {
                s_IgnoredSceneFilters.Add(new SceneFilter(sceneName, null));
            }

#if RESTRICT_INTERNAL_CALLS
            Type sceneType = typeof(Scene);

            GetLoadingStateInternal = sceneType.GetMethod("GetLoadingStateInternal", BindingFlags.Static | BindingFlags.NonPublic);
            
            MethodInfo getGUIDInternalMethod = sceneType.GetMethod("GetGUIDInternal", BindingFlags.Static | BindingFlags.NonPublic);
            if (getGUIDInternalMethod != null)
            {
                GetGUIDInternal = (GetGUIDInternalDelegate) getGUIDInternalMethod.CreateDelegate(typeof(GetGUIDInternalDelegate));
            }
#endif // RESTRICT_INTERNAL_CALLS
        }

        #region Load/Unload Events

        /// <summary>
        /// Delegate to execute when a scene is loaded/unloaded.
        /// </summary>
        public delegate void SceneLoadAction(SceneBinding inScene, object inContext);

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
        static public void OnLoaded(Scene inScene, object inContext = null)
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
        static public void OnUnload(Scene inScene, object inContext = null)
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

            public bool Match(SceneBinding inScene)
            {
                if (!string.IsNullOrEmpty(Name))
                {
                    return WildcardMatch.Match(inScene.Name, Name);
                }

                if (!string.IsNullOrEmpty(Path))
                {
                    return WildcardMatch.Match(inScene.Path, Path);
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

            for (int i = s_IgnoredSceneFilters.Count - 1; i >= 0; --i)
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

            for (int i = s_IgnoredSceneFilters.Count - 1; i >= 0; --i)
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

            for (int i = s_IgnoredSceneFilters.Count - 1; i >= 0; --i)
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

            for (int i = s_IgnoredSceneFilters.Count - 1; i >= 0; --i)
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
        static public bool IsIgnored(SceneBinding inScene)
        {
            for (int i = 0; i < s_IgnoredSceneFilters.Count; ++i)
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
        static public IEnumerable<SceneBinding> FindScenes(SceneCategories inCategories)
        {
            bool bBuild = (inCategories & SceneCategories.Build) == SceneCategories.Build;
            bool bLoaded = (inCategories & SceneCategories.Loaded) == SceneCategories.Loaded;
            bool bActive = (inCategories & SceneCategories.ActiveOnly) == SceneCategories.ActiveOnly;
            bool bIncludeIgnored = (inCategories & SceneCategories.IncludeIgnored) == SceneCategories.IncludeIgnored;

            if (bActive)
            {
                SceneBinding active = SceneManager.GetActiveScene();
                if (!bIncludeIgnored && IsIgnored(active))
                    yield break;

                if (bBuild && active.BuildIndex < 0)
                    yield break;

                if (bLoaded && !active.IsLoaded())
                    yield break;

                yield return active;
            }
            else if (bBuild)
            {
                foreach (var scene in AllBuildScenes(bIncludeIgnored))
                {
                    if (bLoaded && !scene.IsLoaded())
                        continue;

                    yield return scene;
                }
            }
            else if (bLoaded)
            {
                foreach (var scene in AllLoadedScenes(bIncludeIgnored))
                {
                    yield return scene;
                }
            }
        }

        /// <summary>
        /// Finds a scene, filtering by category.
        /// </summary>
        static public SceneBinding FindScene(SceneCategories inCategories)
        {
            foreach (var scene in FindScenes(inCategories))
                return scene;

            return default(SceneBinding);
        }

        /// <summary>
        /// Finds scenes, filtering by name and category.
        /// </summary>
        static public IEnumerable<SceneBinding> FindScenesByName(string inNameFilter, SceneCategories inCategories)
        {
            foreach (var scene in FindScenes(inCategories))
            {
                if (WildcardMatch.Match(scene.Name, inNameFilter))
                    yield return scene;
            }
        }

        /// <summary>
        /// Finds a scene, filtering by name and category.
        /// </summary>
        static public SceneBinding FindSceneByName(string inNameFilter, SceneCategories inCategories)
        {
            foreach (var scene in FindScenes(inCategories))
            {
                if (WildcardMatch.Match(scene.Name, inNameFilter))
                    return scene;
            }

            return default(SceneBinding);
        }

        /// <summary>
        /// Finds scenes, filtering by path and category.
        /// </summary>
        static public IEnumerable<SceneBinding> FindScenesByPath(string inPathFilter, SceneCategories inCategories = SceneCategories.Loaded)
        {
            foreach (var scene in FindScenes(inCategories))
            {
                if (WildcardMatch.Match(scene.Path, inPathFilter))
                    yield return scene;
            }
        }

        /// <summary>
        /// Finds a scene, filtering by path and category.
        /// </summary>
        static public SceneBinding FindSceneByPath(string inPathFilter, SceneCategories inCategories)
        {
            foreach (var scene in FindScenes(inCategories))
            {
                if (WildcardMatch.Match(scene.Path, inPathFilter))
                    return scene;
            }

            return default(SceneBinding);
        }

        /// <summary>
        /// Finds a scene, filtering by id and category.
        /// </summary>
        static public SceneBinding FindSceneById(StringHash32 inId, SceneCategories inCategories)
        {
            foreach (var scene in FindScenes(inCategories))
            {
                if (scene.Id == inId)
                    return scene;
            }

            return default(SceneBinding);
        }

        /// <summary>
        /// Finds a scene with the given build index.
        /// </summary>
        static public SceneBinding FindSceneByIndex(int inIndex)
        {
            string path = SceneUtility.GetScenePathByBuildIndex(inIndex);
            return new SceneBinding(inIndex, path);
        }

        /// <summary>
        /// Returns all scenes in the build.
        /// </summary>
        static public IEnumerable<SceneBinding> AllBuildScenes(bool inbIncludeIgnored = false)
        {
            int buildSceneCount = SceneManager.sceneCountInBuildSettings;
            for (int i = 0; i < buildSceneCount; ++i)
            {
                string path = SceneUtility.GetScenePathByBuildIndex(i);
                SceneBinding binding = new SceneBinding(i, path);
                if (inbIncludeIgnored || !IsIgnored(binding))
                    yield return binding;
            }
        }

        /// <summary>
        /// Gathers all scenes in the build.
        /// </summary>
        static public int AllBuildScenes(ICollection<SceneBinding> outScenes, bool inbIncludeIgnored = false)
        {
            int buildSceneCount = SceneManager.sceneCountInBuildSettings;

            int returnedCount = 0;
            for (int i = 0; i < buildSceneCount; ++i)
            {
                string path = SceneUtility.GetScenePathByBuildIndex(i);
                SceneBinding binding = new SceneBinding(i, path);
                if (inbIncludeIgnored || !IsIgnored(binding))
                {
                    outScenes.Add(binding);
                    ++returnedCount;
                }
            }
            return returnedCount;
        }

        /// <summary>
        /// Returns all active scenes.
        /// </summary>
        static public IEnumerable<SceneBinding> AllLoadedScenes(bool inbIncludeIgnored = false)
        {
            int sceneCount = SceneManager.sceneCount;
            for (int i = 0; i < sceneCount; ++i)
            {
                SceneBinding sceneBinding = SceneManager.GetSceneAt(i);
                if (inbIncludeIgnored || !IsIgnored(sceneBinding))
                    yield return sceneBinding;
            }
        }

        /// <summary>
        /// Returns all active scenes.
        /// </summary>
        static public int AllLoadedScenes(ICollection<SceneBinding> outScenes, bool inbIncludeIgnored = false)
        {
            int sceneCount = SceneManager.sceneCount;

            int returnedCount = 0;
            for (int i = 0; i < sceneCount; ++i)
            {
                SceneBinding sceneBinding = SceneManager.GetSceneAt(i);
                if (inbIncludeIgnored || !IsIgnored(sceneBinding))
                {
                    outScenes.Add(sceneBinding);
                    ++returnedCount;
                }
            }

            return returnedCount;
        }

        /// <summary>
        /// Returns the currently active scene.
        /// </summary>
        static public SceneBinding ActiveScene()
        {
            return SceneManager.GetActiveScene();
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
            foreach (var go in s_CachedRootGOs)
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
            for (int i = 0, count = allComponents.Count; i < count; ++i)
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
            for (int i = 0, count = allComponents.Count; i < count; ++i)
            {
                inAction(inScene, allComponents[i], inContext);
            }
        }

        #endregion // ForEachComponent

        #region Load Wrappers

        /// <summary>
        /// Loads a scene asynchronously, dispatching the appropriate load and unload events.
        /// </summary>
        static public AsyncOperation LoadAsync(SceneBinding inScene, object inContext = null, LoadSceneMode inMode = LoadSceneMode.Single)
        {
            if (!inScene.IsValid())
            {
                Debug.LogErrorFormat("Cannot load invalid scene '{0}'", inScene.Path);
                return null;
            }

            if (inMode == LoadSceneMode.Single)
            {
                foreach (var scene in FindScenes(SceneCategories.Loaded))
                    scene.BroadcastUnload(inContext);
            }

            AsyncOperation loadOp = SceneManager.LoadSceneAsync(inScene.Path, inMode);
            loadOp.completed += (AsyncOperation op) =>
            {
                inScene.BroadcastLoaded(inContext);
            };
            return loadOp;
        }

        /// <summary>
        /// Unloads a scene asynchronously, dispatching the appropriate unload events.
        /// </summary>
        static public AsyncOperation UnloadAsync(SceneBinding inScene, object inContext = null, UnloadSceneOptions inOptions = UnloadSceneOptions.None)
        {
            if (!inScene.IsValid())
            {
                Debug.LogErrorFormat("Cannot unload invalid scene '{0}'", inScene.Path);
                return null;
            }

            inScene.BroadcastUnload(inContext);
            return SceneManager.UnloadSceneAsync(inScene.Scene, inOptions);
        }

        #endregion // Load Wrappers

        #region Internal Methods

        /// <summary>
        /// Scene loading state.
        /// Exposed version of UnityEngine.SceneManagement.Scene.LoadingState
        /// </summary>
        public enum LoadingState
        {
            NotLoaded,
            Loading,
            Loaded,
            Unloading
        }

#if RESTRICT_INTERNAL_CALLS

        private delegate string GetGUIDInternalDelegate(int inHandle);
        static private readonly MethodInfo GetLoadingStateInternal;
        static private readonly GetGUIDInternalDelegate GetGUIDInternal;

        [ThreadStatic]
        static private object[] s_GetLoadingStateArgsArray;

#endif // RESTRICT_INTERNAL_CALLS

        /// <summary>
        /// Returns the loading state of the given scene.
        /// </summary>
#if !RESTRICT_INTERNAL_CALLS
        [IntrinsicIL("ldarga.s inScene; call UnityEngine.SceneManagement.Scene::get_loadingState(); conv.i4; ret;")]
#endif // !RESTRICT_INTERNAL_CALLS
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public LoadingState GetLoadingState(this Scene inScene)
        {
#if RESTRICT_INTERNAL_CALLS
            if (!inScene.IsValid())
            {
                return LoadingState.NotLoaded;
            }
            else if (inScene.isLoaded)
            {
                return LoadingState.Loaded;
            }
            else if (GetLoadingStateInternal != null)
            {
                var argsArray = (s_GetLoadingStateArgsArray ?? (s_GetLoadingStateArgsArray = new object[1]));
                argsArray[0] = inScene.handle;
                return (LoadingState) Convert.ToInt32(GetLoadingStateInternal.Invoke(null, argsArray));
            }
            else
            {
                return LoadingState.NotLoaded;
            }
#else
            throw new NotImplementedException();
#endif // RESTRICT_INTERNAL_CALLS
        }

        /// <summary>
        /// Returns the GUID of the given scene.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#if !RESTRICT_INTERNAL_CALLS
        [IntrinsicIL("ldarga.s inScene; call UnityEngine.SceneManagement.Scene::get_guid(); ret;")]
#endif // !RESTRICT_INTERNAL_CALLS
        static public string GetGUID(this Scene inScene)
        {
#if RESTRICT_INTERNAL_CALLS
            if (GetGUIDInternal != null)
            {
                return GetGUIDInternal(inScene.handle);
            }
            else
            {
                return Guid.Empty.ToString();
            }
#else
            throw new NotImplementedException();
#endif // RESTRICT_INTERNAL_CALLS
        }

        #endregion // Internal Methods
    }
}