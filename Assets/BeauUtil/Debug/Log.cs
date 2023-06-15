/*
 * Copyright (C) 2021. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    20 May 2021
 * 
 * File:    Log.cs
 * Purpose: Conditionally-compiled logging.
 */

#if (UNITY_EDITOR && !IGNORE_UNITY_EDITOR) || DEVELOPMENT_BUILD || DEVELOPMENT
#define ENABLE_LOGGING_BEAUUTIL
#endif // (UNITY_EDITOR && !IGNORE_UNITY_EDITOR) || DEVELOPMENT_BUILD || DEVELOPMENT

#if NET_4_6
#define INJECT_LOCATION
#endif // NET_4_6

using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;

namespace BeauUtil.Debugger
{
    /// <summary>
    /// Logging utilities (only in development builds)
    /// </summary>
    static public class Log
    {
        #region Location

        #if INJECT_LOCATION

        /// <summary>
        /// Attempts to returns the location of the caller.
        /// </summary>
        static public string Here([CallerFilePath] string inCallerFilePath = "", [CallerLineNumber] int inCallerLineNumber = 0)
        {
            return string.Format("{0} @{1}", Path.GetFileName(inCallerFilePath), inCallerLineNumber);
        }

        #else 

        /// <summary>
        /// Attempts to return the location of the caller.
        /// </summary>
        static public string Here()
        {
            return Assert.GetLocationFromStack(1);
        }

        #endif // INJECT_LOCATION

        #endregion // Location

        #region Formatting

        /// <summary>
        /// Formats a string, converting any arguments over to their debug versions if available.
        /// </summary>
        static public string Format(string inFormat, object inArg0)
        {
            DebugFormat(ref inArg0);
            return string.Format(inFormat, inArg0);
        }

        /// <summary>
        /// Formats a string, converting any arguments over to their debug versions if available.
        /// </summary>
        static public string Format(string inFormat, object inArg0, object inArg1)
        {
            DebugFormat(ref inArg0);
            DebugFormat(ref inArg1);
            return string.Format(inFormat, inArg0, inArg1);
        }

        /// <summary>
        /// Formats a string, converting any arguments over to their debug versions if available.
        /// </summary>
        static public string Format(string inFormat, object inArg0, object inArg1, object inArg2)
        {
            DebugFormat(ref inArg0);
            DebugFormat(ref inArg1);
            DebugFormat(ref inArg2);
            return string.Format(inFormat, inArg0, inArg1, inArg2);
        }

        /// <summary>
        /// Formats a string, converting any arguments over to their debug versions if available.
        /// </summary>
        static public string Format(string inFormat, params object[] inArguments)
        {
            if (inArguments != null)
            {
                for(int i = 0, length = inArguments.Length; i < length; i++)
                    DebugFormat(ref inArguments[i]);
            }

            return string.Format(inFormat, inArguments);
        }

        static private void DebugFormat(ref object ioObject)
        {
            IDebugString debugString = ioObject as IDebugString;
            if (debugString != null)
                ioObject = debugString.ToDebugString();
        }

        #endregion // Formatting
    
        #region Message

