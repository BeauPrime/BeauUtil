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
using System.Text;

namespace BeauUtil.Tags
{
    public sealed partial class TagString : IDisposable, ICopyCloneable<TagString>
    {
        private const int InitialNodeCount = 4;
        private const int DefaultCapacity = 1024;

        private StringBuilder m_RichText;
        private StringBuilder m_StrippedText;
        private TagNodeData[] m_Nodes = null;
        private int m_NodeCount = 0;
        private int m_EventCount = 0;
        private ListSlice<TagNodeData> m_NodeList;
        private StringArena m_TagArena;

        private string m_CachedRichText;
        private string m_CachedStrippedText;

        public TagString() : this(DefaultCapacity)
        {
        }

        public TagString(int inTextCapacity)
        {
            m_RichText = new StringBuilder(inTextCapacity);
            m_StrippedText = new StringBuilder(inTextCapacity);
            m_TagArena = new StringArena(inTextCapacity);
        }

        #region Output Data

        /// <summary>
        /// Text string to be rendered.
        /// Includes rich text.
        /// </summary>
        public string RichTextString
        {
            get { return m_CachedRichText ?? (m_CachedRichText = m_RichText.ToString()); }
            set { m_RichText.Length = 0; m_RichText.Append(value); m_CachedRichText = value ?? string.Empty; }
        }

        /// <summary>
        /// Text string to be rendered.
        /// Includes rich text.
        /// </summary>
        public StringBuilder RichText
        {
            get { return m_RichText; }
        }

        /// <summary>
        /// Visible text string to be rendered.
        /// Contains no rich text.
        /// </summary>
        public string VisibleTextString
        {
            get { return m_CachedStrippedText ?? (m_CachedStrippedText = m_StrippedText.ToString()); }
            set { m_StrippedText.Length = 0; m_StrippedText.Append(value); m_CachedStrippedText = value ?? string.Empty; }
        }

        /// <summary>
        /// Visible text string to be rendered.
        /// Contains no rich text.
        /// </summary>
        public StringBuilder VisibleText
        {
            get { return m_StrippedText; }
        }

        /// <summary>
        /// List of nodes generated from the processed string.
        /// </summary>
        public ListSlice<TagNodeData> Nodes { get { return m_NodeList; } }

        /// <summary>
        /// Number of generated event nodes.
        /// </summary>
        public int EventCount { get { return m_EventCount; } }

        /// <summary>
        /// Arena containing tag data.
        /// </summary>
        public StringArena TagAllocator { get { return m_TagArena; } }

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
        public void AddText(ushort inVisibleCharacterOffset, ushort inVisibleCharacterCount, ushort inRichCharacterOffset, ushort inRichCharacterCount)
        {
            if (inVisibleCharacterCount == 0 && inRichCharacterCount == 0)
                return;

            if (m_Nodes == null)
            {
                m_Nodes = new TagNodeData[InitialNodeCount];
            }

            TagNodeData last = m_NodeCount > 0 ? m_Nodes[m_NodeCount - 1] : default(TagNodeData);
            if (m_NodeCount == 0 || last.Type != TagNodeType.Text)
            {
                AddNode(TagNodeData.TextNode(inVisibleCharacterOffset, inVisibleCharacterCount, inRichCharacterOffset, inRichCharacterCount));
            }
            else
            {
                last.Text.VisibleCharacterCount += inVisibleCharacterCount;
                last.Text.RichCharacterCount += inRichCharacterCount;
                m_Nodes[m_NodeCount - 1] = last;
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
            m_RichText.Length = 0;
            m_StrippedText.Length = 0;
            m_CachedRichText = m_CachedStrippedText = null;
            m_TagArena.Reset();

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
            m_RichText = null;
            m_StrippedText = null;
            m_TagArena = null;

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
            m_RichText.Length = 0;
            m_RichText.Append(inClone.m_RichText);

            m_StrippedText.Length = 0;
            m_StrippedText.Append(inClone.m_StrippedText);

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