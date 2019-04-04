/*
 * Copyright (C) 2017-2019. Filament Games, LLC. All rights reserved.
 * Author:  Alex Beauchesne
 * Date:    4 April 2019
 * 
 * File:    NonAllocPhysicsRaycaster.cs
 * Purpose: Non-allocating physics raycaster
 */

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System;

namespace BeauUtil
{
    /// <summary>
    /// Physics Raycaster that uses non-allocating raycasts.
    /// </summary>
    [AddComponentMenu("BeauUtil/NonAlloc Physics Raycaster")]
    public class NonAllocPhysicsRaycaster : PhysicsRaycaster
    {
        private class RaycastHitComparison : IComparer<RaycastHit>
        {
            public int Compare(RaycastHit x, RaycastHit y)
            {
                float diff = x.distance - y.distance;
                if (diff < 0)
                    return -1;
                if (diff > 0 )
                    return 1;
                return 0;
            }
        }

        static private IComparer<RaycastHit> s_CachedSorter = new RaycastHitComparison();
        
        private RaycastHit[] m_RaycastHitArray = new RaycastHit[8];

        protected override void OnEnable()
        {
            base.OnEnable();
            if (m_RaycastHitArray == null)
                m_RaycastHitArray = new RaycastHit[m_MaxRayIntersections];
            else if (m_RaycastHitArray.Length != m_MaxRayIntersections)
                Array.Resize(ref m_RaycastHitArray, m_MaxRayIntersections);
        }

        public override void Raycast(PointerEventData eventData, List<RaycastResult> resultAppendList)
        {
            if (eventCamera == null)
                return;

            Ray ray = default(Ray);
            float maxDist = 0;
            ComputeRayAndDistance(eventData, ref ray, ref maxDist);
            int numHits = Physics.RaycastNonAlloc(ray, m_RaycastHitArray, maxDist, finalEventMask);

            if (numHits > 1)
                Array.Sort(m_RaycastHitArray, 0, numHits, s_CachedSorter);

            if (numHits != 0)
            {
                for (int i = 0; i < numHits; ++i)
                {
                    RaycastResult item = new RaycastResult
                    {
                        gameObject = m_RaycastHitArray[i].collider.gameObject,
                        module = this,
                        distance = m_RaycastHitArray[i].distance,
                        worldPosition = m_RaycastHitArray[i].point,
                        worldNormal = m_RaycastHitArray[i].normal,
                        screenPosition = eventData.position,
                        index = (float)resultAppendList.Count,
                        sortingLayer = 0,
                        sortingOrder = 0
                    };
                    resultAppendList.Add(item);

                    m_RaycastHitArray[i] = default(RaycastHit);
                }

                for (int i = numHits; i < m_MaxRayIntersections; ++i)
                    m_RaycastHitArray[i] = default(RaycastHit);
            }
        }
    }
}