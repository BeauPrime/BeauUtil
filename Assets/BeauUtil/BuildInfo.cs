/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    2 Sept 2020
 * 
 * File:    BuildInfo.cs
 * Purpose: Cached build information.
 */

using System;
using System.IO;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Networking;

[assembly: InternalsVisibleTo("BeauUtil.Editor")]

namespace BeauUtil
{
    /// <summary>
    /// Cached build information.
    /// </summary>
    static public class BuildInfo
    {
        static public readonly string InfoFilename = "__buildInfo.txt";
        static public readonly string InfoPath = Path.Combine(Application.streamingAssetsPath, InfoFilename);

        private enum LoadState
        {
            NotLoaded,
            Loading,
            Error,
            Loaded
        }

        static private LoadState s_CurrentState;
        static private string s_CachedBuildId;
        static private string s_CachedBuildDate;
        static private string s_CachedBuildTag;
        static private string s_CachedBuildBranch;
        static private string s_CachedBundleVersion;
        static private Action s_LoadCallback;

        /// <summary>
        /// Starts loading build information.
        /// Pass in a callback to be notified
        /// when the loading process is completed.
        /// </summary>
        static public void Load(Action inCallback = null)
        {
            switch(s_CurrentState)
            {
                case LoadState.Error:
                case LoadState.Loaded:
                    {
                        if (inCallback != null)
                            inCallback();
                        break;
                    }
                
                case LoadState.NotLoaded:
                    {
                        RegisterCallback(inCallback);
                        RetrieveBuildInfo();
                        break;
                    }

                case LoadState.Loading:
                    {
                        RegisterCallback(inCallback);
                        break;
                    }
            }
        }

        /// <summary>
        /// Returns the current build id.
        /// </summary>
        static public string Id()
        {
            RetrieveBuildInfo();
            return s_CachedBuildId;
        }

        /// <summary>
        /// Returns the current build date.
        /// </summary>
        static public string Date()
        {
            RetrieveBuildInfo();
            return s_CachedBuildDate;
        }

        /// <summary>
        /// Returns the current build tag.
        /// </summary>
        static public string Tag()
        {
            RetrieveBuildInfo();
            return s_CachedBuildTag;
        }

        /// <summary>
        /// Returns the current bundle version.
        /// </summary>
        static public string BundleVersion()
        {
            RetrieveBuildInfo();
            return s_CachedBundleVersion;
        }

        /// <summary>
        /// Returns the current build branch.
        /// </summary>
        static public string Branch()
        {
            RetrieveBuildInfo();
            return s_CachedBuildBranch;
        }

        /// <summary>
        /// Returns if build information is available.
        /// </summary>
        static public bool IsAvailable()
        {
            return s_CurrentState == LoadState.Loaded;
        }

        /// <summary>
        /// Returns if build information is loading.
        /// </summary>
        static public bool IsLoading()
        {
            return s_CurrentState == LoadState.Loading;
        }

        #region Loading

        static private void RegisterCallback(Action inCallback)
        {
            if (inCallback != null)
            {
                if (s_LoadCallback == null)
                    s_LoadCallback = inCallback;
                else
                    s_LoadCallback += inCallback;
            }
        }

        static private void RetrieveBuildInfo()
        {
            if (s_CurrentState != LoadState.NotLoaded)
                return;

            #if UNITY_EDITOR
            s_CachedBuildId = "EDITOR";
            if (UnityEditor.EditorApplication.isPlaying)
            {
                DateTime t = DateTime.Now;
                t = t.AddSeconds(-Time.realtimeSinceStartup);
                s_CachedBuildDate = GenerateBuildDate(t);
            }
            else
            {
                s_CachedBuildDate = string.Empty;
            }
            s_CachedBuildTag = string.Empty;
            s_CachedBundleVersion = UnityEditor.PlayerSettings.bundleVersion ?? string.Empty;
            s_CachedBuildBranch = string.Empty;

            Loaded();
            #else
            s_CurrentState = LoadState.Loading;
            switch (Application.platform)
            {
                case RuntimePlatform.Android:
                case RuntimePlatform.WebGLPlayer:
                    RetrieveInfoFromStreamingAsync();
                    break;
                default:
                    RetrieveInfoFromStreamingBlocking();
                    break;
            }
            #endif // UNITY_EDITOR
        }

