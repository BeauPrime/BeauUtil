using System.Runtime.InteropServices;
using BeauUtil;
using UnityEngine;
using UnityEngine.Rendering;

public class MeshTest : MonoBehaviour
{
    public DynamicMeshFilter MeshFilter;
    public MeshData16<DefaultSpriteVertexFormat> MeshData;

    public int QuadCount;
    public float QuadDistance;
    public float QuadSize;
    public int FrameSkip = 3;

    public void Awake()
    {
        MeshData = new MeshData16<DefaultSpriteVertexFormat>(16);
        Application.targetFrameRate = 60;
    }

    public void Update()
    {
        if (FrameSkip > 1 && (Time.frameCount % FrameSkip) != 0)
            return;

        MeshData.Clear();

        for (int i = 0; i < QuadCount; i++)
        {
            DefaultSpriteVertexFormat a, b, c, d;
            a.Color = b.Color = c.Color = d.Color = new Color(RNG.Instance.NextFloat(), RNG.Instance.NextFloat(), RNG.Instance.NextFloat());
            a.UV = new Vector2(0, 0);
            b.UV = new Vector2(0, 1);
            c.UV = new Vector2(1, 0);
            d.UV = new Vector2(1, 1);
            Vector2 center = RNG.Instance.NextVector2(0, QuadDistance);
            Vector2 size = RNG.Instance.NextVector2(QuadSize / 2, QuadSize);
            a.Position = center + size;
            b.Position = center + new Vector2(size.x, -size.y);
            c.Position = center + new Vector2(-size.x, size.y);
            d.Position = center + new Vector2(-size.x, -size.y);
            MeshData.AddQuad(a, b, c, d);
        }

        MeshFilter.Upload(MeshData);
    }
}