using System;
using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;
using BeauUtil.Blocks;
using System.Collections.Generic;
using BeauUtil.Tags;
using System.Text;
using BeauUtil.Streaming;

namespace BeauUtil.Examples
{
    public class TextPackage : IDataBlockPackage<TextBlock>
    {
        private string m_Name;
        private Dictionary<string, TextBlock> m_Blocks = new Dictionary<string, TextBlock>();

        public TextPackage(string inName)
        {
            m_Name = inName;
        }

        public string Name() { return m_Name; }

        #region IReadOnlyCollection

        public int Count { get { return m_Blocks.Count; } }

        public IEnumerator<TextBlock> GetEnumerator()
        {
            return m_Blocks.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion // IReadOnlyCollection

        private class Generator : AbstractBlockGenerator<TextBlock, TextPackage>
        {
            static private readonly char[] SpaceSplit = new char[] { ' ' };

            public override TextPackage CreatePackage(string inFileName)
            {
                return new TextPackage(inFileName);
            }

            public override bool TryEvaluatePackage(IBlockParserUtil inUtil, TextPackage inPackage, TextBlock inCurrentBlock, TagData inMetadata, StringBuilder inLine)
            {
                if (inMetadata.Id == "print")
                {
                    Debug.LogFormat("[{0}] {1}", inUtil.Position, inMetadata.Data);
                    return true;
                }

                return base.TryEvaluatePackage(inUtil, inPackage, inCurrentBlock, inMetadata, inLine);
            }

            public override bool TryCreateBlock(IBlockParserUtil inUtil, TextPackage inPackage, TagData inId, out TextBlock outBlock)
            {
                string id = inId.Id.ToString();

                if (inPackage.m_Blocks.ContainsKey(id))
                {
                    outBlock = null;
                    return false;
                }

                outBlock = new TextBlock(id);
                inPackage.m_Blocks.Add(id, outBlock);
                return true;
            }

            public override void OnEnd(IBlockParserUtil inUtil, TextPackage inPackage, bool inbError)
            {
                Debug.LogFormat("[TextPackage] Parsing '{0}' complete, {1} nodes, {2}", inPackage.Name(), inPackage.Count, inbError ? "ERROR" : "NO ERRORS");
            }
        }

        static public TextPackage Parse(TextAsset inTextAsset)
        {
            return BlockParser.Parse(CharStreamParams.FromTextAsset(inTextAsset), BlockParsingRules.Default, new Generator());
        }

        static public void Merge(TextAsset inTextAsset, TextPackage ioTarget)
        {
            BlockParser.Parse(ref ioTarget, CharStreamParams.FromTextAsset(inTextAsset), BlockParsingRules.Default, new Generator());
        }
    }
}