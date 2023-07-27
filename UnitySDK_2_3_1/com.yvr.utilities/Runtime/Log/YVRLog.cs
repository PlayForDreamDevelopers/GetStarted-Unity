using System.Collections.Generic;

namespace YVR.Utilities
{
    /// <summary>
    /// Extension class for all object to provider 
    /// </summary>
    public static class YVRLog
    {
        private static List<LoggerBase> loggersList { get; } = new List<LoggerBase>();

        /// <summary>
        /// Global enable flag for all loggers, if false, all loggers will not output any log
        /// </summary>
        public static bool enable = true;

        /// <summary>
        /// Output log within all registered logger in Debug priority
        /// </summary>
        /// <param name="context"> The context where output the log </param>
        /// <param name="message"> The log message </param>
        public static void Debug(this object context, string message)
        {
            if (!enable) return;
            loggersList?.ForEach(logger => logger.Debug(context, message));
        }

        /// <summary>
        /// Output log within all registered logger in Debug priority
        /// </summary>
        /// <param name="message"> The log message </param>
        public static void Debug(string message) => Debug(null, message);


        /// <summary>
        /// Output log within all registered logger in info priority
        /// </summary>
        /// <param name="context"> The context where output the log </param>
        /// <param name="message"> The log message </param>
        public static void Info(this object context, string message)
        {
            if (!enable) return;
            loggersList?.ForEach(logger => logger.Info(context, message));
        }

        /// <summary>
        /// Output log within all registered logger in Info priority
        /// </summary>
        /// <param name="message"> The log message </param>
        public static void Info(string message) => Info(null, message);

        /// <summary>
        /// Output log within all registered logger in warn priority
        /// </summary>
        /// <param name="context"> The context where output the log </param>
        /// <param name="message"> The log message </param>
        public static void Warn(this object context, string message)
        {
            if (!enable) return;
            loggersList?.ForEach(logger => logger.Warn(context, message));
        }

        /// <summary>
        /// Output log within all registered logger in Warn priority
        /// </summary>
        /// <param name="message"> The log message </param>
        public static void Warn(string message) => Warn(null, message);

        /// <summary>
        /// Output log within all registered logger in error priority
        /// </summary>
        /// <param name="context"> The context where output the log </param>
        /// <param name="message"> The log message </param>
        public static void Error(this object context, string message)
        {
            if (!enable) return;
            loggersList?.ForEach(logger => logger.Error(context, message));
        }

        /// <summary>
        /// Output log within all registered logger in Error priority
        /// </summary>
        /// <param name="message"> The log message </param>
        public static void Error(string message) => Error(null, message);

        /// <summary>
        /// Constructor for YLog
        /// </summary>
        static YVRLog() { RegisterLogger(new YLogLogger()); }

        /// <summary>
        /// Set target logger as the only used logger
        /// </summary>
        /// <param name="targetLogger">Target logger</param>
        public static void SetLogger(LoggerBase targetLogger)
        {
            ClearLoggers();
            RegisterLogger(targetLogger);
        }

        /// <summary>
        /// Clear all registered loggers
        /// </summary>
        public static void ClearLoggers() { loggersList.Clear(); }

        /// <summary>
        /// Register target logger
        /// </summary>
        /// <param name="targetLogger">Target logger</param>
        public static void RegisterLogger(LoggerBase targetLogger) { loggersList.Add(targetLogger); }

        /// <summary>
        /// Register target logger
        /// </summary>
        /// <param name="targetLogger">Target logger</param>
        public static void UnregisterLogger(LoggerBase targetLogger) { loggersList.Remove(targetLogger); }
    }
}