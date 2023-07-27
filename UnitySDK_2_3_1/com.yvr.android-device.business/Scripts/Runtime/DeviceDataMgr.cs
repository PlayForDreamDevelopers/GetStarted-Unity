namespace YVR.AndroidDevice.Business
{
    public static class DeviceDataMgr
    {
        private const string k_GetBtMac = "getBtMac";
        private const string k_GetDeviceModel = "getDeviceModel";
        private const string k_GetDeviceSn = "getDeviceSn";
        private const string k_GetSoftwareVersion = "getSoftwareVersion";
        private const string k_GetWifiMac = "getWifiMac";
        private const string k_SwitchHomeState = "changeThirdHomeState";


        public static string btMac => ToBServiceMgr.serviceInstance.Call<string>(k_GetBtMac);
        public static string deviceModel => ToBServiceMgr.serviceInstance.Call<string>(k_GetDeviceModel);
        public static string deviceSn => ToBServiceMgr.serviceInstance.Call<string>(k_GetDeviceSn);
        public static string softwareVersion => ToBServiceMgr.serviceInstance.Call<string>(k_GetSoftwareVersion);
        public static string wifiMac => ToBServiceMgr.serviceInstance.Call<string>(k_GetWifiMac);

        public static bool SetHomeState(string packageName, bool enable)
        {
            return ToBServiceMgr.serviceInstance.Call<bool>(k_SwitchHomeState, packageName, enable);
        }
    }
}