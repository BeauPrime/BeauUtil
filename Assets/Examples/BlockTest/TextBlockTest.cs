using System;
using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;
using BeauUtil.Blocks;
using System.Collections.Generic;
using UnityEngine.UI;
using System.IO;
using BeauUtil.Debugger;
using BeauUtil.Streaming;
using UnityEngine.Scripting;
using AOT;

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

            MethodInvocationHelper helper = default;
            helper.TryLoad(typeof(TextBlockTest).GetMethod("SomeTestFunc"), DefaultStringConverter.Instance);
            helper.TryInvoke(null, null, DefaultStringConverter.Instance, null, out NonBoxedValue _);

            MethodInvocationHelper helper2 = default;
            helper2.TryLoad(typeof(TextBlockTest).GetMethod("SomeTestFunc2"), DefaultStringConverter.Instance);
            helper2.TryInvoke(null, null, DefaultStringConverter.Instance, null, out NonBoxedValue _);
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

        [Preserve]
        static public void SomeTestFunc() {
            Log.Msg("yay i was called");
        }

        [Preserve, MonoPInvokeCallback(typeof(Action))]
        static public void SomeTestFunc2() {
            Log.Msg("yay i was also called");
        }
    }
}