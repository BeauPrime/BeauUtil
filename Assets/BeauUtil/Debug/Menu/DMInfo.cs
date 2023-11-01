/*
 * Copyright (C) 2021. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    8 Apr 2021
 * 
 * File:    DMInfo.cs
 * Purpose: Debug menu info.
 */

using System;

namespace BeauUtil.Debugger
{
    /// <summary>
    /// Debug menu configuration.
    /// </summary>
    public class DMInfo
    {
        public DMHeaderInfo Header;
        public readonly RingBuffer<DMElementInfo> Elements;
        public float MinimumWidth;

        /// <summary>
        /// Callback when the menu is entered.
        /// </summary>
        public readonly CastableEvent<DMInfo> OnEnter = new CastableEvent<DMInfo>();

        /// <summary>
        /// Callback when the menu is exited.
        /// </summary>
        public readonly CastableEvent<DMInfo> OnExit = new CastableEvent<DMInfo>();

        public DMInfo(string inHeader, int inCapacity = 0)
        {
            Header.Label = inHeader;
            Elements = new RingBuffer<DMElementInfo>(inCapacity, RingBufferMode.Expand);
        }

        /// <summary>
        /// Sets the minimum width of this menu.
        /// </summary>
        public DMInfo SetMinWidth(float inMinimumWidth)
        {
            MinimumWidth = inMinimumWidth;
            return this;
        }

        /// <summary>
        /// Adds a divider element to the menu.
        /// </summary>
        public DMInfo AddDivider()
        {
            if (Elements.Count == 0 || Elements.PeekBack().Type != DMElementType.Divider)
            {
                Elements.PushBack(DMElementInfo.CreateDivider());
            }
            return this;
        }

        /// <summary>
        /// Adds a button element to the menu.
        /// </summary>
        public DMInfo AddButton(string inLabel, DMButtonCallback inCallback, DMPredicate inPredicate = null, int inIndent = 0)
        {
            Elements.PushBack(DMElementInfo.CreateButton(inLabel, inCallback, inPredicate, inIndent));
            return this;
        }

        /// <summary>
        /// Adds a toggle element to the menu.
        /// </summary>
        public DMInfo AddToggle(string inLabel, DMPredicate inGetter, DMToggleCallback inSetter, DMPredicate inPredicate = null, int inIndent = 0)
        {
            Elements.PushBack(DMElementInfo.CreateToggle(inLabel, inGetter, inSetter, inPredicate, inIndent));
            return this;
        }

        /// <summary>
        /// Adds a submenu element to the menu.
        /// </summary>
        public DMInfo AddSubmenu(DMInfo inSubmenu, DMPredicate inPredicate = null, int inIndent = 0)
        {
            Elements.PushBack(DMElementInfo.CreateSubmenu(inSubmenu.Header.Label + " >", inSubmenu, inPredicate));
            return this;
        }

        /// <summary>
        /// Adds a submenu element to the menu.
        /// </summary>
        public DMInfo AddSubmenu(string inLabel, DMInfo inSubmenu, DMPredicate inPredicate = null, int inIndent = 0)
        {
            Elements.PushBack(DMElementInfo.CreateSubmenu(inLabel, inSubmenu, inPredicate));
            return this;
        }

        /// <summary>
        /// Adds a text element to the menu.
        /// </summary>
        public DMInfo AddText(string inLabel, DMTextDelegate inGetter, int inIndent = 0)
        {
            Elements.PushBack(DMElementInfo.CreateText(inLabel, inGetter, inIndent));
            return this;
        }

        /// <summary>
        /// Adds a slider element to the menu.
        /// </summary>
        public DMInfo AddSlider(string inLabel, DMFloatDelegate inGetter, DMSetFloatDelegate inSetter, float inMinValue, float inMaxValue, float inIncrement = 0, DMFloatTextDelegate inValueString = null, DMPredicate inPredicate = null, int inIndent = 0)
        {
            Elements.PushBack(DMElementInfo.CreateSlider(inLabel, inGetter, inSetter, inMinValue, inMaxValue, inIncrement, inValueString, inPredicate, inIndent));
            return this;
        }

        /// <summary>
        /// Adds a slider element to the menu.
        /// </summary>
        public DMInfo AddSlider(string inLabel, DMFloatDelegate inGetter, DMSetFloatDelegate inSetter, float inMinValue, float inMaxValue, float inIncrement, string inValueFormat, DMPredicate inPredicate = null, int inIndent = 0)
        {
            Elements.PushBack(DMElementInfo.CreateSlider(inLabel, inGetter, inSetter, inMinValue, inMaxValue, inIncrement, inValueFormat, inPredicate, inIndent));
            return this;
        }

        /// <summary>
        /// Clears all elements.
        /// </summary>
        public DMInfo Clear()
        {
            Elements.Clear();
            return this;
        }
    
        /// <summary>
        /// Returns if the given predicate passes, or is null.
        /// </summary>
        static public bool EvaluateOptionalPredicate(DMPredicate inPredicate)
        {
            return inPredicate == null || inPredicate();
        }
    }
}