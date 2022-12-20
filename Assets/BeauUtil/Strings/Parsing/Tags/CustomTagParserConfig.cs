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
using BeauUtil.Variants;

namespace BeauUtil.Tags
{
    /// <summary>
    /// Customizable tag parser configuration object.
    /// </summary>
    public sealed class CustomTagParserConfig : IReplaceProcessor, IEventProcessor
    {
        private readonly MatchRuleSet<ReplaceRule> m_ReplaceRules = new MatchRuleSet<ReplaceRule>(16);
        private readonly Dictionary<int, string> m_CharReplace = new Dictionary<int, string>(4);
        private readonly IReplaceProcessor m_ReplaceInheritFrom;
        private readonly ReplaceRule.Builder m_ReplaceBuilder;

        private readonly MatchRuleSet<EventRule> m_EventRules = new MatchRuleSet<EventRule>(16);
        private readonly IEventProcessor m_EventInheritFrom;
        private readonly EventRule.Builder m_EventBuilder;

        private bool m_Locked = false;

        public CustomTagParserConfig()
        {
            m_ReplaceBuilder = new ReplaceRule.Builder(m_ReplaceRules);
            m_EventBuilder = new EventRule.Builder(m_EventRules);
        }

        public CustomTagParserConfig(CustomTagParserConfig inInherit)
            : this()
        {
            m_ReplaceInheritFrom = inInherit;
            m_EventInheritFrom = inInherit;
        }

        public CustomTagParserConfig(IReplaceProcessor inInheritReplace, IEventProcessor inInheritEvent)
            : this()
        {
            m_ReplaceInheritFrom = inInheritReplace;
            m_EventInheritFrom = inInheritEvent;
        }

        #region Delegate Types

        public delegate string ReplaceDelegate();
        public delegate string ReplaceWithContextDelegate(TagData inTag, object inContext);
        public delegate string ReplaceSourceContextDelegate(StringSlice inSource, object inContext);
        public delegate bool TryReplaceDelegate(TagData inTag, object inContext, out string outReplace);

        public delegate void EventDelegate(TagData inTag, object inContext, ref TagEventData ioData);

        #endregion // Delegate Types

        #region Text Replace

        public class ReplaceRule
        {
            private bool m_HandleClosing;
            private string m_ConstantText;
            private string m_ConstantClosingText;

            private ReplaceDelegate m_ReplaceCallback;
            private ReplaceDelegate m_ReplaceClosingCallback;

            private ReplaceWithContextDelegate m_ReplaceWithContextCallback;
            private ReplaceWithContextDelegate m_ReplaceWithContextClosingCallback;

            private ReplaceSourceContextDelegate m_ReplaceSourceContextCallback;
            private ReplaceSourceContextDelegate m_ReplaceSourceContextClosingCallback;

            private TryReplaceDelegate m_TryReplaceCallback;

            internal bool Evaluate(TagData inTag, StringSlice inSource, object inContext, out string outReplace)
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

                    if (m_ReplaceSourceContextClosingCallback != null)
                    {
                        outReplace = m_ReplaceSourceContextClosingCallback(inSource, inContext);
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

                if (m_ReplaceSourceContextCallback != null)
                {
                    outReplace = m_ReplaceSourceContextCallback(inSource, inContext);
                    return true;
                }

                outReplace = m_ConstantText ?? string.Empty;
                return true;
            }
        
            #region Builder

            public sealed class Builder : MatchRuleBuilder<ReplaceRule, Builder>
            {
                internal Builder(MatchRuleSet<ReplaceRule> inRuleSet)
                    : base(inRuleSet)
                { }

                /// <summary>
                /// Tags will be replaced using the given callback.
                /// </summary>
                public Builder TryReplaceWith(TryReplaceDelegate inTryReplace)
                {
                    m_Rule.m_ConstantText = null;
                    m_Rule.m_ConstantClosingText = null;
                    m_Rule.m_ReplaceCallback = null;
                    m_Rule.m_ReplaceClosingCallback = null;
                    m_Rule.m_ReplaceWithContextCallback = null;
                    m_Rule.m_ReplaceWithContextClosingCallback = null;
                    m_Rule.m_ReplaceSourceContextCallback = null;
                    m_Rule.m_ReplaceSourceContextClosingCallback = null;
                    m_Rule.m_TryReplaceCallback = inTryReplace;
                    m_Rule.m_HandleClosing = false;
                    return this;
                }

                /// <summary>
                /// Tags will be replaced with the given string.
                /// </summary>
                public Builder ReplaceWith(string inText)
                {
                    m_Rule.m_ConstantText = inText;
                    m_Rule.m_ReplaceCallback = null;
                    m_Rule.m_ReplaceWithContextCallback = null;
                    m_Rule.m_ReplaceSourceContextCallback = null;
                    m_Rule.m_TryReplaceCallback = null;
                    return this;
                }

