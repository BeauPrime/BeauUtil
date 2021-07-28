/*
 * Copyright (C) 2017-2021. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    15 March 2021
 * 
 * File:    PointerListener.cs
 * Purpose: Pointer event proxy.
*/

#if UNITY_EDITOR || DEVELOPMENT_BUILD
#define DEVELOPMENT
#endif

using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace BeauUtil.UI
{
    /// <summary>
    /// Pointer proxy handler.
    /// </summary>
    [AddComponentMenu("BeauUtil/UI/Pointer Listener")]
    public class PointerListener : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
    {
        #region Types

        [Serializable]
        public class PointerEvent : UnityEvent<PointerEventData> { }

        #endregion // Types

        #region Inspector

        [SerializeField] private PointerEvent m_OnPointerEnter = new PointerEvent();
        [SerializeField] private PointerEvent m_OnPointerExit = new PointerEvent();
        [SerializeField] private PointerEvent m_OnPointerDown = new PointerEvent();
        [SerializeField] private PointerEvent m_OnPointerUp = new PointerEvent();
        [SerializeField] private PointerEvent m_OnClick = new PointerEvent();

        #endregion // Inspector

        [NonSerialized] private uint m_EnteredMask;
        [NonSerialized] private uint m_DownMask;

        public object UserData;

        public PointerEvent onPointerEnter { get { return m_OnPointerEnter; } }
        public PointerEvent onPointerExit { get { return m_OnPointerExit; } }
        public PointerEvent onPointerDown { get { return m_OnPointerDown; } }
        public PointerEvent onPointerUp { get { return m_OnPointerUp; } }
        public PointerEvent onClick { get { return m_OnClick; } }

        public bool IsPointerEntered() { return m_EnteredMask != 0 ; }
        public bool IsPointerEntered(int inPointerId) { return (m_EnteredMask & CalculateMask(inPointerId)) != 0;}
        public bool IsPointerDown() { return m_DownMask != 0; }
        public bool IsPointerDown(int inPointerId) { return (m_DownMask & CalculateMask(inPointerId)) != 0;}

        [MethodImpl(256)]
        static private uint CalculateMask(int inPointerId) { return 1U << ((inPointerId + 32) % 32); }

        #region Handlers

        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        {
            uint mask = CalculateMask(eventData.pointerId);
            if ((m_EnteredMask & mask) != 0)
                return;

            m_EnteredMask |= mask;
            m_OnPointerEnter.Invoke(eventData);
        }

        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
            uint mask = CalculateMask(eventData.pointerId);
            if ((m_EnteredMask & mask) == 0)
                return;

            m_EnteredMask &= ~mask;
            m_OnPointerExit.Invoke(eventData);
        }

        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            uint mask = CalculateMask(eventData.pointerId);
            if ((m_DownMask & mask) != 0)
                return;

            m_DownMask |= mask;
            m_OnPointerDown.Invoke(eventData);
        }

        void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
        {
            uint mask = CalculateMask(eventData.pointerId);
            if ((m_DownMask & mask) == 0)
                return;

            m_DownMask &= ~mask;
            m_OnPointerUp.Invoke(eventData);
        }

        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        {
            m_OnClick.Invoke(eventData);
        }

        #endregion // Handlers

        /// <summary>
        /// Attempts to retrieve the userdata from the PointerListener
        /// attached to the given PointerEventData's game object.
        /// </summary>
        static public bool TryGetUserData<T>(PointerEventData inEventData, out T outValue)
        {
            GameObject go = inEventData.pointerCurrentRaycast.gameObject;
            PointerListener listener = go.GetComponent<PointerListener>();
            if (listener != null)
            {
                if (listener.UserData is T)
                {
                    outValue = (T) listener.UserData;
                    return true;
                }
            }

            outValue = default(T);
            return false;
        }

        /// <summary>
        /// Attempts to retrieve the component userdata from the PointerListener
        /// attached to the given PointerEventData's game object.
        /// Will also search the game object for the given component.
        /// </summary>
        static public bool TryGetComponentUserData<T>(PointerEventData inEventData, out T outValue) where T : UnityEngine.Component
        {
            GameObject go = inEventData.pointerCurrentRaycast.gameObject;
            T component = go.GetComponent<T>();
            if (component != null)
            {
                outValue = component;
                return true;
            }

            PointerListener listener = go.GetComponent<PointerListener>();
            if (listener != null)
            {
                if (listener.UserData is T)
                {
                    outValue = (T) listener.UserData;
                    return true;
                }
            }

            outValue = default(T);
            return false;
        }
    }
}