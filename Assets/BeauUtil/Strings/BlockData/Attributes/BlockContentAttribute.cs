/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    29 August 2020
 * 
 * File:    BlockContentAttribute.cs
 * Purpose: Attribute marking block content for a data block.
 */

using System;

namespace BeauUtil.Blocks
{
    /// <summary>
    /// Attribute marking block content.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class BlockContentAttribute : Attribute
    {
        public BlockContentMode Mode { get; private set; }
        public char LineSeparator { get; private set; }

        public BlockContentAttribute(char inLineSeparator = '\n')
            : this(BlockContentMode.BatchContent, inLineSeparator)
        { }
        
        public BlockContentAttribute(BlockContentMode inMode, char inLineSeparator = '\n')
        {
            Mode = inMode;
            LineSeparator = inLineSeparator;
        }
    }

    /// <summary>
    /// Mode dictating the behavior of BlockContentAttribute.
    /// </summary>
    public enum BlockContentMode
    {
        /// <summary>
        /// Will accumulate all content lines before invoking the field/property/method.
        /// </summary>
        BatchContent,

        /// <summary>
        /// Will call the method for each line.
        /// Only works if the content attribute is attached to a method.
        /// </summary>
        LineByLine,
    }
}