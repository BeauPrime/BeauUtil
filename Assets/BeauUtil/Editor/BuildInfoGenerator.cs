/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    2 Sept 2020
 * 
 * File:    BuildInfoGenerator.cs
 * Purpose: Generator for caching build information.
 */

#if UNITY_EDITOR
using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.Callbacks;
using UnityEngine;
#endif // UNITY_EDITOR

namespace BeauUtil.Editor
{
    /// <summary>
    /// Cached build information.
    /// </summary>
    static public class BuildInfoGenerator
    {
        /// <summary>
        /// Whether or not build information should be generated.
        /// </summary>
        static public bool Enabled = false;

        /// <summary>
        /// Additional information to use for build info generation.
        /// </summary>
        static public string BuildTag = string.Empty;

        /// <summary>
        /// Build identifier length. Defaults to 10.
        /// Must be between 7 and 40.
        /// </summary>
        static public int IdLength = 10;

        #region IPreprocessBuild

#if UNITY_2018_1_OR_NEWER
        private class InfoGenerator : IPreprocessBuildWithReport
        {
            public int callbackOrder { get { return 100; } }

            public void OnPreprocessBuild(BuildReport report)
            {
                TryGenerateBuildInfo();
            }
        }
#else
        private class InfoGenerator : IPreprocessBuild
        {
            public int callbackOrder { get { return 100; } }

            public void OnPreprocessBuild(BuildTarget target, string path)
            {
                TryGenerateBuildInfo();
            }
        }
#endif // UNITY_2018_1_OR_NEWER

        [MenuItem("Assets/BeauUtil/Generate Build Info", false, 1001)]
        static private void GenerateFromMenuCommand()
        {
            TryGenerateBuildInfo(true);
        }

        static private void TryGenerateBuildInfo(bool inbForce = false)
        {
            if (!inbForce && !Enabled)
                return;

            DateTime buildTime = DateTime.UtcNow;
            
            string buildId = BuildUtils.GenerateBuildId(buildTime, BuildTag);
            if (IdLength >= 7 && IdLength < buildId.Length)
                buildId = buildId.Substring(0, IdLength);

            StringBuilder dataBuilder = new StringBuilder(1024);
            dataBuilder.Append(buildId);
            dataBuilder.Append('\n').Append(buildTime.ToFileTimeUtc());
            dataBuilder.Append('\n').Append(PlayerSettings.bundleVersion ?? string.Empty);
            dataBuilder.Append('\n');
            if (!string.IsNullOrEmpty(BuildTag))
                StringUtils.Escape(BuildTag, dataBuilder);

            string fileContents = dataBuilder.Flush();
            
            string infoDirectory = Path.GetDirectoryName(BuildInfo.InfoPath);
            try
            {
                Directory.CreateDirectory(infoDirectory);
                File.WriteAllText(BuildInfo.InfoPath, fileContents);
                Debug.LogFormat("[BuildInfoGenerator] Generated build data to '{0}':\n{1}", BuildInfo.InfoPath, fileContents);
                AssetDatabase.ImportAsset("Assets/StreamingAssets/" + BuildInfo.InfoFilename, ImportAssetOptions.ForceUpdate);
            }
            catch(Exception e)
            {
                Debug.LogErrorFormat("[BuildInfoGenerator] Encountered exception when generating build info:\n{0}", e.ToString());
            }
        }

        #endregion // IPreprocessBuild
    }
}