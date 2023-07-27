﻿using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Internal;
using YVR.Utilities;

namespace YVR.Core
{
    /// <summary>
    /// Used to represent composite layer, which contains overlay / underlay
    /// </summary>
    public class YVRCompositeLayer : MonoBehaviour
    {
        private static readonly Dictionary<YVRRenderLayerType, ILayerShapeHandler> s_ShapeHandlerDic
            = new Dictionary<YVRRenderLayerType, ILayerShapeHandler>()
            {
                {YVRRenderLayerType.Quad, new QuadShapeHandler()},
                {YVRRenderLayerType.Cylinder, new CylinderShapeHandler()}
            };

        public Action onRegenerateHole;

        [SerializeField] private YVRRenderLayerType m_Shape = YVRRenderLayerType.Quad;

        public YVRRenderLayerType shape
        {
            get => m_Shape;
            set
            {
                if (m_Shape == value) return;
                m_Shape = value;
                RecreateLayer(depth);
            }
        }

        [SerializeField] private int m_CircleSegments = 10;

        public int circleSegments => m_CircleSegments;

        [Range(1, 180)] [SerializeField] private float m_Angle = 1;

        public float angle
        {
            get => m_Angle;
            set => m_Angle = Mathf.Clamp(value, 1f, 180f);
        }

        /// <summary>
        /// The displayed texture on composite layer
        /// </summary>
        public Texture texture = null;

        /// <summary>
        /// Composite layer depth.
        /// If depth less-than 0, the layer will work as underLayer, otherwise, the layer will works as overlay
        /// </summary>
        [SerializeField] protected int compositionDepth = -1;

        /// <summary>
        /// Render scale for composite layer resolution.
        /// While render scale is 1.0, composite layer resolution will equal to the resolution of [texture](xref: YVR.Core.YVRCompositeLayer.texture)
        /// </summary>
        [SerializeField] protected float renderScale = 1.0f;

        /// <summary>
        /// Render content is dynamic
        /// If the content you are rendering is dynamic, set this value to true, otherwise the rendered image will remain static even if the data content is updated.
        /// This is done to reduce the performance cost of static page rendering.
        /// </summary>
        [SerializeField] protected bool isDynamic = false;

        /// <summary>
        /// Should update composite layer texture to native automatically
        /// </summary>
        [SerializeField] protected bool autoUpdateContent = false;

        /// <summary>
        /// Should init native composite layer automatically
        /// </summary>
        [SerializeField] protected bool autoInitLayer = true;

        [SerializeField] protected bool requireToForceUpdateContent = false;

        private YVRManager m_YVRManager = null;
        private YVRManager yvrManager => m_YVRManager ??= YVRManager.instance;
        private Texture m_CachedTexture = null;
        protected IYVRLayerHandle layerHandler = null;
        private int m_TextureHandle = -1;
        private int m_TempCacheDepth;

        private bool m_IsShowing = false;

        public int depth => compositionDepth;

        /// <summary>
        /// ID of the texture
        /// </summary>
        protected virtual int textureHandle
        {
            get
            {
                if (m_CachedTexture == texture || texture == null) return m_TextureHandle;
                m_TextureHandle = (int) texture.GetNativeTexturePtr();
                m_CachedTexture = texture;
                return m_TextureHandle;
            }
        }

        public void SetTexture(Texture tex, int texID = -1)
        {
            texture = tex;
            m_CachedTexture = tex;
            m_TextureHandle = texID == -1 ? (int) texture.GetNativeTexturePtr() : texID;
        }

        /// <summary>
        /// The mask id of render layer
        /// </summary>
        public int renderLayerId { get; set; } = -1;

        private Transform m_Transform = null;

        /// <summary>
        /// The width of the actual texture used in the compositeLayer
        /// </summary>
        protected virtual int width => texture ? (int) (texture.width * renderScale) : 0;

        /// <summary>
        /// The height of the actual texture used in the compositeLayer
        /// </summary>
        protected virtual int height => texture ? (int) (texture.height * renderScale) : 0;

        protected int swapChainBufferCount => isDynamic ? 3 : 1;

        public Action<int> onLayerCreatedGfx = null;
        public Action onLayerDestroyedGfx = null;
        public Action onLayerAddedGfx = null;
        public Action onLayerRemovedGfx = null;
        public Action<bool, int> onLayerDepthSetGfx = null;

        private bool m_IsCreatingRT = false;
        private int m_DiscardCreatingLayerCount = 0;
        private object m_CreatingLayerLockObj = new object();

        [ExcludeFromDocs]
        protected void OnEnable() { Show(); }

        private void Awake()
        {
            m_Transform = transform;

#if UNITY_ANDROID && !UNITY_EDITOR
            layerHandler = new YVRLayerAndroidHandler();
#else
            layerHandler = new YVRLayerEditorHandle();
#endif
        }

        private void Start()
        {
            if (autoInitLayer)
                InitCompositeLayer(compositionDepth);
        }

        protected virtual void LateUpdate()
        {
            if (!requireToForceUpdateContent && (!isDynamic || !autoUpdateContent)) return;
            UpdateCompositeLayerContent();
        }

