/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    30 July 2020
 * 
 * File:    PropagatedProperty.cs
 * Purpose: Property propagation struct.
 */

#if CSHARP_7_3_OR_NEWER
#define EXPANDED_REFS
#endif // CSHARP_7_3_OR_NEWER

using System;
using System.Collections.Generic;

namespace BeauUtil
{
    public struct PropagatedProperty<T> where T : struct, IEquatable<T>
    {
        public T Self;
        public T? SelfMod;

        public T Children;
        public T? ChildrenMod;

        public T PropagatedSelfState
        {
            get { return m_SelfState; }
        }

        public T PropagatedChildState
        {
            get { return m_ChildState; }
        }

        internal T m_LastFromParent;
        internal T m_SelfState;
        internal T m_ChildState;
    }
}