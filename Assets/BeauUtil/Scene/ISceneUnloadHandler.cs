/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    28 July 2020
 * 
 * File:    ISceneUnloadHandler.cs
 * Purpose: Scene unload event handler.
 */

using UnityEngine.SceneManagement;

namespace BeauUtil
{
    public interface ISceneUnloadHandler
    {
        void OnSceneUnload(SceneBinding inScene, object inContext);
    }
}