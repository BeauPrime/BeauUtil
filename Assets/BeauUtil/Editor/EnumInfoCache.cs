/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    14 Oct 2019
 * 
 * File:    EnumInfoCache.cs
 * Purpose: Cached and customized information about an enum.
 */

using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace BeauUtil.Editor
{
    internal class EnumInfoCache
    {
        #region Types

        /// <summary>
        /// Basic info about an enum's type.
        /// </summary>
        internal enum TypeFlags
        {
            Normal = 0x00,
            Flags = 0x01,
            Labeled = 0x02,
            Sorted = 0x04
        }

        /// <summary>
        /// Info on a specific enum type.
        /// </summary>
        internal class Info
        {
            private TypeFlags m_Flags;
            private NamedItemList<Enum> m_labeledList;
            private string[] m_FlagNames;
            private FlagMapping[] m_FlagMapping;

            public TypeFlags Flags { get { return m_Flags; } }

            public NamedItemList<Enum> LabeledList { get { return m_labeledList; } }

            public string[] FlagNames { get { return m_FlagNames; } }

            public int MapToFlagInput(Enum inCurrentValue)
            {
                int currentValAsInt = Convert.ToInt32(inCurrentValue);

                int flagInput = 0;
                for (int i = 0; i < m_FlagMapping.Length; ++i)
                {
                    var map = m_FlagMapping[i];
                    if (map.Input == 0 && currentValAsInt != 0)
                        continue;

                    if ((currentValAsInt & map.Input) == map.Input)
                    {
                        flagInput |= map.Output;
                    }
                }
                return flagInput;
            }

            public Enum MapFromFlagOutput(Enum inOriginalValue, int inFlagInput, int inFlagOutput)
            {
                int changes = inFlagInput ^ inFlagOutput;
                int valueOutput = Convert.ToInt32(inOriginalValue);

                for (int i = 0; i < m_FlagMapping.Length; ++i)
                {
                    var map = m_FlagMapping[i];

                    // skip if this didn't change
                    if ((changes & map.Output) == 0)
                        continue;

                    if ((inFlagOutput & map.Output) == map.Output)
                    {
                        if (map.Input == 0)
                        {
                            valueOutput = 0;
                            break;
                        }
                        valueOutput |= map.Input;
                    }
                    else
                    {
                        valueOutput &= ~map.Input;
                    }
                }

                return (Enum) Enum.ToObject(inOriginalValue.GetType(), valueOutput);
            }

            public void GenerateForType(Type inType)
            {
                m_Flags = GetEnumTypeFlags(inType);

                bool bIsLabeled = (m_Flags & TypeFlags.Labeled) == TypeFlags.Labeled;
                if (bIsLabeled)
                {
                    bool bIsSorted = (m_Flags & TypeFlags.Sorted) == TypeFlags.Sorted;
                    m_labeledList = CreateLabeledList(inType, bIsSorted);
                }

                bool bIsFlags = (m_Flags & TypeFlags.Flags) == TypeFlags.Flags;
                if (bIsFlags)
                {
                    CreateFlagMapping(inType, m_labeledList, out m_FlagNames, out m_FlagMapping);
                }
            }
        }

        private struct FlagMapping
        {
            public readonly int Input;
            public readonly int Output;

            public FlagMapping(int inInput, int inOutput)
            {
                Input = inInput;
                Output = inOutput;
            }
        }

        #endregion // Types

        private Dictionary<Type, Info> m_InfoMap = new Dictionary<Type, Info>();

        public Info GetInfo<T>()
        {
            return GetInfo(typeof(T));
        }

        public Info GetInfo(Type inType)
        {
            Info info;
            if (!m_InfoMap.TryGetValue(inType, out info))
            {
                info = new Info();
                info.GenerateForType(inType);
                m_InfoMap.Add(inType, info);
            }
            return info;
        }

        #region Generation

        /// <summary>
        /// Generates the set of flags for a given enum type.
        /// </summary>
        static private TypeFlags GetEnumTypeFlags(Type inType)
        {
            TypeFlags typeFlags = TypeFlags.Normal;
            if (inType.IsDefined(typeof(FlagsAttribute)))
            {
                typeFlags |= TypeFlags.Flags;
            }

            LabeledEnum labeledEnumAttr = (LabeledEnum) inType.GetCustomAttribute(typeof(LabeledEnum));
            if (labeledEnumAttr != null)
            {
                typeFlags |= TypeFlags.Labeled;
                if (labeledEnumAttr.Sorted)
                    typeFlags |= TypeFlags.Sorted;
            }
            else
            {
                foreach (var field in inType.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.DeclaredOnly))
                {
                    if (field.IsDefined(typeof(HiddenAttribute)) || field.IsDefined(typeof(ObsoleteAttribute)))
                        continue;

                    if (field.IsDefined(typeof(LabelAttribute)) || field.IsDefined(typeof(OrderAttribute)))
                    {
                        typeFlags |= TypeFlags.Labeled;
                        typeFlags |= TypeFlags.Sorted;
                        break;
                    }
                }
            }

            return typeFlags;
        }

        /// <summary>
        /// Generates the labeled list for the enum type.
        /// </summary>
        static private NamedItemList<Enum> CreateLabeledList(Type inType, bool inbSorted = false)
        {
            NamedItemList<Enum> itemList = new NamedItemList<Enum>();
            foreach (var field in inType.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.DeclaredOnly))
            {
                if (field.IsDefined(typeof(HiddenAttribute)) || field.IsDefined(typeof(ObsoleteAttribute)))
                    continue;

                LabelAttribute labeledAttr = (LabelAttribute) field.GetCustomAttribute(typeof(LabelAttribute));
                OrderAttribute orderAttr = (OrderAttribute) field.GetCustomAttribute(typeof(OrderAttribute));

                string name;
                int order;
                Enum val = (Enum) field.GetValue(null);

                if (labeledAttr != null)
                {
                    name = labeledAttr.Name;
                }
                else
                {
                    name = ObjectNames.NicifyVariableName(val.ToString());
                }

                if (orderAttr != null)
                {
                    order = orderAttr.Order;
                }
                else if (inbSorted)
                {
                    order = 100;
                }
                else
                {
                    order = Convert.ToInt32(val);
                }

                itemList.Add(val, name, order);
            }
            return itemList;
        }

        /// <summary>
        /// Generates the flag mapping for the given enum type.
        /// </summary>
        static private void CreateFlagMapping(Type inType, NamedItemList<Enum> inLabeledList, out string[] outFlagNames, out FlagMapping[] outMapping)
        {
            List<FlagMapping> mappings;
            List<string> names;

            if (inLabeledList != null)
            {
                int labeledCount = inLabeledList.Count;
                names = new List<string>(labeledCount);
                mappings = new List<FlagMapping>(labeledCount);

                for (int i = 0; i < inLabeledList.Count; ++i)
                {
                    Enum val = inLabeledList.Get(i);
                    int input = Convert.ToInt32(val);
                    int output = 1 << i;

                    if (Mathf.IsPowerOfTwo(input))
                    {
                        names.Add(inLabeledList.SortedStrings() [i]);
                        mappings.Add(new FlagMapping(input, output));
                    }
                }
            }
            else
            {
                FieldInfo[] fields = inType.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.DeclaredOnly);
                int fieldCount = fields.Length;

                names = new List<string>(fieldCount);
                mappings = new List<FlagMapping>(fieldCount);

                for (int i = 0; i < fieldCount; ++i)
                {
                    FieldInfo field = fields[i];

                    if (field.IsDefined(typeof(HiddenAttribute)) || field.IsDefined(typeof(ObsoleteAttribute)))
                        continue;

                    LabelAttribute labeledAttr = (LabelAttribute) field.GetCustomAttribute(typeof(LabelAttribute));

                    string name;
                    Enum val = (Enum) field.GetValue(null);

                    if (labeledAttr != null)
                    {
                        name = labeledAttr.Name;
                    }
                    else
                    {
                        name = ObjectNames.NicifyVariableName(val.ToString());
                    }

                    int input = Convert.ToInt32(val);
                    int output = 1 << i;

                    if (Mathf.IsPowerOfTwo(input))
                    {
                        names.Add(name);
                        mappings.Add(new FlagMapping(input, output));
                    }
                }
            }

            outFlagNames = names.ToArray();
            outMapping = mappings.ToArray();
        }

        #endregion // Generation

        static public readonly EnumInfoCache Instance = new EnumInfoCache();
    }
}