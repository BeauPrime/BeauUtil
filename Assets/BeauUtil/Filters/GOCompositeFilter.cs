/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    4 Dec 2020
 * 
 * File:    GOCompositeFilter.cs
 * Purpose: Filter for a GameObject.
 */

using System;
using UnityEngine;

namespace BeauUtil
{
    /// <summary>
    /// Filter for a GameObject.
    /// </summary>
    public class GOCompositeFilter : IObjectFilter<GameObject>, IObjectFilter<Collider>, IObjectFilter<Collider2D>
    {
        public ColliderFilter Collider;
        public IObjectFilter<Collider> CustomColliderFilter;
        public IObjectFilter<Collider2D> CustomCollider2DFilter;
        
        public LayerMaskFilter LayerMask;
        public TagsFilter Tags;
        public NameFilter Name;
        public CastableFunc<GameObject, bool> CustomFunc;
        public IObjectFilter<GameObject> CustomGOFilter;

        public ComponentFilter Component;
        public CastableFunc<Component, bool> ComponentFunc;

        #region Builder

        public GOCompositeFilter AsTriggerCollider()
        {
            Collider.IsTrigger = true;
            return this;
        }

        public GOCompositeFilter AsSolidCollider()
        {
            Collider.IsTrigger = false;
            return this;
        }

        public GOCompositeFilter FromRigidbody()
        {
            Collider.UseRigidbody = true;
            return this;
        }

        public GOCompositeFilter FromCollider()
        {
            Collider.UseRigidbody = false;
            return this;
        }

        public GOCompositeFilter WithLayerMask(LayerMask inMask)
        {
            LayerMask.Mask = inMask;
            return this;
        }

        public GOCompositeFilter WithTag(string inTag)
        {
            Tags.Tags.Add(inTag);
            return this;
        }

        public GOCompositeFilter WithTags(params string[] inTags)
        {
            for(int i = 0; i < inTags.Length; ++i)
                Tags.Tags.Add(inTags[i]);
            return this;
        }

        public GOCompositeFilter WithName(string inNameFilter)
        {
            Name.Pattern = inNameFilter;
            return this;
        }

        public GOCompositeFilter WithCustomGOFilter(IObjectFilter<GameObject> inCustom)
        {
            if (inCustom == null)
                throw new ArgumentNullException("inCustom");
            if (inCustom == this)
                throw new ArgumentException("Cannot use self as filter", "inCustom");

            CustomGOFilter = inCustom;
            return this;
        }

        public GOCompositeFilter WithCustomFunc(CastableFunc<GameObject, bool> inCustom)
        {
            CustomFunc = inCustom;
            return this;
        }

        public GOCompositeFilter WithComponent<T>()
        {
            Component.OnObject<T>();
            return this;
        }

        public GOCompositeFilter WithComponentInParent<T>()
        {
            Component.InParent<T>();
            return this;
        }

        public GOCompositeFilter WithComponentInChildren<T>()
        {
            Component.InChildren<T>();
            return this;
        }

        public GOCompositeFilter WithCustomComponentFunc(CastableFunc<Component, bool> inCustom)
        {
            ComponentFunc = inCustom;
            return this;
        }

        #endregion // Builder

        #region Filtering

        public virtual bool Allow(GameObject inObject)
        {
            if (!LayerMask.Allow(inObject))
                return false;

            if (!Tags.Allow(inObject))
                return false;

            if (!Name.Allow(inObject))
                return false;

            if (CustomGOFilter != null && !CustomGOFilter.Allow(inObject))
                return false;

            if (CustomFunc != null && !CustomFunc.Invoke(inObject))
                return false;

            Component component;
            if (!Component.Filter(inObject, out component))
                return false;

            if (!component.IsReferenceNull())
            {
                if (ComponentFunc != null && !ComponentFunc.Invoke(component))
                    return false;
            }

            return true;
        }

        public virtual bool Allow(Collider inObject)
        {
            if (!Collider.Allow(inObject))
                return false;

            if (CustomColliderFilter != null && !CustomColliderFilter.Allow(inObject))
                return false;

            GameObject go;
            if (Collider.UseRigidbody && inObject.attachedRigidbody)
                go = inObject.attachedRigidbody.gameObject;
            else
                go = inObject.gameObject;

            return Allow(go);
        }

        public virtual bool Allow(Collider2D inObject)
        {
            if (!Collider.Allow(inObject))
                return false;

            if (CustomCollider2DFilter != null && !CustomCollider2DFilter.Allow(inObject))
                return false;

            GameObject go;
            if (Collider.UseRigidbody && inObject.attachedRigidbody)
                go = inObject.attachedRigidbody.gameObject;
            else
                go = inObject.gameObject;

            return Allow(go);
        }

        public virtual int CalculateSpecificity()
        {
            int specificity = 0;

            if (Collider.IsTrigger.HasValue)
                specificity += 1;
            
            specificity += Bits.Count(LayerMask.Mask);
            if (Tags.Tags.Count > 0)
                specificity += Tags.Tags.Capacity - Tags.Tags.Count;

            specificity += MatchRule.CalculateSpecificity(Name.Pattern, true);

            if (CustomFunc != null)
                specificity += 10;

            if (Component.ComponentType != null)
            {
                specificity += Reflect.GetInheritanceDepth(Component.ComponentType);
                if (Component.LookupDirection == ComponentLookupDirection.Self)
                    specificity += 4;
            }

            if (ComponentFunc != null)
                specificity += 10;

            return specificity;
        }
    
        #endregion // Filtering
    }
}