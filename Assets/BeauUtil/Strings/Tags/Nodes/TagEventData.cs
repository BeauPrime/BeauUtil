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
        public StringHash Type;

        /// <summary>
        /// Indicates if this closes a previous event.
        /// </summary>
        public bool IsClosing;

        /// <summary>
        /// String argument.
        /// </summary>
        public string StringArgument;

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

        public TagEventData(StringHash inType)
        {
            Type = inType;
            IsClosing = false;
            StringArgument = null;
            Argument0 = Variant.Null;
            Argument1 = Variant.Null;
            AdditionalData = null;
        }

        public TagEventData(StringHash inType, string inStringArg)
        {
            Type = inType;
            IsClosing = false;
            StringArgument = inStringArg;
            Argument0 = Variant.Null;
            Argument1 = Variant.Null;
            AdditionalData = null;
        }

        public TagEventData(StringHash inType, StringSlice inStringArg)
        {
            Type = inType;
            IsClosing = false;
            StringArgument = inStringArg.ToString();
            Argument0 = Variant.Null;
            Argument1 = Variant.Null;
            AdditionalData = null;
        }

        public TagEventData(StringHash inType, Variant inArgument)
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
        /// Resets arguments.
        /// </summary>
        public void Reset()
        {
            Type = StringHash.Null;
            IsClosing = false;
            StringArgument = null;
            Argument0 = Variant.Null;
            Argument1 = Variant.Null;
            AdditionalData = null;
        }
    }
}