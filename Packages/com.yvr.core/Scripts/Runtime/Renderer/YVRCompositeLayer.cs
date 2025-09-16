using System;
using System.Collections.Generic;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Internal;
using UnityEngine.Rendering;
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
                {YVRRenderLayerType.Cylinder, new CylinderShapeHandler()},
                {YVRRenderLayerType.Equirect, new Equirect2ShapeHandler()},
            };

        public Action onRegenerateHole;

        [SerializeField] protected YVRRenderLayerType m_Shape = YVRRenderLayerType.Quad;

        public YVRRenderLayerType shape
        {
            get => m_Shape;
            set
            {
                if (m_Shape == value) return;
                m_Shape = value;
                RecreateLayer();
            }
        }

        [SerializeField] protected int m_CircleSegments = 10;

        public int circleSegments => m_CircleSegments;

        [Range(0.01f, 180f)][SerializeField] protected float m_CylinderAngle = 1;

        public float cylinderAngle
        {
            get => m_CylinderAngle;
            set => m_CylinderAngle = Mathf.Clamp(value, 0.01f, 180f);
        }

        [SerializeField] protected float m_Radius = 0f;
        public float radius
        {
            get => m_Radius;
            set => m_Radius = value;
        }

        [SerializeField] protected YVRQualityManager.LayerSettingsType m_SuperSamplingType = YVRQualityManager.LayerSettingsType.None;

        public YVRQualityManager.LayerSettingsType SuperSamplingType
        {
            get => m_SuperSamplingType;
            set
            {
                if (m_SuperSamplingType == value) return;
                m_SuperSamplingType = value;
                ApplyLayerSettings();
            }
        }

        [SerializeField] protected YVRQualityManager.LayerSettingsType m_SharpenType = YVRQualityManager.LayerSettingsType.None;

        public YVRQualityManager.LayerSettingsType SharpenType
        {
            get => m_SharpenType;
            set
            {
                if (m_SharpenType == value) return;
                m_SharpenType = value;
                ApplyLayerSettings();
            }
        }

        public bool isExternalTexture = false;

        /// <summary>
        /// The displayed texture on composite layer
        /// </summary>
        public Texture texture = null;

        [SerializeField] protected Rect sourceRectLeft = new Rect(0f, 0f, 1f, 1f);
        public Rect SourceRectLeft
        {
            get
            {
                sourceRectLeft = ResizeRect(sourceRectLeft);
                return sourceRectLeft;
            }
            set
            {
                value = ResizeRect(value);
                if (sourceRectLeft != value)
                {
                    bool separateLayerBefore = separateLayer;
                    sourceRectLeft = value;
                    if (separateLayer != separateLayerBefore)
                    {
                        RecreateLayer(depth, separateLayerBefore);
                    }
                }
            }
        }

        [SerializeField] protected Rect destRectLeft = new Rect(0f, 0f, 1f, 1f);
        public Rect DestRectLeft
        {
            get
            {
                destRectLeft = ResizeRect(destRectLeft);
                return destRectLeft;
            }
            set
            {
                value = ResizeRect(value);
                if(destRectLeft != value)
                {
                    bool separateLayerBefore = separateLayer;
                    destRectLeft = value;
                    if (separateLayer != separateLayerBefore)
                    {
                        RecreateLayer(depth, separateLayerBefore);
                    }
                }
            }
        }

        /// <summary>
        /// The displayed texture on right eye composite layer, textureLeft will be used for both eye;
        /// </summary>
        public Texture rightEyeTexture = null;

        [SerializeField] protected Rect sourceRectRight = new Rect(0f, 0f, 1f, 1f);
        public Rect SourceRectRight
        {
            get
            {
               sourceRectRight = ResizeRect(sourceRectRight);
               return sourceRectRight;
            }
            set
            {
                value = ResizeRect(value);
                if (sourceRectRight != value)
                {
                    bool separateLayerBefore = separateLayer;
                    sourceRectRight = value;
                    if (separateLayer != separateLayerBefore)
                    {
                        RecreateLayer(depth, separateLayerBefore);
                    }
                }
            }
        }

        [SerializeField] protected Rect destRectRight = new Rect(0f, 0f, 1f, 1f);
        public Rect DestRectRight
        {
            get
            {
                destRectRight = ResizeRect(destRectRight);
                return destRectRight;
            }
            set
            {
                value = ResizeRect(value);
                if (destRectRight != value)
                {
                    bool separateLayerBefore = separateLayer;
                    destRectRight = value;
                    if (separateLayer != separateLayerBefore)
                    {
                        RecreateLayer(depth, separateLayerBefore);
                    }
                }
            }
        }

        [Range(0, 1)][SerializeField] private float m_Alpha = 1;

        public float alpha
        {
            get => m_Alpha;
            set => m_Alpha = Mathf.Clamp01(value);
        }
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
        //[SerializeField] protected bool autoUpdateContent = false;

        /// <summary>
        /// Should init native composite layer automatically
        /// </summary>
        [SerializeField] protected bool autoInitLayer = true;

        private bool requireToForceUpdateContent = false;

        private YVRManager m_YVRManager = null;
        private YVRManager yvrManager => m_YVRManager ??= YVRManager.instance;
        private Texture m_CachedTexture = null;
        private Texture m_RightEyeCachedTexture = null;
        protected IYVRLayerHandle layerHandler = null;
        private int m_TextureHandle = -1;
        private int m_RightEyeTextureHandle = -1;
        private int m_TempCacheDepth;

        private bool m_IsShowing = false;
        private bool m_IsRecreatingLayer = false;

        public int depth => compositionDepth;

        /// <summary>
        /// ID of the texture
        /// </summary>
        protected virtual int textureHandle
        {
            get
            {
                if (m_CachedTexture == texture || texture == null) return m_TextureHandle;
                m_TextureHandle = (int)texture.GetNativeTexturePtr();
                m_CachedTexture = texture;
                return m_TextureHandle;
            }
        }

        protected virtual int rightEyeTextureHandle
        {
            get
            {
                if (rightEyeTexture == null) return textureHandle;
                if (m_RightEyeCachedTexture == rightEyeTexture) return m_RightEyeTextureHandle;
                m_RightEyeTextureHandle = (int)rightEyeTexture.GetNativeTexturePtr();
                m_RightEyeCachedTexture = rightEyeTexture;
                return m_RightEyeTextureHandle;
            }
        }

        public void SetTexture(Texture tex, int texID = -1)
        {
            bool separateLayerBefore = separateLayer;
            texture = tex;
            m_CachedTexture = tex;
            m_TextureHandle = texID == -1 ? (int)texture.GetNativeTexturePtr() : texID;
            if (separateLayerBefore != separateLayer)
            {
                RecreateLayer(depth, separateLayerBefore);
            }
        }

        public void SetTexture(Texture tex, YVRRenderLayerEyeMask eyeMask, int texID = -1)
        {
            if (eyeMask == YVRRenderLayerEyeMask.kEyeMaskBoth || eyeMask == YVRRenderLayerEyeMask.kEyeMaskLeft)
            {
                SetTexture(tex, texID);
            }
            else
            {
                bool separateLayerBefore = separateLayer;
                rightEyeTexture = tex;
                m_RightEyeCachedTexture = tex;
                m_RightEyeTextureHandle = texID == -1 ? (int)texture.GetNativeTexturePtr() : texID;
                if (separateLayerBefore != separateLayer)
                {
                    RecreateLayer(depth, separateLayerBefore);
                }
            }
        }

        /// <summary>
        /// The mask id of render layer
        /// </summary>
        public int renderLayerId { get; set; } = -1;

        public int rightEyeRenderLayerId { get; set; } = -1;

        private bool separateLayer
        {
            get
            {
                return separateSwapChain || SourceRectLeft != SourceRectRight || DestRectLeft != DestRectRight;
            }
        }

        private bool separateSwapChain
        {
            get
            {
                return (texture != rightEyeTexture && rightEyeTexture != null);
            }
        }

        private Transform m_Transform = null;

        /// <summary>
        /// The width of the actual texture used in the compositeLayer
        /// if texture type is external texture, the size of the texture is controlled by the producer and possibly changes at any time
        /// the texture width set to 100 is just for source rect calculate
        /// </summary>
        protected virtual int width => isExternalTexture ? 100 :
                                       texture ? (int)(texture.width * renderScale) : 0;

        /// <summary>
        /// The height of the actual texture used in the compositeLayer
        /// if texture type is external texture, the size of the texture is controlled by the producer and possibly changes at any time
        /// the texture height set to 100 is just for source rect calculate
        /// </summary>
        protected virtual int height => isExternalTexture ? 100 :
                                        texture ? (int)(texture.height * renderScale) : 0;

        protected int swapChainBufferCount => isDynamic ? 3 : 1;

        public Action<int> onLayerCreatedGfx = null;
        public Action onLayerDestroyedGfx = null;
        public Action onLayerAddedGfx = null;
        public Action onLayerRemovedGfx = null;
        public Action<bool, int> onLayerDepthSetGfx = null;

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
            if (!requireToForceUpdateContent && !isDynamic) return;
            UpdateCompositeLayerContent();
        }

        /// <summary>
        /// Init native composite layer, register composite layer update operations.
        /// </summary>
        /// <param name="depth">The depth of the composite layer</param>
        public void InitCompositeLayer(int depth = int.MinValue)
        {
            if (depth != int.MinValue) compositionDepth = depth;

            var layerCreateInfo = new YVRLayerCreateInfo(compositionDepth, width, height, swapChainBufferCount, m_Shape, -1, separateLayer ? YVRRenderLayerEyeMask.kEyeMaskLeft : YVRRenderLayerEyeMask.kEyeMaskBoth, isExternalTexture, isActiveAndEnabled);
            layerHandler.PrepareCreateLayerAsync(() =>
            {
                if (renderLayerId != -1) return;
                int layerId = layerHandler.CreateLayer(layerCreateInfo);
                int rightEyeLayerId = -1;
                if (separateLayer)
                {
                    layerCreateInfo.renderLayerEyeMask = YVRRenderLayerEyeMask.kEyeMaskRight;
                    layerCreateInfo.batchedLayerId = separateSwapChain ? -1 : layerId;
                    rightEyeLayerId = layerHandler.CreateLayer(layerCreateInfo);
                }
                OnLayerCreatedGfx(layerId, rightEyeLayerId);
                OnLayerAddedGfx();
                ApplyLayerSettings();
                if (!isDynamic) requireToForceUpdateContent = true;
            });
        }

        /// <summary>
        /// Show the composite layer
        /// </summary>
        protected void Show()
        {
            layerHandler.PrepareSetLayerVisible(true, () =>
            {
                if (renderLayerId != -1)
                {
                    layerHandler.SetLayerVisible(renderLayerId, true);
                    if (separateLayer)
                    {
                        layerHandler.SetLayerVisible(rightEyeRenderLayerId, true);
                    }
                    OnLayerAddedGfx();
                }
            });
        }

        /// <summary>
        /// Hide the composite layer
        /// </summary>
        protected void Hide()
        {
            layerHandler.PrepareSetLayerVisible(false, () =>
            {
                if (renderLayerId != -1)
                {
                    layerHandler.SetLayerVisible(renderLayerId, false);
                    if (separateLayer)
                    {
                        layerHandler.SetLayerVisible(rightEyeRenderLayerId, false);
                    }
                    OnLayerAddedGfx();
                }
            });
        }

        public void RecreateLayer()
        {
            RecreateLayer(depth, separateLayer);
        }

        public void RecreateLayer(int depth)
        {
            RecreateLayer(depth, separateLayer);
        }

        public void RecreateLayer(int depth, bool separateLayerBefore)
        {
            m_IsRecreatingLayer = true;
            layerHandler.PrepareDestroyLayerAsync(() =>
            {
                m_IsRecreatingLayer = false;
                if (renderLayerId != -1)
                {
                    layerHandler.DestroyLayer(renderLayerId, false);
                    if (separateLayerBefore)
                    {
                        layerHandler.DestroyLayer(rightEyeRenderLayerId, false);
                    }
                    OnLayerDestroyedGfx();
                }
            });

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
            {
                layerHandler.SetLayerDepthAsync(renderLayerId, depth, OnLayerDepthSetGfx);
                if (separateLayer)
                {
                    layerHandler.SetLayerDepthAsync(rightEyeRenderLayerId, depth, OnLayerDepthSetGfx);
                }
            }
        }

        protected virtual void OnLayerCreatedGfx(int layerId, int rightEyeLayerId)
        {
            renderLayerId = layerId;
            rightEyeRenderLayerId = rightEyeLayerId;
            onLayerCreatedGfx?.Invoke(layerId);
        }

        protected virtual void OnLayerDestroyedGfx()
        {
            renderLayerId = -1;
            rightEyeRenderLayerId = -1;
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
            if (renderLayerId == -1 || m_IsRecreatingLayer) return;

            ILayerShapeHandler layerShapeHandler = GetLayerShapeHandler();
            layerShapeHandler?.HandleLayerPose(layerHandler, renderLayerId, m_Transform, yvrManager, m_CylinderAngle, DestRectLeft);
            if (separateLayer)
            {
                layerShapeHandler?.HandleLayerPose(layerHandler, rightEyeRenderLayerId, m_Transform, yvrManager, m_CylinderAngle, DestRectRight);
            }

            UpdateLayerShape();
        }

        private void UpdateLayerShape()
        {
            if (m_Transform == null) return;
            if (renderLayerId == -1 || m_IsRecreatingLayer) return;

            ILayerShapeHandler layerShapeHandler = GetLayerShapeHandler();
            layerHandler.SetLayerImageRect(renderLayerId, SourceRectLeft);
            layerShapeHandler?.HandleLayerShape(layerHandler, renderLayerId, m_Transform, m_CylinderAngle, m_Radius, DestRectLeft);
            if (separateLayer)
            {
                layerHandler.SetLayerImageRect(rightEyeRenderLayerId, SourceRectRight);
                layerShapeHandler?.HandleLayerShape(layerHandler, rightEyeRenderLayerId, m_Transform, m_CylinderAngle, m_Radius, DestRectRight);
            }
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
            GfxHelper.instance.TriggerGfxThreadCallback(() =>
            {
                if (!Application.isEditor && (renderLayerId == -1 || textureHandle == -1 || !m_IsShowing || m_IsRecreatingLayer || isExternalTexture)) return;
                SwapBufferLayer();
                CopyTextureToColorHandle();
            });
        }

        public virtual IntPtr GetExternalAndroidSurface()
        {
            if (!Application.isEditor && (!isExternalTexture || renderLayerId == -1)) return IntPtr.Zero;
            return layerHandler.GetLayerSurface(renderLayerId);
        }

        public void UpdateCommandBufferMainThread(CommandBuffer commandBuffer)
        {
            if (!Application.isEditor && (renderLayerId == -1 || textureHandle == -1 || !m_IsShowing))
            {
                if (m_CachedTexture is RenderTexture)
                {
                    commandBuffer.SetRenderTarget((RenderTexture)m_CachedTexture);
                    commandBuffer.ClearRenderTarget(false, true, Color.clear);
                }

                if (separateLayer && m_RightEyeCachedTexture != null && m_RightEyeCachedTexture is RenderTexture)
                {
                    commandBuffer.SetRenderTarget((RenderTexture)m_RightEyeCachedTexture);
                    commandBuffer.ClearRenderTarget(false, true, Color.clear);
                }
                return;
            }
        }

        public void SwapBufferLayer()
        {
            if (!Application.isEditor && (renderLayerId == -1 || textureHandle == -1 || !m_IsShowing || m_IsRecreatingLayer || isExternalTexture)) return;
            layerHandler.SwapBufferLayer(renderLayerId);
            if (separateLayer)
            {
                layerHandler.SwapBufferLayer(rightEyeRenderLayerId);
            }
        }

        public void UpdateCommandBuffer()
        {
            if (!Application.isEditor && (renderLayerId == -1 || textureHandle == -1 || !m_IsShowing || m_IsRecreatingLayer || isExternalTexture)) return;
            CopyTextureToColorHandle();
        }

        public void SetSurfaceDimensions(int width, int height)
        {
            layerHandler.SetSurfaceDimensions(renderLayerId, width, height);
            if (separateSwapChain)
            {
                layerHandler.SetSurfaceDimensions(renderLayerId, width, height);
            }
        }

        protected virtual void CopyTextureToColorHandle()
        {
            int destTextureId = layerHandler.GetLayerColorHandle(renderLayerId, -1);
            CopyTexture(textureHandle, destTextureId, alpha);
            if (separateSwapChain)
            {
                destTextureId = layerHandler.GetLayerColorHandle(rightEyeRenderLayerId, -1);
                CopyTexture(rightEyeTextureHandle, destTextureId, alpha);
            }

            requireToForceUpdateContent = false;
        }

        private void CopyTexture(int sourceTextureId, int destTextureId, float alpha)
        {
            if (!isExternalTexture)
            {
                GfxHelper.instance.CopyTexture(sourceTextureId, destTextureId, alpha, new Rect(0, 0, 1, 1), new Rect(0, 0, 1, 1));
            }
        }

        protected void ApplyLayerSettings()
        {
            if (renderLayerId == -1) return;

            bool enableSuperSampling = SuperSamplingType != YVRQualityManager.LayerSettingsType.None;
            bool expensiveSuperSampling = SuperSamplingType == YVRQualityManager.LayerSettingsType.Quality;
            bool enableSharpen = SharpenType != YVRQualityManager.LayerSettingsType.None;
            bool expensiveSharpen = SharpenType == YVRQualityManager.LayerSettingsType.Quality;
            layerHandler.SetLayerSettings(renderLayerId, enableSuperSampling, expensiveSuperSampling, enableSharpen, expensiveSharpen);

            if (separateLayer)
            {
                layerHandler.SetLayerSettings(rightEyeRenderLayerId, enableSuperSampling, expensiveSuperSampling, enableSharpen, expensiveSharpen);
            }
        }

        private Rect ResizeRect(Rect rect)
        {
            rect.x = Mathf.Clamp01(rect.x);
            rect.y = Mathf.Clamp01(rect.y);
            rect.width = Mathf.Clamp01(rect.width);//Mathf.Clamp(rect.width, 0f, 1f - rect.x);
            rect.height = Mathf.Clamp01(rect.height);//Mathf.Clamp(rect.height, 0f, 1f - rect.y);
            return rect;
        }

        //[ExcludeFromDocs]
        protected void OnDisable() { Hide(); }

        [ExcludeFromDocs]
        protected virtual void OnDestroy()
        {
            m_Transform = null;
            layerHandler.PrepareDestroyLayerAsync(() =>
            {
                if (renderLayerId != -1)
                {
                    layerHandler.DestroyLayer(renderLayerId, true);
                    if (rightEyeRenderLayerId != -1)
                    {
                        layerHandler.DestroyLayer(rightEyeRenderLayerId, true);
                    }

                    OnLayerDestroyedGfx();
                }
            });
        }
    }
}