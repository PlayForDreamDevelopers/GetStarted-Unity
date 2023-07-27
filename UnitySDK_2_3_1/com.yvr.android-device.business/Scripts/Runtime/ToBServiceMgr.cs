using UnityEngine;
using YVR.Utilities;

namespace YVR.AndroidDevice.Business
{
    public class ToBServiceMgr
    {
        private static AndroidJavaObject s_ServiceInstance = null;

        private const string k_ClassName = "com.yvr.tobsettings.ToBServiceHelper";
        private const string k_UnityActivity = "com.unity3d.player.UnityPlayer";
        private const string k_GetContext = "currentActivity";
        private const string k_GetInstance = "getInstance";
        private const string k_Init = "init";

        private static AndroidJavaObject GetServiceInstance()
        {
            Debug.Log("GetServiceInstance");
            var unityActivityClass = new AndroidJavaClass(k_UnityActivity);
            var context = unityActivityClass.GetStatic<AndroidJavaObject>(k_GetContext);
            var binderHelperClass = new AndroidJavaClass(k_ClassName);

            binderHelperClass.CallStatic(k_Init, context);
            s_ServiceInstance = binderHelperClass.CallStatic<AndroidJavaObject>(k_GetInstance);

            Debug.Log($"GetServiceInstance is {s_ServiceInstance == null}");

            return s_ServiceInstance;
        }

        public static AndroidJavaObject serviceInstance => s_ServiceInstance ??= GetServiceInstance();
    }
}