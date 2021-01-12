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

#if UNITY_WEBGL && !UNITY_EDITOR
#define DISABLE_STACK_TRACE
#endif // UNITY_WEBGL && !UNITY_EDITOR

using System.Diagnostics;
using System;
using System.Text;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;

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
        #region Types

        private enum ErrorResult
        {
            Break,
            Ignore,
            IgnoreAll
        }

        private class AssertException : Exception
        {
            public AssertException(string inMessage)
                : base(AssertExceptionPrefix + inMessage)
            {}
        }

        #endregion // Types

        private const string AssertExceptionPrefix = "[Assert Failed]\n";
        private const string AssertConditionPrefix = "AssertException: " + AssertExceptionPrefix;
        private const string StackTraceDisabledMessage = "[Stack Trace Disabled]";

        static private bool s_Broken;
        static private readonly HashSet<StringHash64> s_IgnoredLocations = new HashSet<StringHash64>();

        [RuntimeInitializeOnLoadMethod]
        static private void Initialize()
        {
            #if DEVELOPMENT
            UnityEngine.Application.logMessageReceived += Appliation_logMessageReceived;
            #endif // DEVELOPMENT

            #if UNITY_EDITOR
            UnityEditor.EditorApplication.pauseStateChanged += (s) => {
                if (s == PauseState.Unpaused)
                {
                    s_Broken = false;
                }
            };
            #endif // UNITY_EDITOR
        }

        static private void Appliation_logMessageReceived(string condition, string stackTrace, UnityEngine.LogType type)
        {
            switch(type)
            {
                case UnityEngine.LogType.Exception:
                    {
                        if (!condition.StartsWith(AssertConditionPrefix))
                        {
                            OnFail(GetLocationFromTrace(stackTrace), condition, null);
                        }
                        break;
                    }
            }
        }

        #region True

        /// <summary>
        /// Asserts that a condition is true.
        /// </summary>
        [Conditional("DEVELOPMENT"), Conditional("UNITY_EDITOR"), Conditional("UNITY_DEVELOPMENT")]
        static public void True(bool inbValue)
        {
            if (!inbValue)
            {
                OnFail(GetLocationFromStack(1), "Value is true", null);
            }
        }

        /// <summary>
        /// Asserts that a condition is true.
        /// </summary>
        [Conditional("DEVELOPMENT"), Conditional("UNITY_EDITOR"), Conditional("UNITY_DEVELOPMENT")]
        static public void True(bool inbValue, string inMessage)
        {
            if (!inbValue)
            {
                OnFail(GetLocationFromStack(1), "Value is true", inMessage);
            }
        }

        /// <summary>
        /// Asserts that a condition is true.
        /// </summary>
        [Conditional("DEVELOPMENT"), Conditional("UNITY_EDITOR"), Conditional("UNITY_DEVELOPMENT")]
        static public void True(bool inbValue, string inFormat, params object[] inParams)
        {
            if (!inbValue)
            {
                OnFail(GetLocationFromStack(1), "Value is true", string.Format(inFormat, inParams));
            }
        }

        #endregion // True

        #region False

        /// <summary>
        /// Asserts that a condition is true.
        /// </summary>
        [Conditional("DEVELOPMENT"), Conditional("UNITY_EDITOR"), Conditional("UNITY_DEVELOPMENT")]
        static public void False(bool inbValue)
        {
            if (inbValue)
            {
                OnFail(GetLocationFromStack(1), "Value is false", null);
            }
        }

        /// <summary>
        /// Asserts that a condition is true.
        /// </summary>
        [Conditional("DEVELOPMENT"), Conditional("UNITY_EDITOR"), Conditional("UNITY_DEVELOPMENT")]
        static public void False(bool inbValue, string inMessage)
        {
            if (inbValue)
            {
                OnFail(GetLocationFromStack(1), "Value is false", inMessage);
            }
        }

        /// <summary>
        /// Asserts that a condition is true.
        /// </summary>
        [Conditional("DEVELOPMENT"), Conditional("UNITY_EDITOR"), Conditional("UNITY_DEVELOPMENT")]
        static public void False(bool inbValue, string inFormat, params object[] inParams)
        {
            if (inbValue)
            {
                OnFail(GetLocationFromStack(1), "Value is false", string.Format(inFormat, inParams));
            }
        }

        #endregion // False

        #region NotNull

        /// <summary>
        /// Asserts that a value is not null.
        /// </summary>
        [Conditional("DEVELOPMENT"), Conditional("UNITY_EDITOR"), Conditional("UNITY_DEVELOPMENT")]
        static public void NotNull<T>(T inValue) where T : class
        {
            if (inValue == null)
            {
                OnFail(GetLocationFromStack(1), string.Format("Object {0} is not null", typeof(T).Name), null);
            }
        }

        /// <summary>
        /// Asserts that a value is not null.
        /// </summary>
        [Conditional("DEVELOPMENT"), Conditional("UNITY_EDITOR"), Conditional("UNITY_DEVELOPMENT")]
        static public void NotNull<T>(T inValue, string inMessage) where T : class
        {
            if (inValue == null)
            {
                OnFail(GetLocationFromStack(1), string.Format("Object {0} is not null", typeof(T).Name), inMessage);
            }
        }

        /// <summary>
        /// Asserts that a value is not null.
        /// </summary>
        [Conditional("DEVELOPMENT"), Conditional("UNITY_EDITOR"), Conditional("UNITY_DEVELOPMENT")]
        static public void NotNull<T>(T inValue, string inFormat, params object[] inParams) where T : class
        {
            if (inValue == null)
            {
                OnFail(GetLocationFromStack(1), string.Format("Object {0} is not null", typeof(T).Name), string.Format(inFormat, inParams));
            }
        }

        #endregion // False

        #region Internal

        static private void OnFail(string inLocation, string inCondition, string inMessage)
        {
            StringHash64 locationHash = string.Format("{0}@{1}", inCondition, inLocation);
            if (s_IgnoredLocations.Contains(locationHash))
                return;

            #if UNITY_EDITOR

            if (EditorApplication.isCompiling)
                return;

            #endif // UNITY_EDITOR

            StringBuilder builder = new StringBuilder();
            builder.Append("Location: ").Append(inLocation)
                .Append("\nCondition: ").Append(inCondition ?? string.Empty);

            if (!string.IsNullOrEmpty(inMessage))
            {
                builder.Append("\n\n").Append(inMessage);
            }

            string fullMessage = builder.Flush();

            UnityEngine.Debug.LogAssertion(fullMessage);

            ErrorResult result = ShowErrorMessage(fullMessage);

            if (result == ErrorResult.IgnoreAll)
            {
                s_IgnoredLocations.Add(locationHash);
            }
            else if (result == ErrorResult.Break)
            {
                s_Broken = true;
                #if UNITY_EDITOR
                if (EditorApplication.isPlaying)
                    UnityEngine.Debug.Break();
                #else
                // TODO: Crash screen?
                #endif // UNITY_EDITOR
            }
        }

        static private ErrorResult ShowErrorMessage(string inMessage)
        {
            #if UNITY_EDITOR
            if (EditorApplication.isPlaying && !s_Broken)
            {
                int result = EditorUtility.DisplayDialogComplex("Assert Failed", inMessage, "Ignore", "Ignore All", "Break");
                if (result == 0)
                    return ErrorResult.Ignore;
                if (result == 1)
                    return ErrorResult.IgnoreAll;
            }
            #endif // UNITY_EDITOR

            return ErrorResult.Break;
        }

        static private string GetLocationFromStack(int inDepth)
        {
            #if !DISABLE_STACK_TRACE

            StackTrace trace = new StackTrace();
            if (trace != null)
            {
                StackFrame frame = trace.GetFrame(1 + inDepth);
                if (frame != null)
                {
                    int lineNumber = frame.GetFileLineNumber();
                    int columnNumber = frame.GetFileColumnNumber();

                    string fileName = frame.GetFileName();
                    if (!string.IsNullOrEmpty(fileName) && lineNumber > 0)
                    {
                        return string.Format("{0} @{1},{2}", fileName, lineNumber, columnNumber);
                    }

                    MethodBase method = frame.GetMethod();
                    if (method != null)
                    {
                        if (lineNumber > 0)
                        {
                            return string.Format("{0}::{1} @{2},{3}", method.DeclaringType.Name, method.Name, lineNumber, columnNumber);
                        }
                        else
                        {
                            return string.Format("{0}::{1} @0x{2:X}", method.DeclaringType.Name, method.Name, frame.GetILOffset());
                        }
                    }
                }
            }

            #endif // DISABLE_STACK_TRACE

            return StackTraceDisabledMessage;
        }

        static private string GetLocationFromTrace(string inTrace)
        {
            // TODO: Implement
            return StackTraceDisabledMessage;
        }

        #endregion // Internal
    }
}