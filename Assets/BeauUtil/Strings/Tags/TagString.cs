/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    1 August 2020
 * 
 * File:    TagString.cs
 * Purpose: String containing display text and nodes dictating how that text should be displayed.
 */

#if CSHARP_7_3_OR_NEWER
#define EXPANDED_REFS
#endif // CSHARP_7_3_OR_NEWER

using System;

namespace BeauUtil.Tags
{
    public sealed partial class TagString : IDisposable, ICopyCloneable<TagString>
    {
        private const int InitialNodeCount = 4;

        private string m_RichText = string.Empty;
        private string m_StrippedText = string.Empty;
        private TagNodeData[] m_Nodes = null;
        private int m_NodeCount = 0;
        private int m_EventCount = 0;
        private ListSlice<TagNodeData> m_NodeList;

        #region Output Data

        /// <summary>
        /// Text string to be rendered.
        /// Includes rich text.
        /// </summary>
        public string RichText
        {
            get { return m_RichText; }
            set { m_RichText = value ?? string.Empty; }
        }

        /// <summary>
        /// Visible text string to be rendered.
        /// Contains no rich text.
        /// </summary>
        public string VisibleText
        {
            get { return m_StrippedText; }
            set { m_StrippedText = value ?? string.Empty; }
        }

        /// <summary>
        /// List of nodes generated from the processed string.
        /// </summary>
        public ListSlice<TagNodeData> Nodes { get { return m_NodeList; } }

        /// <summary>
        /// Number of generated event nodes.
        /// </summary>
        public int EventCount { get { return m_EventCount; } }

        #endregion // Output Data

        #region Operations

        /// <summary>
        /// Adds a node to the node list.
        /// </summary>
#if EXPANDED_REFS
        public void AddNode(in TagNodeData inNode)
#else
        public void AddNode(TagNodeData inNode)
#endif // EXPANDED_REFS
        {
            if (m_Nodes == null)
            {
                m_Nodes = new TagNodeData[InitialNodeCount];
            }
            else if (m_NodeCount >= m_Nodes.Length)
            {
                Array.Resize(ref m_Nodes, m_Nodes.Length * 2);
            }

            m_Nodes[m_NodeCount++] = inNode;
            m_NodeList = new ListSlice<TagNodeData>(m_Nodes, 0, m_NodeCount);

            if (inNode.Type == TagNodeType.Event)
                ++m_EventCount;
        }

        /// <summary>
        /// Adds text characters.
        /// </summary>
        public void AddText(uint inVisibleCharacterCount)
        {
            if (inVisibleCharacterCount == 0)
                return;

            if (m_Nodes == null)
            {
                m_Nodes = new TagNodeData[InitialNodeCount];
            }

            if (m_NodeCount == 0 || m_Nodes[m_NodeCount - 1].Type != TagNodeType.Text)
            {
                AddNode(TagNodeData.TextNode(inVisibleCharacterCount));
            }
            else
            {
                m_Nodes[m_NodeCount - 1].Text.VisibleCharacterCount += inVisibleCharacterCount;
            }
        }

        /// <summary>
        /// Adds a new Event node.
        /// </summary>
#if EXPANDED_REFS
        public void AddEvent(in TagEventData inEventData)
#else
        public void AddEvent(in TagEventData inEventData)
#endif // EXPANDED_REFS
        {
            AddNode(TagNodeData.EventNode(inEventData));
        }
        
        /// <summary>
        /// Clears all data.
        /// </summary>
        public void Clear()
        {
            m_RichText = string.Empty;
            m_StrippedText = string.Empty;

            if (m_Nodes != null)
            {
                Array.Clear(m_Nodes, 0, m_NodeCount);
                m_NodeCount = 0;
                m_EventCount = 0;
                m_NodeList = default(ListSlice<TagNodeData>);
            }
        }

        /// <summary>
        /// Attempts to locate an event with the given id.
        /// </summary>
        public bool TryFindEvent(StringHash32 inEventId, out TagEventData outEventData)
        {
            var nodes = Nodes;
            for(int i = 0; i < nodes.Length; i++)
            {
                TagNodeData node = nodes[i];
                if (node.Type == TagNodeType.Event && node.Event.Type == inEventId)
                {
                    outEventData = node.Event;
                    return true;
                }
            }

            outEventData = default;
            return false;
        }

        // Destroys node structure
        private void DestroyNodes()
        {
            if (m_Nodes != null)
            {
                Array.Clear(m_Nodes, 0, m_NodeCount);
                m_Nodes = null;
                m_NodeCount = 0;
                m_EventCount = 0;
                m_NodeList = default(ListSlice<TagNodeData>);
            }
        }

        #endregion // Operations

        #region IDisposable

        /// <summary>
        /// Disposes of all data in TagString.
        /// </summary>
        public void Dispose()
        {
            m_RichText = string.Empty;
            m_StrippedText = string.Empty;

            if (m_Nodes != null)
            {
                Array.Clear(m_Nodes, 0, m_NodeCount);
                m_Nodes = null;
                m_NodeCount = 0;
                m_EventCount = 0;
                m_NodeList = default(ListSlice<TagNodeData>);
            }
        }
    
        #endregion // IDisposable

        #region ICopyCloneable

        /// <summary>
        /// Clones this TagString.
        /// </summary>
        /// <returns></returns>
        public TagString Clone()
        {
            return CloneUtils.DefaultClone(this);
        }

        /// <summary>
        /// Copies data from another TagString.
        /// </summary>
        public void CopyFrom(TagString inClone)
        {
            m_RichText = inClone.m_RichText;
            m_StrippedText = inClone.m_StrippedText;

            if (inClone.m_Nodes == null)
            {
                DestroyNodes();
            }
            else
            {
                if (m_Nodes == null)
                    m_Nodes = new TagNodeData[inClone.m_Nodes.Length];
                else if (m_Nodes.Length < inClone.m_Nodes.Length)
                    Array.Resize(ref m_Nodes, inClone.m_Nodes.Length);

                m_NodeCount = inClone.m_NodeCount;
                Array.Copy(inClone.m_Nodes, m_Nodes, m_NodeCount);

                m_NodeList = new ListSlice<TagNodeData>(m_Nodes, 0, m_NodeCount);
            }
        }

        #endregion // ICopyCloneable
    }
}