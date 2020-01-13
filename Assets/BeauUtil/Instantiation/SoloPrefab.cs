/*
 * Copyright (C) 2017-2020. Filament Games, LLC. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    4 April 2019
 * 
 * File:    SoloPrefab.cs
 * Purpose: Marks a MonoBehaviour class as spawnable by itself.
 */

using System;
using System.Collections;
using UnityEngine;

namespace BeauUtil
{
    /// <summary>
    /// Type can be instantiated by itself on an empty GameObject.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class SoloPrefab : Prefab
    {
        protected override string GetVariantName()
        {
            return string.Empty;
        }

        protected override void EnsureLoaded<T>() { }

        protected override IEnumerator EnsureLoadedAsync<T>()
        {
            yield break;
        }

        protected override bool GetLoaded<T>()
        {
            return true;
        }

        protected override T GetPrefab<T>()
        {
            return null;
        }

        protected override void EnsureUnloaded<T>() { }

        protected override T Spawn<T>()
        {
            GameObject go = new GameObject(typeof(T).Name);
            return go.AddComponent<T>();
        }
    }
}
