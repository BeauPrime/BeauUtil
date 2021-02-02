/*
 * Copyright (C) 2017-2021. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    1 Feb 2021
 * 
 * File:    ScriptedDataBlockPackage.cs
 * Purpose: Package of data blocks.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;
using System;

namespace BeauUtil.Blocks
{
    /// <summary>
    /// Data block ScriptableObject.
    /// </summary>
    public abstract class ScriptableDataBlockPackage<TBlock> : CustomTextAsset, IDataBlockPackage<TBlock>
        where TBlock : class, IDataBlock
    {
        [NonSerialized] internal bool m_Parsed;

        #region Parse

        public void Parse<TPackage>(IBlockParsingRules inRules, IBlockGenerator<TBlock, TPackage> inGenerator, BlockMetaCache inCache = null)
            where TPackage : ScriptableDataBlockPackage<TBlock>
        {
            if (m_Parsed)
                return;

            TPackage self = (TPackage) this;
            BlockParser.Parse(ref self, name, Source(), inRules, inGenerator, inCache);
        }

        public IEnumerator ParseAsync<TPackage>(IBlockParsingRules inRules, IBlockGenerator<TBlock, TPackage> inGenerator, BlockMetaCache inCache = null)
            where TPackage : ScriptableDataBlockPackage<TBlock>
        {
            if (m_Parsed)
                return null;
            
            TPackage self = (TPackage) this;
            return BlockParser.ParseAsync(ref self, name, Source(), inRules, inGenerator, inCache);
        }

        #endregion // Parse

        #region IDataBlockPackage

        public abstract int Count { get; }
        public abstract IEnumerator<TBlock> GetEnumerator();
        
        public virtual void Clear()
        {
            m_Parsed = false;
        }
        
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        
        #endregion // IDataBlockPackage

        #region Generator

        public abstract class GeneratorBase<TPackage> : AbstractBlockGenerator<TBlock, TPackage>
            where TPackage : ScriptableDataBlockPackage<TBlock>
        {
            public override TPackage CreatePackage(string inFileName)
            {
                TPackage instance = ScriptableObject.CreateInstance<TPackage>();
                instance.name = inFileName;
                return instance;
            }

            public override void OnEnd(IBlockParserUtil inUtil, TPackage inPackage, bool inbError)
            {
                inPackage.m_Parsed = true;
            }
        }

        #endregion // Generator
    }
}