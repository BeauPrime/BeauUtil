/*
 * Copyright (C) 2017-2021. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    25 Jan 2021
 * 
 * File:    IService.cs
 * Purpose: Interface for a service.
 */

namespace BeauUtil.Services
{
    /// <summary>
    /// Interface for services that can be started and stopped.`
    /// </summary>
    public interface IService
    {
        void InitializeService();
        void ShutdownService();
    }
}