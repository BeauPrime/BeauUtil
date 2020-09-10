/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    9 Sept 2020
 * 
 * File:    OrganizedKeyCodeDrawer.cs
 * Purpose: Better property drawer for KeyCode.
 */

using System;
using UnityEditor;
using UnityEngine;

namespace BeauUtil.Editor
{
    [CustomPropertyDrawer(typeof(KeyCode))]
    internal sealed class OrganizedKeyCodePropertyDrawer : PropertyDrawer
    {
        static private NamedItemList<KeyCode> s_KeyCodes;

        public override void OnGUI(Rect position, UnityEditor.SerializedProperty property, GUIContent label)
        {
            InitializeKeycodeList();

            int val = property.hasMultipleDifferentValues ? int.MinValue : property.intValue;
            KeyCode keyCodeVal = (KeyCode) val;

            label = UnityEditor.EditorGUI.BeginProperty(position, label, property);
            EditorGUI.BeginChangeCheck();
            KeyCode newVal = ListGUI.Popup(position, label, keyCodeVal, s_KeyCodes);
            if (EditorGUI.EndChangeCheck() && newVal != keyCodeVal)
            {
                property.intValue = (int) newVal;
            }
            UnityEditor.EditorGUI.EndProperty();
        }

        static private void InitializeKeycodeList()
        {
            if (s_KeyCodes != null)
                return;

            s_KeyCodes = new NamedItemList<KeyCode>(510);

            foreach(KeyCode code in Enum.GetValues(typeof(KeyCode)))
            {
                if (code == KeyCode.None)
                {
                    s_KeyCodes.Add(code, "None", -1);
                }
                else
                {
                    s_KeyCodes.Add(code, GetName(code));
                }
            }
        }

