/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    30 July 2020
 * 
 * File:    PropertyPropagator.cs
 * Purpose: Property propagation logic.
 */

#if CSHARP_7_3_OR_NEWER
#define EXPANDED_REFS
#endif // CSHARP_7_3_OR_NEWER

using System;
using System.Collections.Generic;

namespace BeauUtil
{
    /// <summary>
    /// Result of a propagation event.
    /// </summary>
    [Flags]
    public enum PropagationResult
    {
        None = 0,

        UpdateSelf = 0x01,
        UpdateChildren = 0x02,
    }

    /// <summary>
    /// Property propagation logic.
    /// </summary>
    public abstract class PropertyPropagator<T> where T : struct, IEquatable<T>
    {
        protected readonly IEqualityComparer<T> m_EqualityComparer;

        protected abstract T Combine(T inParentValue, T inSelfValue);
        protected abstract void Reset(ref PropagatedProperty<T> ioState);

        public void Reset(ref PropagatedProperty<T> ioState, T inFromParent)
        {
            ioState.ChildrenMod = null;
            ioState.SelfMod = null;
            Reset(ref ioState);
            
            ioState.m_LastFromParent = inFromParent;
            
            ioState.m_SelfState = Combine(ioState.Self, inFromParent);
            if (ioState.SelfMod.HasValue)
            {
                ioState.m_SelfState = Combine(ioState.m_SelfState, inFromParent);
            }

            ioState.m_ChildState = Combine(ioState.m_SelfState, ioState.Children);
            if (ioState.ChildrenMod.HasValue)
            {
                ioState.m_ChildState = Combine(ioState.m_ChildState, ioState.ChildrenMod.Value);
            }
        }

        protected PropertyPropagator()
            : this(CompareUtils.DefaultEquals<T>())
        {
        }

        protected PropertyPropagator(IEqualityComparer<T> inComparer)
        {
            m_EqualityComparer = inComparer;
        }

        #if EXPANDED_REFS
        public PropagationResult Propagate(ref PropagatedProperty<T> ioState, in PropagatedProperty<T> inParent, bool inbForce = false)
        #else
        public PropagationResult Propagate(ref PropagatedPropertyState<T> ioState, PropagatedPropertyState<T> inParent, bool inbForce = false)
        #endif // EXPANDED_REFS
        {
            ioState.m_LastFromParent = inParent.m_ChildState;
            return UpdateSelf(ref ioState, inbForce);
        }

        public PropagationResult Propagate(ref PropagatedProperty<T> ioState, T inFromParent, bool inbForce = false)
        {
            ioState.m_LastFromParent = inFromParent;
            return UpdateSelf(ref ioState, inbForce);
        }

        public PropagationResult Update(ref PropagatedProperty<T> ioState, bool inbForce = false)
        {
            PropagationResult result = UpdateSelf(ref ioState, inbForce);
            if ((result & PropagationResult.UpdateSelf) == 0)
                result |= UpdateChildren(ref ioState, inbForce);
            return result;
        }

        public PropagationResult UpdateSelf(ref PropagatedProperty<T> ioState, bool inbForce = false)
        {
            T nextSelfValue = Combine(ioState.Self, ioState.m_LastFromParent);
            if (ioState.SelfMod.HasValue)
            {
                nextSelfValue = Combine(nextSelfValue, ioState.SelfMod.Value);
            }

            if (inbForce || !m_EqualityComparer.Equals(ioState.m_SelfState, nextSelfValue))
            {
                ioState.m_SelfState = nextSelfValue;
                return PropagationResult.UpdateSelf | UpdateChildren(ref ioState, inbForce);
            }

            return PropagationResult.None;
        }

        public PropagationResult UpdateChildren(ref PropagatedProperty<T> ioState, bool inbForce = false)
        {
            T nextChildrenValue = Combine(ioState.m_SelfState, ioState.Children);
            if (ioState.ChildrenMod.HasValue)
            {
                nextChildrenValue = Combine(nextChildrenValue, ioState.ChildrenMod.Value);
            }

            if (inbForce || !m_EqualityComparer.Equals(ioState.m_ChildState, nextChildrenValue))
            {
                ioState.m_ChildState = nextChildrenValue;
                return PropagationResult.UpdateChildren;
            }

            return PropagationResult.None;
        }
    }
}