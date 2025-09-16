using UnityEditor.XR.Management;

namespace YVR.Core.XR
{
    public class YVRXRBuildProcess : XRBuildHelper<YVRXRSettings>
    {
        public override string BuildSettingsKey
        {
            get { return "YVR.Core.XR.YVRXRSettings"; }
        }
    }
}