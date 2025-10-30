/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    2 Sept 2020
 * 
 * File:    Assert.cs
 * Purpose: Conditionally-compiled assertions.
 */

#if (UNITY_EDITOR && !IGNORE_UNITY_EDITOR) || DEVELOPMENT_BUILD
#define DEVELOPMENT
#endif // (UNITY_EDITOR && !IGNORE_UNITY_EDITOR) || DEVELOPMENT_BUILD

#if CSHARP_7_3_OR_NEWER
#define UNMANAGED_CONSTRAINT
#endif // CSHARP_7_3_OR_NEWER

#if NETSTANDARD || NET_STANDARD
#define CODEANALYSIS
#define ISFINITE
#endif // NETSTANDARD || NET_STANDARD

using System.Diagnostics;
using System;
using System.Text;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#if DEVELOPMENT
using System.Text.RegularExpressions;
#endif // DEVELOPMENT

#if CODEANALYSIS
using System.Diagnostics.CodeAnalysis;
#endif // CODEANALYSIS

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

        public enum FailureMode
        {
            User,
            Automatic,
        }

        private class AssertException : Exception
        {
            public AssertException(string inMessage)
                : base(AssertExceptionPrefix + inMessage)
            {}
        }

#if !CODEANALYSIS
        [Conditional("___DUMMY_EXCLUDED")]
        [AttributeUsage(AttributeTargets.Parameter)]
        private sealed class DoesNotReturnIf : Attribute
        {
            public DoesNotReturnIf(bool parameterValue) { }
        }

        [Conditional("___DUMMY_EXCLUDED")]
        [AttributeUsage(AttributeTargets.Method)]
        private sealed class DoesNotReturn : Attribute
        {
        }
#endif // !CODEANALYSIS

        #endregion // Types

        private const string AssertExceptionPrefix = "[Assert Failed]\n";
        private const string AssertConditionPrefix = "AssertException: " + AssertExceptionPrefix;
        private const string StackTraceUnavailableMessage = "[Stack Trace Unavailable]";

#if DEVELOPMENT
        static private bool s_RegisteredLogHook;
