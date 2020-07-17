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
    static public class GUIUtils
    {
        #region GUIContent Extensions

        /// <summary>
        /// Copies the contents of one GUIContent into this GUIContent.
        /// </summary>
        static public void CopyFrom(this GUIContent inContent, GUIContent inSource)
        {
            if (inSource == null)
                throw new ArgumentNullException("inSource", "Cannot copy from null source");
            if (inContent == null)
                throw new ArgumentNullException("inContent", "Cannot copy to null content");

            inContent.text = inSource.text;
            inContent.tooltip = inSource.tooltip;
            inContent.image = inSource.image;
        }

        /// <summary>
        /// Clears the contents of the given GUIContent.
        /// </summary>
        static public void Clear(this GUIContent inContent)
        {
            if (inContent == null)
                throw new ArgumentNullException("inContent", "Cannot clear null content");

            inContent.text = null;
            inContent.tooltip = null;
            inContent.image = null;
        }

        #endregion // GUIContent Extensions
    }
}