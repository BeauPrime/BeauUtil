/*
 * Copyright (C) 2021. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    8 Apr 2021
 * 
 * File:    DMElementInfo.cs
 * Purpose: Debug menu element data.
 */

#if NET_4_6 || CSHARP_7_OR_LATER
#define HAS_ENUM_CONSTRAINT
#endif // NET_4_6 || CSHARP_7_OR_LATER

#if CSHARP_7_3_OR_NEWER
#define UNMANAGED_CONSTRAINT
#endif // CSHARP_7_3_OR_NEWER

using System;
using System.Text;
using UnityEngine;

namespace BeauUtil.Debugger
{
    public struct DMElementInfo
    {
        public DMElementType Type;
        public string Label;
        public object Data;
        public float MinimumWidth;

        public DMButtonInfo Button { get { return (DMButtonInfo) Data; } }
        public DMToggleInfo Toggle { get { return (DMToggleInfo) Data; } }
        public DMSubmenuInfo Submenu { get { return (DMSubmenuInfo) Data; } }
        public DMTextInfo Text { get { return (DMTextInfo) Data; } }
        public DMSliderInfo Slider { get { return (DMSliderInfo) Data; } }
        public DMSelectorInfo Selector { get { return (DMSelectorInfo) Data; } }

        public int Indent;
        public DMPredicate Predicate;
        public uint Flags;

        public DMElementStyle Style;

        #region Factory

        static public DMElementInfo CreateDivider()
        {
            return new DMElementInfo()
            {
               Type = DMElementType.Divider 
            };
        }

        static public DMElementInfo CreatePageBreak()
        {
            return new DMElementInfo()
            {
                Type = DMElementType.PageBreak
            };
        }

        static public DMElementInfo CreateButton(string inLabel, DMButtonCallback inCallback, DMPredicate inPredicate = null, int inIndent = 0)
        {
            return new DMElementInfo()
            {
                Type = DMElementType.Button,
                Label = inLabel,
                Data = new DMButtonInfo()
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
                Data = new DMToggleInfo()
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
                Data = new DMSubmenuInfo()
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
                Data = new DMTextInfo()
                {
                    Getter =  inGetter
                },
                Indent = inIndent
            };
        }

        static public DMElementInfo CreateText(string inLabel, DMTextSBDelegate inGetter, int inIndent = 0)
        {
            return new DMElementInfo()
            {
                Type = DMElementType.Text,
                Label = inLabel,
                Data = new DMTextInfo()
                {
                    GetterSB = inGetter
                },
                Indent = inIndent
            };
        }

        static public DMElementInfo CreateText(string inLabel, int inIndent = 0)
        {
            return new DMElementInfo()
            {
                Type = DMElementType.Text,
                Label = inLabel,
                Data = new DMTextInfo()
                {
                },
                Indent = inIndent
            };
        }

        static public DMElementInfo CreateSlider(string inLabel, DMFloatDelegate inGetter, DMSetFloatDelegate inSetter, float inMinValue, float inMaxValue, float inIncrement = 0, DMFloatTextDelegate inValueString = null, DMPredicate inPredicate = null, int inIndent = 0)
        {
            return new DMElementInfo()
            {
                Type = DMElementType.Slider,
                Label = inLabel,
                Data = new DMSliderInfo()
                {
                    Getter = inGetter,
                    Setter = inSetter,
                    Label = inValueString,
                    Range = new DMSliderRange()
                    {
                        MinValue = inMinValue,
                        MaxValue = inMaxValue,
                        Increment = inIncrement
                    }
                },
                Indent = inIndent,
                Predicate = inPredicate
            };
        }

        static public DMElementInfo CreateSlider(string inLabel, DMFloatDelegate inGetter, DMSetFloatDelegate inSetter, float inMinValue, float inMaxValue, float inIncrement, DMFloatTextSBDelegate inValueString, DMPredicate inPredicate = null, int inIndent = 0)
        {
            return new DMElementInfo()
            {
                Type = DMElementType.Slider,
                Label = inLabel,
                Data = new DMSliderInfo()
                {
                    Getter = inGetter,
                    Setter = inSetter,
                    LabelSB = inValueString,
                    Range = new DMSliderRange()
                    {
                        MinValue = inMinValue,
                        MaxValue = inMaxValue,
                        Increment = inIncrement
                    }
                },
                Indent = inIndent,
                Predicate = inPredicate
            };
        }