                /// <summary>
                /// Tags will be replaced with the result of the given callback.
                /// </summary>
                public Builder ReplaceWith(ReplaceDelegate inDelegate)
                {
                    m_Rule.m_ConstantText = null;
                    m_Rule.m_ReplaceCallback = inDelegate;
                    m_Rule.m_ReplaceWithContextCallback = null;
                    m_Rule.m_ReplaceSourceContextCallback = null;
                    m_Rule.m_TryReplaceCallback = null;
                    return this;
                }

                /// <summary>
                /// Tags will be replaced with the result of the given string.
                /// </summary>
                public Builder ReplaceWith(ReplaceWithContextDelegate inContextDelegate)
                {
                    m_Rule.m_ConstantText = null;
                    m_Rule.m_ReplaceCallback = null;
                    m_Rule.m_ReplaceWithContextCallback = inContextDelegate;
                    m_Rule.m_ReplaceSourceContextCallback = null;
                    m_Rule.m_TryReplaceCallback = null;
                    return this;
                }

                /// <summary>
                /// Tags will be replaced with the result of the given string.
                /// </summary>
                public Builder ReplaceWith(ReplaceSourceContextDelegate inSourceContextDelegate)
                {
                    m_Rule.m_ConstantText = null;
                    m_Rule.m_ReplaceCallback = null;
                    m_Rule.m_ReplaceWithContextCallback = null;
                    m_Rule.m_ReplaceSourceContextCallback = inSourceContextDelegate;
                    m_Rule.m_TryReplaceCallback = null;
                    return this;
                }

                /// <summary>
                /// Closing tags will be replaced with the given string.
                /// </summary>
                public Builder CloseWith(string inText)
                {
                    m_Rule.m_ConstantClosingText = inText;
                    m_Rule.m_ReplaceClosingCallback = null;
                    m_Rule.m_ReplaceWithContextClosingCallback = null;
                    m_Rule.m_ReplaceSourceContextClosingCallback = null;
                    m_Rule.m_TryReplaceCallback = null;
                    m_Rule.m_HandleClosing = true;
                    return this;
                }

                /// <summary>
                /// Closing tags will be replaced with the result of the given callback.
                /// </summary>
                public Builder CloseWith(ReplaceDelegate inDelegate)
                {
                    m_Rule.m_ConstantClosingText = null;
                    m_Rule.m_ReplaceClosingCallback = inDelegate;
                    m_Rule.m_ReplaceWithContextClosingCallback = null;
                    m_Rule.m_ReplaceSourceContextClosingCallback = null;
                    m_Rule.m_TryReplaceCallback = null;
                    m_Rule.m_HandleClosing = true;
                    return this;
                }

                /// <summary>
                /// Closing tags will be replaced with the result of the given string.
                /// </summary>
                public Builder CloseWith(ReplaceWithContextDelegate inContextDelegate)
                {
                    m_Rule.m_ConstantClosingText = null;
                    m_Rule.m_ReplaceClosingCallback = null;
                    m_Rule.m_ReplaceWithContextClosingCallback = inContextDelegate;
                    m_Rule.m_ReplaceSourceContextClosingCallback = null;
                    m_Rule.m_TryReplaceCallback = null;
                    m_Rule.m_HandleClosing = true;
                    return this;
                }

                /// <summary>
                /// Closing tags will be replaced with the result of the given string.
                /// </summary>
                public Builder CloseWith(ReplaceSourceContextDelegate inContextSourceDelegate)
                {
                    m_Rule.m_ConstantClosingText = null;
                    m_Rule.m_ReplaceClosingCallback = null;
                    m_Rule.m_ReplaceWithContextClosingCallback = null;
                    m_Rule.m_ReplaceSourceContextClosingCallback = inContextSourceDelegate;
                    m_Rule.m_TryReplaceCallback = null;
                    m_Rule.m_HandleClosing = true;
                    return this;
                }
            }

            #endregion // Builder
        }

        /// <summary>
        /// Adds a new replace rule.
        /// </summary>
        public ReplaceRule.Builder AddReplace(string inId)
        {
            CheckLocked();

            ReplaceRule.Builder rule = RestartReplace(inId);
            return rule;
        }

        /// <summary>
        /// Adds a new replace rule that replaces with the given text.
        /// </summary>
        public ReplaceRule.Builder AddReplace(string inId, string inReplaceWith)
        {
            CheckLocked();

            ReplaceRule.Builder rule = RestartReplace(inId);
            rule.ReplaceWith(inReplaceWith);
            return rule;
        }

        /// <summary>
        /// Adds a new replace rule that replaces with the result of the given callback.
        /// </summary>
        public ReplaceRule.Builder AddReplace(string inId, ReplaceDelegate inReplace)
        {
            CheckLocked();

            ReplaceRule.Builder rule = RestartReplace(inId);
            rule.ReplaceWith(inReplace);
            return rule;
        }

