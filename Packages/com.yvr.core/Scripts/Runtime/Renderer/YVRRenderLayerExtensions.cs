using UnityEngine;
using UnityEngine.XR;

namespace YVR.Core
{
    public static class YVRRenderLayerExtensions
    {
        public static XRPose ToXRTrackingSpacePose(this Transform transform, in Camera camera)
        {
            XRPose xrHeadPose = XRPose.identity;

            YVRCameraRig.centerEyeDevice.TryGetFeatureValue(CommonUsages.centerEyePosition, out xrHeadPose.position);
            YVRCameraRig.centerEyeDevice.TryGetFeatureValue(CommonUsages.centerEyeRotation, out xrHeadPose.orientation);

            return xrHeadPose * transform.ToXRHeadSpacePose(camera);
        }

        public static XRPose ToXRTrackingSpacePose(this Transform transform, in Camera camera, in XRPose offset)
        {
            XRPose xrHeadPose = XRPose.identity;

            YVRCameraRig.centerEyeDevice.TryGetFeatureValue(CommonUsages.centerEyePosition, out xrHeadPose.position);
            YVRCameraRig.centerEyeDevice.TryGetFeatureValue(CommonUsages.centerEyeRotation, out xrHeadPose.orientation);

            return xrHeadPose * transform.ToXRHeadSpacePose(camera, offset);
        }

        private static XRPose ToXRHeadSpacePose(this Transform transform, in Camera camera)
        {
            return camera.transform.ToYVRPose().Inverse() * transform.ToYVRPose();
        }

        private static XRPose ToXRHeadSpacePose(this Transform transform, in Camera camera, in XRPose offset)
        {
            return camera.transform.ToYVRPose().Inverse() * transform.ToYVRPose(offset);
        }

        public static XRPose ToYVRPose(this Transform transform)
        {
            return new XRPose() {orientation = transform.rotation, position = transform.position};
        }

        public static XRPose ToYVRPose(this Transform transform, in XRPose offset)
        {
            return new XRPose() { orientation = transform.rotation, position = transform.position } * offset;
        }
    }
}