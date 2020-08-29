using System;
using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;
using BeauUtil.Blocks;
using System.Collections.Generic;

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

        public void SetPosition(Vector2 inPosition)
        {
            m_Position = inPosition;
        }

        public void SetColor(Color inColor)
        {
            m_Color = inColor;
        }

        public void AddText(string inText)
        {
            if (string.IsNullOrEmpty(m_Text))
                m_Text = inText;
            else
                m_Text += "\n" + inText.Replace("\\n", "\n");
        }
    }
}