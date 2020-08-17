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

namespace BeauUtil
{
    /// <summary>
    /// Customizable tag parser configuration object.
    /// </summary>
    public class CustomTagParserConfig : TagStringParser.IReplaceProcessor, TagStringParser.IEventProcessor
    {
        private readonly List<ReplaceRule> m_ReplaceRules = new List<ReplaceRule>(16);
        private readonly TagStringParser.IReplaceProcessor m_ReplaceInheritFrom;
        private bool m_ReplaceDirty = false;

        private readonly TagStringParser.IEventProcessor m_EventInheritFrom;
        private readonly List<EventRule> m_EventRules = new List<EventRule>(16);
        private bool m_EventDirty = false;

        private bool m_Locked = false;

        public CustomTagParserConfig()
        {
        }

        public CustomTagParserConfig(CustomTagParserConfig inInherit)
        {
            m_ReplaceInheritFrom = inInherit;
            m_EventInheritFrom = inInherit;
        }

        public CustomTagParserConfig(TagStringParser.IReplaceProcessor inInheritReplace, TagStringParser.IEventProcessor inInheritEvent)
        {
            m_ReplaceInheritFrom = inInheritReplace;
            m_EventInheritFrom = inInheritEvent;
        }

        #region Delegate Types

        public delegate string ReplaceDelegate();
        public delegate string ReplaceWithContextDelegate(TagStringParser.TagData inTag, object inContext);
        public delegate bool TryReplaceDelegate(TagStringParser.TagData inTag, object inContext, out string outReplace);

        public delegate void EventDelegate(TagStringParser.TagData inTag, object inContext, ref TagString.EventData ioData);

        #endregion // Delegate Types

        #region Rule

        public abstract class CustomRule<T> : IComparable<CustomRule<T>> where T : CustomRule<T>
        {
            protected string m_IdMatch;
            protected string[] m_Aliases;
            protected bool m_CaseSensitive;
            protected int m_Specificity;
            protected bool m_HandleClosing;

            protected void SetId(string inId)
            {
                if (m_IdMatch != inId)
                {
                    m_IdMatch = inId;
                    RecalculateSpecificity();
                }
            }

            protected void SetCaseSentitive(bool inbCaseSensitive)
            {
                if (m_CaseSensitive != inbCaseSensitive)
                {
                    m_CaseSensitive = inbCaseSensitive;
                    RecalculateSpecificity();
                }
            }

            protected void AddAliases(params string[] inAliases)
            {
                if (inAliases == null || inAliases.Length == 0)
                    return;

                if (m_Aliases == null)
                {
                    m_Aliases = (string[]) inAliases.Clone();
                }
                else
                {
                    int copyIdx = m_Aliases.Length;
                    Array.Resize(ref m_Aliases, m_Aliases.Length + inAliases.Length);
                    Array.Copy(inAliases, 0, m_Aliases, copyIdx, inAliases.Length);
                }

                RecalculateSpecificity();
            }

            protected void RecalculateSpecificity()
            {
                int specificity = CalculateSpecificity(m_IdMatch, m_CaseSensitive);
                if (m_Aliases != null)
                {
                    foreach (string alias in m_Aliases)
                    {
                        int aliasSpecificity = CalculateSpecificity(alias, m_CaseSensitive);
                        if (aliasSpecificity < specificity)
                            specificity = aliasSpecificity;
                    }
                }

                m_Specificity = specificity;
            }

            static protected int CalculateSpecificity(string inMatchRule, bool inbCaseSensitive)
            {
                if (string.IsNullOrEmpty(inMatchRule))
                    return 0;

                int specificity = ushort.MaxValue - inMatchRule.Length;
                if (inMatchRule.StartsWith("*") || inMatchRule.EndsWith("*"))
                    specificity = Math.Max(inMatchRule.Length - 2, 0);

                if (inbCaseSensitive)
                    specificity *= 2;

                return specificity;
            }

            internal bool CanHandle(StringSlice inTagId)
            {
                if (StringUtils.WildcardMatch(inTagId, m_IdMatch, '*', !m_CaseSensitive))
                    return true;

                if (m_Aliases != null)
                    return StringUtils.WildcardMatch(inTagId, m_Aliases, '*', !m_CaseSensitive);

                return false;
            }

            int IComparable<CustomRule<T>>.CompareTo(CustomRule<T> other)
            {
                int diff = m_Specificity - other.m_Specificity;
                if (diff > 0)
                    return -1;
                if (diff < 0)
                    return 1;
                return 0;
            }

            #region Builder

            /// <summary>
            /// Adds a set of aliases for matching.
            /// Wildcard matching is supported.
            /// </summary>
            public T WithAliases(params string[] inAliases)
            {
                AddAliases(inAliases);
                return (T)this;
            }

            /// <summary>
            /// Matching will be case-sensitive.
            /// By default, matching is case-insensitive.
            /// </summary>
            public T CaseSensitive()
            {
                SetCaseSentitive(true);
                return (T)this;
            }

            /// <summary>
            /// Matching will be case-insensitive.
            /// By default, matching is case-insensitive.
            /// </summary>
            public T CaseInsensitive()
            {
                SetCaseSentitive(false);
                return (T)this;
            }

            #endregion // Builder
        }

        #endregion // Rule

        #region Text Replace

        /// <summary>
        /// Rule for replacing tags with text.
        /// </summary>
        public class ReplaceRule : CustomRule<ReplaceRule>
        {
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

