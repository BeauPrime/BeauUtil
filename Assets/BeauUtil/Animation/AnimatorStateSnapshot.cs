/*
 * Copyright (C) 2023. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    25 July 2023
 * 
 * File:    AnimatorStateSnapshot.cs
 * Purpose: Animator parameter and layer state snapshot class.
 */

using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace BeauUtil
{
    /// <summary>
    /// Animator parameter and layer state snapshot.
    /// </summary>
    public sealed class AnimatorStateSnapshot
    {
        private struct ParamMetadata
        {
            public int NameHash;
            public AnimatorControllerParameterType Type;
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct ParamData
        {
            [FieldOffset(0)] public float Float;
            [FieldOffset(0)] public int Integer;
            [FieldOffset(0)] public bool Bool;
        }

        private struct LayerData
        {
            public int FullHash;
            public float NormalizedTime;
            public float Weight;
        }

        private RuntimeAnimatorController m_SourceController;

        private ParamMetadata[] m_ParamMeta = Array.Empty<ParamMetadata>();
        private ParamData[] m_ParamData = Array.Empty<ParamData>();
        private int m_ParamCount;

        private LayerData[] m_LayerData = Array.Empty<LayerData>();
        private int m_LayerCount;

        public AnimatorStateSnapshot() { }

        public AnimatorStateSnapshot(Animator inSource)
        {
            Read(inSource);
        }

        private void SetAnimController(Animator inSource)
        {
            if (inSource == null)
            {
                throw new ArgumentNullException("inSource");
            }

            RuntimeAnimatorController controller = inSource.runtimeAnimatorController;
            if (controller != m_SourceController)
            {
                m_SourceController = controller;

                int paramCount = inSource.parameterCount;
                int layerCount = inSource.layerCount;

                m_ParamCount = paramCount;
                m_LayerCount = layerCount;

                if (m_ParamMeta.Length < paramCount)
                {
                    Array.Resize(ref m_ParamMeta, paramCount);
                }

                for (int i = 0; i < paramCount; i++)
                {
                    AnimatorControllerParameter parameter = inSource.GetParameter(i);
                    m_ParamMeta[i] = new ParamMetadata()
                    {
                        NameHash = parameter.nameHash,
                        Type = parameter.type
                    };
                }

                if (m_ParamData.Length < paramCount)
                {
                    Array.Resize(ref m_ParamData, paramCount);
                }

                if (m_LayerData.Length < layerCount)
                {
                    Array.Resize(ref m_LayerData, layerCount);
                }
            }
        }

        /// <summary>
        /// Reads the parameter and layer state from the given animator into the snapshot.
        /// </summary>
        public void Read(Animator inSource)
        {
            SetAnimController(inSource);

            for (int i = 0, len = m_ParamCount; i < len; i++)
            {
                ParamMetadata meta = m_ParamMeta[i];
                ref ParamData data = ref m_ParamData[i];
                switch (meta.Type)
                {
                    case AnimatorControllerParameterType.Float:
                    {
                        data.Float = inSource.GetFloat(meta.NameHash);
                        break;
                    }
                    case AnimatorControllerParameterType.Int:
                    {
                        data.Integer = inSource.GetInteger(meta.NameHash);
                        break;
                    }
                    case AnimatorControllerParameterType.Bool:
                    {
                        data.Bool = inSource.GetBool(meta.NameHash);
                        break;
                    }
                }
            }

            for (int i = 0, len = m_LayerCount; i < len; i++)
            {
                AnimatorStateInfo state = inSource.GetCurrentAnimatorStateInfo(i);
                m_LayerData[i] = new LayerData()
                {
                    FullHash = state.fullPathHash,
                    NormalizedTime = state.normalizedTime,
                    Weight = inSource.GetLayerWeight(i)
                };
            }
        }

        /// <summary>
        /// Writes the recorded parameters and layer states to the give target animator.
        /// Target animator must have the same AnimationController as the source for Read()
        /// </summary>
        public void Write(Animator inTarget)
        {
            if (inTarget == null)
            {
                throw new ArgumentNullException("inTarget");
            }
            if (m_SourceController == null)
            {
                throw new InvalidOperationException("Read() must be called at least once before Write()");
            }
            if (inTarget.runtimeAnimatorController != m_SourceController)
            {
                throw new InvalidOperationException(string.Format("Target animator '{0}' does not have matching AnimationController '{1}'", inTarget.name, m_SourceController.name));
            }

            for (int i = 0, len = m_ParamCount; i < len; i++)
            {
                ParamMetadata meta = m_ParamMeta[i];
                ParamData data = m_ParamData[i];
                switch (meta.Type)
                {
                    case AnimatorControllerParameterType.Float:
                    {
                        inTarget.SetFloat(meta.NameHash, data.Float);
                        break;
                    }
                    case AnimatorControllerParameterType.Int:
                    {
                        inTarget.SetInteger(meta.NameHash, data.Integer);
                        break;
                    }
                    case AnimatorControllerParameterType.Bool:
                    {
                        inTarget.SetBool(meta.NameHash, data.Bool);
                        break;
                    }
                    case AnimatorControllerParameterType.Trigger:
                    {
                        inTarget.ResetTrigger(meta.NameHash);
                        break;
                    }
                }
            }

            for (int i = 0, len = m_LayerCount; i < len; i++)
            {
                LayerData data = m_LayerData[i];
                inTarget.SetLayerWeight(i, data.Weight);
                inTarget.Play(data.FullHash, i, data.NormalizedTime);
            }
        }

        /// <summary>
        /// Clears all recorded state.
        /// </summary>
        public void Clear()
        {
            m_ParamCount = 0;
            m_LayerCount = 0;
            m_SourceController = null;
        }
    }
}