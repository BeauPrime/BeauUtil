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

    public void Awake()
    {
        MeshData = new MeshData16<DefaultSpriteVertexFormat>(16);
    }

    public void Update()
    {
        MeshData.Clear();

        for (int i = 0; i < QuadCount; i++)
        {
            DefaultSpriteVertexFormat a, b, c, d;
            a.Color = b.Color = c.Color = d.Color = new Color(RNG.Instance.NextFloat(), RNG.Instance.NextFloat(), RNG.Instance.NextFloat());
            a.UV = new Vector2(0, 0);
            b.UV = new Vector2(0, 1);
            c.UV = new Vector2(1, 0);
            d.UV = new Vector2(1, 1);
            a.Position = RNG.Instance.NextVector2(QuadDistance / 2, QuadDistance);
            b.Position = new Vector2(a.Position.x, -a.Position.y);
            c.Position = new Vector2(-a.Position.x, a.Position.y);
            d.Position = new Vector2(-a.Position.x, -a.Position.y);
            MeshData.AddQuad(a, b, c, d);
        }

        MeshFilter.Upload(MeshData);
    }
}