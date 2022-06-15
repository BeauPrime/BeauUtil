/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    23 August 2020
 * 
 * File:    TagNodeData.cs
 * Purpose: Node information.
 */

#if CSHARP_7_3_OR_NEWER
#define EXPANDED_REFS
#endif // CSHARP_7_3_OR_NEWER

namespace BeauUtil.Tags
{
    /// <summary>
    /// Runtime node.
    /// </summary>
    public struct TagNodeData
    {
        /// <summary>
        /// Node type
        /// </summary>
        public TagNodeType Type;

        /// <summary>
        /// Text arguments.
        /// </summary>
        public TagTextData Text;

        /// <summary>
        /// Event arguments.
        /// </summary>
        public TagEventData Event;

        /// <summary>
        /// Resets node info.
        /// </summary>
        public void Reset()
        {
            Type = TagNodeType.Unassigned;
            Text.Reset();
            Event.Reset();
        }

        /// <summary>
        /// Creates a new text node.
        /// </summary>
        static public TagNodeData TextNode(uint inCharOffset, uint inCharCount)
        {
            TagNodeData node = new TagNodeData();
            node.Type = TagNodeType.Text;
            node.Text.VisibleCharacterOffset = inCharOffset;
            node.Text.VisibleCharacterCount = inCharCount;
            return node;
        }

        /// <summary>
        /// Creates a new event node.
        /// </summary>
#if EXPANDED_REFS
        static public TagNodeData EventNode(in TagEventData inData)
#else
        static public TagNodeData EventNode(EventData inData)
#endif // EXPANDED_REFS
        {
            TagNodeData node = new TagNodeData();
            node.Type = TagNodeType.Event;
            node.Event = inData;
            return node;
        }
    }

    /// <summary>
    /// Type of node
    /// </summary>
    public enum TagNodeType : byte
    {
        // Unassigned node type
        Unassigned = 0,

        // Text node.
        Text = 1,

        // Event node.
        Event = 2,
    }

    /// <summary>
    /// Custom node data interface.
    /// </summary>
    public interface ICustomNodeData { }
}