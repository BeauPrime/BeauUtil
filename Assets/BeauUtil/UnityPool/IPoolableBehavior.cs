/*
 * Copyright (C) 2017-2019. Filament Games, LLC. All rights reserved.
 * Author:  Alex Beauchesne
 * Date:    4 April 2019
 * 
 * File:    IPoolableBehavior.cs
 * Purpose: Interface for poolable MonoBehaviours
 */

namespace BeauUtil
{
    public interface IPoolableBehavior
    {
        void OnActivate();
        void OnRecycle();
    }
}
