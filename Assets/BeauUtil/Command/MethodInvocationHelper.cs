/*
 * Copyright (C) 2022. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    15 Oct 2022
 * 
 * File:    MethodInvocationHelper.cs
 * Purpose: Method invocation helper type.
 */

#if CSHARP_7_3_OR_NEWER
#define EXPANDED_REFS
#endif // CSHARP_7_3_OR_NEWER

// NOTE: Since MethodInvocationHelper instances
// are largely cached for the lifetime of the application,
// using function pointers on a JIT platform is not recommended.
// We could end up with pointers to unoptimized code
// if JIT compilation decides to recompile the function.
// Therefore, only enable this optimization on AOT platforms.

#if UNITY_2021_2_OR_NEWER && ENABLE_IL2CPP && !BEAUUTIL_DISABLE_FUNCTION_POINTERS
#define SPECIALIZE_WITH_FUNCTION_POINTERS
#endif // UNITY_2021_2_OR_NEWER

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using BeauUtil.Variants;
using UnityEngine;

namespace BeauUtil
{
    public struct MethodInvocationHelper
    {
        public const int MaxArguments = 8;

        #region Types

        // no return type
        private delegate void Void_Delegate(); //
        private delegate bool Bool_Delegate(); //
        private delegate int Int_Delegate(); //
        private delegate float Float_Delegate();
        private delegate StringHash32 StringHash32_Delegate(); //
        // private delegate Variant Variant_Delegate();
        private delegate IEnumerator IEnumerator_Delegate(); //

        // bool argument
        private delegate void Void_Bool_Delegate(bool p0); //
        // private delegate bool Bool_Bool_Delegate(bool p0);
        // private delegate int Int_Bool_Delegate(bool p0);
        // private delegate float Float_Bool_Delegate(bool p0);
        // private delegate StringHash32 StringHash32_Bool_Delegate(bool p0);
        // private delegate Variant Variant_Bool_Delegate(bool p0);
        // private delegate IEnumerator IEnumerator_Bool_Delegate(bool p0);

        // int argument
        private delegate void Void_Int_Delegate(int p0); //
        // private delegate bool Bool_Int_Delegate(int p0);
        // private delegate int Int_Int_Delegate(int p0);
        // private delegate float Float_Int_Delegate(int p0);
        // private delegate StringHash32 StringHash32_Int_Delegate(int p0);
        // private delegate Variant Variant_Int_Delegate(int p0);
        // private delegate IEnumerator IEnumerator_Int_Delegate(int p0);

        // float argument
        private delegate void Void_Float_Delegate(float p0); //
        // private delegate bool Bool_Float_Delegate(float p0);
        // private delegate int Int_Float_Delegate(float p0);
        // private delegate float Float_Float_Delegate(float p0);
        // private delegate StringHash32 StringHash32_Float_Delegate(float p0);
        // private delegate Variant Variant_Float_Delegate(float p0);
        private delegate IEnumerator IEnumerator_Float_Delegate(float p0); //

        // string hash argument
        private delegate void Void_StringHash32_Delegate(StringHash32 p0); //
        private delegate bool Bool_StringHash32_Delegate(StringHash32 p0); //
        private delegate int Int_StringHash32_Delegate(StringHash32 p0); //
        private delegate float Float_StringHash32_Delegate(StringHash32 p0);
        private delegate StringHash32 StringHash32_StringHash32_Delegate(StringHash32 p0);
        // private delegate Variant Variant_StringHash32_Delegate(Variant p0);
        private delegate IEnumerator IEnumerator_StringHash32_Delegate(StringHash32 p0);

        // StringSlice argument
        private delegate void Void_StringSlice_Delegate(StringSlice p0); //
        private delegate bool Bool_StringSlice_Delegate(StringSlice p0); //
        // private delegate IEnumerator IEnumerator_StringSlice_Delegate(StringSlice p0);

        // String argument
        private delegate void Void_String_Delegate(string p0); //
        private delegate bool Bool_String_Delegate(string p0); //

        private unsafe struct SpecializationInfo
        {
            public short Type;
            public Delegate Delegate;
#if SPECIALIZE_WITH_FUNCTION_POINTERS
            public void* FunctionPtr;
#endif // SPECIALIZE_WITH_FUNCTION_POINTERS
            public NonBoxedValue DefaultArgument;
        }

