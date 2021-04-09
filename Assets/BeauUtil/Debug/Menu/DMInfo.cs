/*
 * Copyright (C) 2021. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    8 Apr 2021
 * 
 * File:    DMInfo.cs
 * Purpose: Debug menu info.
 */

namespace BeauUtil.Debugger
{
    /// <summary>
    /// Debug menu configuration.
    /// </summary>
    public class DMInfo
    {
        public DMHeaderInfo Header;
        public readonly RingBuffer<DMElementInfo> Elements;

        public DMInfo(string inHeader, int inCapacity = 0)
        {
            Header.Label = inHeader;
            Elements = new RingBuffer<DMElementInfo>(inCapacity, RingBufferMode.Expand);
        }

        public DMInfo AddDivider()
        {
            Elements.PushBack(DMElementInfo.CreateDivider());
            return this;
        }

        public DMInfo AddButton(string inLabel, DMBUttonCallback inCallback, DMPredicate inPredicate = null, int inIndent = 0)
        {
            Elements.PushBack(DMElementInfo.CreateButton(inLabel, inCallback, inPredicate, inIndent));
            return this;
        }

        public DMInfo AddToggle(string inLabel, DMPredicate inGetter, DMToggleCallback inSetter, DMPredicate inPredicate = null, int inIndent = 0)
        {
            Elements.PushBack(DMElementInfo.CreateToggle(inLabel, inGetter, inSetter, inPredicate, inIndent));
            return this;
        }

        public DMInfo AddSubmenu(DMInfo inSubmenu, DMPredicate inPredicate = null, int inIndent = 0)
        {
            Elements.PushBack(DMElementInfo.CreateSubmenu(inSubmenu.Header.Label + " >", inSubmenu, inPredicate));
            return this;
        }

        public DMInfo AddSubmenu(string inLabel, DMInfo inSubmenu, DMPredicate inPredicate = null, int inIndent = 0)
        {
            Elements.PushBack(DMElementInfo.CreateSubmenu(inLabel, inSubmenu, inPredicate));
            return this;
        }

        public DMInfo AddText(string inLabel, DMTextDelegate inGetter, int inIndent = 0)
        {
            Elements.PushBack(DMElementInfo.CreateText(inLabel, inGetter, inIndent));
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