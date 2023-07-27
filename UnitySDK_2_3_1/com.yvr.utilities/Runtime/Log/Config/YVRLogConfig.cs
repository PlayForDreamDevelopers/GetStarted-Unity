using System.Collections.Generic;
using UnityEngine;

namespace YVR.Utilities
{
    [CreateAssetMenu(fileName = "YVRLogConfig", menuName = "YVR/YVRLogConfig")]
    public class YVRLogConfig : ScriptableObject
    {
        public bool enable = true;
        public string tag = "Unity";
        public List<LoggerConfig> loggerConfigs;
    }
}