        /// <summary>
        /// Adds a new replace rule that replaces with the result of the given callback.
        /// </summary>
        public ReplaceRule.Builder AddReplace(string inId, ReplaceWithContextDelegate inReplace)
        {
            CheckLocked();

            ReplaceRule.Builder rule = RestartReplace(inId);
            rule.ReplaceWith(inReplace);
            return rule;
        }

        /// <summary>
        /// Adds a new replace rule that replaces with the result of the given callback.
        /// </summary>
        public ReplaceRule.Builder AddReplace(string inId, ReplaceSourceContextDelegate inReplace)
        {
            CheckLocked();

            ReplaceRule.Builder rule = RestartReplace(inId);
            rule.ReplaceWith(inReplace);
            return rule;
        }

        /// <summary>
        /// Adds a new replace rule that replaces with the result of the given callback.
        /// </summary>
        public ReplaceRule.Builder AddReplace(string inId, TryReplaceDelegate inReplace)
        {
            CheckLocked();

            ReplaceRule.Builder rule = RestartReplace(inId);
            rule.TryReplaceWith(inReplace);
            return rule;
        }

        /// <summary>
        /// Adds a new replace rule for a single character expansion.
        /// </summary>
        public void AddReplace(char inCharacter, string inReplace)
        {
            CheckLocked();

            m_CharReplace[inCharacter] = inReplace;
        }

        public bool TryReplace(TagData inData, StringSlice inSource, object inContext, out string outReplace)
        {
            if (!m_Locked)
                m_ReplaceBuilder.Flush();

            ReplaceRule rule = m_ReplaceRules.FindMatch(inData.Id);
            if (rule != null)
                return rule.Evaluate(inData, inSource, inContext, out outReplace);

            if (m_ReplaceInheritFrom != null)
            {
                return m_ReplaceInheritFrom.TryReplace(inData, inSource, inContext, out outReplace);
            }

            outReplace = null;
            return false;
        }

        public bool TryReplace(char inCharacter, object inContext, out string outReplace)
        {
            if (m_CharReplace.TryGetValue(inCharacter, out outReplace))
                return true;
            
            if (m_ReplaceInheritFrom != null)
            {
                return m_ReplaceInheritFrom.TryReplace(inCharacter, inContext, out outReplace);
            }

            outReplace = null;
            return false;
        }

        private ReplaceRule.Builder RestartReplace(string inPattern)
        {
            m_ReplaceBuilder.Restart(inPattern);
            return m_ReplaceBuilder;
        }

        #endregion // Text Replace

        #region Event

        /// <summary>
        /// Rule for parsing tags into events.
        /// </summary>
        public sealed class EventRule
        {
            private enum DataMode
            {
                None,
                String,
                Float,
                Bool,
                StringHash
            }

            private bool m_HandleClosing;

            private StringHash32 m_EventId;
            private StringHash32 m_EventClosingId;

            private EventDelegate m_EventDelegate;
            private EventDelegate m_EventClosingDelegate;

            private DataMode m_DataMode;
            private Variant m_DefaultValue;
            private string m_DefaultString;

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
                                outEvent.StringArgument = inData.Data;
                            break;
                        }

                    case DataMode.Float:
                        {
                            float arg;
                            if (!StringParser.TryParseFloat(inData.Data, out arg))
                                arg = m_DefaultValue.AsFloat();
                            outEvent.Argument0 = arg;
                            break;
                        }

                    case DataMode.Bool:
                        {
                            bool arg;
                            if (!StringParser.TryParseBool(inData.Data, out arg))
                                arg = m_DefaultValue.AsBool();
                            outEvent.Argument0 = arg;
                            break;
                        }

