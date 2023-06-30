/*
 * Copyright (C) 2023. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    23 June 2023
 * 
 * File:    LLIndices.cs
 * Purpose: Doubly-linked list indices.
*/

using System;
using System.Runtime.CompilerServices;

namespace BeauUtil
{
    /// <summary>
    /// Indexes for a doubly-linked list.
    /// </summary>
    public struct LLNode : IEquatable<LLNode>
    {
        /// <summary>
        /// Next index.
        /// </summary>
        public int Next;

        /// <summary>
        /// Previous index.
        /// </summary>
        public int Prev;

        public LLNode(int inPrev, int inNext)
        {
            Next = inNext;
            Prev = inPrev;
        }

        /// <summary>
        /// Returns if the prev index is invalid.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsHead()
        {
            return Prev < 0;
        }

        /// <summary>
        /// Returns if the next index is invalid.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsTail()
        {
            return Next < 0;
        }

        /// <summary>
        /// Clears pointers.
        /// </summary>
        public void Clear()
        {
            Prev = Next = -1;
        }

        /// <summary>
        /// Invalid indices.
        /// </summary>
        static public readonly LLNode Invalid = new LLNode(-1, -1);

        #region Overrides

        public bool Equals(LLNode other)
        {
            return Next == other.Next && Prev == other.Prev;
        }

        public override int GetHashCode()
        {
            return (Next.GetHashCode() * 17) ^ Prev.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj is LLNode)
                return Equals((LLNode) obj);
            return false;
        }

        public override string ToString()
        {
            if (Next < 0 && Prev < 0)
                return "[INVALID]";

            if (Next < 0)
                return string.Format("[{0} <- TAIL]", Prev);

            if (Prev < 0)
                return string.Format("[HEAD -> {0}]", Next);

            return string.Format("[{0} <-> {1}]", Prev, Next);
        }

        #endregion // Overrides
    }

    /// <summary>
    /// Indexes for a doubly-linked list.
    /// </summary>
    public struct LLNode<TTag> : IEquatable<LLNode<TTag>>
        where TTag : struct
    {
        /// <summary>
        /// Next index.
        /// </summary>
        public int Next;

        /// <summary>
        /// Previous index.
        /// </summary>
        public int Prev;

        /// <summary>
        /// Tagged value.
        /// </summary>
        public TTag Tag;

        public LLNode(int inPrev, int inNext, in TTag inTag)
        {
            Next = inNext;
            Prev = inPrev;
            Tag = inTag;
        }

        /// <summary>
        /// Returns if the prev index is invalid.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsHead()
        {
            return Prev < 0;
        }

        /// <summary>
        /// Returns if the next index is invalid.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsTail()
        {
            return Next < 0;
        }

        /// <summary>
        /// Clears pointers and tagged data.
        /// </summary>
        public void Clear()
        {
            Prev = Next = -1;
            Tag = default(TTag);
        }

        /// <summary>
        /// Invalid indices.
        /// </summary>
        static public readonly LLNode<TTag> Invalid = new LLNode<TTag>(-1, -1, default(TTag));

        #region Overrides

        public bool Equals(LLNode<TTag> other)
        {
            return Next == other.Next && Prev == other.Prev && CompareUtils.DefaultEquals<TTag>().Equals(Tag, other.Tag);
        }

        public override int GetHashCode()
        {
            return (Next.GetHashCode() * 17) ^ Prev.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj is LLNode<TTag>)
                return Equals((LLNode<TTag>) obj);
            return false;
        }

        public override string ToString()
        {
            if (Next < 0 && Prev < 0)
                return "[INVALID]";

            if (Next < 0)
                return string.Format("[{0} <- {1} -> TAIL]", Prev, Tag);

            if (Prev < 0)
                return string.Format("[HEAD <- {0} -> {1}]", Tag, Next);

            return string.Format("[{0} <- {1} -> {2}]", Prev, Tag, Next);
        }

        #endregion // Overrides
    }

    /// <summary>
    /// Tagged index in a tagged linked list table.
    /// </summary>
    public struct LLTaggedIndex<TTag>
        where TTag : struct
    {
        public int Index;
        public TTag Tag;

        public LLTaggedIndex(int inIndex, TTag inTag)
        {
            Tag = inTag;
            Index = inIndex;
        }

        /// <summary>
        /// Invalid tagged index.
        /// </summary>
        static public readonly LLTaggedIndex<TTag> Invalid = new LLTaggedIndex<TTag>(-1, default(TTag));

        static public implicit operator int(LLTaggedIndex<TTag> inTag)
        {
            return inTag.Index;
        }
    }

    /// <summary>
    /// Linked index list data.
    /// </summary>
    public struct LLIndexList
    {
        /// <summary>
        /// Head of the list.
        /// </summary>
        public int Head;

        /// <summary>
        /// Tail index of the list.
        /// </summary>
        public int Tail;

        /// <summary>
        /// Cached length of the list.
        /// </summary>
        public int Length;

        /// <summary>
        /// Empty linked list.
        /// </summary>
        static public readonly LLIndexList Empty = new LLIndexList()
        {
            Head = -1,
            Tail = -1,
            Length = 0
        };

        /// <summary>
        /// Fixes up pointers for empty lists.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static internal void Fixup(ref LLIndexList ioList)
        {
            if (ioList.Length <= 0)
            {
                ioList.Length = 0;
                ioList.Head = ioList.Tail = -1;
            }
        }
    }
}