            internal bool Evaluate(TagStringParser.TagData inTag, object inContext, out string outReplace)
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
            m_ReplaceDirty = true;
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
            m_ReplaceDirty = true;
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
            m_ReplaceDirty = true;
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
            m_ReplaceDirty = true;
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
            m_ReplaceDirty = true;
            rule.TryReplaceWith(inReplace);
            return rule;
        }

        protected void CacheReplace()
        {
            if (!m_ReplaceDirty)
                return;

            m_ReplaceRules.Sort();
            m_ReplaceDirty = false;
        }

        public bool TryReplace(TagStringParser.TagData inData, object inContext, out string outReplace)
        {
            CacheReplace();

            for (int i = 0, count = m_ReplaceRules.Count; i < count; ++i)
            {
                ReplaceRule rule = m_ReplaceRules[i];
                if (rule.CanHandle(inData.Id))
                {
                    return rule.Evaluate(inData, inContext, out outReplace);
                }
            }

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
        public class EventRule : CustomRule<EventRule>
        {
            private enum ArgumentMode
            {
                None,
                String,
                Float,
                Bool
            }

            private PropertyName m_EventId;
            private PropertyName m_EventClosingId;

            private EventDelegate m_EventDelegate;
            private EventDelegate m_EventClosingDelegate;

            private ArgumentMode m_DataMode;
            private float m_DefaultFloat;
            private bool m_DefaultBool;
            private string m_DefaultString;

            #region Builder

            internal EventRule(string inId)
            {
                SetId(inId);
                m_EventId = new PropertyName(inId);
            }

            /// <summary>
            /// Processes tags with the given event id.
            /// </summary>
            public EventRule ProcessWith(PropertyName inId)
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
            public EventRule ProcessWith(PropertyName inId, EventDelegate inDelegate)
            {
                m_EventId = inId;
                m_EventDelegate = inDelegate;
                return this;
            }

            /// <summary>
            /// Processes closing tags with the given id.
            /// </summary>
            public EventRule CloseWith(PropertyName inId)
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
            public EventRule CloseWith(PropertyName inId, EventDelegate inDelegate)
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
                m_DataMode = ArgumentMode.String;
                m_DefaultString = inDefault;
                return this;
            }

            /// <summary>
            /// Preserves additional tag data before processing.
            /// </summary>
            public EventRule WithFloatData(float inDefault = 0)
            {
                m_DataMode = ArgumentMode.Float;
                m_DefaultFloat = inDefault;
                return this;
            }

            /// <summary>
            /// Preserves additional tag data before processing.
            /// </summary>
            public EventRule WithBoolData(bool inbDefault = false)
            {
                m_DataMode = ArgumentMode.Bool;
                m_DefaultBool = inbDefault;
                return this;
            }

            #endregion // Builder

            internal bool Evaluate(TagStringParser.TagData inData, object inContext, out TagString.EventData outEvent)
            {
                outEvent = new TagString.EventData(m_EventId);
                outEvent.IsClosing = inData.IsClosing();

                switch(m_DataMode)
                {
                    case ArgumentMode.String:
                        {
                            if (inData.Data.IsEmpty)
                                outEvent.StringArgument = m_DefaultString;
                            else
                                outEvent.StringArgument = inData.Data.ToString();
                            break;
                        }

                    case ArgumentMode.Float:
                        {
                            float arg;
                            if (inData.Data.IsEmpty || !float.TryParse(inData.Data.ToString(), out arg))
                                arg = m_DefaultFloat;
                            outEvent.NumberArgument = arg;
                            break;
                        }

                    case ArgumentMode.Bool:
                        {
                            bool arg;
                            if (inData.Data.IsEmpty || !bool.TryParse(inData.Data.ToString(), out arg))
                                arg = m_DefaultBool;
                            outEvent.BoolArgument = arg;
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
            m_EventDirty = true;
            return rule;
        }

        /// <summary>
        /// Adds a new event parsing rule.
        /// </summary>
        public EventRule AddEvent(string inId, PropertyName inEventId)
        {
            CheckLocked();

            EventRule rule = new EventRule(inId);
            m_EventRules.Add(rule);
            m_EventDirty = true;

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
            m_EventDirty = true;
            rule.ProcessWith(inDelegate);
            return rule;
        }

        /// <summary>
        /// Adds a new event parsing rule processed with the given delegate.
        /// </summary>
        public EventRule AddEvent(string inId, PropertyName inEventId, EventDelegate inDelegate)
        {
            CheckLocked();

            EventRule rule = new EventRule(inId);
            m_EventRules.Add(rule);
            m_EventDirty = true;
            rule.ProcessWith(inEventId, inDelegate);
            return rule;
        }

        protected void CacheEvents()
        {
            if (!m_EventDirty)
                return;

            m_EventRules.Sort();
            m_EventDirty = false;
        }

        public bool TryProcess(TagStringParser.TagData inData, object inContext, out TagString.EventData outEvent)
        {
            CacheEvents();

            for (int i = 0, count = m_EventRules.Count; i < count; ++i)
            {
                EventRule rule = m_EventRules[i];
                if (rule.CanHandle(inData.Id))
                {
                    return rule.Evaluate(inData, inContext, out outEvent);
                }
            }

            if (m_EventInheritFrom != null)
            {
                return m_EventInheritFrom.TryProcess(inData, inContext, out outEvent);
            }

            outEvent = default(TagString.EventData);
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