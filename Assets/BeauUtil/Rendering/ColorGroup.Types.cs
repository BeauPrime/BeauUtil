/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    4 April 2019
 * 
 * File:    ColorGroup.Types.cs
 * Purpose: Nested ColorGroup types.
 */

using System;
using System.Collections.Generic;
using UnityEngine;

namespace BeauUtil
{
    /// <summary>
    /// Enables, disables, and tints renderers individually or as a group.
    /// </summary>
    public sealed partial class ColorGroup : MonoBehaviour, ICanvasRaycastFilter
    {
        public enum Channel : byte
        {
            Main = 0,
            Emissive = 1,

            CustomA = 200,
            CustomB = 201,
            CustomC = 202
        }

        [Serializable]
        private struct ColorBlock
        {
            [ColorUsage(true, false)]
            public Color Main;

            [Header("Additional Colors")]

            [ColorUsage(true, true)]
            public Color Emissive;

            [Header("Custom Colors")]

            [ColorUsage(true, true)]
            public Color CustomA;

            [ColorUsage(true, true)]
            public Color CustomB;

            [ColorUsage(true, true)]
            public Color CustomC;

            public Color this[Channel inColor]
            {
                get
                {
                    switch (inColor)
                    {
                        case Channel.Main:
                            return Main;

                        case Channel.Emissive:
                            return Emissive;

                        case Channel.CustomA:
                            return CustomA;

                        case Channel.CustomB:
                            return CustomB;

                        case Channel.CustomC:
                            return CustomC;

                        default:
                            throw new ArgumentException("Invalid color id + " + inColor.ToString(), "inColor");
                    }
                }
                set
                {
                    switch (inColor)
                    {
                        case Channel.Main:
                            Main = value;
                            break;

                        case Channel.Emissive:
                            Emissive = value;
                            break;

                        case Channel.CustomA:
                            CustomA = value;
                            break;

                        case Channel.CustomB:
                            CustomB = value;
                            break;

                        case Channel.CustomC:
                            CustomC = value;
                            break;

                        default:
                            throw new ArgumentException("Invalid color id + " + inColor.ToString(), "inColor");
                    }
                }
            }

            public void Combine(ref ColorBlock inSrc, out ColorBlock outTarget)
            {
                outTarget.Main = Main * inSrc.Main;
                outTarget.Emissive = Emissive * inSrc.Emissive;
                outTarget.CustomA = CustomA * inSrc.CustomA;
                outTarget.CustomB = CustomB * inSrc.CustomB;
                outTarget.CustomC = CustomC * inSrc.CustomC;
            }

            static public ColorBlock Default
            {
                get
                {
                    return new ColorBlock()
                    {
                        Main = Color.white,
                            Emissive = Color.white,

                            CustomA = Color.white,
                            CustomB = Color.white,
                            CustomC = Color.white
                    };
                }
            }
        }

        [Serializable]
        private class PropertyConfig
        {
            public string Name;

            public Channel Source = Channel.Main;

            [ColorUsage(true, true)]
            public Color Multiplier = Color.white;

            public bool Enabled = true;

            [NonSerialized] private int m_PropertyId;

            public void ClearCache()
            {
                m_PropertyId = 0;
            }

            private int GetPropertyId()
            {
                if (m_PropertyId == 0)
                    m_PropertyId = Shader.PropertyToID(Name);

                return m_PropertyId;
            }

            public Color Retrieve(MaterialPropertyBlock inBlock)
            {
                Color c = inBlock.GetColor(GetPropertyId());
                c.r /= Multiplier.r;
                c.g /= Multiplier.g;
                c.b /= Multiplier.b;
                c.a /= Multiplier.a;
                return c;
            }

            public void Apply(MaterialPropertyBlock ioBlock, ref ColorBlock inColorBlock)
            {
                if (!Enabled)
                    return;

                ioBlock.SetColor(GetPropertyId(), inColorBlock[Source] * Multiplier);
            }

            static public PropertyConfig CreateDefault()
            {
                return Create(Channel.Main, "_Color");
            }

            static public PropertyConfig Create(Channel inChannel, string inPropertyName)
            {
                return new PropertyConfig()
                {
                    Source = inChannel,
                    Name = inPropertyName
                };
            }
        }

        [Serializable]
        private class MaterialConfig
        {
            public PropertyConfig MainProperty = PropertyConfig.CreateDefault();
            public PropertyConfig[] AdditionalProperties = null;

            public bool ShouldAppply()
            {
                if (MainProperty.Enabled)
                    return true;

                if (AdditionalProperties != null)
                {
                    int numProps = AdditionalProperties.Length;
                    for (int i = numProps - 1; i >= 0; --i)
                    {
                        if (AdditionalProperties[i].Enabled)
                            return true;
                    }
                }

                return false;
            }

            public void Apply(MaterialPropertyBlock ioBlock, ref ColorBlock inColorBlock)
            {
                MainProperty.Apply(ioBlock, ref inColorBlock);
                if (AdditionalProperties != null)
                {
                    int numProps = AdditionalProperties.Length;
                    for (int i = numProps - 1; i >= 0; --i)
                        AdditionalProperties[i].Apply(ioBlock, ref inColorBlock);
                }
            }

            public void ClearCache()
            {
                MainProperty.ClearCache();
                if (AdditionalProperties != null)
                {
                    int numProps = AdditionalProperties.Length;
                    for (int i = numProps - 1; i >= 0; --i)
                        AdditionalProperties[i].ClearCache();
                }
            }

            public void ConfigureForMaterial(Material inMaterial)
            {
                if (inMaterial.HasProperty("_BaseColor"))
                    MainProperty = PropertyConfig.Create(Channel.Main, "_BaseColor");
                else if (inMaterial.HasProperty("_MainColor"))
                    MainProperty = PropertyConfig.Create(Channel.Main, "_MainColor");
                else if (inMaterial.HasProperty("_Color"))
                    MainProperty = PropertyConfig.Create(Channel.Main, "_Color");

                List<PropertyConfig> newProperties = new List<PropertyConfig>();
                if (AdditionalProperties != null)
                    newProperties.AddRange(AdditionalProperties);

                if (inMaterial.HasProperty("_EmissionColor"))
                {
                    if ((inMaterial.globalIlluminationFlags & MaterialGlobalIlluminationFlags.AnyEmissive) != 0)
                        GenerateProperty(newProperties, Channel.Emissive, "_EmissionColor");
                }

                AdditionalProperties = newProperties.ToArray();
            }

            static private void GenerateProperty(List<PropertyConfig> ioPropertyList, Channel inChannel, string inName)
            {
                foreach (var prop in ioPropertyList)
                {
                    if (prop.Name == inName)
                        return;
                }

                PropertyConfig config = PropertyConfig.Create(inChannel, inName);
                ioPropertyList.Add(config);
            }
        }
    }
}