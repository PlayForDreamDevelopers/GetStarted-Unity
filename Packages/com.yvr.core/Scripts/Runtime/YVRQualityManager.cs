using System;
using System.Linq;
using System.Collections;
using UnityEngine;
using UnityEngine.XR;

namespace YVR.Core
{
    /// <summary>
    /// Manager for rendering quality
    /// </summary>
    [Serializable]
    public partial class YVRQualityManager
    {
        [Tooltip("K1: VSync every frame.K2: VSync two frame.")]
        [SerializeField] private VSyncCount _vSyncCount = VSyncCount.k1;

        [SerializeField]
        private FixedFoveatedRenderingLevel _fixedFoveatedRenderingLevel = FixedFoveatedRenderingLevel.Off;

        [SerializeField]
        private bool _fixedFoveatedRenderingDynamic = false;

        [SerializeField]
        private LayerSettingsType _sharpenType = LayerSettingsType.None;

        [SerializeField]
        private bool _enableDynamicResolution = false;

        [Range(0.7f, 1.3f)]
        [SerializeField]
        private float _minDynamicResolutionScale = 1.0f;

        [Range(0.7f, 1.3f)]
        [SerializeField]
        private float _maxDynamicResolutionScale = 1.0f;

        [SerializeField]
        private AdapterResolutionPolicy _powerSetting = AdapterResolutionPolicy.BALANCED;

        /// <summary>
        /// The event which will be triggered when vSync count changed
        /// </summary>
        public event Action<VSyncCount> onVSyncCountChanged = null;

        /// <summary>
        /// The event which will be triggered when ffr level changed
        /// </summary>
        public event Action<FixedFoveatedRenderingLevel, FixedFoveatedRenderingDynamic> onFFRChanged = null;

        /// <summary>
        /// The event which will be triggered when sharpen type changed
        /// </summary>
        public event Action<LayerSettingsType> onSharpenTypeChanged = null;

        /// <summary>
        /// Set or get current vSync count
        /// </summary>
        public VSyncCount vSyncCount
        {
            get { return _vSyncCount; }
            set
            {
                if (_vSyncCount == value) return;

                _vSyncCount = value;
                ApplyVSyncCount(value);
            }
        }

        /// <summary>
        /// Set or get current fixed foveated rendering level
        /// </summary>
        public FixedFoveatedRenderingLevel fixedFoveatedRenderingLevel
        {
            get { return _fixedFoveatedRenderingLevel; }
            set
            {
                if (_fixedFoveatedRenderingLevel == value) return;

                _fixedFoveatedRenderingLevel = value;
                ApplyFFR(value, fixedFoveatedRenderingDynamic);
            }
        }

        /// <summary>
        /// Set or get current fixed foveated rendering dynamic
        /// </summary>
        public bool fixedFoveatedRenderingDynamic
        {
            get { return _fixedFoveatedRenderingDynamic; }
            set
            {
                if (_fixedFoveatedRenderingDynamic == value) return;

                _fixedFoveatedRenderingDynamic = value;
                ApplyFFR(fixedFoveatedRenderingLevel, value);
            }
        }

        /// <summary>
        /// Set or get current eyebuffer sharpenType
        /// </summary>
        public LayerSettingsType sharpenType
        {
            get { return _sharpenType; }
            set
            {
                if (_sharpenType == value) return;
                _sharpenType = value;
                ApplySharpen(value);
            }
        }

        /// <summary>
        /// Gets the recommended MSAA level for optimal quality/performance the current device.
        /// </summary>
        public int recommendAntiAlisingLevel => 4;

        /// <summary>
        /// Whether to use recommend MSAA level, if true, the value of @YVR.Core.YVRQualityManager.antiAliasing will be ignored
        /// </summary>
        public bool useRecommendedMSAALevel = true;

        public bool enableDynamicResolution
        {
            get { return _enableDynamicResolution; }
            set
            {
                if (_enableDynamicResolution == value) return;
                _enableDynamicResolution = value;
            }
        }

