#if UNITY_2021_2_OR_NEWER && !BEAUUTIL_DISABLE_FUNCTION_POINTERS
#define SUPPORTS_FUNCTION_POINTERS
#endif // UNITY_2021_2_OR_NEWER

using System;
using System.Text;
using NUnit.Framework;
using UnityEngine;
using BeauUtil.Graph;

using Log = BeauUtil.Debugger.Log;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEditor;
using System.Reflection;

namespace BeauUtil.UnitTests
{
    public unsafe class UnityTests
    {
        [Test]
        static public void CanMakeSceneReference()
        {
            new SceneReference(0);
            new SceneReference("Assets/Examples/BlockTest/BlockTest.unity");
            SceneReference.FromName("BlockTest");
            Assert.Throws<ArgumentException>(() =>
            {
                SceneReference.FromName("NonExistentScene");
            });
        }
    }
}