        [Conditional("DEVELOPMENT"), Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD"), Conditional("ENABLE_LOGGING_BEAUUTIL")]
        static public void Msg(string inMessage)
        {
            UnityEngine.Debug.LogFormat(inMessage, Array.Empty<object>());
        }

        [Conditional("DEVELOPMENT"), Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD"), Conditional("ENABLE_LOGGING_BEAUUTIL")]
        static public void Msg(string inMessage, object inArg0)
        {
            UnityEngine.Debug.LogFormat(Format(inMessage, inArg0), Array.Empty<object>());
        }

        [Conditional("DEVELOPMENT"), Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD"), Conditional("ENABLE_LOGGING_BEAUUTIL")]
        static public void Msg(string inMessage, object inArg0, object inArg1)
        {
            UnityEngine.Debug.LogFormat(Format(inMessage, inArg0, inArg1), Array.Empty<object>());
        }

        [Conditional("DEVELOPMENT"), Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD"), Conditional("ENABLE_LOGGING_BEAUUTIL")]
        static public void Msg(string inMessage, object inArg0, object inArg1, object inArg2)
        {
            UnityEngine.Debug.LogFormat(Format(inMessage, inArg0, inArg1, inArg2), Array.Empty<object>());
        }

        [Conditional("DEVELOPMENT"), Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD"), Conditional("ENABLE_LOGGING_BEAUUTIL")]
        static public void Msg(string inMessage, params object[] inArgs)
        {
            UnityEngine.Debug.LogFormat(Format(inMessage, inArgs), Array.Empty<object>());
        }

        #endregion // Message

        #region Warn

        [Conditional("DEVELOPMENT"), Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD"), Conditional("ENABLE_LOGGING_BEAUUTIL"), Conditional("ENABLE_LOGGING_WARNINGS_BEAUUTIL")]
        static public void Warn(string inMessage)
        {
            UnityEngine.Debug.LogWarningFormat(inMessage, Array.Empty<object>());
        }

        [Conditional("DEVELOPMENT"), Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD"), Conditional("ENABLE_LOGGING_BEAUUTIL"), Conditional("ENABLE_LOGGING_WARNINGS_BEAUUTIL")]
        static public void Warn(string inMessage, object inArg0)
        {
            UnityEngine.Debug.LogWarningFormat(Format(inMessage, inArg0), Array.Empty<object>());
        }

        [Conditional("DEVELOPMENT"), Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD"), Conditional("ENABLE_LOGGING_BEAUUTIL"), Conditional("ENABLE_LOGGING_WARNINGS_BEAUUTIL")]
        static public void Warn(string inMessage, object inArg0, object inArg1)
        {
            UnityEngine.Debug.LogWarningFormat(Format(inMessage, inArg0, inArg1), Array.Empty<object>());
        }

        [Conditional("DEVELOPMENT"), Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD"), Conditional("ENABLE_LOGGING_BEAUUTIL"), Conditional("ENABLE_LOGGING_WARNINGS_BEAUUTIL")]
        static public void Warn(string inMessage, object inArg0, object inArg1, object inArg2)
        {
            UnityEngine.Debug.LogWarningFormat(Format(inMessage, inArg0, inArg1, inArg2), Array.Empty<object>());
        }

        [Conditional("DEVELOPMENT"), Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD"), Conditional("ENABLE_LOGGING_BEAUUTIL"), Conditional("ENABLE_LOGGING_WARNINGS_BEAUUTIL")]
        static public void Warn(string inMessage, params object[] inArgs)
        {
            UnityEngine.Debug.LogWarningFormat(Format(inMessage, inArgs), Array.Empty<object>());
        }

        #endregion // Warn

        #region Error

        [Conditional("DEVELOPMENT"), Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD"), Conditional("ENABLE_LOGGING_BEAUUTIL"), Conditional("ENABLE_LOGGING_ERRORS_BEAUUTIL")]
        static public void Error(string inMessage)
        {
            UnityEngine.Debug.LogErrorFormat(inMessage, Array.Empty<object>());
        }

        [Conditional("DEVELOPMENT"), Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD"), Conditional("ENABLE_LOGGING_BEAUUTIL"), Conditional("ENABLE_LOGGING_ERRORS_BEAUUTIL")]
        static public void Error(string inMessage, object inArg0)
        {
            UnityEngine.Debug.LogErrorFormat(Format(inMessage, inArg0), Array.Empty<object>());
        }

        [Conditional("DEVELOPMENT"), Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD"), Conditional("ENABLE_LOGGING_BEAUUTIL"), Conditional("ENABLE_LOGGING_ERRORS_BEAUUTIL")]
        static public void Error(string inMessage, object inArg0, object inArg1)
        {
            UnityEngine.Debug.LogErrorFormat(Format(inMessage, inArg0, inArg1), Array.Empty<object>());
        }

        [Conditional("DEVELOPMENT"), Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD"), Conditional("ENABLE_LOGGING_BEAUUTIL"), Conditional("ENABLE_LOGGING_ERRORS_BEAUUTIL")]
        static public void Error(string inMessage, object inArg0, object inArg1, object inArg2)
        {
            UnityEngine.Debug.LogErrorFormat(Format(inMessage, inArg0, inArg1, inArg2), Array.Empty<object>());
        }

        [Conditional("DEVELOPMENT"), Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD"), Conditional("ENABLE_LOGGING_BEAUUTIL"), Conditional("ENABLE_LOGGING_ERRORS_BEAUUTIL")]
        static public void Error(string inMessage, params object[] inArgs)
        {
            UnityEngine.Debug.LogErrorFormat(Format(inMessage, inArgs), Array.Empty<object>());
        }

        #endregion // Error

        #region Stack Traces

        /// <summary>
        /// Scope object that temporarily sets the stack trace level for a given log type.
        /// </summary>
        public struct StackTraceLevelScope : IDisposable
        {
            private UnityEngine.LogType m_Type;
            private UnityEngine.StackTraceLogType m_RestoreLevel;
            private bool m_Disposed;

            public StackTraceLevelScope(UnityEngine.LogType inType, UnityEngine.StackTraceLogType inDesiredLevel)
            {
                m_Type = inType;
                m_RestoreLevel = UnityEngine.Application.GetStackTraceLogType(inType);
                UnityEngine.Application.SetStackTraceLogType(inType, inDesiredLevel);
                m_Disposed = false;
            }

            public void Dispose()
            {
                if (!m_Disposed)
                {
                    m_Disposed = true;
                    UnityEngine.Application.SetStackTraceLogType(m_Type, m_RestoreLevel);
                }
            }
        }

        /// <summary>
        /// Returns a scope that temporarily disables stack trace logging for regular logs.
        /// </summary>
        static public StackTraceLevelScope DisableMsgStackTrace()
        {
            return new StackTraceLevelScope(UnityEngine.LogType.Log, UnityEngine.StackTraceLogType.None);
        }

        /// <summary>
        /// Returns a scope that temporarily disables stack trace logging for warnings.
        /// </summary>
        static public StackTraceLevelScope DisableWarnStackTrace()
        {
            return new StackTraceLevelScope(UnityEngine.LogType.Warning, UnityEngine.StackTraceLogType.None);
        }

        /// <summary>
        /// Returns a scope that temporarily disables stack trace logging for errors.
        /// </summary>
        static public StackTraceLevelScope DisableErrorStackTrace()
        {
            return new StackTraceLevelScope(UnityEngine.LogType.Error, UnityEngine.StackTraceLogType.None);
        }

        #endregion // Stack Traces
    }
}