        public float minDynamicResolutionScale
        {
            get { return _minDynamicResolutionScale; }
            set
            {
                if (_minDynamicResolutionScale == value) return;
                _minDynamicResolutionScale = value;
            }
        }

        public float maxDynamicResolutionScale
        {
            get { return _maxDynamicResolutionScale; }
            set
            {
                if (_maxDynamicResolutionScale == value) return;
                _maxDynamicResolutionScale = value;
            }
        }

        public AdapterResolutionPolicy powerSetting
        {
            get { return _powerSetting; }
            set
            {
                if (_powerSetting == value) return;
                _powerSetting = value;
                ApplyPowerSetting(value);
            }
        }

        private const int _pixelStepPerFrame = 32;

        /// <summary>
        /// Using the data from unity Inspector panel to initialize vSync count and ffr level
        /// </summary>
        public void Initialize()
        {
            YVRManager.instance.StartCoroutine(Wait2FramesBeforeInitializing());
            if (enableDynamicResolution)
            {
                XRSettings.eyeTextureResolutionScale = maxDynamicResolutionScale;
            }
        }

        public void Update()
        {
            if (enableDynamicResolution)
            {
                YVRExtent2DInt recommendedResolution = default;
                if (YVRPlugin.Instance.GetRecommendedResolution(ref recommendedResolution))
                {
                    recommendedResolution.width = (int)(maxDynamicResolutionScale * recommendedResolution.width);

                    recommendedResolution.width = Mathf.Max(recommendedResolution.width,
                        (int)(XRSettings.eyeTextureWidth * XRSettings.renderViewportScale) - _pixelStepPerFrame);
                    recommendedResolution.width = Mathf.Min(recommendedResolution.width,
                        (int)(XRSettings.eyeTextureWidth * XRSettings.renderViewportScale) + _pixelStepPerFrame);

                    float scalingFactor = (float)recommendedResolution.width / XRSettings.eyeTextureWidth;
                    scalingFactor = Mathf.Max(scalingFactor, minDynamicResolutionScale / maxDynamicResolutionScale);
                    scalingFactor = Mathf.Min(scalingFactor, 1.0f);

                    XRSettings.renderViewportScale = scalingFactor;
                }
            }
        }

        private void ApplyVSyncCount(VSyncCount value)
        {
            YVRPlugin.Instance.SetVSyncCount(value);
            QualitySettings.vSyncCount = (int) value;
            onVSyncCountChanged?.SafeInvoke(value);
        }

        private void ApplyFFR(FixedFoveatedRenderingLevel value, bool isDynamicEnable)
        {
            FixedFoveatedRenderingDynamic isDynamic = isDynamicEnable ? FixedFoveatedRenderingDynamic.Enabled : FixedFoveatedRenderingDynamic.Disabled;
            YVRPlugin.Instance.SetFoveation((int) value, (int)isDynamic);
            onFFRChanged?.SafeInvoke(value, isDynamic);
        }

        private void ApplySharpen(LayerSettingsType type)
        {
            bool enableSharpen = type != LayerSettingsType.None;
            bool expensiveSharpen = type == LayerSettingsType.Quality;
            YVRPlugin.Instance.SetEyeBufferLayerSettings(false, false, enableSharpen, expensiveSharpen);
            onSharpenTypeChanged?.SafeInvoke(type);
        }

        private void ApplyPowerSetting(AdapterResolutionPolicy adapterResolutionPolicy)
        {
            YVRPlugin.Instance.SetAdapterResolutionPolicy(adapterResolutionPolicy);
        }

        // The native color buffer may be allocate after several frames
        // Thus waiting a second to ensure the eye buffer has been created
        private IEnumerator Wait2FramesBeforeInitializing()
        {
            ApplyVSyncCount(vSyncCount);
            ApplyPowerSetting(powerSetting);
            yield return new WaitForSecondsRealtime(1.0f);
            ApplyFFR(fixedFoveatedRenderingLevel, fixedFoveatedRenderingDynamic);
            ApplySharpen(sharpenType);
        }
    }
}