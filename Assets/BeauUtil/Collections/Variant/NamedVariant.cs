/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    7 Sept 2020
 * 
 * File:    NamedVariant.cs
 * Purpose: Variant container with a name.
 */

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace BeauUtil.Variants
{
    /// <summary>
    /// Data variant with a name.
    /// </summary>
    [DebuggerDisplay("{ToDebugString()}")]
    public struct NamedVariant : IKeyValuePair<StringHash, Variant>
    {
        public StringHash Id;
        public Variant Value;

        public NamedVariant(StringHash inId, Variant inValue)
        {
            Id = inId;
            Value = inValue;
        }

        #region Overrides

        public override string ToString()
        {
            return string.Format("{0}: {1}", Id, Value);
        }

        public string ToDebugString()
        {
            return string.Format("{0}: {1}", Id.ToDebugString(), Value.ToDebugString());
        }

        #endregion // Overrides

        #region IKeyValuePair

        StringHash IKeyValuePair<StringHash, Variant>.Key { get { return Id; } }

        Variant IKeyValuePair<StringHash, Variant>.Value { get { return Value; } }

        #endregion // IKeyValuePair
    }
}