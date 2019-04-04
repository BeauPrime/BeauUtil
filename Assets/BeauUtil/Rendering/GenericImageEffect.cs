using System;
using UnityEngine;

namespace BeauUtil
{
    /// <summary>
    /// Generic implementation of a camera image effect.
    /// </summary>
    [ExecuteInEditMode]
    [RequireComponent(typeof(Camera))]
    [AddComponentMenu("BeauUtil/Rendering/Generic Image Effect")]
    public class GenericImageEffect : MonoBehaviour
    {
        #region Inspector

        [SerializeField]
        protected Material m_Material;

        #endregion

        [NonSerialized]
        private Camera m_Camera;
        [NonSerialized]
        private bool m_RenderWithMaterial;
        [NonSerialized]
        protected RenderTexture m_RenderTexture;

        protected Action m_PrepareMaterial;

        public Material Material
        {
            get { return m_Material; }
            set
            {
                m_Material = value;
                m_RenderWithMaterial = m_Camera && m_Material && m_Material.shader.isSupported;
            }
        }

        protected virtual void Awake()
        {
            UpdateRenderState();
        }

        #if UNITY_EDITOR
        protected virtual void OnValidate()
        {
            UpdateRenderState();
        }
        #endif

        protected virtual Vector2Int GetRenderTextureSize(Camera inCamera)
        {
            return new Vector2Int(inCamera.pixelWidth, inCamera.pixelHeight);
        }

        private void OnPreRender()
        {
            if (!m_RenderWithMaterial)
                return;

            Vector2Int renderSize = GetRenderTextureSize(m_Camera);
            m_RenderTexture = RenderTexture.GetTemporary(renderSize.x, renderSize.y, 24);
            m_Camera.targetTexture = m_RenderTexture;

            if (m_PrepareMaterial != null)
                m_PrepareMaterial();
        }

        private void OnPostRender()
        {
            if (!m_RenderWithMaterial)
                return;

            m_Camera.targetTexture = null;

            if (m_PrepareMaterial != null)
                m_PrepareMaterial();

            Graphics.Blit(m_RenderTexture, (RenderTexture) null, m_Material);

            RenderTexture.ReleaseTemporary(m_RenderTexture);
            m_RenderTexture = null;
        }

        private void UpdateRenderState()
        {
            m_Camera = GetComponent<Camera>();
            m_RenderWithMaterial = m_Camera && m_Material && m_Material.shader.isSupported;
        }
    }
}