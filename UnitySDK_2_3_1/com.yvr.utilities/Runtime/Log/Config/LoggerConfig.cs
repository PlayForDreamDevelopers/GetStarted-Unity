using System;
using System.Collections.Generic;

namespace YVR.Utilities
{
    [Serializable]
    public struct LoggerConfig
    {
        public LoggerType loggerType;
        public PrefixType prefixType;
        public LogPriority priority;
        public Dictionary<string, LogPriority> prefix2PriorityDict;
    }
}