        /// <summary>
        /// Init native composite layer, register composite layer update operations.
        /// </summary>
        /// <param name="depth">The depth of the composite layer</param>
        public void InitCompositeLayer(int depth = int.MinValue)
        {
            if (depth != int.MinValue) compositionDepth = depth;

            if (renderLayerId != -1) return;

            lock (m_CreatingLayerLockObj)
            {
                // In case of the previous layer is creating, another creating layer request is coming
                // In these case, we need to discard the previous layer while it is created
                if (m_IsCreatingRT) m_DiscardCreatingLayerCount++;
                m_IsCreatingRT = true;
            }

            var layerCreateInfo
                = new YVRLayerCreateInfo(compositionDepth, width, height, swapChainBufferCount, m_Shape,
                                         isActiveAndEnabled);
            layerHandler.CreateLayerAsync(layerCreateInfo, (layerId) =>
            {
                lock (m_CreatingLayerLockObj)
                {
                    if (m_DiscardCreatingLayerCount != 0)
                    {
                        layerHandler.DestroyLayerAsync(layerId, true);
                        m_DiscardCreatingLayerCount--;
                    }
                    else
                    {
                        OnLayerCreatedGfx(layerId);
                        OnLayerAddedGfx();
                        if (!isDynamic)
                            requireToForceUpdateContent = true;
                    }

                    m_IsCreatingRT = false;
                }
            });
        }

        /// <summary>
        /// Show the composite layer
        /// </summary>
        protected void Show()
        {
            if (renderLayerId != -1)
                layerHandler.AddLayerAsync(renderLayerId, OnLayerAddedGfx);
        }

        /// <summary>
        /// Hide the composite layer
        /// </summary>
        protected void Hide()
        {
            if (renderLayerId != -1)
                layerHandler.RemoveLayerAsync(renderLayerId, OnLayerRemovedGfx);
        }


        public void RecreateLayer(int depth)
        {
            if (renderLayerId != -1)
            {
                layerHandler.DestroyLayerAsync(renderLayerId, false);
                renderLayerId = -1;
            }

            InitCompositeLayer(depth);
        }

        /// <summary>
        /// Set the render depth of the composite layer
        /// </summary>
        /// <param name="depth">The new render depth</param>
        public void SetLayerDepth(int depth)
        {
            m_TempCacheDepth = depth;
            if (renderLayerId != -1)
                layerHandler.SetLayerDepthAsync(renderLayerId, depth, OnLayerDepthSetGfx);
        }

        protected virtual void OnLayerCreatedGfx(int layerId)
        {
            renderLayerId = layerId;
            onLayerCreatedGfx?.Invoke(layerId);
        }

        protected virtual void OnLayerDestroyedGfx()
        {
            renderLayerId = -1;
            onLayerDestroyedGfx?.Invoke();
        }

        protected virtual void OnLayerAddedGfx()
        {
            m_IsShowing = true;
            onLayerAddedGfx?.Invoke();

            AddUpdateLayerPoseAction();
        }

        private void AddUpdateLayerPoseAction()
        {
#if UNITY_INPUT_SYSTEM
            Application.onBeforeRender -= UpdateLayerPose;
            Application.onBeforeRender += UpdateLayerPose;
#else
            yvrManager.cameraRig.afterRigBeforeRenderUpdated -= UpdateLayerPose;
            yvrManager.cameraRig.afterRigBeforeRenderUpdated += UpdateLayerPose;
#endif
        }

        private void RemoveUpdateLayerPoseAction()
        {
#if UNITY_INPUT_SYSTEM
            Application.onBeforeRender -= UpdateLayerPose;
#else
            yvrManager.cameraRig.afterRigBeforeRenderUpdated -= UpdateLayerPose;
#endif
        }

        protected virtual void OnLayerDepthSetGfx(bool result)
        {
            if (result) compositionDepth = m_TempCacheDepth;
            onLayerDepthSetGfx?.Invoke(result, compositionDepth);
        }

        protected virtual void OnLayerRemovedGfx()
        {
            m_IsShowing = false;
            onLayerRemovedGfx?.Invoke();

            RemoveUpdateLayerPoseAction();
        }


        private void UpdateLayerPose()
        {
            // In some cases, this game object has been destroyed, however layer has not been removed (due to the gfx thread delay, about 1 or 2 frame).
            // In these cases, this function has not been unRegistered from afterRigBeforeRenderUpdate.
            // In these cases, when this function be called, the real native data in m_Transform may has been released. Thus, we manually set m_Transform to be null, and do null check here to avoid null reference.
            if (m_Transform == null) return;
            if (renderLayerId == -1) return;

            ILayerShapeHandler layerShapeHandler = GetLayerShapeHandler();
            layerShapeHandler?.HandleLayerPose(layerHandler, renderLayerId, m_Transform, yvrManager, m_Angle);

            UpdateLayerShape();
        }

        private void UpdateLayerShape()
        {
            if (m_Transform == null) return;
            if (renderLayerId == -1) return;

            ILayerShapeHandler layerShapeHandler = GetLayerShapeHandler();
            layerShapeHandler?.HandleLayerShape(layerHandler, renderLayerId, m_Transform, m_Angle);
        }

        private ILayerShapeHandler GetLayerShapeHandler()
        {
            if (!s_ShapeHandlerDic.ContainsKey(m_Shape))
            {
                this.Error("Unsupport Layer RenderLayerType! RenderLayerType:" + m_Shape);
                return null;
            }

            return s_ShapeHandlerDic[m_Shape];
        }

        public virtual void UpdateCompositeLayerContent()
        {
            if (!Application.isEditor && (renderLayerId == -1 || textureHandle == -1 || !m_IsShowing)) return;

            GfxHelper.instance.TriggerGfxThreadCallback(() =>
            {
                layerHandler.SwapBufferLayer(renderLayerId);
                int destTextureId = layerHandler.GetLayerColorHandle(renderLayerId, -1);
                GfxHelper.instance.CopyTexture(textureHandle, destTextureId);
                requireToForceUpdateContent = false;
            });
        }

        [ExcludeFromDocs]
        protected void OnDisable() { Hide(); }

        [ExcludeFromDocs]
        protected virtual void OnDestroy()
        {
            m_Transform = null;
            layerHandler.DestroyLayerAsync(renderLayerId, true, OnLayerDestroyedGfx);
        }
    }
}