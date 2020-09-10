/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    2 Sept 2020
 * 
 * File:    Assert.cs
 * Purpose: Conditionally-compiled assertions.
 */

#if UNITY_EDITOR || UNITY_DEVELOPMENT
#define DEVELOPMENT
#endif // UNITY_EDITOR || UNITY_DEVELOPMENT

#if UNITY_WEBGL
#define DISABLE_STACK_TRACE
#endif // UNITY_WEBGL

using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif // UNITY_EDITOR

namespace BeauUtil.Debugger
{
    /// <summary>
    /// Conditionally-compiled assertions.
    /// </summary>
    static public class Assert
    {
        [RuntimeInitializeOnLoadMethod]
        static private void Initialize()
        {
            #if DEVELOPMENT
            UnityEngine.Application.logMessageReceived += Appliation_logMessageReceived;
            #endif // DEVELOPMENT
        }

        static private void Appliation_logMessageReceived(string condition, string stackTrace, UnityEngine.LogType type)
        {
            switch(type)
            {
                case UnityEngine.LogType.Exception:
                    {
                        break;
                    }
            }
        }
    }
}