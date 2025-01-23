using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.UIElements;

using Profiling = BeauUtil.Debugger.Profiling;
using ProfileTimeUnits = BeauUtil.Debugger.ProfileTimeUnits;

namespace BeauUtil.UnitTests
{
    static public class MathTests
    {
        [Test]
        static public void CanDecomposeTRS()
        {
            int count = 1000;
            while (count-- > 0)
            {
                TRS trs = new TRS(RNG.Instance.NextVector3(), Quaternion.Euler(RNG.Instance.NextVector3(0, 360)), RNG.Instance.NextVector3(1, 92) * RNG.Instance.NextFloat(0, 1));
                Matrix4x4 mat = trs.Matrix;
                bool check = TRS.TryCreateFromMatrix(mat, out var newTRS);
                Assert.IsTrue(check, "could not decompose");
            }

        }

        [Test]
        static public void CanComputeQuaternionFastAxis()
        {
            int iterCount = ushort.MaxValue;

            Quaternion q;
            Vector3 v, v2;
            Vector3 cachedF = Vector3.forward;
            Vector3 cachedU = Vector3.up;
            Vector3 cachedR = Vector3.right;

            for (int i = 0; i < iterCount; i++)
            {
                q = Quaternion.Euler(i * 4, i * -54 + 30, i * 13 - 24);
                v = q * cachedF;
                v2 = Geom.Forward(q);
                Assert.IsTrue(v == v2, "Fast quaternion axis is not accurate");
            }

            using (Profiling.AvgTime("forward (mult)", iterCount, ProfileTimeUnits.Cycles))
            {
                for (int i = 0; i < iterCount; i++)
                {
                    q = Quaternion.Euler(i * 4, i * -7 + 30, i * 13 - 24);
                    v = q * cachedF;
                }
            }

            using (Profiling.AvgTime("forward (fast)", iterCount, ProfileTimeUnits.Cycles))
            {
                for (int i = 0; i < iterCount; i++)
                {
                    q = Quaternion.Euler(i * 4, i * -7 + 30, i * 13 - 24);
                    v = Geom.Forward(q);
                }
            }

            using (Profiling.AvgTime("up (mult)", iterCount, ProfileTimeUnits.Cycles))
            {
                for (int i = 0; i < iterCount; i++)
                {
                    q = Quaternion.Euler(i * 4, i * -7 + 30, i * 13 - 24);
                    v = q * cachedU;
                }
            }

            using (Profiling.AvgTime("up (fast)", iterCount, ProfileTimeUnits.Cycles))
            {
                for (int i = 0; i < iterCount; i++)
                {
                    q = Quaternion.Euler(i * 4, i * -7 + 30, i * 13 - 24);
                    v = Geom.Up(q);
                }
            }

            using (Profiling.AvgTime("right (mult)", iterCount, ProfileTimeUnits.Cycles))
            {
                for (int i = 0; i < iterCount; i++)
                {
                    q = Quaternion.Euler(i * 4, i * -7 + 30, i * 13 - 24);
                    v = q * cachedR;
                }
            }

            using (Profiling.AvgTime("right (fast)", iterCount, ProfileTimeUnits.Cycles))
            {
                for (int i = 0; i < iterCount; i++)
                {
                    q = Quaternion.Euler(i * 4, i * -7 + 30, i * 13 - 24);
                    v = Geom.Right(q);
                }
            }
        }

        [Test]
        static public void CanSetMatrixTranslation()
        {
            int iterCount = ushort.MaxValue;

            Quaternion rotation;
            Vector3 scale;
            Vector3 translation;

            rotation = Quaternion.identity;
            scale = new Vector3(0.23f, 0.23f, 0.23f);

            Matrix4x4 cachedOriginalMatrix = Matrix4x4.TRS(default(Vector3), rotation, scale);
            Matrix4x4 freshMatrix;

            for (int i = 0; i < iterCount; i++)
            {
                translation = new Vector3(i * 4, i * -7 + 30, i * 13 - 24);
                freshMatrix = Matrix4x4.TRS(translation, rotation, scale);
                Geom.SetTranslation(ref cachedOriginalMatrix, translation);
                Assert.IsTrue(freshMatrix == cachedOriginalMatrix, "Setting translation is not accurate");
            }

            using (Profiling.AvgTime("trs", iterCount, ProfileTimeUnits.Cycles))
            {
                for (int i = 0; i < iterCount; i++)
                {
                    translation = new Vector3(i * 4, i * -7 + 30, i * 13 - 24);
                    freshMatrix = Matrix4x4.TRS(translation, rotation, scale);
                }
            }

            using (Profiling.AvgTime("matrix mult", iterCount, ProfileTimeUnits.Cycles))
            {
                for (int i = 0; i < iterCount; i++)
                {
                    translation = new Vector3(i * 4, i * -7 + 30, i * 13 - 24);
                    freshMatrix = cachedOriginalMatrix * Matrix4x4.Translate(translation);
                }
            }

            using (Profiling.AvgTime("set translation", iterCount, ProfileTimeUnits.Cycles))
            {
                for (int i = 0; i < iterCount; i++)
                {
                    translation = new Vector3(i * 4, i * -7 + 30, i * 13 - 24);
                    Geom.SetTranslation(ref cachedOriginalMatrix, translation);
                }
            }

            using (Profiling.AvgTime("set translation (simple)", iterCount, ProfileTimeUnits.Cycles))
            {
                for (int i = 0; i < iterCount; i++)
                {
                    translation = new Vector3(i * 4, i * -7 + 30, i * 13 - 24);
                    cachedOriginalMatrix.m03 = translation.x;
                    cachedOriginalMatrix.m13 = translation.y;
                    cachedOriginalMatrix.m23 = translation.z;
                }
            }
        }

        [Test]
        static public void CanWrapValues()
        {
            Assert.AreEqual(-180, MathUtils.Wrap(180, -180, 180));
            Assert.AreEqual(18, MathUtils.Wrap(180, -1, 80));
        }
    }
}