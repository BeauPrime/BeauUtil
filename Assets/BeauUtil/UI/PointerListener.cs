/*
 * Copyright (C) 2017-2021. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    15 March 2021
 * 
 * File:    PointerListener.cs
 * Purpose: Pointer event proxy.
*/

using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BeauUtil.UI
{
    /// <summary>
    /// Pointer proxy handler.
    /// </summary>
    [AddComponentMenu("BeauUtil/UI/Pointer Listener")]
    public class PointerListener : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
    {
        #region Types

#if BEAUUTIL_USE_LEGACY_UNITYEVENTS
        public sealed class PointerEvent : UnityEvent<PointerEventData> { }
#else
        public sealed class PointerEvent : TinyUnityEvent<PointerEventData> { }
#endif // BEAUUTIL_USE_LEGACY_UNITYEVENTS
        #endregion // Types

#if BEAUUTIL_USE_LEGACY_UNITYEVENTS
        [SerializeField] private PointerEvent m_OnPointerEnter = new PointerEvent();
        [SerializeField] private PointerEvent m_OnPointerExit = new PointerEvent();
        [SerializeField] private PointerEvent m_OnPointerDown = new PointerEvent();
        [SerializeField] private PointerEvent m_OnPointerUp = new PointerEvent();
        [SerializeField] private PointerEvent m_OnClick = new PointerEvent();
#else
        private PointerEvent m_OnPointerEnter = new PointerEvent();
        private PointerEvent m_OnPointerExit = new PointerEvent();
        private PointerEvent m_OnPointerDown = new PointerEvent();
        private PointerEvent m_OnPointerUp = new PointerEvent();
        private PointerEvent m_OnClick = new PointerEvent();
#endif // BEAUUTIL_USE_LEGACY_UNITYEVENTS

        [Header("Configuration")]
        [Tooltip("If set, OnPointerClick events will always fire on a click, even if an attached Selectable is not considered Interactable")]
        [SerializeField] private bool m_AlwaysFireClickEvents;

        [NonSerialized] private Selectable m_Selectable;
        [NonSerialized] private bool? m_SelectableWasInteractive;
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

        /// <summary>
        /// Returns if this object is considered Interactive.
        /// In the presence of a Selectable component, this corresponds to its selectable state.
        /// If not, this will return true.
        /// </summary>
        public bool IsInteractable()
        {
            this.CacheComponent(ref m_Selectable);
            return isActiveAndEnabled && (!m_Selectable || m_Selectable.IsInteractable());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static private uint CalculateMask(int inPointerId) { return 1U << ((inPointerId + 32) & 31); }

        #region Handlers

        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        {
            uint mask = CalculateMask(eventData.pointerId);
            if ((m_EnteredMask & mask) != 0)
                return;

            m_EnteredMask |= mask;

            this.CacheComponent(ref m_Selectable);

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
            this.CacheComponent(ref m_Selectable);
            m_OnPointerDown.Invoke(eventData);
        }

        void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
        {
            uint mask = CalculateMask(eventData.pointerId);
            if ((m_DownMask & mask) == 0)
                return;

            m_DownMask &= ~mask;
            m_SelectableWasInteractive = !m_Selectable || m_Selectable.IsInteractable();
            m_OnPointerUp.Invoke(eventData);
        }

        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        {
            bool execute = BypassClickFilter || m_AlwaysFireClickEvents;
            if (!execute)
            {
                if (!m_SelectableWasInteractive.HasValue)
                {
                    this.CacheComponent(ref m_Selectable);
                    execute = !m_Selectable || m_Selectable.IsInteractable();
                }
                else
                {
                    execute = m_SelectableWasInteractive.Value;
                }
            }

            if (execute)
            {
                m_OnClick.Invoke(eventData);
            }

            m_SelectableWasInteractive = null;
        }

        #endregion // Handlers

        #region Filtering

        /// <summary>
        /// If set, click events will always be dispatched,
        /// event if an associated Selectable is not currently interactable.
        /// Set this to true if you are manually calling ExecuteEvents.Execute.
        /// </summary>
        static public bool BypassClickFilter = false;

        #endregion // Filtering

        /// <summary>
        /// Attempts to retrieve the PointerListener
        /// attached to the given PointerEventData's game object.
        /// </summary>
        static public bool TryGetListener(PointerEventData inEventData, out PointerListener outListener)
        {
            GameObject go = inEventData.pointerCurrentRaycast.gameObject;
            if (go)
            {
                return go.TryGetComponent(out outListener);
            }

            outListener = null;
            return false;
        }

        /// <summary>
        /// Attempts to retrieve the userdata from the PointerListener
        /// attached to the given PointerEventData's game object.
        /// </summary>
        static public bool TryGetUserData<T>(PointerEventData inEventData, out T outValue)
        {
            GameObject go = inEventData.pointerCurrentRaycast.gameObject;
            if (go)
            {
                PointerListener listener = go.GetComponent<PointerListener>();
                if (listener != null)
                {
                    if (listener.UserData is T)
                    {
                        outValue = (T) listener.UserData;
                        return true;
                    }
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
            if (go)
            {
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
            }

            outValue = default(T);
            return false;
        }
    }
}