/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    4 April 2019
 * 
 * File:    RaycastZone.cs
 * Purpose: Non-drawing graphic with optional debug rendering.
*/

#if (UNITY_EDITOR && !IGNORE_UNITY_EDITOR) || DEVELOPMENT_BUILD
#define DEVELOPMENT
#endif

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BeauUtil.UI
{
    /// <summary>
    /// Accepts raycasts but does not render.
    /// Used to create invisible click zones.
    /// </summary>
    [AddComponentMenu("BeauUtil/UI/Raycast Zone")]
    public class RaycastZone : Graphic
    {
#if DEVELOPMENT

        protected override void OnEnable()
        {
            base.OnEnable();

            if (!Application.isPlaying)
                return;
            s_RaycastZones.Add(this);

            canvasRenderer.materialCount = 1;
            canvasRenderer.SetMaterial(Graphic.defaultGraphicMaterial, 0);
            canvasRenderer.SetTexture(Texture2D.whiteTexture);
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            if (!Application.isPlaying)
                return;

            s_RaycastZones.Remove(this);
        }

        public override void SetMaterialDirty() { return; }
        public override void SetVerticesDirty() { if (!s_DebugRender) return; base.SetVerticesDirty(); }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();

            if (!s_DebugRender)
            {
                return;
            }

            var r = GetPixelAdjustedRect();
            var v = new Vector4(r.x, r.y, r.x + r.width, r.y + r.height);

            Color32 color32 = color;
            color32.a /= 2;
            vh.AddVert(new Vector3(v.x, v.y), color32, new Vector2(0f, 0f));
            vh.AddVert(new Vector3(v.x, v.w), color32, new Vector2(0f, 1f));
            vh.AddVert(new Vector3(v.z, v.w), color32, new Vector2(1f, 1f));
            vh.AddVert(new Vector3(v.z, v.y), color32, new Vector2(1f, 0f));

            vh.AddTriangle(0, 1, 2);
            vh.AddTriangle(2, 3, 0);
        }

        protected override void UpdateGeometry()
        {
            base.UpdateGeometry();
        }

        static private void TriggerRebuild()
        {
            for (int i = s_RaycastZones.Count - 1; i >= 0; --i)
                s_RaycastZones[i].SetVerticesDirty();
        }

        static private List<RaycastZone> s_RaycastZones = new List<RaycastZone>(64);
        static private bool s_DebugRender;

        static public void DebugRender()
        {
            s_DebugRender = true;
            TriggerRebuild();
        }

        static public void DisableDebug()
        {
            TriggerRebuild();
            s_DebugRender = false;
        }

        static public void ToggleDebug()
        {
            if (s_DebugRender)
                DisableDebug();
            else
                DebugRender();
        }

#else

        public override void SetMaterialDirty() { return; }
        public override void SetVerticesDirty() { return; }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
        }

        protected override void UpdateGeometry()
        {
        }

        static public void DebugRender() {}
        static public void DisableDebug() {}
        static public void ToggleDebug() {}

#endif
    }
}
