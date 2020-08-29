using System;
using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;
using BeauUtil.Blocks;
using System.Collections.Generic;
using UnityEngine.UI;
using System.IO;

namespace BeauUtil.Examples
{
    public class TextBlockTest : MonoBehaviour
    {
        [EditModeOnly] public TextAsset[] toParse;

        private TextPackage m_Package;
        private List<GameObject> m_Spawned = new List<GameObject>();

        public void Start()
        {
            Reprocess();
        }

        public void Update()
        {
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.R))
                Reprocess();
        }

        private void Reprocess()
        {
            foreach(var obj in m_Spawned)
            {
                Destroy(obj);
            }

            m_Spawned.Clear();

            m_Package = new TextPackage("AllPackages");
            foreach(var asset in toParse)
            {
                TextPackage.Merge(asset, m_Package);
            }

            foreach(var node in m_Package)
            {
                GameObject nodeObj = new GameObject(node.Id());
                nodeObj.transform.position = node.Position();
                TextMesh nodeText = nodeObj.AddComponent<TextMesh>();
                nodeText.anchor = TextAnchor.MiddleCenter;
                nodeText.color = node.Color();
                nodeText.text = node.Text();
                m_Spawned.Add(nodeObj);
            }
        }
    }
}