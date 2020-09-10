/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    3 Sept 2020
 * 
 * File:    ConsoleActiveScene.cs
 * Purpose: Displays active scene.
 */

using System;
using UnityEngine;
using BeauUtil;
using TMPro;
using UnityEngine.SceneManagement;

namespace BeauUtil.Debugger
{
    public class ConsoleActiveScene : MonoBehaviour
    {
        #region Inspector

        [SerializeField] private TMP_Text m_SceneNameText = null;

        #endregion // Inspector

        private void OnEnable()
        {
            Refresh();
            SceneManager.activeSceneChanged += OnSceneChanged;
        }

        private void OnDisable()
        {
            SceneManager.activeSceneChanged -= OnSceneChanged;
        }

        private void OnSceneChanged(Scene ignored, Scene inNext)
        {
            Refresh();
        }

        private void Refresh()
        {
            Scene active = SceneManager.GetActiveScene();
            string name = active.name;
            if (m_SceneNameText)
            {
                m_SceneNameText.SetText(name);
            }
        }
    }
}