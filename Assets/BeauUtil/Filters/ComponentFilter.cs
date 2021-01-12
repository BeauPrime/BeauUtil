/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    4 Dec 2020
 * 
 * File:    ComponentFilter.cs
 * Purpose: Filter for a GameObject for a specific component.
 */

using System;
using UnityEngine;

namespace BeauUtil
{
    /// <summary>
    /// Filter a GameObject by a component.
    /// </summary>
    public struct ComponentFilter : IObjectFilter<GameObject>
    {
        public Type ComponentType;
        public ComponentLookupDirection LookupDirection;

        public void OnObject<T>()
        {
            ComponentType = typeof(T);
            LookupDirection = ComponentLookupDirection.Self;
        }

        public void InParent<T>()
        {
            ComponentType = typeof(T);
            LookupDirection = ComponentLookupDirection.Parent;
        }

        public void InChildren<T>()
        {
            ComponentType = typeof(T);
            LookupDirection = ComponentLookupDirection.Children;
        }

        public void Clear()
        {
            ComponentType = null;
            LookupDirection = ComponentLookupDirection.Self;
        }

        public bool Filter(GameObject inObject, out Component outComponent)
        {
            if (ComponentType == null)
            {
                outComponent = null;
                return true;
            }

            switch(LookupDirection)
            {
                case ComponentLookupDirection.Self:
                    outComponent = inObject.GetComponent(ComponentType);
                    return !outComponent.IsReferenceNull();

                case ComponentLookupDirection.Parent:
                    outComponent = inObject.GetComponentInParent(ComponentType);
                    return !outComponent.IsReferenceNull();

                case ComponentLookupDirection.Children:
                    outComponent = inObject.GetComponentInChildren(ComponentType);
                    return !outComponent.IsReferenceNull();

                default:
                    throw new InvalidOperationException("Unknown LookupDirection " + LookupDirection.ToString());
            }
        }

        public bool Allow(GameObject inObject)
        {
            Component c;
            return Filter(inObject, out c);
        }
    }
}