        private struct UnspecializedInfo
        {
            public object[] Arguments;
            public object[] DefaultArguments;

            public BindContextAttribute BindContext;
            public int ContextOffset;
        }

        #endregion // Types

        public MethodInfo Method;
        public MethodSignature Signature;

        private SpecializationInfo m_Specialized;
        private UnspecializedInfo m_Unspecialized;
        private int m_RequiredParamCount;
        private int m_MaxParamCount;
        private bool m_SplitStringArgs;

        public bool TryLoad(MethodInfo inMethod, IStringConverter inStringConverter)
        {
            Method = inMethod;

            MethodSignature signature = new MethodSignature(inMethod);
            Signature = signature;

            int paramCount = signature.Parameters.Length;
            ParameterInfo firstParam = paramCount > 0 ? signature.Parameters[0] : null;
            ParameterInfo firstUserParam = firstParam;
            BindContextAttribute contextBind = firstParam?.GetCustomAttribute<BindContextAttribute>();
            m_Unspecialized.BindContext = contextBind;
            m_Unspecialized.ContextOffset = contextBind != null ? 1 : 0;
            m_MaxParamCount = m_RequiredParamCount = signature.Parameters.Length - m_Unspecialized.ContextOffset;

            // split if multiple arguments
            if (m_MaxParamCount > 1 || firstUserParam == null)
            {
                m_SplitStringArgs = true;
            }
            else
            {
                bool hasNoParse = Attribute.IsDefined(firstUserParam, typeof(NoParseAttribute));
                m_SplitStringArgs = !(hasNoParse && (firstUserParam.ParameterType == typeof(string) || firstUserParam.ParameterType == typeof(StringSlice))); // ignore split if 1 arg, string or stringslice, and NoParse attribute specified
            }

            int specializationIndex;
            if (contextBind != null)
            {
                firstUserParam = paramCount > 1 ? signature.Parameters[1] : null;
                specializationIndex = -1;
            }
            else if (inMethod.IsStatic)
            {
                specializationIndex = Array.IndexOf(SpecializationHashTable, signature.SignatureId);
                if (specializationIndex >= 0)
                {
                    m_Unspecialized.Arguments = null;
                    m_Unspecialized.DefaultArguments = Array.Empty<object>();

                    m_Specialized.Type = (short) specializationIndex;
                    m_Specialized.Delegate = inMethod.CreateDelegate(SpecializationTable[specializationIndex]);
#if SPECIALIZE_WITH_FUNCTION_POINTERS
                    unsafe
                    {
                        m_Specialized.FunctionPtr = (void*) Marshal.GetFunctionPointerForDelegate(m_Specialized.Delegate);
                    }
#endif // SPECIALIZE_WITH_FUNCTION_POINTERS
                    if (firstParam != null && firstParam.IsOptional)
                    {
                        m_Specialized.DefaultArgument = new NonBoxedValue(firstParam.DefaultValue);
                        m_RequiredParamCount = 0;
                    }
                    else
                    {
                        m_RequiredParamCount = m_MaxParamCount;
                    }

                    return true;
                }
            }

            m_Specialized.Type = -1;
            m_Unspecialized.DefaultArguments = Array.Empty<object>();
            Array.Resize(ref m_Unspecialized.Arguments, paramCount);
            bool hasDefaults = false;

            int reqParams = 0;
            for (int i = m_Unspecialized.ContextOffset; i < paramCount; i++)
            {
                ParameterInfo p = signature.Parameters[i];

                if (!inStringConverter.CanConvertTo(p.ParameterType))
                    return false;

                if (p.IsOptional)
                {
                    if (!hasDefaults)
                    {
                        Array.Resize(ref m_Unspecialized.DefaultArguments, paramCount);
                        hasDefaults = true;
                    }

                    m_Unspecialized.DefaultArguments[i] = p.DefaultValue;
                }
                else
                {
                    reqParams++;
                }
            }

            m_RequiredParamCount = reqParams;
            return true;
        }

