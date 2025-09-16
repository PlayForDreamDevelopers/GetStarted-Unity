using UnityEngine;
using YVR.Core;
#if XR_CORE_UTILS
using Unity.XR.CoreUtils;
#endif

namespace YVR.Core
{
    public class Equirect2ShapeHandler : ILayerShapeHandler
    {
        private const float PI = 3.14159265358979323846f;

        public void HandleLayerPose(IYVRLayerHandle layerHandle, params object[] data)
        {
            int renderLayerId = (int) data[0];
            Transform transform = data[1] as Transform;
            YVRManager yvrManager = data[2] as YVRManager;
            XRPose pose = new XRPose();
#if XR_CORE_UTILS
        if (GameObject.FindObjectOfType<XROrigin>() != null)
        {
            pose = transform.ToXRTrackingSpacePose(GameObject.FindObjectOfType<XROrigin>().Camera);
        }
        else
        {
            pose = transform.ToXRTrackingSpacePose(yvrManager.cameraRenderer.centerEyeCamera);
        }
#else
            pose = transform.ToXRTrackingSpacePose(yvrManager.cameraRenderer.centerEyeCamera);
#endif

            layerHandle.SetLayerPose(renderLayerId, pose);
        }

        public void HandleLayerShape(IYVRLayerHandle layerHandle, params object[] data)
        {
            int renderLayerId = (int) data[0];
            float radius = (float) data[3];
            Rect destRect = (Rect)data[4];

            float deg2Rad = (PI / 180f);
            float lowerVerticalAngle = (-90f + 180f * destRect.y) * deg2Rad;
            float upperVerticalAngle = lowerVerticalAngle + (180f * destRect.height) * deg2Rad;
            float centralHorizontalAngle = 360f * destRect.width * deg2Rad;

            layerHandle.SetLayerEquirect2Param(renderLayerId, radius, centralHorizontalAngle, upperVerticalAngle, lowerVerticalAngle);
        }
    }
}