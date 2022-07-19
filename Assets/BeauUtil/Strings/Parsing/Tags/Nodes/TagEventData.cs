/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    23 August 2020
 * 
 * File:    TagEventData.cs
 * Purpose: Information for an event node.
 */

#if CSHARP_7_3_OR_NEWER
#define EXPANDED_REFS
#endif // CSHARP_7_3_OR_NEWER

using System;
using BeauUtil.Variants;
using UnityEngine;

namespace BeauUtil.Tags
{
    /// <summary>
    /// Arguments for an event node.
    /// </summary>
    public struct TagEventData
    {
        /// <summary>
        /// Id of event type.
        /// </summary>
        public StringHash32 Type;

        /// <summary>
        /// Indicates if this closes a previous event.
        /// </summary>
        public bool IsClosing;

        /// <summary>
        /// String argument.
        /// </summary>
        public StringSlice StringArgument;

        /// <summary>
        /// Variant argument 0.
        /// </summary>
        public Variant Argument0;

        /// <summary>
        /// Variant argument 1.
        /// </summary>
        public Variant Argument1;

        /// <summary>
        /// Additional data, if required
        /// </summary>
        public ICustomNodeData AdditionalData;

        #region Constructors

        public TagEventData(StringHash32 inType)
        {
            Type = inType;
            IsClosing = false;
            StringArgument = null;
            Argument0 = Variant.Null;
            Argument1 = Variant.Null;
            AdditionalData = null;
        }

        public TagEventData(StringHash32 inType, string inStringArg)
        {
            Type = inType;
            IsClosing = false;
            StringArgument = inStringArg;
            Argument0 = Variant.Null;
            Argument1 = Variant.Null;
            AdditionalData = null;
        }

        public TagEventData(StringHash32 inType, StringSlice inStringArg)
        {
            Type = inType;
            IsClosing = false;
            StringArgument = inStringArg;
            Argument0 = Variant.Null;
            Argument1 = Variant.Null;
            AdditionalData = null;
        }

        public TagEventData(StringHash32 inType, Variant inArgument)
        {
            Type = inType;
            IsClosing = false;
            StringArgument = null;
            Argument0 = inArgument;
            Argument1 = Variant.Null;
            AdditionalData = null;
        }

        #endregion // Constructors

        /// <summary>
        /// Returns the first float argument.
        /// </summary>
        public float GetFloat()
        {
            return Argument0.AsFloat();
        }

        /// <summary>
        /// Sets the first float argument.
        /// </summary>
        public void SetFloat(float inValue)
        {
            Argument0 = inValue;
        }

        /// <summary>
        /// Returns the first bool argument.
        /// </summary>
        public bool GetBool()
        {
            return Argument0.AsBool();
        }

        /// <summary>
        /// Sets the first bool argument.
        /// </summary>
        public void SetBool(bool inbValue)
        {
            Argument0 = inbValue;
        }

        /// <summary>
        /// Returns the first StringHash32 argument.
        /// </summary>
        public StringHash32 GetStringHash()
        {
            return Argument0.AsStringHash();
        }

        /// <summary>
        /// Sets the first StringHash32 argument.
        /// </summary>
        public void SetStringHash(StringHash32 inValue)
        {
            Argument0 = inValue;
        }

        /// <summary>
        /// Sets the argument string.
        /// </summary>
        public void SetCommaSeparatedArgs(StringSlice inString)
        {
            StringArgument = inString;
        }

        /// <summary>
        /// Extracts comma-separated string arguments from the StringArgument.
        /// </summary>
        public TempList8<StringSlice> ExtractStringArgs()
        {
            TempList8<StringSlice> args = default(TempList8<StringSlice>);
            StringArgument.Split(StringUtils.ArgsList.Splitter.Instance, StringSplitOptions.None, ref args);
            return args;
        }

        /// <summary>
        /// Resets arguments.
        /// </summary>
        public void Reset()
        {
            Type = StringHash32.Null;
            IsClosing = false;
            StringArgument = null;
            Argument0 = Variant.Null;
            Argument1 = Variant.Null;
            AdditionalData = null;
        }
    }
}