using System.Diagnostics;
using System.Runtime.InteropServices;
using BeauUtil;
using BeauUtil.Debugger;
using UnityEngine;
using UnityEngine.Rendering;

public class MeshTest : MonoBehaviour
{
    public DynamicMeshFilter MeshFilter;
    public MeshData32<VertexP3C1> MeshData;

    public int QuadCount;
    public float QuadDistance;
    public float QuadSize;
    public int FrameSkip = 3;
    public ProfileTimeUnits ProfileTime;

    public void Awake()
    {
        MeshData = new MeshData32<VertexP3C1>(16, MeshTopology.Triangles);
        Application.targetFrameRate = 60;
    }

    public void Update()
    {
        if (FrameSkip > 1 && (Time.frameCount % FrameSkip) != 0)
            return;

        MeshData.Clear();

        //using (Profiling.Time("mesh generation", ProfileTime))
        {
            for (int i = 0; i < QuadCount; i++)
            {
                VertexP3C1 a, b, c, d;
                a.Color = b.Color = c.Color = d.Color = new Color(RNG.Instance.NextFloat(), RNG.Instance.NextFloat(), RNG.Instance.NextFloat());
                Vector2 center = RNG.Instance.NextVector2(0, QuadDistance);
                Vector2 size = RNG.Instance.NextVector2(QuadSize / 2, QuadSize);
                a.Position = center + size;
                b.Position = center + new Vector2(size.x, -size.y);
                c.Position = center + new Vector2(-size.x, size.y);
                d.Position = center + new Vector2(-size.x, -size.y);
                Matrix4x4 rot = Matrix4x4.Rotate(Quaternion.Euler(RNG.Instance.NextVector3(16, 32)));
                var block = MeshData.AddQuad(a, b, c, d);
                //using (Profiling.Time("rotate mesh", ProfileTimeUnits.Microseconds))
                {
                    MeshData.Transform(block, rot);
                }
            }
        }

        using (Profiling.Time("mesh upload", ProfileTime))
        {
            MeshFilter.Upload(MeshData, MeshDataUploadFlags.DontRecalculateBounds);
        }
    }
}