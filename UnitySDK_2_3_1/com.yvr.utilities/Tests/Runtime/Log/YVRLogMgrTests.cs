#if USE_YVR_TEST_FRAMRWORK
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using YVR.Test.Framework;
using Object = UnityEngine.Object;

namespace YVR.Utilities.Test
{
    [TestFixture, Category("Log")]
    public class YVRLogMgrTests : IPrebuildSetup, IPostBuildCleanup
    {
        private class MyClass1 { }

        private class MyClass2 { }

        private string m_ScenePath = "Packages/com.yvr.utilities/Tests/Runtime/Log/Config/YVRLogConfigTests.unity";
        public void Setup() { TestUtils.AddRequiredScene(m_ScenePath); }
        public void Cleanup() { TestUtils.RemoveRequiredScene(m_ScenePath); }

        private void ReInit(YVRLogConfig config)
        {
            YVRLogMgr logMgr = YVRLogMgr.instance;
            logMgr.TrySetFieldValue("config", config);
            MethodInfo initMethod
                = typeof(YVRLogMgr).GetMethod("Init", BindingFlags.Instance | BindingFlags.NonPublic);

            initMethod.Invoke(logMgr, null);
        }


        [UnityTest, Order(0)]
        public IEnumerator _00_LoadSceneSucceed()
        {
            yield return TestUtils.TestScene(m_ScenePath, () =>
            {
                string currentSceneName = SceneManager.GetActiveScene().name;
                Debug.Log("Current Scene: " + currentSceneName);
                Assert.That(currentSceneName.Contains("YVRLogConfigTests"));
            });
        }

        [Test, Order(1)]
        public void _01_EmptyConfig_NoException()
        {
            Assert.That(() =>
            {
                ReInit(null);
            }, Throws.Nothing);
        }

        [Test, Order(2)]
        public void _02_Config_NoException()
        {
            var config = ScriptableObject.CreateInstance<YVRLogConfig>();
            Assert.That(() =>
            {
                ReInit(config);
            }, Throws.Nothing);
            Object.Destroy(config);
        }

        [Test, Order(3)]
        public void _03_DisableConfig_NoOutput()
        {
            var config = ScriptableObject.CreateInstance<YVRLogConfig>();
            config.enable = false;

            YVRLogMgr.instance.ConfigYVRLog(config);

            this.Debug("Debug");
            this.Info("Info");
            this.Warn("Info");
            this.Error("Error");
            LogAssert.NoUnexpectedReceived();

            Object.Destroy(config);
        }

        [Test, Order(4)]
        public void _04_OnlyUnityLogConfig_AllOutput()
        {
            var config = ScriptableObject.CreateInstance<YVRLogConfig>();
            config.enable = true;
            config.loggerConfigs = new List<LoggerConfig>()
            {
                new LoggerConfig() {loggerType = LoggerType.Unity}
            };

            YVRLogMgr.instance.ConfigYVRLog(config);

            this.Debug("Debug");
            this.Info("Info");
            this.Warn("Warning");
            this.Error("Error");
            LogAssert.Expect(LogType.Log, "Debug");
            LogAssert.Expect(LogType.Log, "Info");
            LogAssert.Expect(LogType.Warning, "Warning");
            LogAssert.Expect(LogType.Error, "Error");

            Object.Destroy(config);
        }

        [Test, Order(5)]
        public void _05_UnityLogConfig_NoController_WarnPriority_GetWarningButStillFiltering()
        {
            var config = ScriptableObject.CreateInstance<YVRLogConfig>();
            config.enable = true;
            config.loggerConfigs = new List<LoggerConfig>()
            {
                new LoggerConfig() {loggerType = LoggerType.Unity, priority = LogPriority.Warn}
            };

            YVRLogMgr.instance.ConfigYVRLog(config);

            this.Debug("Debug");
            this.Info("Info");
            this.Warn("Warning");
            this.Error("Error");
            LogAssert.Expect(LogType.Warning, "Warning");
            LogAssert.Expect(LogType.Error, "Error");
            LogAssert.NoUnexpectedReceived();

            Object.Destroy(config);
        }

        [Test, Order(6)]
        public void _06_UnityLogConfig_PriorityPrefix_GetPrefix()
        {
            var config = ScriptableObject.CreateInstance<YVRLogConfig>();
            config.enable = true;
            config.loggerConfigs = new List<LoggerConfig>()
            {
                new LoggerConfig()
                    {loggerType = LoggerType.Unity, prefixType = PrefixType.Priority}
            };

            YVRLogMgr.instance.ConfigYVRLog(config);

            this.Debug("Debug");
            string priorityForDebug = $"Priority: {nameof(LogPriority.Debug)}";

            LogAssert.Expect(LogType.Log, $"[{priorityForDebug}] Debug");
            Object.Destroy(config);
        }

        [Test, Order(7)]
        public void _07_UnityLogConfig_Prefix2Priority_PrefixPriorityControllerWork()
        {
            var config = ScriptableObject.CreateInstance<YVRLogConfig>();
            config.enable = true;
            config.loggerConfigs = new List<LoggerConfig>()
            {
                new LoggerConfig()
                {
                    loggerType = LoggerType.Unity, prefixType = PrefixType.Context, priority = LogPriority.Highest,
                    prefix2PriorityDict = new Dictionary<string, LogPriority>()
                    {
                        {"Class1", LogPriority.Info},
                        {"Class2", LogPriority.Warn},
                    }
                },
            };

            YVRLogMgr.instance.ConfigYVRLog(config);

            var myClass1 = new MyClass1();
            var myClass2 = new MyClass2();

            this.Debug("This Debug");
            this.Info("This Info");
            this.Warn("This Warn");
            this.Error("This Error");

            myClass1.Debug("MyClass1 Debug");
            myClass1.Info("MyClass1 Info");
            myClass1.Warn("MyClass1 Warn");
            myClass1.Error("MyClass1 Error");

            myClass2.Debug("MyClass2 Debug");
            myClass2.Info("MyClass2 Info");
            myClass2.Warn("MyClass2 Warn");
            myClass2.Error("MyClass2 Error");

            LogAssert.Expect(LogType.Log, "[Context: MyClass1] MyClass1 Info");
            LogAssert.Expect(LogType.Warning, "[Context: MyClass1] MyClass1 Warn");
            LogAssert.Expect(LogType.Error, "[Context: MyClass1] MyClass1 Error");

            LogAssert.Expect(LogType.Warning, "[Context: MyClass2] MyClass2 Warn");
            LogAssert.Expect(LogType.Error, "[Context: MyClass2] MyClass2 Error");

            Object.Destroy(config);
        }
    }
}
#endif