        public bool TryInvoke(object inTarget, StringSlice inArgumentsAsString, IStringConverter inConverter, object inContext, out NonBoxedValue outReturnValue)
        {
            if (!m_SplitStringArgs)
            {
                inArgumentsAsString = StringUtils.ArgsList.TrimQuotes(inArgumentsAsString);

                if (!ValidateArgumentCount(Math.Sign(inArgumentsAsString.Length)))
                {
                    outReturnValue = null;
                    return false;
                }

                NonBoxedValue firstArg;
                if (!inConverter.TryConvertTo(inArgumentsAsString, Signature.Parameters[m_Unspecialized.ContextOffset].ParameterType, inContext, out firstArg))
                {
                    UnityEngine.Debug.LogErrorFormat("[MethodInvocationHelper] Unable to convert string '{0}' to expected type {1}", inArgumentsAsString.ToString(), Signature.Parameters[m_Unspecialized.ContextOffset].ParameterType.Name);
                    outReturnValue = null;
                    return false;
                }

                return TrySpecializedInvoke(inTarget, firstArg, 1, inContext, out outReturnValue);
            }

            TempList8<StringSlice> split = default(TempList8<StringSlice>);
            int count = inArgumentsAsString.Split(StringUtils.ArgsList.Splitter.UnescapedInstance, StringSplitOptions.None, ref split);

            if (!ValidateArgumentCount(count))
            {
                outReturnValue = null;
                return false;
            }

            if (count == 0)
            {
                return TrySpecializedInvoke(inTarget, null, 0, inContext, out outReturnValue);
            }
            else if (count == 1)
            {
                NonBoxedValue firstArg;
                if (!inConverter.TryConvertTo(split[0], Signature.Parameters[m_Unspecialized.ContextOffset].ParameterType, inContext, out firstArg))
                {
                    UnityEngine.Debug.LogErrorFormat("[MethodInvocationHelper] Unable to convert string '{0}' to expected type {1}", split[1].ToString(), Signature.Parameters[m_Unspecialized.ContextOffset].ParameterType.Name);
                    outReturnValue = null;
                    return false;
                }

                return TrySpecializedInvoke(inTarget, firstArg, 1, inContext, out outReturnValue);
            }
            else
            {
                int idx = m_Unspecialized.ContextOffset;
                for (int i = 0; i < count; i++)
                {
                    NonBoxedValue arg;
                    if (!inConverter.TryConvertTo(split[i], Signature.Parameters[idx].ParameterType, inContext, out arg))
                    {
                        UnityEngine.Debug.LogErrorFormat("[MethodInvocationHelper] Unable to convert string '{0}' to expected type {1}", split[i].ToString(), Signature.Parameters[idx].ParameterType.Name);
                        ResetArgCache();
                        outReturnValue = null;
                        return false;
                    }
                    m_Unspecialized.Arguments[idx] = arg.AsObject();
                    idx++;
                }
                return DoUnspecializedInvoke(inTarget, idx, inContext, out outReturnValue);
            }
        }