        static private string GetName(KeyCode inCode)
        {
            if (inCode >= KeyCode.A && inCode <= KeyCode.Z)
                return "Alphabet/" + inCode.ToString();
            if (inCode >= KeyCode.Alpha0 && inCode <= KeyCode.Alpha9)
                return "Numbers/" + (inCode - KeyCode.Alpha0).ToString();
            if (inCode >= KeyCode.F1 && inCode <= KeyCode.F15)
                return "Function/" + inCode.ToString();
            if (inCode >= KeyCode.Keypad0 && inCode <= KeyCode.KeypadEquals)
                return "Keypad/" + inCode.ToString().Substring(6);
            if (inCode >= KeyCode.Mouse0 && inCode <= KeyCode.Mouse6)
                return "Mouse/" + (inCode - KeyCode.Mouse0).ToString();
            if (inCode == KeyCode.LeftArrow)
                return "Arrows/Left";
            if (inCode == KeyCode.RightArrow)
                return "Arrows/Right";
            if (inCode == KeyCode.UpArrow)
                return "Arrows/Up";
            if (inCode == KeyCode.DownArrow)
                return "Arrows/Down";
            if (inCode >= KeyCode.JoystickButton0 && inCode <= KeyCode.Joystick8Button19)
            {
                if (inCode <= KeyCode.JoystickButton19)
                    return "Joystick/Any/Button " + (inCode - KeyCode.JoystickButton0).ToString();
                if (inCode <= KeyCode.Joystick1Button19)
                    return "Joystick/1/Button " + (inCode - KeyCode.Joystick1Button0).ToString();
                if (inCode <= KeyCode.Joystick2Button19)
                    return "Joystick/2/Button " + (inCode - KeyCode.Joystick2Button0).ToString();
                if (inCode <= KeyCode.Joystick3Button19)
                    return "Joystick/3/Button " + (inCode - KeyCode.Joystick3Button0).ToString();
                if (inCode <= KeyCode.Joystick4Button19)
                    return "Joystick/4/Button " + (inCode - KeyCode.Joystick4Button0).ToString();
                if (inCode <= KeyCode.Joystick5Button19)
                    return "Joystick/5/Button " + (inCode - KeyCode.Joystick5Button0).ToString();
                if (inCode <= KeyCode.Joystick6Button19)
                    return "Joystick/6/Button " + (inCode - KeyCode.Joystick6Button0).ToString();
                if (inCode <= KeyCode.Joystick7Button19)
                    return "Joystick/7/Button " + (inCode - KeyCode.Joystick7Button0).ToString();
                return "Joystick/8/Button " + (inCode - KeyCode.Joystick8Button0).ToString();
            }

            switch(inCode)
            {
                case KeyCode.Backspace:
                    return "Common/Backspace";
                case KeyCode.Tab:
                    return "Common/Tab";
                case KeyCode.Clear:
                    return "Special/Clear";
                case KeyCode.Return:
                    return "Common/Return";
                case KeyCode.Pause:
                    return "Special/Pause";
                case KeyCode.Escape:
                    return "Common/Escape";
                case KeyCode.Space:
                    return "Common/Space";
                case KeyCode.Exclaim:
                    return "Punctuation/Exclaim";
                case KeyCode.DoubleQuote:
                    return "Punctuation/Double Quote";
                case KeyCode.Hash:
                    return "Punctuation/Hash";
                case KeyCode.Dollar:
                    return "Punctuation/Dollar";
                case KeyCode.Percent:
                    return "Punctuation/Percent";
                case KeyCode.Ampersand:
                    return "Punctuation/Ampersand";
                case KeyCode.Quote:
                    return "Punctuation/Quote";
                case KeyCode.LeftParen:
                    return "Grouping/Left Paran";
                case KeyCode.RightParen:
                    return "Grouping/Right Paren";
                case KeyCode.Asterisk:
                    return "Symbols/Asterisk";
                case KeyCode.Plus:
                    return "Symbols/Plus";
                case KeyCode.Comma:
                    return "Punctuation/Comma";
                case KeyCode.Minus:
                    return "Symbols/Minus";
                case KeyCode.Period:
                    return "Punctuation/Period";
                case KeyCode.Slash:
                    return "Symbols/Slash";
                case KeyCode.Colon:
                    return "Punctuation/Colon";
                case KeyCode.Semicolon:
                    return "Punctuation/Semicolon";
                case KeyCode.Less:
                    return "Symbols/Less";
                case KeyCode.Equals:
                    return "Symbols/Equals";
                case KeyCode.Greater:
                    return "Symbols/Greater";
                case KeyCode.Question:
                    return "Punctuation/Question";
                case KeyCode.At:
                    return "Symbols/At (@)";
                case KeyCode.LeftBracket:
                    return "Grouping/Left Bracket";
                case KeyCode.Backslash:
                    return "Punctuation/Backslash";
                case KeyCode.RightBracket:
                    return "Grouping/Right Bracket";
                case KeyCode.Caret:
                    return "Punctuation/Caret";
                case KeyCode.Underscore:
                    return "Punctuation/Underscore";
                case KeyCode.BackQuote:
                    return "Punctuation/Back Quote";
                case KeyCode.LeftCurlyBracket:
                    return "Grouping/Left Curly Bracket";
                case KeyCode.Pipe:
                    return "Grouping/Pipe";
                case KeyCode.RightCurlyBracket:
                    return "Grouping/Right Curly Bracket";
                case KeyCode.Tilde:
                    return "Symbols/Tilde";
                case KeyCode.Delete:
                    return "Common/Delete";
                case KeyCode.Insert:
                    return "Common/Insert";
                case KeyCode.Home:
                    return "Common/Home";
                case KeyCode.End:
                    return "Common/End";
                case KeyCode.PageUp:
                    return "Special/Page Up";
                case KeyCode.PageDown:
                    return "Special/Page Down";
                case KeyCode.Numlock:
                    return "Special/Num Lock";
                case KeyCode.CapsLock:
                    return "Special/Caps Lock";
                case KeyCode.ScrollLock:
                    return "Special/Scroll Lock";
                case KeyCode.RightShift:
                    return "Modifiers/Right Shift";
                case KeyCode.LeftShift:
                    return "Modifiers/Left Shift";
                case KeyCode.RightControl:
                    return "Modifiers/Right Control";
                case KeyCode.LeftControl:
                    return "Modifiers/Left Control";
                case KeyCode.RightAlt:
                    return "Modifiers/Right Alt";
                case KeyCode.LeftAlt:
                    return "Modifiers/Left Alt";
                case KeyCode.RightCommand:
                    return "Modifiers/Right Command or Apple";
                case KeyCode.LeftCommand:
                    return "Modifiers/Left Command or Apple";
                case KeyCode.LeftWindows:
                    return "Modifiers/Left Windows";
                case KeyCode.RightWindows:
                    return "Modifiers/Right Windows";
                case KeyCode.AltGr:
                    return "Modifiers/Alt Gr";
                case KeyCode.Help:
                    return "Special/Help";
                case KeyCode.Print:
                    return "Special/Print";
                case KeyCode.SysReq:
                    return "Special/Sys Req";
                case KeyCode.Break:
                    return "Special/Break";
                case KeyCode.Menu:
                    return "Special/Menu";
            }

            return inCode.ToString();
        }
    }
}