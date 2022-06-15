using System;
using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;
using BeauUtil.Blocks;
using System.Collections.Generic;
using UnityEngine.Scripting;

namespace BeauUtil.Examples
{
    public class TextBlock : IDataBlock
    {
        private string m_Id;
        private Vector2 m_Position;
        private Color m_Color = UnityEngine.Color.white;
        private string m_Text;

        public TextBlock(string inId)
        {
            m_Id = inId;
        }

        public string Id() { return m_Id; }
        public Vector2 Position() { return m_Position; }
        public Color Color() { return m_Color; }
        public string Text() { return m_Text; }

        [BlockMeta("position"), Preserve]
        private void SetPosition(float inX, float inY)
        {
            m_Position = new Vector2(inX, inY);
        }

        [BlockMeta("color"), Preserve]
        private void SetColor(string inColorString)
        {
            m_Color = Colors.HTML(inColorString);
        }

        [BlockContent, Preserve]
        private void SetText(string inText)
        {
            m_Text = inText;
        }
    }
}