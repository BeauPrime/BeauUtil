/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    30 Sept 2020
 * 
 * File:    IHotReloadable.cs
 * Purpose: Interface for an asset that can be reloaded at runtime.
 */

namespace BeauUtil.IO
{
    /// <summary>
    /// Interface for an asset that is reloadable at runtime.
    /// </summary>
    public interface IHotReloadable
    {
        /// <summary>
        /// Asset identifier.
        /// </summary>
        StringHash32 Id { get; }
        
        /// <summary>
        /// Asset type identifier.
        /// </summary>
        StringHash32 Tag { get; }

        /// <summary>
        /// Returns if this asset needs to be reloaded.
        /// </summary>
        HotReloadOperation NeedsReload();
        
        /// <summary>
        /// Performs a reload on this asset.
        /// </summary>
        void HotReload(HotReloadOperation inAction);
    }
}