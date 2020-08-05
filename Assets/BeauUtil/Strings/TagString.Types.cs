/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    1 August 2020
 * 
 * File:    TagString.Runtime.cs
 * Purpose: Additional types for TagString.
 */

#if CSHARP_7_3_OR_NEWER
#define EXPANDED_REFS
#endif // CSHARP_7_3_OR_NEWER

using UnityEngine;

namespace BeauUtil
{
    public partial class TagString
    {
        #region Nodes

        /// <summary>
        /// Runtime node.
        /// </summary>
        public struct Node
        {
            /// <summary>
            /// Node type
            /// </summary>
            public NodeType Type;

            /// <summary>
            /// Text arguments.
            /// </summary>
            public TextData Text;

            /// <summary>
            /// Event arguments.
            /// </summary>
            public EventData Event;

            /// <summary>
            /// Resets node info.
            /// </summary>
            public void Reset()
            {
                Type = NodeType.Unassigned;
                Text.Reset();
                Event.Reset();
            }

            /// <summary>
            /// Creates a new text node.
            /// </summary>
            static public Node TextNode(uint inCharCount)
            {
                Node node = new Node();
                node.Type = NodeType.Text;
                node.Text.VisibleCharacterCount = inCharCount;
                return node;
            }

            /// <summary>
            /// Creates a new event node.
            /// </summary>
#if EXPANDED_REFS
            static public Node EventNode(in EventData inData)
#else
            static public Node EventNode(EventData inData)
#endif // EXPANDED_REFS
            {
                Node node = new Node();
                node.Type = NodeType.Event;
                node.Event = inData;
                return node;
            }
        }

        /// <summary>
        /// Arguments for a text node.
        /// </summary>
        public struct TextData
        {
            /// <summary>
            /// Number of visible characters to type out.
            /// </summary>
            public uint VisibleCharacterCount;

            /// <summary>
            /// Resets arguments.
            /// </summary>
            public void Reset()
            {
                VisibleCharacterCount = 0;
            }
        }

        /// <summary>
        /// Arguments for an event node.
        /// </summary>
        public struct EventData
        {
            /// <summary>
            /// Id of event type.
            /// </summary>
            public PropertyName Type;

            /// <summary>
            /// String argument.
            /// </summary>
            public string StringArgument;
            
            /// <summary>
            /// Number argument.
            /// </summary>
            public float NumberArgument;

            /// <summary>
            /// Bool argument.
            /// </summary>
            public bool BoolArgument
            {
                get { return NumberArgument > 0; }
                set { NumberArgument = value ? 1 : 0; }
            }

            /// <summary>
            /// Additional data, if required
            /// </summary>
            public INodeData AdditionalData;

            #region Constructors

            public EventData(PropertyName inType)
            {
                Type = inType;
                StringArgument = null;
                NumberArgument = 0;
                AdditionalData = null;  
            }

            public EventData(PropertyName inType, string inStringArg)
            {
                Type = inType;
                StringArgument = inStringArg;
                NumberArgument = 0;
                AdditionalData = null;  
            }

            public EventData(PropertyName inType, StringSlice inStringArg)
            {
                Type = inType;
                StringArgument = inStringArg.ToString();
                NumberArgument = 0;
                AdditionalData = null;  
            }

            public EventData(PropertyName inType, float inNumber)
            {
                Type = inType;
                StringArgument = null;
                NumberArgument = inNumber;
                AdditionalData = null;  
            }

            public EventData(PropertyName inType, INodeData inData)
            {
                Type = inType;
                StringArgument = null;
                NumberArgument = 0;
                AdditionalData = inData;  
            }

            #endregion // Constructors

            /// <summary>
            /// Resets arguments.
            /// </summary>
            public void Reset()
            {
                Type = default(PropertyName);
                StringArgument = null;
                NumberArgument = 0;
                AdditionalData = null;
            }
        }

        /// <summary>
        /// Type of node
        /// </summary>
        public enum NodeType : byte
        {
            // Unassigned node type
            Unassigned = 0,

            // Text node.
            Text = 1,

            // Event node.
            Event = 2,
        }

        /// <summary>
        /// Node data interface.
        /// </summary>
        public interface INodeData { }

        #endregion // Nodes

    }
}