                    case DataMode.StringHash:
                        {
                            StringHash32 arg;
                            if (!StringHash32.TryParse(inData.Data, out arg))
                                arg = m_DefaultValue.AsStringHash();
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
        
            #region Builder

            public sealed class Builder : MatchRuleBuilder<EventRule, Builder>
            {
                internal Builder(MatchRuleSet<EventRule> inRuleSet)
                    : base(inRuleSet)
                { }

                public override void Flush()
                {
                    if (m_IdMatch != null && m_Rule.m_EventId.IsEmpty)
                    {
                        m_Rule.m_EventId = m_IdMatch;
                    }

                    base.Flush();
                }

                /// <summary>
                /// Processes tags with the given event id.
                /// </summary>
                public EventRule.Builder ProcessWith(StringHash32 inId)
                {
                    m_Rule.m_EventId = inId;
                    return this;
                }

                /// <summary>
                /// Process tags with the given delegate.
                /// </summary>
                public EventRule.Builder ProcessWith(EventDelegate inDelegate)
                {
                    m_Rule.m_EventDelegate = inDelegate;
                    return this;
                }

                /// <summary>
                /// Process tags with the given event id and delegate.
                /// </summary>
                public EventRule.Builder ProcessWith(StringHash32 inId, EventDelegate inDelegate)
                {
                    m_Rule.m_EventId = inId;
                    m_Rule.m_EventDelegate = inDelegate;
                    return this;
                }

                /// <summary>
                /// Processes closing tags with the given id.
                /// </summary>
                public EventRule.Builder CloseWith(StringHash32 inId)
                {
                    m_Rule.m_EventClosingId = inId;
                    m_Rule.m_HandleClosing = true;
                    return this;
                }

                /// <summary>
                /// Processes closing tags with the given delegate.
                /// </summary>
                public EventRule.Builder CloseWith(EventDelegate inDelegate)
                {
                    m_Rule.m_EventClosingDelegate = inDelegate;
                    m_Rule.m_HandleClosing = true;
                    return this;
                }

                /// <summary>
                /// Processes closing tags with the given id and delegate.
                /// </summary>
                public EventRule.Builder CloseWith(StringHash32 inId, EventDelegate inDelegate)
                {
                    m_Rule.m_EventClosingId = inId;
                    m_Rule.m_EventClosingDelegate = inDelegate;
                    m_Rule.m_HandleClosing = true;
                    return this;
                }

                /// <summary>
                /// Preserves additional tag data before processing.
                /// </summary>
                public EventRule.Builder WithStringData(string inDefault = null)
                {
                    m_Rule.m_DataMode = DataMode.String;
                    m_Rule.m_DefaultString = inDefault;
                    return this;
                }

                /// <summary>
                /// Preserves additional tag data before processing.
                /// </summary>
                public EventRule.Builder WithFloatData(float inDefault = 0)
                {
                    m_Rule.m_DataMode = DataMode.Float;
                    m_Rule.m_DefaultValue = inDefault;
                    return this;
                }

                /// <summary>
                /// Preserves additional tag data before processing.
                /// </summary>
                public EventRule.Builder WithBoolData(bool inbDefault = false)
                {
                    m_Rule.m_DataMode = DataMode.Bool;
                    m_Rule.m_DefaultValue = inbDefault;
                    return this;
                }

                /// <summary>
                /// Preserves additional tag data before processing.
                /// </summary>
                public EventRule.Builder WithStringHashData(StringHash32 inDefault = default(StringHash32))
                {
                    m_Rule.m_DataMode = DataMode.StringHash;
                    m_Rule.m_DefaultValue = inDefault;
                    return this;
                }
            }

            #endregion // Builder
        }

        /// <summary>
        /// Adds a new event parsing rule.
        /// </summary>
        public EventRule.Builder AddEvent(string inId)
        {
            CheckLocked();

            EventRule.Builder rule = RestartEvent(inId);
            return rule;
        }

        /// <summary>
        /// Adds a new event parsing rule.
        /// </summary>
        public EventRule.Builder AddEvent(string inId, StringHash32 inEventId)
        {
            CheckLocked();

            EventRule.Builder rule = RestartEvent(inId);

            rule.ProcessWith(inEventId);
            return rule;
        }

        /// <summary>
        /// Adds a new event parsing rule processed with the given delegate.
        /// </summary>
        public EventRule.Builder AddEvent(string inId, EventDelegate inDelegate)
        {
            CheckLocked();

            EventRule.Builder rule = RestartEvent(inId);
            rule.ProcessWith(inDelegate);
            return rule;
        }

        /// <summary>
        /// Adds a new event parsing rule processed with the given delegate.
        /// </summary>
        public EventRule.Builder AddEvent(string inId, StringHash32 inEventId, EventDelegate inDelegate)
        {
            CheckLocked();

            EventRule.Builder rule = RestartEvent(inId);
            rule.ProcessWith(inEventId, inDelegate);
            return rule;
        }

        public bool TryProcess(TagData inData, object inContext, out TagEventData outEvent)
        {
            if (!m_Locked)
                m_EventBuilder.Flush();

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

        private EventRule.Builder RestartEvent(string inPattern)
        {
            m_EventBuilder.Restart(inPattern);
            return m_EventBuilder;
        }

        #endregion // Event

        #region Locking

        private void CheckLocked()
        {
            if (m_Locked)
                throw new InvalidOperationException("Parser config has marked read-only");
        }

        /// <summary>
        /// Marks this parser config as read-only.
        /// </summary>
        public void Lock()
        {
            if (!m_Locked)
            {
                m_ReplaceBuilder.Flush();
                m_ReplaceRules.EnsureSorted();

                m_EventBuilder.Flush();
                m_EventRules.EnsureSorted();
                
                m_Locked = true;
            }
        }

        #endregion // Locking
    }
}