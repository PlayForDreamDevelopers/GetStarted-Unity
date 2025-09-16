using UnityEngine;
using YVR.Core;
#if XR_CORE_UTILS
using Unity.XR.CoreUtils;
#endif

namespace YVR.Core
{
    public class QuadShapeHandler : ILayerShapeHandler
    {
        public void HandleLayerPose(IYVRLayerHandle layerHandle, params object[] data)
        {
            int renderLayerId = (int) data[0];
            Transform transform = data[1] as Transform;
            YVRManager yvrManager = data[2] as YVRManager;
            Rect destRect = (Rect)data[4];

            destRect.width = Mathf.Clamp(destRect.width, 0f, 1f - destRect.x);
            destRect.height = Mathf.Clamp(destRect.height, 0f, 1f - destRect.y);

            float offsetX = (-transform.lossyScale.x / 2f) + (destRect.x * transform.lossyScale.x) + (destRect.width * transform.lossyScale.x / 2f);
            float offsetY = (-transform.lossyScale.y / 2f) + (destRect.y * transform.lossyScale.y) + (destRect.height * transform.lossyScale.y / 2f);

            XRPose offsetPose = new XRPose() { position = new Vector3(offsetX, offsetY), orientation = Quaternion.identity };
            XRPose pose = new XRPose();
#if XR_CORE_UTILS
        if (GameObject.FindObjectOfType<XROrigin>() != null)
        {
            pose = transform.ToXRTrackingSpacePose(GameObject.FindObjectOfType<XROrigin>().Camera, offsetPose);
        }
        else
        {
            pose = transform.ToXRTrackingSpacePose(yvrManager.cameraRenderer.centerEyeCamera, offsetPose);
        }
#else
            pose = transform.ToXRTrackingSpacePose(yvrManager.cameraRenderer.centerEyeCamera, offsetPose);
#endif

            layerHandle.SetLayerPose(renderLayerId, pose);
        }

        public void HandleLayerShape(IYVRLayerHandle layerHandle, params object[] data)
        {
            int renderLayerId = (int) data[0];
            Transform transform = data[1] as Transform;
            Rect destRect = (Rect)data[4];

            destRect.width = Mathf.Clamp(destRect.width, 0f, 1f - destRect.x);
            destRect.height = Mathf.Clamp(destRect.height, 0f, 1f - destRect.y);

            float sizeX = destRect.width * transform.lossyScale.x;
            float sizeY = destRect.height * transform.lossyScale.y;

            layerHandle.SetLayerSize(renderLayerId, new XRSize(sizeX, sizeY));
        }
    }
}