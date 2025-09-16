using System;
using System.Runtime.InteropServices;

namespace YVR.Core
{
    [Serializable]
    public enum YVRRenderLayerEyeMask
    {
        kEyeMaskLeft = 0x00000001,
        kEyeMaskRight = 0x00000002,
        kEyeMaskBoth = 0x00000003
    }

    [Serializable]
    public enum YVRRenderLayerType
    {
        Quad = 1,
        Cylinder,
        Equirect,
    }

    public struct YVRLayerCreateInfo
    {
        public int depth;
        public int width;
        public int height;
        public int swapChainBufferCount;
        public int textureType;
        public int layerFlags;
        public int batchedLayerId;
        [MarshalAs(UnmanagedType.U1)] public bool separateSwapChain;
        [MarshalAs(UnmanagedType.U1)] public bool autoShowAfterCreated;
        [MarshalAs(UnmanagedType.U1)] public bool isSurfaceSwapChain;
        public YVRRenderLayerType renderLayerType;
        public YVRRenderLayerEyeMask renderLayerEyeMask;

        public YVRLayerCreateInfo(int depth, int width, int height, int swapChainBufferCount,
                                  YVRRenderLayerType renderLayerType, int batchedLayerId = -1, YVRRenderLayerEyeMask renderLayerEyeMask = YVRRenderLayerEyeMask.kEyeMaskBoth, bool isSurfaceSwapChain = false, bool autoShowAfterCreated = true)
            : this(depth, width, height, swapChainBufferCount, 0, 2, false, autoShowAfterCreated, isSurfaceSwapChain, renderLayerType, renderLayerEyeMask, batchedLayerId) { }

        public YVRLayerCreateInfo(int depth, int width, int height, int swapChainBufferCount, int textureType,
                                  int layerFlags, bool separateSwapChain, bool autoShowAfterCreated, bool isSurfaceSwapChain, YVRRenderLayerType renderLayerType, YVRRenderLayerEyeMask renderLayerEyeMask, int batchedLayerId)
        {
            this.depth = depth;
            this.width = width;
            this.height = height;
            this.swapChainBufferCount = swapChainBufferCount;
            this.textureType = textureType;
            this.layerFlags = layerFlags;
            this.separateSwapChain = separateSwapChain;
            this.isSurfaceSwapChain = isSurfaceSwapChain;
            this.autoShowAfterCreated = autoShowAfterCreated;
            this.renderLayerType = renderLayerType;
            this.renderLayerEyeMask = renderLayerEyeMask;
            this.batchedLayerId = batchedLayerId;
        }
    }
}