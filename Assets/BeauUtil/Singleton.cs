/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    4 April 2019
 * 
 * File:    Singleton.cs
 * Purpose: Singleton behavior.
 */

using System;
using UnityEngine;

namespace BeauUtil
{
    /// <summary>
    /// Generic singleton MonoBehaviour.
    /// </summary>
    [DisallowMultipleComponent]
    public class Singleton<SType> : MonoBehaviour where SType : Singleton<SType>
    {
        static private SType s_Instance;
        static private bool s_ApplicationQuitting = false;

        static private Transform GetRoot()
        {
            GameObject rootGO = GameObject.Find("Singletons");
            if (rootGO == null)
            {
                rootGO = new GameObject("Singletons");
                DontDestroyOnLoad(rootGO);
            }
            return rootGO.transform;
        }

        /// <summary>
        /// Ensures the singleton is created.
        /// </summary>
        static public SType Create()
        {
            if (s_Instance == null)
            {
                s_Instance = FindObjectOfType<SType>();

                // If in the process of shutting down, we don't want
                // to instantiate another instance and risk a memory leak
                if (s_Instance == null && !s_ApplicationQuitting)
                {
                    s_Instance = Prefab.Instantiate<SType>();
                    if (s_Instance == null)
                        throw new Exception("Unable to spawn singleton with type " + typeof(SType).Name);

                    if (!s_Instance.IsLocalToScene)
                        s_Instance.transform.SetParent(GetRoot());
                    s_Instance.gameObject.name = typeof(SType).Name + "::Instance";
                    s_Instance.OnAssigned();
                }
            }
            return s_Instance;
        }

        /// <summary>
        /// Ensures the singleton is destroyed.
        /// </summary>
        static public void Destroy()
        {
            if (s_Instance != null)
            {
                Destroy(s_Instance);
                s_Instance = null;
            }
        }

        /// <summary>
        /// Returns the instance of the singleton.
        /// </summary>
        static public SType I
        {
            get
            {
                if (s_Instance == null)
                    return Create();
                return s_Instance;
            }
        }

        /// <summary>
        /// Returns if the singleton exists.
        /// </summary>
        static public bool Exists
        {
            get { return s_Instance != null; }
        }

        /// <summary>
        /// Returns if this singleton is only local to the current scene.
        /// </summary>
        protected virtual bool IsLocalToScene
        {
            get { return false; }
        }

        /// <summary>
        /// Returns if this object is the singleton instance.
        /// Useful for executing logic in Awake() or OnDestroy().
        /// </summary>
        protected bool IsInstance()
        {
            return s_Instance == this;
        }

        /// <summary>
        /// Called when the object is woken up.
        /// </summary>
        protected virtual void Awake()
        {
            if (s_Instance == null)
            {
                s_Instance = (SType) this;
                if (!IsLocalToScene)
                    transform.SetParent(GetRoot());
                OnAssigned();
            }
            else if (!ReferenceEquals(s_Instance, this))
                Destroy(this);
        }

        /// <summary>
        /// Called when the object is destroyed.
        /// </summary>
        protected virtual void OnDestroy()
        {
            if (s_Instance == this)
                s_Instance = null;
        }

        protected virtual void OnApplicationQuit()
        {
            s_ApplicationQuitting = true;
        }

        /// <summary>
        /// Called when the instance is initialized.
        /// </summary>
        protected virtual void OnAssigned() { }
    }
}