        static public DMElementInfo CreateSlider(string inLabel, DMFloatDelegate inGetter, DMSetFloatDelegate inSetter, float inMinValue, float inMaxValue, float inIncrement, string inValueFormat, DMPredicate inPredicate = null, int inIndent = 0)
        {
            return new DMElementInfo()
            {
                Type = DMElementType.Slider,
                Label = inLabel,
                Data = new DMSliderInfo()
                {
                    Getter = inGetter,
                    Setter = inSetter,
                    Label = (v) => string.Format(inValueFormat, v),
                    Range = new DMSliderRange()
                    {
                        MinValue = inMinValue,
                        MaxValue = inMaxValue,
                        Increment = inIncrement
                    }
                },
                Indent = inIndent,
                Predicate = inPredicate
            };
        }

        static public DMElementInfo CreateSelector(string inLabel, DMIntDelegate inGetter, DMSetIntDelegate inSetter, string[] inLabels, DMPredicate inPredicate = null, int inIndent = 0)
        {
            Assert.True(inLabels != null && inLabels.Length > 0);
            return new DMElementInfo()
            {
                Type = DMElementType.Selector,
                Label = inLabel,
                Data = new DMSelectorInfo()
                {
                    Getter = inGetter,
                    Setter = inSetter,
                    Labels = inLabels
                },
                Indent = inIndent,
                Predicate = inPredicate
            };
        }

        static public DMElementInfo CreateSelector(string inLabel, DMIntDelegate inGetter, DMSetIntDelegate inSetter, int[] inValues, string[] inLabels, DMPredicate inPredicate = null, int inIndent = 0)
        {
            Assert.True(inLabels != null && inLabels.Length > 0);
            Assert.True(inValues != null && inValues.Length == inLabels.Length);

            return new DMElementInfo()
            {
                Type = DMElementType.Selector,
                Label = inLabel,
                Data = new DMSelectorInfo()
                {
                    Getter = inGetter,
                    Setter = inSetter,
                    Labels = inLabels,
                    Values = inValues
                },
                Indent = inIndent,
                Predicate = inPredicate
            };
        }

        #endregion // Factory
    }

    public delegate void DMButtonCallback();
    public delegate void DMToggleCallback(bool inbValue);

    public delegate string DMTextDelegate();
    public delegate void DMTextSBDelegate(StringBuilder ioBuilder);
    public delegate bool DMSetTextDelegate(string inValue);
    
    public delegate bool DMPredicate();
    public delegate float DMFloatDelegate();
    public delegate void DMSetFloatDelegate(float inValue);
    public delegate string DMFloatTextDelegate(float inValue);
    public delegate void DMFloatTextSBDelegate(StringBuilder ioBuilder, float inValue);

    public delegate int DMIntDelegate();
    public delegate void DMSetIntDelegate(int inValue);

    public struct DMHeaderInfo
    {
        public string Label;
    }

    public struct DMElementStyle
    {
        public Color32 ContentColor;
        public DMTextAnchorFlags Anchors;
    }

    /// <summary>
    /// Button-specific element data.
    /// </summary>
    public class DMButtonInfo
    {
        public DMButtonCallback Callback;
    }

    /// <summary>
    /// Toggle-specific element data.
    /// </summary>
    public class DMToggleInfo
    {
        public DMPredicate Getter;
        public DMToggleCallback Setter;
    }

    /// <summary>
    /// Submenu-specific element data.
    /// </summary>
    public class DMSubmenuInfo
    {
        public DMInfo Submenu;
    }

    /// <summary>
    /// Text-specific element data.
    /// </summary>
    public class DMTextInfo
    {
        public DMTextDelegate Getter;
        public DMTextSBDelegate GetterSB;
        public string Text;
    }

