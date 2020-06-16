/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    4 April 2019
 * 
 * File:    Persist.cs
 * Purpose: Marks a gameobject and its children as persistent.
 */

using System.Collections.Generic;
using UnityEngine;

namespace BeauUtil
{
    /// <summary>
    /// This game object and its children will persist across scene changes.
    /// </summary>
    [AddComponentMenu("BeauUtil/Persist")]
    public sealed class Persist : MonoBehaviour
    {
        static private readonly HashSet<string> s_ExistingIDs = new HashSet<string>();

        [SerializeField, Tooltip("If set, further Persist GameObjects with this ID will be destroyed.")]
        private string m_UniqueID = string.Empty;

        [SerializeField]
        private bool m_WakeUpChildren = true;

        private bool m_Initialized = false;

        private void Awake()
        {
            if (!string.IsNullOrEmpty(m_UniqueID))
            {
                if (s_ExistingIDs.Contains(m_UniqueID))
                {
                    DestroyImmediate(gameObject);
                    return;
                }

                s_ExistingIDs.Add(m_UniqueID);
            }

            if (m_WakeUpChildren)
            {
                for (int i = transform.childCount - 1; i >= 0; --i)
                    transform.GetChild(i).gameObject.SetActive(true);
            }

            gameObject.transform.SetParent(null, true);
            DontDestroyOnLoad(gameObject);
            m_Initialized = true;
        }

        private void OnDestroy()
        {
            if (!m_Initialized)
                return;

            if (!string.IsNullOrEmpty(m_UniqueID))
            {
                s_ExistingIDs.Remove(m_UniqueID);
            }
        }

        #if UNITY_EDITOR

        [UnityEditor.CustomEditor(typeof(Persist))]
        private class Editor : UnityEditor.Editor
        {
            public override void OnInspectorGUI()
            {
                base.OnInspectorGUI();

                if (GUILayout.Button("Generate GUID"))
                {
                    Persist p = (Persist) target;
                    p.m_UniqueID = System.Guid.NewGuid().ToString();
                    UnityEditor.EditorUtility.SetDirty(p);
                }
            }
        }

        #endif
    }
}