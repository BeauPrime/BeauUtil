/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    28 July 2020
 * 
 * File:    ISceneLoadHandler.cs
 * Purpose: Scene load event handler.
 */

using UnityEngine.SceneManagement;

namespace BeauUtil
{
    public interface ISceneLoadHandler
    {
        void OnSceneLoad(Scene inScene, object inContext);
    }
}