/*
 * Copyright (C) 2017-2020. Filament Games, LLC. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    5 Dec 2019
 * 
 * File:    BuildUtils.cs
 * Purpose: Utility methods for dealing with builds.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace BeauUtil.Editor
{
    /// <summary>
    /// Contains utility functions for dealing with builds.
    /// </summary>
    static public class BuildUtils
    {
        private const string DefinePrefix = "-define:";
        static private readonly string[] DefineFiles = new string[] { "Assets/mcs.rsp", "Assets/us.rsp", "Assets/csc.rsp" };

        static private string GetDefineString(string inDefines)
        {
            if (string.IsNullOrEmpty(inDefines))
                return string.Empty;

            StringBuilder sb = new StringBuilder();
            sb.Append(DefinePrefix).Append(inDefines);
            sb.Replace(',', ';').Replace('.', ';').Replace(' ', ';');

            while (sb.Length > 0 && sb[sb.Length - 1] == ';')
                --sb.Length;
            if (sb.Length <= DefinePrefix.Length)
                return string.Empty;

            return sb.ToString();
        }

        /// <summary>
        /// Writes the given defines strings to the appropriate files.
        /// </summary>
        static public void WriteDefines(string inDefines)
        {
            string defineString = GetDefineString(inDefines);

            if (string.IsNullOrEmpty(defineString))
            {
                foreach (var fileName in DefineFiles)
                {
                    File.Delete(fileName);
                    File.Delete(fileName + ".meta");
                }
            }
            else
            {
                foreach (var fileName in DefineFiles)
                {
                    File.WriteAllText(fileName, defineString);
                }
            }
        }

        /// <summary>
        /// Forces the assemblies to recompile.
        /// Specify directories to force a subset to recompile.
        /// </summary>
        static public void ForceRecompile(string[] inDirectories = null)
        {
            HashSet<Assembly> affectedAssemblies = new HashSet<Assembly>();
            foreach (var file in AssetDBUtils.FindAssets<MonoScript>(null, inDirectories))
            {
                Type type = file.GetClass();
                if (type != null && affectedAssemblies.Add(type.Assembly))
                {
                    string filePath = AssetDatabase.GetAssetPath(file);
                    Debug.LogFormat("[BuildUtils] Reimporting '{0}'", filePath);
                    AssetDatabase.ImportAsset(filePath, ImportAssetOptions.ForceUpdate);
                }
            }

            if (affectedAssemblies.Count > 0)
            {
                EditorApplication.ExecuteMenuItem("Assets/Open C# Project");
                // TODO(Beau): Replace with a more stable method for syncing C# projects
            }
        }
    }
}