    /// <summary>
    /// Slider-specific element data.
    /// </summary>
    public class DMSliderInfo
    {
        public DMFloatDelegate Getter;
        public DMSetFloatDelegate Setter;
        public DMFloatTextDelegate Label;
        public DMFloatTextSBDelegate LabelSB;
        public DMSliderRange Range;
    }

    /// <summary>
    /// Selector-specific element data.
    /// </summary>
    public class DMSelectorInfo
    {
        public DMIntDelegate Getter;
        public DMSetIntDelegate Setter;

        public int[] Values;
        public string[] Labels;

        static internal int GetValue(DMSelectorInfo inSelector, int inIndex)
        {
            if (inIndex < 0)
            {
                return -1;
            }

            inIndex = Math.Min(inIndex, inSelector.Labels.Length - 1);
            if (inSelector.Values != null)
            {
                return inSelector.Values[inIndex];
            }

            return inIndex;
        }

        static internal int GetIndex(DMSelectorInfo inSelector, int inValue)
        {
            if (inSelector.Values != null)
            {
                for(int i = 0; i < inSelector.Values.Length; i++)
                {
                    if (inSelector.Values[i] == inValue)
                        return i;
                }

                return -1;
            }

            if (inValue < 0)
            {
                return -1;
            }

            return Math.Min(inValue, inSelector.Labels.Length - 1);
        }
    }

    /// <summary>
    /// Slider range.
    /// </summary>
    public struct DMSliderRange
    {
        public float MinValue;
        public float MaxValue;
        public float Increment;

        public DMSliderRange(float inMinValue, float inMaxValue, float inIncrement = 0)
        {
            MinValue = inMinValue;
            MaxValue = inMaxValue;
            Increment = inIncrement;
        }

        public DMSliderRange(int inMinValue, int inMaxValue, int inIncrement = 1)
        {
            MinValue = inMinValue;
            MaxValue = inMaxValue;
            Increment = inIncrement;
        }

        static internal float GetValue(DMSliderRange inRange, float inPercentage)
        {
            float val = inRange.MinValue + inPercentage * (inRange.MaxValue - inRange.MinValue);
            if (inRange.Increment > 0)
            {
                val = MathUtils.Quantize(val, inRange.Increment);
            }
            return val;
        }

        static internal int GetTotalIncrements(DMSliderRange inRange)
        {
            if (inRange.Increment <= 0)
            {
                return 0;
            }

            return (int) Math.Max(1, Math.Round(Math.Abs(inRange.MaxValue - inRange.MinValue) / inRange.Increment));
        }

        static internal float GetPercentage(DMSliderRange inRange, float inValue)
        {
            return (inValue - inRange.MinValue) / (inRange.MaxValue - inRange.MinValue);
        }
    }

    /// <summary>
    /// Text input element info.
    /// </summary>
    public class DMTextInputInfo
    {
        public DMTextDelegate Getter;
        public DMSetTextDelegate Setter;

        public DMTextInputFlags Flags;
        public int MaxCharacters;
    }

    public enum DMTextInputFlags : uint
    {
        Alphanumeric = 0x01,
        Integer = 0x02,
        Decimal = 0x04,
        Password = 0x08
    }

    public enum DMTextAnchorFlags : uint
    {
        Center = 0,
        Left = 0x01,
        Right = 0x02,
    }

    /// <summary>
    /// Debug menu element type.
    /// </summary>
    public enum DMElementType : byte
    {
        Divider,
        Button,
        Toggle,
        Submenu,
        Text,
        Slider,
        TextInput,
        Selector,
        PageBreak
    }

    /// <summary>
    /// Element heights configuration.
    /// </summary>
    [Serializable]
    public struct DMElementHeights
    {
        public int Button;
        public int Text;
        public int Slider;
        public int TextInput;
        public int Selector;

        static public readonly DMElementHeights Default = new DMElementHeights()
        {
            Button = 1,
            Text = 1,
            Slider = 2,
            TextInput = 2,
            Selector = 2,
        };
    }
}