        static private void RetrieveInfoFromStreamingAsync()
        {
            UnityWebRequest request = new UnityWebRequest(InfoPath);
            request.downloadHandler = new DownloadHandlerBuffer();
            var handler = request.SendWebRequest();
            handler.completed += (r) => HandleAsyncCompleted(handler);
        }

        static private void HandleAsyncCompleted(UnityWebRequestAsyncOperation inHandler)
        {
            if (!inHandler.isDone)
            {
                Error("Request failed to complete");
                return;
            }

            UnityWebRequest request = inHandler.webRequest;
            if (request.isNetworkError)
            {
                Error("Request encountered a network error: {0}", request.error);
                return;
            }

            if (request.isHttpError)
            {
                Error("Request encountered an http error {0}: {1}", request.responseCode, request.error);
                return;
            }

            try
            {
                string responseText = request.downloadHandler.text;
                RetrieveInfoFromString(responseText);
            }
            catch(Exception e)
            {
                Error("Exception\n{0}", e.ToString());
            }
            finally
            {
                request.Dispose();
            }
        }

        static private void RetrieveInfoFromStreamingBlocking()
        {
            if (File.Exists(InfoPath))
            {
                try
                {
                    string allText = File.ReadAllText(InfoPath);
                    RetrieveInfoFromString(allText);
                }
                catch(Exception e)
                {
                    Error("Exception\n{0}", e.ToString());
                }
            }
            else
            {
                Error("File does not exist");
            }
        }

        static private void RetrieveInfoFromString(string inString)
        {
            if (string.IsNullOrEmpty(inString))
            {
                Error("File was empty");
                return;
            }

            try
            {
                StringSlice[] lines = StringSlice.Split(inString, new char[] { '\n' }, StringSplitOptions.None);
                if (lines.Length < 3)
                {
                    Error("File is missing information");
                    return;
                }
                s_CachedBuildId = lines[0].ToString();
                DateTime buildDate = DateTime.FromFileTimeUtc(StringParser.ParseLong(lines[1], 0));
                s_CachedBuildDate = GenerateBuildDate(buildDate);
                s_CachedBundleVersion = lines[2].ToString();
                s_CachedBuildTag = lines.Length >= 4 ? lines[3].Unescape() : string.Empty;
                s_CachedBuildBranch = lines.Length >= 5 ? lines[4].Unescape() : string.Empty;

                Loaded();
            }
            catch(Exception e)
            {
                Error("Exception\n{0}", e.ToString());
            }
        }

        static private void Loaded()
        {
            Debug.LogFormat("[BuildInfo] Loaded build info\nBuild:   {0}\nDate:    {1}\nVersion: {2}\nTag:     {3}\nBranch:     {4}", s_CachedBuildId, s_CachedBuildDate, s_CachedBundleVersion, s_CachedBuildTag, s_CachedBuildBranch);
            s_CurrentState = LoadState.Loaded;

            if (s_LoadCallback != null)
            {
                s_LoadCallback();
                s_LoadCallback = null;
            }
        }

        static private void Error(string inExplanation, params object[] inParams)
        {
            string explanation = string.Format(inExplanation, inParams);
            Debug.LogErrorFormat("[BuildInfo] Unable to load build information from '{0}'\n{1}", InfoPath, inExplanation);
            s_CurrentState = LoadState.Error;

            s_CachedBuildId = "UNAVAILABLE";
            s_CachedBuildDate = string.Empty;
            s_CachedBuildTag = string.Empty;
            s_CachedBuildTag = string.Empty;

            if (s_LoadCallback != null)
            {
                s_LoadCallback();
                s_LoadCallback = null;
            }
        }

        #endregion // Loading

        /// <summary>
        /// Utility method, generates a a datetime string.
        /// </summary>
        static internal string GenerateBuildDate(DateTime inTime)
        {
            return inTime.ToString("yyyy MMM dd @ HH:mm:ss");
        }
    }
}