        private bool TrySpecializedInvoke(object inTarget, NonBoxedValue inArgument, int inPassedArgumentCount, object inContext, out NonBoxedValue outReturnValue)
        {
            short specializedType = m_Specialized.Type;
            if (specializedType >= 0)
            {
                // if only argument is optional, and passed argument is empty
                if (inPassedArgumentCount == 0 && m_RequiredParamCount == 0 && m_MaxParamCount == 1)
                {
                    inArgument = m_Specialized.DefaultArgument;
                }

#if SPECIALIZE_WITH_FUNCTION_POINTERS
                unsafe
                {
                    void* func = m_Specialized.FunctionPtr;

                    switch (specializedType)
                    {
                        case 0:
                            ((delegate*<void>) func)();
                            outReturnValue = default(NonBoxedValue);
                            return true;

                        case 1:
                            outReturnValue = ((delegate*<bool>) func)();
                            return true;

                        case 2:
                            outReturnValue = ((delegate*<int>) func)();
                            return true;

                        case 3:
                            outReturnValue = ((delegate*<float>) func)();
                            return true;

                        case 4:
                            outReturnValue = ((delegate*<StringHash32>) func)();
                            return true;

                        case 5:
                            outReturnValue = new NonBoxedValue(((delegate*<IEnumerator>) func)());
                            return true;

                        case 6:
                            ((delegate*<bool, void>) func)(inArgument.AsBool());
                            outReturnValue = default(NonBoxedValue);
                            return true;

                        case 7:
                            ((delegate*<int, void>) func)(inArgument.AsInt32());
                            outReturnValue = default(NonBoxedValue);
                            return true;

                        case 8:
                            ((delegate*<float, void>) func)(inArgument.AsFloat());
                            outReturnValue = default(NonBoxedValue);
                            return true;

                        case 9:
                            outReturnValue = new NonBoxedValue(((delegate*<float, IEnumerator>) func)(inArgument.AsFloat()));
                            return true;

                        case 10:
                            ((delegate*<StringHash32, void>) func)(inArgument.AsStringHash());
                            outReturnValue = default(NonBoxedValue);
                            return true;

                        case 11:
                            outReturnValue = ((delegate*<StringHash32, bool>) func)(inArgument.AsStringHash());
                            return true;

                        case 12:
                            outReturnValue = ((delegate*<StringHash32, int>) func)(inArgument.AsStringHash());
                            return true;

                        case 13:
                            outReturnValue = ((delegate*<StringHash32, float>) func)(inArgument.AsStringHash());
                            return true;

                        case 14:
                            outReturnValue = ((delegate*<StringHash32, StringHash32>) func)(inArgument.AsStringHash());
                            return true;

                        case 15:
                            outReturnValue = new NonBoxedValue(((delegate*<StringHash32, IEnumerator>) func)(inArgument.AsStringHash()));
                            return true;

                        case 16:
                            ((delegate*<StringSlice, void>) func)(inArgument.AsStringSlice());
                            outReturnValue = default(NonBoxedValue);
                            return true;

                        case 17:
                            outReturnValue = ((delegate*<StringSlice, bool>) func)(inArgument.AsStringSlice());
                            return true;

                        case 18:
                            ((delegate*<string, void>) func)(inArgument.AsString());
                            outReturnValue = default(NonBoxedValue);
                            return true;

                        case 19:
                            outReturnValue = ((delegate*<string, bool>) func)(inArgument.AsString());
                            return true;

                        default:
                            throw new InvalidOperationException("Unknown specialization index " + specializedType);
                    }
                }
#else
                Delegate func = m_Specialized.Delegate;

                switch(specializedType)
                {
                    case 0:
                        ((Void_Delegate) func)();
                        outReturnValue = default(NonBoxedValue);
                        return true;

                    case 1:
                        outReturnValue = ((Bool_Delegate) func)();
                        return true;

                    case 2:
                        outReturnValue = ((Int_Delegate) func)();
                        return true;

                    case 3:
                        outReturnValue = ((Float_Delegate) func)();
                        return true;

                    case 4:
                        outReturnValue = ((StringHash32_Delegate) func)();
                        return true;

                    case 5:
                        outReturnValue = new NonBoxedValue(((IEnumerator_Delegate) func)());
                        return true;

                    case 6:
                        ((Void_Bool_Delegate) func)(inArgument.AsBool());
                        outReturnValue = default(NonBoxedValue);
                        return true;

                    case 7:
                        ((Void_Int_Delegate) func)(inArgument.AsInt32());
                        outReturnValue = default(NonBoxedValue);
                        return true;

                    case 8:
                        ((Void_Float_Delegate) func)(inArgument.AsFloat());
                        outReturnValue = default(NonBoxedValue);
                        return true;

                    case 9:
                        outReturnValue = new NonBoxedValue(((IEnumerator_Float_Delegate) func)(inArgument.AsFloat()));
                        return true;

                    case 10:
                        ((Void_StringHash32_Delegate) func)(inArgument.AsStringHash());
                        outReturnValue = default(NonBoxedValue);
                        return true;

                    case 11:
                        outReturnValue = ((Bool_StringHash32_Delegate) func)(inArgument.AsStringHash());
                        return true;

                    case 12:
                        outReturnValue = ((Int_StringHash32_Delegate) func)(inArgument.AsStringHash());
                        return true;

                    case 13:
                        outReturnValue = ((Float_StringHash32_Delegate) func)(inArgument.AsStringHash());
                        return true;

                    case 14:
                        outReturnValue = ((StringHash32_StringHash32_Delegate) func)(inArgument.AsStringHash());
                        return true;

                    case 15:
                        outReturnValue = new NonBoxedValue(((IEnumerator_StringHash32_Delegate) func)(inArgument.AsStringHash()));
                        return true;

                    case 16:
                        ((Void_StringSlice_Delegate) func)(inArgument.AsStringSlice());
                        outReturnValue = default(NonBoxedValue);
                        return true;

                    case 17:
                        outReturnValue = ((Bool_StringSlice_Delegate) func)(inArgument.AsStringSlice());
                        return true;

                    case 18:
                        ((Void_String_Delegate) func)(inArgument.AsString());
                        outReturnValue = default(NonBoxedValue);
                        return true;

                    case 19:
                        outReturnValue = ((Bool_String_Delegate) func)(inArgument.AsString());
                        return true;

                    default:
                        throw new InvalidOperationException("Unknown specialization index " + specializedType);
                }

#endif // SPECIALIZE_WITH_FUNCTION_POINTERS
            }

            if (inPassedArgumentCount > 0)
            {
                m_Unspecialized.Arguments[m_Unspecialized.ContextOffset] = inArgument.AsObject();
            }

            return DoUnspecializedInvoke(inTarget, inPassedArgumentCount + m_Unspecialized.ContextOffset, inContext, out outReturnValue);
        }

