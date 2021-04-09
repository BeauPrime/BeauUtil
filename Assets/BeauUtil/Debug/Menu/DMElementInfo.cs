/*
 * Copyright (C) 2021. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    8 Apr 2021
 * 
 * File:    DMElementInfo.cs
 * Purpose: Debug menu element data.
 */

namespace BeauUtil.Debugger
{
    public struct DMElementInfo
    {
        public DMElementType Type;
        public string Label;

        public DMButtonInfo Button;
        public DMToggleInfo Toggle;
        public DMSubmenuInfo Submenu;
        public DMTextInfo Text;

        public int Indent;
        public DMPredicate Predicate;

        static public DMElementInfo CreateDivider()
        {
            return new DMElementInfo()
            {
               Type = DMElementType.Divider 
            };
        }

        static public DMElementInfo CreateButton(string inLabel, DMBUttonCallback inCallback, DMPredicate inPredicate = null, int inIndent = 0)
        {
            return new DMElementInfo()
            {
                Type = DMElementType.Button,
                Label = inLabel,
                Button = new DMButtonInfo()
                {
                    Callback = inCallback,
                },
                Indent = inIndent,
                Predicate = inPredicate
            };
        }

        static public DMElementInfo CreateToggle(string inLabel, DMPredicate inGetter, DMToggleCallback inSetter, DMPredicate inPredicate = null, int inIndent = 0)
        {
            return new DMElementInfo()
            {
                Type = DMElementType.Toggle,
                Label = inLabel,
                Toggle = new DMToggleInfo()
                {
                    Getter = inGetter,
                    Setter = inSetter
                },
                Predicate = inPredicate
            };
        }

        static public DMElementInfo CreateSubmenu(string inLabel, DMInfo inSubmenu, DMPredicate inPredicate = null, int inIndent = 0)
        {
            return new DMElementInfo()
            {
                Type = DMElementType.Submenu,
                Label = inLabel,
                Submenu = new DMSubmenuInfo()
                {
                    Submenu = inSubmenu
                },
                Indent = inIndent,
                Predicate = inPredicate
            };
        }

        static public DMElementInfo CreateText(string inLabel, DMTextDelegate inGetter, int inIndent = 0)
        {
            return new DMElementInfo()
            {
                Type = DMElementType.Text,
                Label = inLabel,
                Text = new DMTextInfo()
                {
                    Getter =  inGetter
                },
                Indent = inIndent
            };
        }
    }
    
    public struct DMHeaderInfo
    {
        public string Label;
    }

    public delegate void DMBUttonCallback();
    public delegate void DMToggleCallback(bool inbValue);
    public delegate string DMTextDelegate();
    public delegate bool DMPredicate();

    public struct DMButtonInfo
    {
        public DMBUttonCallback Callback;
    }

    public struct DMToggleInfo
    {
        public DMPredicate Getter;
        public DMToggleCallback Setter;
    }

    public struct DMSubmenuInfo
    {
        public DMInfo Submenu;
    }

    public struct DMTextInfo
    {
        public DMTextDelegate Getter;
    }

    public enum DMElementType : byte
    {
        Divider,
        Button,
        Toggle,
        Submenu,
        Text
    }
}