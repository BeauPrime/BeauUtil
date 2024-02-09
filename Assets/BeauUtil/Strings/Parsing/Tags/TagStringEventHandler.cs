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

namespace BeauUtil.Tags
{
    /// <summary>
    /// Collection of event handlers to be used in conjunction with EventData instances.
    /// </summary>
    public class TagStringEventHandler : IDisposable
    {
        #region Types

        public delegate void InstantEventDelegate();
        public delegate void InstantEventWithContextDelegate(TagEventData inEvent, object inContext);

        public delegate IEnumerator CoroutineEventDelegate();
        public delegate IEnumerator CoroutineEventWithContextDelegate(TagEventData inEvent, object inContext);

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
            public IEnumerator Execute(in TagEventData inEvent, object inContext)
#else
            public IEnumerator Execute(EventData inEvent, object inContext)
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

        private TagStringEventHandler m_Base;
        private Dictionary<StringHash32, Handler> m_Handlers = new Dictionary<StringHash32, Handler>(16);

        public TagStringEventHandler() { }

        public TagStringEventHandler(TagStringEventHandler inBase)
        {
            m_Base = inBase;
        }

        /// <summary>
        /// The base TagStringEventHandler.
        /// Any events not explicitly defined on this handler
        /// will be deferred to the base handler.
        /// </summary>
        public TagStringEventHandler Base
        {
            get { return m_Base; }
            set
            {
                if (m_Base != value)
                {
                    if (value != null)
                    {
                        if (value == this || value.m_Base == this)
                            throw new InvalidOperationException("Provided parent would cause infinite loop");
                    }

                    m_Base = value;
                }
            }
        }

        #region Register/Deregister

        /// <summary>
        /// Registers an instant event handler.
        /// </summary>
        public TagStringEventHandler Register(StringHash32 inId, InstantEventDelegate inInstant)
        {
            m_Handlers[inId] = new Handler(inInstant);
            return this;
        }

        /// <summary>
        /// Registers an instant event handler with context arguments.
        /// </summary>
        public TagStringEventHandler Register(StringHash32 inId, InstantEventWithContextDelegate inInstantWithContext)
        {
            m_Handlers[inId] = new Handler(inInstantWithContext);
            return this;
        }

        /// <summary>
        /// Registers a coroutine event handler.
        /// </summary>
        public TagStringEventHandler Register(StringHash32 inId, CoroutineEventDelegate inCoroutine)
        {
            m_Handlers[inId] = new Handler(inCoroutine);
            return this;
        }

        /// <summary>
        /// Registers a coroutine event handler with context arguments.
        /// </summary>
        public TagStringEventHandler Register(StringHash32 inId, CoroutineEventWithContextDelegate inCoroutineWithContext)
        {
            m_Handlers[inId] = new Handler(inCoroutineWithContext);
            return this;
        }

        /// <summary>
        /// Deregisters an event handler.
        /// </summary>
        public TagStringEventHandler Deregister(StringHash32 inId)
        {
            m_Handlers.Remove(inId);
            return this;
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
        public bool TryEvaluate(in TagEventData inEventData, object inContext, out IEnumerator outCoroutine)
        #else
        public bool TryEvaluate(TagEventData inEventData, object inContext, out IEnumerator outCoroutine)
        #endif // EXPANDED_REFS
        {
            StringHash32 id = inEventData.Type;
            Handler handler;
            if (m_Handlers.TryGetValue(id, out handler))
            {
                outCoroutine = handler.Execute(inEventData, inContext);
                return true;
            }

            if (m_Base != null)
            {
                return m_Base.TryEvaluate(inEventData, inContext, out outCoroutine);
            }

            Debug.LogErrorFormat("[TagStringEventHandler] Unable to handle event type '{0}'", id.ToDebugString());
            outCoroutine = null;
            return false;
        }

        #region IDisposable

        public void Dispose()
        {
            m_Base = null;
            if (m_Handlers != null)
            {
                m_Handlers.Clear();
                m_Handlers = null;
            }
        }

        #endregion // IDisposable
    }
}