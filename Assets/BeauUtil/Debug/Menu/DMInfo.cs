/*
 * Copyright (C) 2021. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    8 Apr 2021
 * 
 * File:    DMInfo.cs
 * Purpose: Debug menu info.
 */

#if NET_4_6 || CSHARP_7_OR_LATER
#define HAS_ENUM_CONSTRAINT
#endif // NET_4_6 || CSHARP_7_OR_LATER

#if CSHARP_7_3_OR_NEWER
#define UNMANAGED_CONSTRAINT
#endif // CSHARP_7_3_OR_NEWER

using System;
using System.Text;

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
            if (Elements.Count > 0 && Elements.PeekBack().Type != DMElementType.Divider)
            {
                Elements.PushBack(DMElementInfo.CreateDivider());
            }
            return this;
        }

        /// <summary>
        /// Adds a page break element to the menu.
        /// </summary>
        public DMInfo AddPageBreak()
        {
            if (Elements.Count > 0 && Elements.PeekBack().Type != DMElementType.PageBreak)
            {
                Elements.PushBack(DMElementInfo.CreatePageBreak());
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
        /// Adds a text element to the menu.
        /// </summary>
        public DMInfo AddText(string inLabel, DMTextSBDelegate inGetter, int inIndent = 0)
        {
            Elements.PushBack(DMElementInfo.CreateText(inLabel, inGetter, inIndent));
            return this;
        }

        /// <summary>
        /// Adds a text element to the menu.
        /// </summary>
        public DMInfo AddText(string inLabel, int inIndent = 0)
        {
            Elements.PushBack(DMElementInfo.CreateText(inLabel, inIndent));
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
        public DMInfo AddSlider(string inLabel, DMFloatDelegate inGetter, DMSetFloatDelegate inSetter, float inMinValue, float inMaxValue, float inIncrement, DMFloatTextSBDelegate inValueString, DMPredicate inPredicate = null, int inIndent = 0)
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
        /// Adds a selector element to the menu.
        /// </summary>
        public DMInfo AddSelector(string inLabel, DMIntDelegate inGetter, DMSetIntDelegate inSetter, string[] inLabels, DMPredicate inPredicate = null, int inIndent = 0)
        {
            Elements.PushBack(DMElementInfo.CreateSelector(inLabel, inGetter, inSetter, inLabels, inPredicate, inIndent));
            return this;
        }

        /// <summary>
        /// Adds a selector element to the menu.
        /// </summary>
        public DMInfo AddSelector(string inLabel, DMIntDelegate inGetter, DMSetIntDelegate inSetter, int[] inValues, string[] inLabels, DMPredicate inPredicate = null, int inIndent = 0)
        {
            Elements.PushBack(DMElementInfo.CreateSelector(inLabel, inGetter, inSetter, inValues, inLabels, inPredicate, inIndent));
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

        #region Strings

        /// <summary>
        /// Shared string builder.
        /// </summary>
        static public readonly StringBuilder SharedTextBuilder = new StringBuilder(128, 128);

        /// <summary>
        /// Returns the nicified name for the given field/type name.
        /// </summary>
        static public unsafe string InspectorName(string inName)
        {
            char* buff = stackalloc char[inName.Length * 2];
            bool wasUpper = true, isUpper;
            int charsWritten = 0;

            int i = 0;
            if (inName.Length > 1)
            {
                char first = inName[0];
                if (first == '_')
                {
                    i = 1;
                }
                else if (first == 'm' || first == 's' || first == 'k')
                {
                    char second = inName[1];
                    if (second == '_' || char.IsUpper(second))
                    {
                        i = 2;
                    }
                }
            }

            for (; i < inName.Length; i++)
            {
                char c = inName[i];
                isUpper = char.IsUpper(c);
                if (isUpper && !wasUpper && charsWritten > 0)
                {
                    buff[charsWritten++] = ' ';
                }
                buff[charsWritten++] = c;

                wasUpper = isUpper;
            }

            return new string(buff, 0, charsWritten);
        }

        #endregion // Strings

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