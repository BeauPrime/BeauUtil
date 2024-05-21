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

        /// <summary>
        /// Sorts the given menu alphabetically.
        /// </summary>
        static public void SortByLabel(DMInfo inMenu)
        {
            inMenu.Elements.Sort((a, b) => StringComparer.OrdinalIgnoreCase.Compare(a.Label, b.Label));
        }

        #region Submenu Utilities

        /// <summary>
        /// Finds a named submenu within the given parent menu.
        /// </summary>
        static public DMInfo FindSubmenu(DMInfo inParent, string inLabel)
        {
            int index = inParent.Elements.FindIndex((e, l) => {
                return e.Type == DMElementType.Submenu && e.Submenu.Submenu.Header.Label == l;
            }, inLabel);
            if (index >= 0)
                return inParent.Elements[index].Submenu.Submenu;
            else
                return null;
        }

        /// <summary>
        /// Finds or creates a named submenu within the given parent menu.
        /// </summary>
        static public DMInfo FindOrCreateSubmenu(DMInfo inParent, string inLabel)
        {
            int index = inParent.Elements.FindIndex((e, l) => {
                return e.Type == DMElementType.Submenu && e.Submenu.Submenu.Header.Label == l;
            }, inLabel);
            if (index >= 0)
            {
                return inParent.Elements[index].Submenu.Submenu;
            }
            else
            {
                DMInfo newMenu = new DMInfo(inLabel);
                inParent.AddSubmenu(newMenu);
                return newMenu;
            }
        }

        /// <summary>
        /// Merges the given submenu into the given parent menu.
        /// If a submenu with the same name already exists,
        /// the contents of the given submenu will be merged
        /// into the existing one, optionally separated by a divider.
        /// Returns the merged submenu.
        /// </summary>
        static public DMInfo MergeSubmenu(DMInfo inParent, DMInfo inSubMenu, bool inbSeparateWithDivider = true)
        {
            DMInfo existingSubmenu = FindSubmenu(inParent, inSubMenu.Header.Label);
            if (existingSubmenu != null)
            {
                if (inbSeparateWithDivider && existingSubmenu.Elements.Count > 0)
                {
                    existingSubmenu.AddDivider();
                }
                foreach(var element in inSubMenu.Elements)
                {
                    existingSubmenu.Elements.PushBack(element);
                }
                existingSubmenu.MinimumWidth = Math.Max(inSubMenu.MinimumWidth, existingSubmenu.MinimumWidth);
                inSubMenu.Clear();
                return existingSubmenu;
            }
            else
            {
                inParent.AddSubmenu(inSubMenu);
                return inSubMenu;
            }
        }

        #endregion // Submenu Utilities
    }
}