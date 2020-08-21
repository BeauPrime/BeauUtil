/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    19 August 2020
 * 
 * File:    SceneCategories.cs
 * Purpose: Scene filtering categories.
 */

namespace BeauUtil
{
    /// <summary>
    /// Scene categories.
    /// </summary>
    public enum SceneCategories : byte
    {
        // All Scenes in the build settings
        Build = 0x01,

        // All Loaded Scenes
        Loaded = 0x02,

        // Only the Active Scene
        ActiveOnly = 0x04,

        // Includes Ignored Scenes
        IncludeIgnored = 0x08,

        // All Loaded Scenes, including ignored ones
        AllLoaded = Loaded | IncludeIgnored,

        // All Scenes in the build settings, including ignored ones
        AllBuild = Build | IncludeIgnored
    }
}