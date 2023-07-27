using System;

namespace YVR.Utilities
{
    [Serializable]
    public enum LoggerType
    {
        YLog = 1 << 0,
        Unity = 1 << 1
    }
}