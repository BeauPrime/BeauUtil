/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    29 Jan 2020
 * 
 * File:    GUIUtils.cs
 * Purpose: Additional utilities for GUI.
 */

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace BeauUtil.Editor
{
    public class PooledGUIContent : GUIContent, IDisposable
    {
        void IDisposable.Dispose()
        {
            Free(this);
        }

        #region GUIContent pool

        static private readonly List<PooledGUIContent> s_ContentStack = new List<PooledGUIContent>(32);

        /// <summary>
        /// Allocates temp content.
        /// </summary>
        static public PooledGUIContent Alloc()
        {
            int index = s_ContentStack.Count - 1;
            if (index < 0)
                return new PooledGUIContent();
            PooledGUIContent content = s_ContentStack[index];
            content.Clear();
            s_ContentStack.RemoveAt(index);
            return content;
        }

        /// <summary>
        /// Allocates temp content.
        /// </summary>
        static public PooledGUIContent Alloc(GUIContent inClone)
        {
            PooledGUIContent alloced = Alloc();
            alloced.CopyFrom(inClone);
            return alloced;
        }

        /// <summary>
        /// Allocates temp content.
        /// </summary>
        static public PooledGUIContent Alloc(string inText, string inTooltip = null, Texture inImage = null)
        {
            PooledGUIContent alloced = Alloc();
            alloced.text = inText;
            alloced.tooltip = inTooltip;
            alloced.image = inImage;
            return alloced;
        }

        /// <summary>
        /// Frees the temporary content.
        /// </summary>
        static public void Free(PooledGUIContent inContent)
        {
            if (inContent == null)
                throw new NullReferenceException("Cannot free null content");
            if (s_ContentStack.Contains(inContent))
                throw new InvalidOperationException("Cannot free the same GUIContent multiple times");
            inContent.Clear();
            s_ContentStack.Add(inContent);
        }

        #endregion // GUIContent pool
    }
}