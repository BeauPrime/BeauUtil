/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    3 August 2020
 * 
 * File:    CommonPropagators.cs
 * Purpose: Common propagation types.
 */

#if CSHARP_7_3_OR_NEWER
#define EXPANDED_REFS
#endif // CSHARP_7_3_OR_NEWER

using System;
using System.Collections.Generic;

namespace BeauUtil
{
    public class BoolPropagator : PropertyPropagator<bool>
    {
        public readonly bool DefaultSelf;
        public readonly bool DefaultChildren;

        public BoolPropagator(bool inbDefaultSelf) : this(inbDefaultSelf, inbDefaultSelf) { }

        public BoolPropagator(bool inbDefaultSelf, bool inbDefaultChildren)
        {
            DefaultSelf = inbDefaultSelf;
            DefaultChildren = inbDefaultChildren;
        }

        protected override bool Combine(bool inParentValue, bool inSelfValue)
        {
            return inParentValue && inSelfValue;
        }

        protected override void Reset(ref PropagatedProperty<bool> ioState)
        {
            ioState.Self = DefaultSelf;
            ioState.Children = DefaultChildren;
        }
    }
}