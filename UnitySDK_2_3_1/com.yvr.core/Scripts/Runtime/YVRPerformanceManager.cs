namespace YVR.Core
{
    /// <summary>
    /// The manager for device performance
    /// </summary>
    public static class YVRPerformanceManager
    {
        /// <summary>
        ///  Gets or sets the current CPU performance level (0-4). Lower performance levels save more power.
        /// </summary>
        public static int cpuLevel
        {
            get => YVRPlugin.Instance.GetCPULevel();
            set => YVRPlugin.Instance.SetPerformanceLevel(value, gpuLevel);
        }

        /// <summary>
        /// Gets or sets the current GPU performance level (0-5). Lower performance levels save more power.
        /// </summary>
        public static int gpuLevel
        {
            get => YVRPlugin.Instance.GetGPULevel();
            set => YVRPlugin.Instance.SetPerformanceLevel(cpuLevel, value);
        }

        /// <summary>
        /// Get the gpu utilize level(0.0-1.0)
        /// </summary>
        public static float gpuUtilLevel => YVRPlugin.Instance.GetGPUUtilLevel();

        /// <summary>
        /// Get the cpu utilize level(0.0-1.0)
        /// </summary>
        public static float cpuUtilLevel => YVRPlugin.Instance.GetCPUUtilLevel();
    }
}