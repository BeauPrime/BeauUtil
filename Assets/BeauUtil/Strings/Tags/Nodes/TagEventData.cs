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
        public PropertyName Type;

        /// <summary>
        /// Indicates if this closes a previous event.
        /// </summary>
        public bool IsClosing;

        /// <summary>
        /// String argument.
        /// </summary>
        public string StringArgument;

        /// <summary>
        /// Number argument.
        /// </summary>
        public float NumberArgument;

        /// <summary>
        /// Bool argument.
        /// </summary>
        public bool BoolArgument
        {
            get { return NumberArgument > 0; }
            set { NumberArgument = value ? 1 : 0; }
        }

        /// <summary>
        /// Additional data, if required
        /// </summary>
        public ICustomNodeData AdditionalData;

        #region Constructors

        public TagEventData(PropertyName inType)
        {
            Type = inType;
            IsClosing = false;
            StringArgument = null;
            NumberArgument = 0;
            AdditionalData = null;
        }

        public TagEventData(PropertyName inType, string inStringArg)
        {
            Type = inType;
            IsClosing = false;
            StringArgument = inStringArg;
            NumberArgument = 0;
            AdditionalData = null;
        }

        public TagEventData(PropertyName inType, StringSlice inStringArg)
        {
            Type = inType;
            IsClosing = false;
            StringArgument = inStringArg.ToString();
            NumberArgument = 0;
            AdditionalData = null;
        }

        public TagEventData(PropertyName inType, float inNumber)
        {
            Type = inType;
            IsClosing = false;
            StringArgument = null;
            NumberArgument = inNumber;
            AdditionalData = null;
        }

        public TagEventData(PropertyName inType, ICustomNodeData inData)
        {
            Type = inType;
            IsClosing = false;
            StringArgument = null;
            NumberArgument = 0;
            AdditionalData = inData;
        }

        #endregion // Constructors

        /// <summary>
        /// Resets arguments.
        /// </summary>
        public void Reset()
        {
            Type = default(PropertyName);
            IsClosing = false;
            StringArgument = null;
            NumberArgument = 0;
            AdditionalData = null;
        }
    }
}