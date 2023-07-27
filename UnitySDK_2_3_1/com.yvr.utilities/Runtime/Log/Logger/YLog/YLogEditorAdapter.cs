using UnityEngine;
using UnityEngine.Internal;
using UnityEngine.TestTools;

namespace YVR.Utilities
{
    [ExcludeFromDocs, ExcludeFromCoverage]
    // YLogLoggerEditor will not handle RamLog related operations
    public class YLogEditorAdapter : IYLogAdapter
    {
        public void DebugHandle(string msg) { Debug.Log(msg); }

        public void RamDebugHandle(string msg) { }

        public void InfoHandle(string msg) { Debug.Log(msg); }

        public void RamInfoHandle(string msg) { }

        public void WarnHandle(string msg) { Debug.LogWarning(msg); }

        public void RamWarnHandle(string msg) { }

        public void ErrorHandle(string msg) { Debug.LogError(msg); }

        public void RamErrorHandle(string msg) { }

        public void ConfigureYLog(string tag, int ramLogSize = 5) { }

        public void SaveLog() { }

        public void SetLogcatOutputHandler(bool viaUnity) { }
    }
}