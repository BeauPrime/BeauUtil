/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    10 August 2020
 * 
 * File:    TagStringEventHandler.cs
 * Purpose: Handler for dealing with tag string events.
 */

#if CSHARP_7_3_OR_NEWER
#define EXPANDED_REFS
#endif // CSHARP_7_3_OR_NEWER

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BeauUtil
{
    /// <summary>
    /// Collection of event handlers to be used in conjunction with TagString.EventData instances.
    /// </summary>
    public class TagStringEventHandler : IDisposable
    {
        #region Types

        public delegate void InstantEventDelegate();
        public delegate void InstantEventWithContextDelegate(TagString.EventData inEvent, object inContext);

        public delegate IEnumerator CoroutineEventDelegate();
        public delegate IEnumerator CoroutineEventWithContextDelegate(TagString.EventData inEvent, object inContext);

        private struct Handler
        {
            public readonly InstantEventDelegate Instant;
            public readonly InstantEventWithContextDelegate InstantWithContext;
            
            public readonly CoroutineEventDelegate Coroutine;
            public readonly CoroutineEventWithContextDelegate CoroutineWithContext;

            public Handler(InstantEventDelegate inInstant)
            {
                Instant = inInstant;
                InstantWithContext = null;
                Coroutine = null;
                CoroutineWithContext = null;
            }

            public Handler(InstantEventWithContextDelegate inInstantWithContext)
            {
                Instant = null;
                InstantWithContext = inInstantWithContext;
                Coroutine = null;
                CoroutineWithContext = null;
            }

            public Handler(CoroutineEventDelegate inCoroutine)
            {
                Instant = null;
                InstantWithContext = null;
                Coroutine = inCoroutine;
                CoroutineWithContext = null;
            }

            public Handler(CoroutineEventWithContextDelegate inCoroutineWithContext)
            {
                Instant = null;
                InstantWithContext = null;
                Coroutine = null;
                CoroutineWithContext = inCoroutineWithContext;
            }

            #if EXPANDED_REFS
            public IEnumerator Execute(in TagString.EventData inEvent, object inContext)
            #else
            public IEnumerator Execute(TagString.EventData inEvent, object inContext)
            #endif // EXPANDED_REFS
            {
                if (Instant != null)
                {
                    Instant();
                    return null;
                }

                if (InstantWithContext != null)
                {
                    InstantWithContext(inEvent, inContext);
                    return null;
                }

                if (Coroutine != null)
                {
                    return Coroutine();
                }

                if (CoroutineWithContext != null)
                {
                    return CoroutineWithContext(inEvent, inContext);
                }

                return null;
            }
        }

        #endregion // Types

        private TagStringEventHandler m_InheritFrom;
        private Dictionary<PropertyName, Handler> m_Handlers = new Dictionary<PropertyName, Handler>(16);

        public TagStringEventHandler() { }

        public TagStringEventHandler(TagStringEventHandler inInheritFrom)
        {
            m_InheritFrom = inInheritFrom;
        }

        #region Register/Deregister

        /// <summary>
        /// Registers an instant event handler.
        /// </summary>
        public void Register(PropertyName inId, InstantEventDelegate inInstant)
        {
            m_Handlers[inId] = new Handler(inInstant);
        }

        /// <summary>
        /// Registers an instant event handler with context arguments.
        /// </summary>
        public void Register(PropertyName inId, InstantEventWithContextDelegate inInstantWithContext)
        {
            m_Handlers[inId] = new Handler(inInstantWithContext);
        }

        /// <summary>
        /// Registers a coroutine event handler.
        /// </summary>
        public void Register(PropertyName inId, CoroutineEventDelegate inCoroutine)
        {
            m_Handlers[inId] = new Handler(inCoroutine);
        }

        /// <summary>
        /// Registers a coroutine event handler with context arguments.
        /// </summary>
        public void Register(PropertyName inId, CoroutineEventWithContextDelegate inCoroutineWithContext)
        {
            m_Handlers[inId] = new Handler(inCoroutineWithContext);
        }

        /// <summary>
        /// Deregisters an event handler.
        /// </summary>
        public void Deregister(PropertyName inId)
        {
            m_Handlers.Remove(inId);
        }

        /// <summary>
        /// Clears all event handlers.
        /// </summary>
        public void Clear()
        {
            m_Handlers.Clear();
        }

        #endregion // Register/Deregister

        /// <summary>
        /// Attempts to handle an incoming event.
        /// Outputs the coroutine to execute if the handler is a coroutine.
        /// </summary>
        #if EXPANDED_REFS
        public bool TryEvaluate(in TagString.EventData inEventData, object inContext, out IEnumerator outCoroutine)
        #else
        public bool TryEvaluate(TagString.EventData inEventData, object inContext, out IEnumerator outCoroutine)
        #endif // EXPANDED_REFS
        {
            PropertyName id = inEventData.Type;
            Handler handler;
            if (m_Handlers.TryGetValue(id, out handler))
            {
                outCoroutine = handler.Execute(inEventData, inContext);
                return true;
            }

            if (m_InheritFrom != null)
            {
                return m_InheritFrom.TryEvaluate(inEventData, inContext, out outCoroutine);
            }

            Debug.LogErrorFormat("[TagStringEventHandler] Unable to handle event type '{0}'", id);
            outCoroutine = null;
            return false;
        }

        #region IDisposable

        public void Dispose()
        {
            m_InheritFrom = null;
            if (m_Handlers != null)
            {
                m_Handlers.Clear();
                m_Handlers = null;
            }
        }

        #endregion // IDisposable
    }
}