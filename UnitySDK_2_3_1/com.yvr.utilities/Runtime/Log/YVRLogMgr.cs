using System.Linq;

namespace YVR.Utilities
{
    public class YVRLogMgr : MonoBehaviorSingleton<YVRLogMgr>
    {
        public YVRLogConfig config = null;

        protected override void Init()
        {
            base.Init();
            if (config == null) return;

            ConfigYVRLog(config);
        }

        public void ConfigYVRLog(YVRLogConfig config = null)
        {
            if (config != null) this.config = config;
            config ??= this.config;

            YVRLog.enable = config.enable;
            YVRLog.ClearLoggers();
            YLogLogger.ConfigureYLog(config.tag);

            config.loggerConfigs?.ForEach(cfg =>
            {
                LoggerBase logger = GenerateLogger(cfg.loggerType);
                LogPrefixBase prefix = GeneratePrefixBase(cfg.prefixType);
                LoggerControllerBase controller = GenerateLoggerController(cfg);
                logger.SetPrefix(prefix);
                logger.SetController(controller);
                YVRLog.RegisterLogger(logger);
            });

            LoggerBase GenerateLogger(LoggerType loggerType)
            {
                LoggerBase logger = null;
                if (loggerType == LoggerType.YLog) logger = new YLogLogger();
                if (loggerType == LoggerType.Unity) logger = new UnityLogger();

                return logger;
            }

            LogPrefixBase GeneratePrefixBase(PrefixType prefixType)
            {
                LogPrefixBase prefix = null;
                if (prefixType.HasFlag(PrefixType.Priority)) prefix = new PriorityLogPrefix();
                if (prefixType.HasFlag(PrefixType.Context)) prefix = new ContextLogPrefix(prefix);
                return prefix;
            }

            LoggerControllerBase GenerateLoggerController(LoggerConfig cfg)
            {
                LoggerControllerBase controller = null;

                var type = LoggerControllerType.None;
                if (cfg.priority != LogPriority.Lowest)
                    type = LoggerControllerType.Priority;
                if (cfg.prefix2PriorityDict != null && cfg.prefix2PriorityDict.Count != 0)
                    type = LoggerControllerType.PrefixPriority;


                if (type == LoggerControllerType.Priority)
                {
                    controller = new LoggerPriorityController();
                    ((LoggerPriorityController) controller).priority = cfg.priority;
                }
                else if (type == LoggerControllerType.PrefixPriority)
                {
                    controller = new LoggerPrefixPriorityController();
                    var prefixPriorityController = (LoggerPrefixPriorityController) controller;
                    prefixPriorityController.priority = cfg.priority;
                    cfg.prefix2PriorityDict.Keys.ToList().ForEach(key =>
                    {
                        prefixPriorityController.AddPrefix2PriorityMap(key, cfg.prefix2PriorityDict[key]);
                    });
                }

                return controller;
            }
        }
    }
}