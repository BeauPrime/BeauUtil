/*
 * Copyright (C) 2022. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    28 Oct 2022
 * 
 * File:    ICameraCallbacks.cs
 * Purpose: Camera callback interfaces.
 */

using System;
using UnityEngine;

namespace BeauUtil
{
    public interface ICameraPreCullCallback
    {
        void OnCameraPreCull(Camera inCamera, CameraCallbackSource inSource);
    }

    public interface ICameraPreRenderCallback
    {
        void OnCameraPreRender(Camera inCamera, CameraCallbackSource inSource);
    }

    public interface ICameraPostRenderCallback
    {
        void OnCameraPostRender(Camera inCamera, CameraCallbackSource inSource);
    }

    public enum CameraCallbackSource : uint
    {
        None = 0,

        Default,
        SRP,
    }
}