        private bool DoUnspecializedInvoke(object inTarget, int inDefaultOffset, object inContext, out NonBoxedValue outReturnValue)
        {
            for (int i = inDefaultOffset, len = m_Unspecialized.DefaultArguments.Length; i < len; i++)
                m_Unspecialized.Arguments[i] = m_Unspecialized.DefaultArguments[i];

            if (m_Unspecialized.ContextOffset > 0)
                m_Unspecialized.Arguments[0] = m_Unspecialized.BindContext.Bind(inContext);

            try
            {
                outReturnValue = new NonBoxedValue(Method.Invoke(inTarget, m_Unspecialized.Arguments));
                return true;
            }
            finally
            {
                ResetArgCache();
            }
        }

        private bool ValidateArgumentCount(int inArgumentCount)
        {
            if (inArgumentCount < m_RequiredParamCount || inArgumentCount > m_MaxParamCount)
            {
                UnityEngine.Debug.LogErrorFormat("[MethodInvocationHelper] Method '{0}' requires between {1} and {2} arguments, but {3} provided", Method.Name, m_RequiredParamCount, m_MaxParamCount, inArgumentCount);
                return false;
            }

            return true;
        }

        private void ResetArgCache()
        {
            Array.Clear(m_Unspecialized.Arguments, 0, m_Unspecialized.Arguments.Length);
        }

        #region Static

        static private readonly Type[] SpecializationTable = new Type[]
        {
            typeof(Void_Delegate), typeof(Bool_Delegate), typeof(Int_Delegate), typeof(Float_Delegate), typeof(StringHash32_Delegate), typeof(IEnumerator_Delegate), // 0-5
            typeof(Void_Bool_Delegate), // 6
            typeof(Void_Int_Delegate), // 7
            typeof(Void_Float_Delegate), typeof(IEnumerator_Float_Delegate), // 8-9
            typeof(Void_StringHash32_Delegate), typeof(Bool_StringHash32_Delegate), typeof(Int_StringHash32_Delegate), typeof(Float_StringHash32_Delegate), typeof(StringHash32_StringHash32_Delegate), typeof(IEnumerator_StringHash32_Delegate), // 10-15
            typeof(Void_StringSlice_Delegate), typeof(Bool_StringSlice_Delegate), // 16-17
            typeof(Void_String_Delegate), typeof(Bool_String_Delegate) // 18-19
        };
        static private readonly uint[] SpecializationHashTable;

        static MethodInvocationHelper()
        {
            SpecializationHashTable = new uint[SpecializationTable.Length];
            for (int i = 0; i < SpecializationTable.Length; i++)
            {
                SpecializationHashTable[i] = MethodSignature.GetDelegateId(SpecializationTable[i]);
            }
        }

        static public bool TryCreate(MethodInfo inMethod, IStringConverter inStringConverter, out MethodInvocationHelper outHelper)
        {
            outHelper = new MethodInvocationHelper();
            return outHelper.TryLoad(inMethod, inStringConverter);
        }

        #endregion // Static
    }
}