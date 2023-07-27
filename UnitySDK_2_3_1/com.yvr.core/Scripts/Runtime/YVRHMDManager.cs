﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YVR.Core
{
    /// <summary>
    /// The manager for hmd device data
    /// </summary>
    public class YVRHMDManager
    {
        /// <summary>
        /// Get the battery level
        /// </summary>
        public float batteryLevel => YVRPlugin.Instance.GetBatteryLevel();

        /// <summary>
        /// Get the battery temperature
        /// </summary>
        public float batteryTemperature => YVRPlugin.Instance.GetBatteryTemperature();

        /// <summary>
        /// Get the battery state(-1:get fail;1:unkown;2:Charging;3:discharging;4:not charging;5:full )
        /// </summary>
        public int batteryStatus => YVRPlugin.Instance.GetBatteryStatus();

        /// <summary>
        /// Get the volume level
        /// </summary>
        public float volumeLevel => YVRPlugin.Instance.GetVolumeLevel();

        /// <summary>
        /// Get whether user is currently wearing the display.
        /// </summary>
        public bool isUserPresent => YVRPlugin.Instance.IsUserPresent();

        public void SetPassthrough(bool enable)
        {
            YVRPlugin.Instance.SetPassthrough(enable);
        }
    }
}