#endif // DEVELOPMENT

        static private FailureMode s_FailureMode;
        static private bool s_Broken;
        static private readonly HashSet<StringHash64> s_IgnoredLocations = new HashSet<StringHash64>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static private void Initialize()
        {
#if DEVELOPMENT
            Application.SetStackTraceLogType(LogType.Exception, StackTraceLogType.ScriptOnly);
            UnityEngine.Application.logMessageReceived += Application_logMessageReceived;
            s_RegisteredLogHook = true;
#else
            s_FailureMode = FailureMode.Automatic;
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

#if DEVELOPMENT

        static private void Application_logMessageReceived(string condition, string stackTrace, UnityEngine.LogType type)
        {
#if UNITY_EDITOR

            if (EditorApplication.isCompiling || EditorApplication.isUpdating || BuildPipeline.isBuildingPlayer)
                return;

#endif // UNITY_EDITOR

            switch(type)
            {
                case UnityEngine.LogType.Exception:
                    {
                        if (!condition.StartsWith(AssertConditionPrefix))
                        {
                            OnFail(GetLocationFromTrace(stackTrace, 0), condition, null, true);
                        }
                        break;
                    }
            }
        }

#endif // DEVELOPMENT

        [Conditional("DEVELOPMENT"), Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        static public void RegisterLogHook()
        {
#if DEVELOPMENT
            if (!s_RegisteredLogHook)
            {
                s_RegisteredLogHook = true;
                UnityEngine.Application.logMessageReceived += Application_logMessageReceived;
            }
#endif // DEVELOPMENT
        }

        [Conditional("DEVELOPMENT"), Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        static public void DeregisterLogHook()
        {
#if DEVELOPMENT
            if (s_RegisteredLogHook)
            {
                s_RegisteredLogHook = false;
                UnityEngine.Application.logMessageReceived -= Application_logMessageReceived;
            }
#endif // DEVELOPMENT
        }

        [Conditional("DEVELOPMENT"), Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        static public void SetFailureMode(FailureMode inMode)
        {
#if DEVELOPMENT
            s_FailureMode = inMode;
#endif // DEVELOPMENT
        }

        /// <summary>
        /// Resets any existing broken flags and ignored 
        /// </summary>
        [Conditional("DEVELOPMENT"), Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        static public void ClearBreak()
        {
#if DEVELOPMENT
            s_Broken = false;
#endif // DEVELOPMENT
        }

        /// <summary>
        /// Clears all ignored asserts.
        /// </summary>
        [Conditional("DEVELOPMENT"), Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        static public void ClearAllIgnores()
        {
#if DEVELOPMENT
            s_IgnoredLocations.Clear();
#endif // DEVELOPMENT
        }

        #region Fail

        /// <summary>
        /// Immediately fails.
        /// </summary>
        [DoesNotReturn]
        static public void Fail()
        {
            OnFail(GetLocationFromStack(1), "Assert Fail", null);
        }

        /// <summary>
        /// Immediately fails.
        /// </summary>
        [DoesNotReturn]
        static public void Fail(string inMessage)
        {
            OnFail(GetLocationFromStack(1), "Assert Fail", inMessage);
        }

        /// <summary>
        /// Immediately fails.
        /// </summary>
        [DoesNotReturn]
        static public void Fail<P0>(string inFormat, P0 inParam0)
        {
            OnFail(GetLocationFromStack(1), "Assert Fail", Log.Format(inFormat, inParam0));
        }

        /// <summary>
        /// Immediately fails.
        /// </summary>
        [DoesNotReturn]
        static public void Fail<P0, P1>(string inFormat, P0 inParam0, P1 inParam1)
        {
            OnFail(GetLocationFromStack(1), "Assert Fail", Log.Format(inFormat, inParam0, inParam1));
        }

        /// <summary>
        /// Immediately fails.
        /// </summary>
        [DoesNotReturn]
        static public void Fail<P0, P1, P2>(string inFormat, P0 inParam0, P1 inParam1, P2 inParam2)
        {
           OnFail(GetLocationFromStack(1), "Assert Fail", Log.Format(inFormat, inParam0, inParam1, inParam2));
        }

        /// <summary>
        /// Immediately fails.
        /// </summary>
        [DoesNotReturn]
        static public void Fail(string inFormat, params object[] inParams)
        {
            OnFail(GetLocationFromStack(1), "Assert Fail", Log.Format(inFormat, inParams));
        }

        #endregion // Fail

        #region True

        /// <summary>
        /// Asserts that a condition is true.
        /// </summary>
        [Conditional("DEVELOPMENT"), Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        static public void True([DoesNotReturnIf(false)] bool inbValue)
        {
            if (!inbValue)
            {
                OnFail(GetLocationFromStack(1), "Value is true", null);
            }
        }

        /// <summary>
        /// Asserts that a condition is true.
        /// </summary>
        [Conditional("DEVELOPMENT"), Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        static public void True([DoesNotReturnIf(false)] bool inbValue, string inMessage)
        {
            if (!inbValue)
            {
                OnFail(GetLocationFromStack(1), "Value is true", inMessage);
            }
        }

        /// <summary>
        /// Asserts that a condition is true.
        /// </summary>
        [Conditional("DEVELOPMENT"), Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        static public void True<P0>([DoesNotReturnIf(false)] bool inbValue, string inFormat, P0 inParam0)
        {
            if (!inbValue)
            {
                OnFail(GetLocationFromStack(1), "Value is true", Log.Format(inFormat, inParam0));
            }
        }

        /// <summary>
        /// Asserts that a condition is true.
        /// </summary>
        [Conditional("DEVELOPMENT"), Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        static public void True<P0, P1>([DoesNotReturnIf(false)] bool inbValue, string inFormat, P0 inParam0, P1 inParam1)
        {
            if (!inbValue)
            {
                OnFail(GetLocationFromStack(1), "Value is true", Log.Format(inFormat, inParam0, inParam1));
            }
        }

        /// <summary>
        /// Asserts that a condition is true.
        /// </summary>
        [Conditional("DEVELOPMENT"), Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        static public void True<P0, P1, P2>([DoesNotReturnIf(false)] bool inbValue, string inFormat, P0 inParam0, P1 inParam1, P2 inParam2)
        {
            if (!inbValue)
            {
                OnFail(GetLocationFromStack(1), "Value is true", Log.Format(inFormat, inParam0, inParam1, inParam2));
            }
        }

        /// <summary>
        /// Asserts that a condition is true.
        /// </summary>
        [Conditional("DEVELOPMENT"), Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        static public void True([DoesNotReturnIf(false)] bool inbValue, string inFormat, params object[] inParams)
        {
            if (!inbValue)
            {
                OnFail(GetLocationFromStack(1), "Value is true", Log.Format(inFormat, inParams));
            }
        }

        #endregion // True

        #region False

        /// <summary>
        /// Asserts that a condition is false.
        /// </summary>
        [Conditional("DEVELOPMENT"), Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        static public void False([DoesNotReturnIf(true)] bool inbValue)
        {
            if (inbValue)
            {
                OnFail(GetLocationFromStack(1), "Value is false", null);
            }
        }

        /// <summary>
        /// Asserts that a condition is false.
        /// </summary>
        [Conditional("DEVELOPMENT"), Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        static public void False([DoesNotReturnIf(true)] bool inbValue, string inMessage)
        {
            if (inbValue)
            {
                OnFail(GetLocationFromStack(1), "Value is false", inMessage);
            }
        }

        /// <summary>
        /// Asserts that a condition is false.
        /// </summary>
        [Conditional("DEVELOPMENT"), Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        static public void False<P0>([DoesNotReturnIf(true)] bool inbValue, string inFormat, P0 inParam0)
        {
            if (inbValue)
            {
                OnFail(GetLocationFromStack(1), "Value is false", Log.Format(inFormat, inParam0));
            }
        }

        /// <summary>
        /// Asserts that a condition is false.
        /// </summary>
        [Conditional("DEVELOPMENT"), Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        static public void False<P0, P1>([DoesNotReturnIf(true)] bool inbValue, string inFormat, P0 inParam0, P1 inParam1)
        {
            if (inbValue)
            {
                OnFail(GetLocationFromStack(1), "Value is false", Log.Format(inFormat, inParam0, inParam1));
            }
        }

        /// <summary>
        /// Asserts that a condition is false.
        /// </summary>
        [Conditional("DEVELOPMENT"), Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        static public void False<P0, P1, P2>([DoesNotReturnIf(true)] bool inbValue, string inFormat, P0 inParam0, P1 inParam1, P2 inParam2)
        {
            if (inbValue)
            {
                OnFail(GetLocationFromStack(1), "Value is false", Log.Format(inFormat, inParam0, inParam1, inParam2));
            }
        }

        /// <summary>
        /// Asserts that a condition is false.
        /// </summary>
        [Conditional("DEVELOPMENT"), Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        static public void False([DoesNotReturnIf(true)] bool inbValue, string inFormat, params object[] inParams)
        {
            if (inbValue)
            {
                OnFail(GetLocationFromStack(1), "Value is false", Log.Format(inFormat, inParams));
            }
        }

        #endregion // False

        #region NotNull

        /// <summary>
        /// Asserts that a value is not null.
        /// </summary>
        [Conditional("DEVELOPMENT"), Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        static public void NotNull<T>(T inValue) where T : class
        {
            if (ReferenceEquals(inValue, null))
            {
                OnFail(GetLocationFromStack(1), string.Format("Object {0} is not null", typeof(T).Name), null);
            }
        }

        /// <summary>
        /// Asserts that a value is not null.
        /// </summary>
        [Conditional("DEVELOPMENT"), Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        static public void NotNull<T>(T inValue, string inMessage) where T : class
        {
            if (ReferenceEquals(inValue, null))
            {
                OnFail(GetLocationFromStack(1), string.Format("Object {0} is not null", typeof(T).Name), inMessage);
            }
        }

        /// <summary>
        /// Asserts that a value is not null.
        /// </summary>
        [Conditional("DEVELOPMENT"), Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        static public void NotNull<T, P0>(T inValue, string inFormat, P0 inParam0) where T : class
        {
            if (ReferenceEquals(inValue, null))
            {
                OnFail(GetLocationFromStack(1), string.Format("Object {0} is not null", typeof(T).Name), Log.Format(inFormat, inParam0));
            }
        }

        /// <summary>
        /// Asserts that a value is not null.
        /// </summary>
        [Conditional("DEVELOPMENT"), Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        static public void NotNull<T, P0, P1>(T inValue, string inFormat, P0 inParam0, P1 inParam1) where T : class
        {
            if (ReferenceEquals(inValue, null))
            {
                OnFail(GetLocationFromStack(1), string.Format("Object {0} is not null", typeof(T).Name), Log.Format(inFormat, inParam0, inParam1));
            }
        }

        /// <summary>
        /// Asserts that a value is not null.
        /// </summary>
        [Conditional("DEVELOPMENT"), Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        static public void NotNull<T, P0, P1, P2>(T inValue, string inFormat, P0 inParam0, P1 inParam1, P2 inParam2) where T : class
        {
            if (ReferenceEquals(inValue, null))
            {
                OnFail(GetLocationFromStack(1), string.Format("Object {0} is not null", typeof(T).Name), Log.Format(inFormat, inParam0, inParam1, inParam2));
            }
        }

        /// <summary>
        /// Asserts that a value is not null.
        /// </summary>
        [Conditional("DEVELOPMENT"), Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        static public void NotNull<T>(T inValue, string inFormat, params object[] inParams) where T : class
        {
            if (ReferenceEquals(inValue, null))
            {
                OnFail(GetLocationFromStack(1), string.Format("Object {0} is not null", typeof(T).Name), Log.Format(inFormat, inParams));
            }
        }

        #endregion // NotNull

        #region NotNull (Pointer)

#if UNMANAGED_CONSTRAINT

        /// <summary>
        /// Asserts that a pointer is not null.
        /// </summary>
        [Conditional("DEVELOPMENT"), Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        static public unsafe void NotNull<T>(T* inPointer) where T : unmanaged
        {
            if (inPointer == null)
            {
                OnFail(GetLocationFromStack(1), string.Format("Pointer {0} is not null", typeof(T).Name), null);
            }
        }

        /// <summary>
        /// Asserts that a pointer is not null.
        /// </summary>
        [Conditional("DEVELOPMENT"), Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        static public unsafe void NotNull<T>(T* inPointer, string inMessage) where T : unmanaged
        {
            if (inPointer == null)
            {
                OnFail(GetLocationFromStack(1), string.Format("Pointer {0} is not null", typeof(T).Name), inMessage);
            }
        }

        /// <summary>
        /// Asserts that a pointer is not null.
        /// </summary>
        [Conditional("DEVELOPMENT"), Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        static public unsafe void NotNull<T, P0>(T* inPointer, string inFormat, P0 inParam0) where T : unmanaged
        {
            if (inPointer == null)
            {
                OnFail(GetLocationFromStack(1), string.Format("Pointer {0} is not null", typeof(T).Name), Log.Format(inFormat, inParam0));
            }
        }

        /// <summary>
        /// Asserts that a pointer is not null.
        /// </summary>
        [Conditional("DEVELOPMENT"), Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        static public unsafe void NotNull<T, P0, P1>(T* inPointer, string inFormat, P0 inParam0, P1 inParam1) where T : unmanaged
        {
            if (inPointer == null)
            {
                OnFail(GetLocationFromStack(1), string.Format("Pointer {0} is not null", typeof(T).Name), Log.Format(inFormat, inParam0, inParam1));
            }
        }

        /// <summary>
        /// Asserts that a pointer is not null.
        /// </summary>
        [Conditional("DEVELOPMENT"), Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        static public unsafe void NotNull<T, P0, P1, P2>(T* inPointer, string inFormat, P0 inParam0, P1 inParam1, P2 inParam2) where T : unmanaged
        {
            if (inPointer == null)
            {
                OnFail(GetLocationFromStack(1), string.Format("Pointer {0} is not null", typeof(T).Name), Log.Format(inFormat, inParam0, inParam1, inParam2));
            }
        }

        /// <summary>
        /// Asserts that a pointer is not null.
        /// </summary>
        [Conditional("DEVELOPMENT"), Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        static public unsafe void NotNull<T>(T* inPointer, string inFormat, params object[] inParams) where T : unmanaged
        {
            if (inPointer == null)
            {
                OnFail(GetLocationFromStack(1), string.Format("Pointer {0} is not null", typeof(T).Name), Log.Format(inFormat, inParams));
            }
        }

#endif // UNMANAGED_CONSTRAINT

        #region Void Pointer

        /// <summary>
        /// Asserts that a pointer is not null.
        /// </summary>
        [Conditional("DEVELOPMENT"), Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        static public unsafe void NotNull(void* inPointer)
        {
            if (inPointer == null)
            {
                OnFail(GetLocationFromStack(1), "Pointer is not null", null);
            }
        }

        /// <summary>
        /// Asserts that a pointer is not null.
        /// </summary>
        [Conditional("DEVELOPMENT"), Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        static public unsafe void NotNull(void* inPointer, string inMessage)
        {
            if (inPointer == null)
            {
                OnFail(GetLocationFromStack(1), "Pointer is not null", inMessage);
            }
        }

        /// <summary>
        /// Asserts that a pointer is not null.
        /// </summary>
        [Conditional("DEVELOPMENT"), Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        static public unsafe void NotNull<P0>(void* inPointer, string inFormat, P0 inParam0)
        {
            if (inPointer == null)
            {
                OnFail(GetLocationFromStack(1), "Pointer is not null", Log.Format(inFormat, inParam0));
            }
        }

        /// <summary>
        /// Asserts that a pointer is not null.
        /// </summary>
        [Conditional("DEVELOPMENT"), Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        static public unsafe void NotNull<P0, P1>(void* inPointer, string inFormat, P0 inParam0, P1 inParam1)
        {
            if (inPointer == null)
            {
                OnFail(GetLocationFromStack(1), "Pointer is not null", Log.Format(inFormat, inParam0, inParam1));
            }
        }

        /// <summary>
        /// Asserts that a pointer is not null.
        /// </summary>
        [Conditional("DEVELOPMENT"), Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        static public unsafe void NotNull<P0, P1, P2>(void* inPointer, string inFormat, P0 inParam0, P1 inParam1, P2 inParam2)
        {
            if (inPointer == null)
            {
                OnFail(GetLocationFromStack(1), "Pointer is not null", Log.Format(inFormat, inParam0, inParam1, inParam2));
            }
        }

        /// <summary>
        /// Asserts that a pointer is not null.
        /// </summary>
        [Conditional("DEVELOPMENT"), Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        static public unsafe void NotNull(void* inPointer, string inFormat, params object[] inParams)
        {
            if (inPointer == null)
            {
                OnFail(GetLocationFromStack(1), "Pointer is not null", Log.Format(inFormat, inParams));
            }
        }

        #endregion // Void Pointer

        #endregion // NotNullPtr

        #region NotNullOrDestroyed

        /// <summary>
        /// Asserts that a value is not null or destroyed.
        /// </summary>
        [Conditional("DEVELOPMENT"), Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        static public void NotNullOrDestroyed<T>(T inValue) where T : class
        {
            if (ReferenceEquals(inValue, null))
            {
                OnFail(GetLocationFromStack(1), string.Format("Object {0} is not null", typeof(T).Name), null);
            }
            if (IsDestroyed(inValue))
            {
                OnFail(GetLocationFromStack(1), string.Format("Object {0} is not destroyed", typeof(T).Name), null);
            }
        }

        /// <summary>
        /// Asserts that a value is not null or destroyed.
        /// </summary>
        [Conditional("DEVELOPMENT"), Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        static public void NotNullOrDestroyed<T>(T inValue, string inMessage) where T : class
        {
            if (ReferenceEquals(inValue, null))
            {
                OnFail(GetLocationFromStack(1), string.Format("Object {0} is not null", typeof(T).Name), inMessage);
            }
            if (IsDestroyed(inValue))
            {
                OnFail(GetLocationFromStack(1), string.Format("Object {0} is not destroyed", typeof(T).Name), inMessage);
            }
        }

        /// <summary>
        /// Asserts that a value is not null or destroyed.
        /// </summary>
        [Conditional("DEVELOPMENT"), Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        static public void NotNullOrDestroyed<T, P0>(T inValue, string inFormat, P0 inParam0) where T : class
        {
            if (ReferenceEquals(inValue, null))
            {
                OnFail(GetLocationFromStack(1), string.Format("Object {0} is not null", typeof(T).Name), Log.Format(inFormat, inParam0));
            }
            if (IsDestroyed(inValue))
            {
                OnFail(GetLocationFromStack(1), string.Format("Object {0} is not destroyed", typeof(T).Name), Log.Format(inFormat, inParam0));
            }
        }

        /// <summary>
        /// Asserts that a value is not null or destroyed.
        /// </summary>
        [Conditional("DEVELOPMENT"), Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        static public void NotNullOrDestroyed<T, P0, P1>(T inValue, string inFormat, P0 inParam0, P1 inParam1) where T : class
        {
            if (ReferenceEquals(inValue, null))
            {
                OnFail(GetLocationFromStack(1), string.Format("Object {0} is not null", typeof(T).Name), Log.Format(inFormat, inParam0, inParam1));
            }
            if (IsDestroyed(inValue))
            {
                OnFail(GetLocationFromStack(1), string.Format("Object {0} is not destroyed", typeof(T).Name), Log.Format(inFormat, inParam0, inParam1));
            }
        }

        /// <summary>
        /// Asserts that a value is not null or destroyed.
        /// </summary>
        [Conditional("DEVELOPMENT"), Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        static public void NotNullOrDestroyed<T, P0, P1, P2>(T inValue, string inFormat, P0 inParam0, P1 inParam1, P2 inParam2) where T : class
        {
            if (ReferenceEquals(inValue, null))
            {
                OnFail(GetLocationFromStack(1), string.Format("Object {0} is not null", typeof(T).Name), Log.Format(inFormat, inParam0, inParam1, inParam2));
            }
            if (IsDestroyed(inValue))
            {
                OnFail(GetLocationFromStack(1), string.Format("Object {0} is not destroyed", typeof(T).Name), Log.Format(inFormat, inParam0, inParam1, inParam2));
            }
        }

        /// <summary>
        /// Asserts that a value is not null or destroyed.
        /// </summary>
        [Conditional("DEVELOPMENT"), Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        static public void NotNullOrDestroyed<T>(T inValue, string inFormat, params object[] inParams) where T : class
        {
            if (ReferenceEquals(inValue, null))
            {
                OnFail(GetLocationFromStack(1), string.Format("Object {0} is not null", typeof(T).Name), Log.Format(inFormat, inParams));
            }
            if (IsDestroyed(inValue))
            {
                OnFail(GetLocationFromStack(1), string.Format("Object {0} is not destroyed", typeof(T).Name), Log.Format(inFormat, inParams));
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static private bool IsDestroyed<T>(T inValue) where T : class
        {
            UnityEngine.Object castedObj;
            return !ReferenceEquals(castedObj = inValue as UnityEngine.Object, null) && !castedObj;
        }

        #endregion // NotNullOrDestroyed

        #region Finite

#if !ISFINITE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static private bool IsFinite(float inValue)
        {
            return !float.IsInfinity(inValue) && !float.IsNaN(inValue);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static private bool IsFinite(double inValue)
        {
            return !double.IsInfinity(inValue) && !double.IsNaN(inValue);
        }
#endif // !ISFINITE

        /// <summary>
        /// Asserts that a floating point value is finite.
        /// </summary>
        [Conditional("DEVELOPMENT"), Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        static public void Finite(float inValue)
        {
#if ISFINITE
            if (!float.IsFinite(inValue))
#else
            if (!IsFinite(inValue))
#endif // ISFINITE
            {
                OnFail(GetLocationFromStack(1), string.Format("Floating point value {0} is not finite", inValue), null);
            }
        }

        /// <summary>
        /// Asserts that a floating point value is finite.
        /// </summary>
        [Conditional("DEVELOPMENT"), Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        static public void Finite(float inValue, string inMessage)
        {
#if ISFINITE
            if (!float.IsFinite(inValue))
#else
            if (!IsFinite(inValue))
#endif // ISFINITE
            {
                OnFail(GetLocationFromStack(1), string.Format("Floating point value {0} is not finite", inValue), inMessage);
            }
        }

        /// <summary>
        /// Asserts that a floating point value is finite.
        /// </summary>
        [Conditional("DEVELOPMENT"), Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        static public void Finite<P0>(float inValue, string inFormat, P0 inParam0)
        {
#if ISFINITE
            if (!float.IsFinite(inValue))
#else
            if (!IsFinite(inValue))
#endif // ISFINITE
            {
                OnFail(GetLocationFromStack(1), string.Format("Floating point value {0} is not finite", inValue), Log.Format(inFormat, inParam0));
            }
        }

        /// <summary>
        /// Asserts that a floating point value is finite.
        /// </summary>
        [Conditional("DEVELOPMENT"), Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        static public void Finite<P0, P1>(float inValue, string inFormat, P0 inParam0, P1 inParam1)
        {
#if ISFINITE
            if (!float.IsFinite(inValue))
#else
            if (!IsFinite(inValue))
#endif // ISFINITE
            {
                OnFail(GetLocationFromStack(1), string.Format("Floating point value {0} is not finite", inValue), Log.Format(inFormat, inParam0, inParam1));
            }
        }

        /// <summary>
        /// Asserts that a floating point value is finite.
        /// </summary>
        [Conditional("DEVELOPMENT"), Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        static public void Finite<P0, P1, P2>(float inValue, string inFormat, P0 inParam0, P1 inParam1, P2 inParam2)
        {
#if ISFINITE
            if (!float.IsFinite(inValue))
#else
            if (!IsFinite(inValue))
#endif // ISFINITE
            {
                OnFail(GetLocationFromStack(1), string.Format("Floating point value {0} is not finite", inValue), Log.Format(inFormat, inParam0, inParam1, inParam2));
            }
        }

        /// <summary>
        /// Asserts that a floating point value is finite.
        /// </summary>
        [Conditional("DEVELOPMENT"), Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        static public void Finite(float inValue, string inFormat, params object[] inParams)
        {
#if ISFINITE
            if (!float.IsFinite(inValue))
#else
            if (!IsFinite(inValue))
#endif // ISFINITE
            {
                OnFail(GetLocationFromStack(1), string.Format("Floating point value {0} is not finite", inValue), Log.Format(inFormat, inParams));
            }
        }

        /// <summary>
        /// Asserts that a floating point value is finite.
        /// </summary>
        [Conditional("DEVELOPMENT"), Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        static public void Finite(double inValue)
        {
#if ISFINITE
            if (!double.IsFinite(inValue))
#else
            if (!IsFinite(inValue))
#endif // ISFINITE
            {
                OnFail(GetLocationFromStack(1), string.Format("Floating point value {0} is not finite", inValue), null);
            }
        }

        /// <summary>
        /// Asserts that a floating point value is finite.
        /// </summary>
        [Conditional("DEVELOPMENT"), Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        static public void Finite(double inValue, string inMessage)
        {
#if ISFINITE
            if (!double.IsFinite(inValue))
#else
            if (!IsFinite(inValue))
#endif // ISFINITE
            {
                OnFail(GetLocationFromStack(1), string.Format("Floating point value {0} is not finite", inValue), inMessage);
            }
        }

        /// <summary>
        /// Asserts that a floating point value is finite.
        /// </summary>
        [Conditional("DEVELOPMENT"), Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        static public void Finite<P0>(double inValue, string inFormat, P0 inParam0)
        {
#if ISFINITE
            if (!double.IsFinite(inValue))
#else
            if (!IsFinite(inValue))
#endif // ISFINITE
            {
                OnFail(GetLocationFromStack(1), string.Format("Floating point value {0} is not finite", inValue), Log.Format(inFormat, inParam0));
            }
        }

        /// <summary>
        /// Asserts that a floating point value is finite.
        /// </summary>
        [Conditional("DEVELOPMENT"), Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        static public void Finite<P0, P1>(double inValue, string inFormat, P0 inParam0, P1 inParam1)
        {
#if ISFINITE
            if (!double.IsFinite(inValue))
#else
            if (!IsFinite(inValue))
#endif // ISFINITE
            {
                OnFail(GetLocationFromStack(1), string.Format("Floating point value {0} is not finite", inValue), Log.Format(inFormat, inParam0, inParam1));
            }
        }

        /// <summary>
        /// Asserts that a floating point value is finite.
        /// </summary>
        [Conditional("DEVELOPMENT"), Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        static public void Finite<P0, P1, P2>(double inValue, string inFormat, P0 inParam0, P1 inParam1, P2 inParam2)
        {
#if ISFINITE
            if (!double.IsFinite(inValue))
#else
            if (!IsFinite(inValue))
#endif // ISFINITE
            {
                OnFail(GetLocationFromStack(1), string.Format("Floating point value {0} is not finite", inValue), Log.Format(inFormat, inParam0, inParam1, inParam2));
            }
        }

        /// <summary>
        /// Asserts that a floating point value is finite.
        /// </summary>
        [Conditional("DEVELOPMENT"), Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        static public void Finite(double inValue, string inFormat, params object[] inParams)
        {
#if ISFINITE
            if (!double.IsFinite(inValue))
#else
            if (!IsFinite(inValue))
#endif // ISFINITE
            {
                OnFail(GetLocationFromStack(1), string.Format("Floating point value {0} is not finite", inValue), Log.Format(inFormat, inParams));
            }
        }

        #endregion // Finite

        #region Internal

        static private void OnFail(string inLocation, string inCondition, string inMessage, bool fromException = false)
        {
            if (s_Broken)
                return;
            
            StringHash64 locationHash = StringHash64.Fast(string.Format("{0}@{1}", inCondition, inLocation));
            if (s_IgnoredLocations.Contains(locationHash))
                return;

            StringBuilder builder = new StringBuilder();
            builder.Append("Location: ").Append(inLocation)
                .Append("\nCondition: ").Append(inCondition ?? string.Empty);

            if (!string.IsNullOrEmpty(inMessage))
            {
                builder.Append("\n\n").Append(inMessage);
            }

            string fullMessage = builder.Flush();

            UnityEngine.Debug.LogAssertion(fullMessage);

            bool bPrevCursorState = Cursor.visible;
            Cursor.visible = true;
            ErrorResult result = ShowErrorMessage(fullMessage);
            Cursor.visible = bPrevCursorState;

            if (result == ErrorResult.IgnoreAll)
            {
                s_IgnoredLocations.Add(locationHash);
            }
            else if (result == ErrorResult.Break)
            {
#if UNITY_EDITOR
                if (EditorApplication.isPlaying)
                {
                    UnityEngine.Debug.Break();
                    s_Broken = true;
                }
                else if (!fromException)
                {
                    throw new AssertException(fullMessage);
                }
#else
                s_Broken = true;
                if (!fromException)
                    throw new AssertException(fullMessage);
#endif // UNITY_EDITOR
            }
        }

        static private ErrorResult ShowErrorMessage(string inMessage)
        {
            if (!s_Broken && s_FailureMode == FailureMode.User)
            {
#if (UNITY_EDITOR && !IGNORE_UNITY_EDITOR)
                if (EditorApplication.isPlaying)
                {
                    int result = EditorUtility.DisplayDialogComplex("Assert Failed", inMessage, "Ignore", "Ignore All", "Break");
                    if (result == 0)
                        return ErrorResult.Ignore;
                    if (result == 1)
                        return ErrorResult.IgnoreAll;
                }
#elif (UNITY_WEBGL && !UNITY_EDITOR)
                BeauUtil_DisplayAssertionAlert(inMessage);
#endif // (UNITY_EDITOR && !IGNORE_UNITY_EDITOR)
            }

            return ErrorResult.Break;
        }

        static internal string GetLocationFromStack(int inDepth)
        {
            StackTrace trace;
            StackFrame frame;
            trace = new StackTrace(true);

            //Console.WriteLine(trace.ToString());

            if (trace != null && (frame = trace.GetFrame(inDepth + 1)) != null)
            {
                MethodBase method = frame.GetMethod();
                if (method != null)
                {
                    string file = frame.GetFileName();
                    int lineNum = frame.GetFileLineNumber();

                    if (!string.IsNullOrEmpty(file))
                    {
                        return string.Format("{0}:{1} @({2}:{3})", method.DeclaringType.FullName, method.Name, frame.GetFileName(), frame.GetFileLineNumber());
                    }
                    else
                    {
                        return string.Format("{0}:{1} @(0x{2:X})", method.DeclaringType.FullName, method.Name, frame.GetILOffset());
                    }
                }
            }

            return StackTraceUnavailableMessage;
        }

#if DEVELOPMENT

        static private Regex TraceLocationRegexA;
        static private Regex TraceLocationRegexB;

        static private string GetLocationFromTrace(string inTrace, int inDepth)
        {
            //Console.WriteLine("DUMPING STACK TRACE");
            //Console.WriteLine(inTrace.Replace("\n", "\\n"));

            TraceLocationRegexA = TraceLocationRegexA ?? new Regex("^(.+?)\\s\\(at (.+?):(\\d+)\\)", RegexOptions.Multiline);
            
            var matches = TraceLocationRegexA.Matches(inTrace);
            if (matches.Count > inDepth)
            {
                var match = matches[inDepth];
                return string.Format("{0} @({1}:{2})", match.Groups[1].Value, match.Groups[2].Value, match.Groups[3].Value);
            }

            TraceLocationRegexB = TraceLocationRegexB ?? new Regex("^\\s+?at (.+?)\\s\\[(0x\\d+?)\\] in (.+?):(\\d+)", RegexOptions.Multiline);

            matches = TraceLocationRegexB.Matches(inTrace);
            if (matches.Count > inDepth)
            {
                var match = matches[inDepth];
                return string.Format("{0} @({1}:{2})", match.Groups[1].Value, match.Groups[3].Value, match.Groups[4].Value);
            }

            return StackTraceUnavailableMessage;
        }

#endif // DEVELOPMENT

#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        static private extern void BeauUtil_DisplayAssertionAlert(string message);
#endif // UNITY_WEBGL && !UNITY_EDITOR

        #endregion // Internal
    }
}