/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    8 August 2020
 * 
 * File:    CustomTagParserConfig.cs
 * Purpose: Config for the TagStringParser,
 */

using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace BeauUtil.Tags
{
    /// <summary>
    /// Customizable tag parser configuration object.
    /// </summary>
    public class CustomTagParserConfig : IReplaceProcessor, IEventProcessor
    {
        private readonly MatchRuleSet<ReplaceRule> m_ReplaceRules = new MatchRuleSet<ReplaceRule>(16);
        private readonly IReplaceProcessor m_ReplaceInheritFrom;

        private readonly MatchRuleSet<EventRule> m_EventRules = new MatchRuleSet<EventRule>(16);
        private readonly IEventProcessor m_EventInheritFrom;

        private bool m_Locked = false;

        public CustomTagParserConfig()
        {
        }

        public CustomTagParserConfig(CustomTagParserConfig inInherit)
        {
            m_ReplaceInheritFrom = inInherit;
            m_EventInheritFrom = inInherit;
        }

        public CustomTagParserConfig(IReplaceProcessor inInheritReplace, IEventProcessor inInheritEvent)
        {
            m_ReplaceInheritFrom = inInheritReplace;
            m_EventInheritFrom = inInheritEvent;
        }

        #region Delegate Types

        public delegate string ReplaceDelegate();
        public delegate string ReplaceWithContextDelegate(TagData inTag, object inContext);
        public delegate bool TryReplaceDelegate(TagData inTag, object inContext, out string outReplace);

        public delegate void EventDelegate(TagData inTag, object inContext, ref TagEventData ioData);

        #endregion // Delegate Types

        #region Text Replace

        /// <summary>
        /// Rule for replacing tags with text.
        /// </summary>
        public class ReplaceRule : MatchRule<ReplaceRule>
        {
            private bool m_HandleClosing;
            private string m_ConstantText;
            private string m_ConstantClosingText;

            private ReplaceDelegate m_ReplaceCallback;
            private ReplaceDelegate m_ReplaceClosingCallback;

            private ReplaceWithContextDelegate m_ReplaceWithContextCallback;
            private ReplaceWithContextDelegate m_ReplaceWithContextClosingCallback;

            private TryReplaceDelegate m_TryReplaceCallback;

            #region Builder

            internal ReplaceRule(string inId)
            {
                SetId(inId);
            }

            /// <summary>
            /// Tags will be replaced using the given callback.
            /// </summary>
            public ReplaceRule TryReplaceWith(TryReplaceDelegate inTryReplace)
            {
                m_ConstantText = null;
                m_ConstantClosingText = null;
                m_ReplaceCallback = null;
                m_ReplaceClosingCallback = null;
                m_ReplaceWithContextCallback = null;
                m_ReplaceWithContextClosingCallback = null;
                m_TryReplaceCallback = inTryReplace;
                m_HandleClosing = false;
                return this;
            }

            /// <summary>
            /// Tags will be replaced with the given string.
            /// </summary>
            public ReplaceRule ReplaceWith(string inText)
            {
                m_ConstantText = inText;
                m_ReplaceCallback = null;
                m_ReplaceWithContextCallback = null;
                m_TryReplaceCallback = null;
                return this;
            }

            /// <summary>
            /// Tags will be replaced with the result of the given callback.
            /// </summary>
            public ReplaceRule ReplaceWith(ReplaceDelegate inDelegate)
            {
                m_ConstantText = null;
                m_ReplaceCallback = inDelegate;
                m_ReplaceWithContextCallback = null;
                m_TryReplaceCallback = null;
                return this;
            }

            /// <summary>
            /// Tags will be replaced with the result of the given string.
            /// </summary>
            public ReplaceRule ReplaceWith(ReplaceWithContextDelegate inContextDelegate)
            {
                m_ConstantText = null;
                m_ReplaceCallback = null;
                m_ReplaceWithContextCallback = inContextDelegate;
                m_TryReplaceCallback = null;
                return this;
            }

            /// <summary>
            /// Closing tags will be replaced with the given string.
            /// </summary>
            public ReplaceRule CloseWith(string inText)
            {
                m_ConstantClosingText = inText;
                m_ReplaceClosingCallback = null;
                m_ReplaceWithContextClosingCallback = null;
                m_TryReplaceCallback = null;
                m_HandleClosing = true;
                return this;
            }

            /// <summary>
            /// Closing tags will be replaced with the result of the given callback.
            /// </summary>
            public ReplaceRule CloseWith(ReplaceDelegate inDelegate)
            {
                m_ConstantClosingText = null;
                m_ReplaceClosingCallback = inDelegate;
                m_ReplaceWithContextClosingCallback = null;
                m_TryReplaceCallback = null;
                m_HandleClosing = true;
                return this;
            }

            /// <summary>
            /// Closing tags will be replaced with the result of the given string.
            /// </summary>
            public ReplaceRule CloseWith(ReplaceWithContextDelegate inContextDelegate)
            {
                m_ConstantClosingText = null;
                m_ReplaceClosingCallback = null;
                m_ReplaceWithContextClosingCallback = inContextDelegate;
                m_TryReplaceCallback = null;
                m_HandleClosing = true;
                return this;
            }

            #endregion // Builder

            internal bool Evaluate(TagData inTag, object inContext, out string outReplace)
            {
                if (m_TryReplaceCallback != null)
                    return m_TryReplaceCallback(inTag, inContext, out outReplace);

                if (m_HandleClosing && inTag.IsClosing())
                {
                    if (!string.IsNullOrEmpty(m_ConstantClosingText))
                    {
                        outReplace = m_ConstantClosingText;
                        return true;
                    }

                    if (m_ReplaceClosingCallback != null)
                    {
                        outReplace = m_ReplaceClosingCallback();
                        return true;
                    }

                    if (m_ReplaceWithContextClosingCallback != null)
                    {
                        outReplace = m_ReplaceWithContextClosingCallback(inTag, inContext);
                        return true;
                    }
                }

                if (m_ReplaceCallback != null)
                {
                    outReplace = m_ReplaceCallback();
                    return true;
                }

                if (m_ReplaceWithContextCallback != null)
                {
                    outReplace = m_ReplaceWithContextCallback(inTag, inContext);
                    return true;
                }

                outReplace = m_ConstantText ?? string.Empty;
                return true;
            }
        }

        /// <summary>
        /// Adds a new replace rule.
        /// </summary>
        public ReplaceRule AddReplace(string inId)
        {
            CheckLocked();

            ReplaceRule rule = new ReplaceRule(inId);
            m_ReplaceRules.Add(rule);
            return rule;
        }

        /// <summary>
        /// Adds a new replace rule that replaces with the given text.
        /// </summary>
        public ReplaceRule AddReplace(string inId, string inReplaceWith)
        {
            CheckLocked();

            ReplaceRule rule = new ReplaceRule(inId);
            m_ReplaceRules.Add(rule);
            rule.ReplaceWith(inReplaceWith);
            return rule;
        }

        /// <summary>
        /// Adds a new replace rule that replaces with the result of the given callback.
        /// </summary>
        public ReplaceRule AddReplace(string inId, ReplaceDelegate inReplace)
        {
            CheckLocked();

            ReplaceRule rule = new ReplaceRule(inId);
            m_ReplaceRules.Add(rule);
            rule.ReplaceWith(inReplace);
            return rule;
        }

        /// <summary>
        /// Adds a new replace rule that replaces with the result of the given callback.
        /// </summary>
        public ReplaceRule AddReplace(string inId, ReplaceWithContextDelegate inReplace)
        {
            CheckLocked();

            ReplaceRule rule = new ReplaceRule(inId);
            m_ReplaceRules.Add(rule);
            rule.ReplaceWith(inReplace);
            return rule;
        }

        /// <summary>
        /// Adds a new replace rule that replaces with the result of the given callback.
        /// </summary>
        public ReplaceRule AddReplace(string inId, TryReplaceDelegate inReplace)
        {
            CheckLocked();

            ReplaceRule rule = new ReplaceRule(inId);
            m_ReplaceRules.Add(rule);
            rule.TryReplaceWith(inReplace);
            return rule;
        }

        public bool TryReplace(TagData inData, object inContext, out string outReplace)
        {
            ReplaceRule rule = m_ReplaceRules.FindMatch(inData.Id);
            if (rule != null)
                return rule.Evaluate(inData, inContext, out outReplace);

            if (m_ReplaceInheritFrom != null)
            {
                return m_ReplaceInheritFrom.TryReplace(inData, inContext, out outReplace);
            }

            outReplace = null;
            return false;
        }

        #endregion // Text Replace

        #region Event

        /// <summary>
        /// Rule for parsing tags into events.
        /// </summary>
        public class EventRule : MatchRule<EventRule>
        {
            private enum DataMode
            {
                None,
                String,
                Float,
                Bool
            }

            private bool m_HandleClosing;

            private StringHash m_EventId;
            private StringHash m_EventClosingId;

            private EventDelegate m_EventDelegate;
            private EventDelegate m_EventClosingDelegate;

            private DataMode m_DataMode;
            private float m_DefaultFloat;
            private bool m_DefaultBool;
            private string m_DefaultString;

            #region Builder

            internal EventRule(string inId)
            {
                SetId(inId);
                m_EventId = new StringHash(inId);
            }

            /// <summary>
            /// Processes tags with the given event id.
            /// </summary>
            public EventRule ProcessWith(StringHash inId)
            {
                m_EventId = inId;
                return this;
            }

            /// <summary>
            /// Process tags with the given delegate.
            /// </summary>
            public EventRule ProcessWith(EventDelegate inDelegate)
            {
                m_EventDelegate = inDelegate;
                return this;
            }

            /// <summary>
            /// Process tags with the given event id and delegate.
            /// </summary>
            public EventRule ProcessWith(StringHash inId, EventDelegate inDelegate)
            {
                m_EventId = inId;
                m_EventDelegate = inDelegate;
                return this;
            }

            /// <summary>
            /// Processes closing tags with the given id.
            /// </summary>
            public EventRule CloseWith(StringHash inId)
            {
                m_EventClosingId = inId;
                m_HandleClosing = true;
                return this;
            }

            /// <summary>
            /// Processes closing tags with the given delegate.
            /// </summary>
            public EventRule CloseWith(EventDelegate inDelegate)
            {
                m_EventClosingDelegate = inDelegate;
                m_HandleClosing = true;
                return this;
            }

            /// <summary>
            /// Processes closing tags with the given id and delegate.
            /// </summary>
            public EventRule CloseWith(StringHash inId, EventDelegate inDelegate)
            {
                m_EventClosingId = inId;
                m_EventClosingDelegate = inDelegate;
                m_HandleClosing = true;
                return this;
            }

            /// <summary>
            /// Preserves additional tag data before processing.
            /// </summary>
            public EventRule WithStringData(string inDefault = null)
            {
                m_DataMode = DataMode.String;
                m_DefaultString = inDefault;
                return this;
            }

            /// <summary>
            /// Preserves additional tag data before processing.
            /// </summary>
            public EventRule WithFloatData(float inDefault = 0)
            {
                m_DataMode = DataMode.Float;
                m_DefaultFloat = inDefault;
                return this;
            }

            /// <summary>
            /// Preserves additional tag data before processing.
            /// </summary>
            public EventRule WithBoolData(bool inbDefault = false)
            {
                m_DataMode = DataMode.Bool;
                m_DefaultBool = inbDefault;
                return this;
            }

            #endregion // Builder

            internal bool Evaluate(TagData inData, object inContext, out TagEventData outEvent)
            {
                outEvent = new TagEventData(m_EventId);
                outEvent.IsClosing = inData.IsClosing();

                switch(m_DataMode)
                {
                    case DataMode.String:
                        {
                            if (inData.Data.IsEmpty)
                                outEvent.StringArgument = m_DefaultString;
                            else
                                outEvent.StringArgument = inData.Data.ToString();
                            break;
                        }

                    case DataMode.Float:
                        {
                            float arg;
                            if (!StringParser.TryParseFloat(inData.Data, out arg))
                                arg = m_DefaultFloat;
                            outEvent.Argument0 = arg;
                            break;
                        }

                    case DataMode.Bool:
                        {
                            bool arg;
                            if (!StringParser.TryParseBool(inData.Data, out arg))
                                arg = m_DefaultBool;
                            outEvent.Argument0 = arg;
                            break;
                        }
                }

                if (m_HandleClosing && outEvent.IsClosing)
                {
                    outEvent.Type = m_EventClosingId;
                    if (m_EventClosingDelegate != null)
                    {
                        m_EventClosingDelegate(inData, inContext, ref outEvent);
                    }

                    return true;
                }
                else
                {
                    if (m_EventDelegate != null)
                    {
                        m_EventDelegate(inData, inContext, ref outEvent);
                    }

                    return true;
                }
            }
        }

        /// <summary>
        /// Adds a new event parsing rule.
        /// </summary>
        public EventRule AddEvent(string inId)
        {
            CheckLocked();

            EventRule rule = new EventRule(inId);
            m_EventRules.Add(rule);
            return rule;
        }

        /// <summary>
        /// Adds a new event parsing rule.
        /// </summary>
        public EventRule AddEvent(string inId, StringHash inEventId)
        {
            CheckLocked();

            EventRule rule = new EventRule(inId);
            m_EventRules.Add(rule);

            rule.ProcessWith(inEventId);
            return rule;
        }

        /// <summary>
        /// Adds a new event parsing rule processed with the given delegate.
        /// </summary>
        public EventRule AddEvent(string inId, EventDelegate inDelegate)
        {
            CheckLocked();

            EventRule rule = new EventRule(inId);
            m_EventRules.Add(rule);
            rule.ProcessWith(inDelegate);
            return rule;
        }

        /// <summary>
        /// Adds a new event parsing rule processed with the given delegate.
        /// </summary>
        public EventRule AddEvent(string inId, StringHash inEventId, EventDelegate inDelegate)
        {
            CheckLocked();

            EventRule rule = new EventRule(inId);
            m_EventRules.Add(rule);
            rule.ProcessWith(inEventId, inDelegate);
            return rule;
        }

        public bool TryProcess(TagData inData, object inContext, out TagEventData outEvent)
        {
            EventRule rule = m_EventRules.FindMatch(inData.Id);
            if (rule != null)
                return rule.Evaluate(inData, inContext, out outEvent);

            if (m_EventInheritFrom != null)
            {
                return m_EventInheritFrom.TryProcess(inData, inContext, out outEvent);
            }

            outEvent = default(TagEventData);
            return false;
        }

        #endregion // Event

        #region Locking

        protected void CheckLocked()
        {
            if (m_Locked)
                throw new InvalidOperationException("Parser config has marked read-only");
        }

        /// <summary>
        /// Marks this parser config as read-only.
        /// </summary>
        public void Lock()
        {
            m_Locked = true;
        }

        #endregion // Locking
    }
}