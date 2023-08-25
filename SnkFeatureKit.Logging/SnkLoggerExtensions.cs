namespace SnkFeatureKit.Logging
{
    public static class SnkLoggerExtensions
    {
        public static void LogTrace(this ISnkLogger logger, string message, params object[] args)
            => logger.Log(SnkLogLevel.Trace, null, message, args);
        public static void LogTrace(this ISnkLogger logger, System.Exception exception, string message = "", params object[] args)
            => logger.Log(SnkLogLevel.Trace, exception, message, args);
        
        public static void LogDebug(this ISnkLogger logger, string message, params object[] args)
            => logger.Log(SnkLogLevel.Debug, null, message, args);
        public static void LogDebug(this ISnkLogger logger, System.Exception exception, string message = "", params object[] args)
            => logger.Log(SnkLogLevel.Debug, exception, message, args);
        
        public static void LogInfo(this ISnkLogger logger, string message, params object[] args)
            => logger.Log(SnkLogLevel.Info, null, message, args);
        public static void LogInfo(this ISnkLogger logger, System.Exception exception, string message = "", params object[] args)
            => logger.Log(SnkLogLevel.Info, exception, message, args);
        
        public static void LogWarn(this ISnkLogger logger, string message, params object[] args)
            => logger.Log(SnkLogLevel.Warn, null, message, args);
        public static void LogWarn(this ISnkLogger logger, System.Exception exception, string message = "", params object[] args)
            => logger.Log(SnkLogLevel.Warn, exception, message, args);
        public static void LogError(this ISnkLogger logger, string message, params object[] args)
            => logger.Log(SnkLogLevel.Error, null, message, args);
        public static void LogError(this ISnkLogger logger, System.Exception exception, string message = "", params object[] args)
            => logger.Log(SnkLogLevel.Error, exception, message, args);
        
        public static void LogCritical(this ISnkLogger logger, string message, params object[] args)
            => logger.Log(SnkLogLevel.Critical, null, message, args);
        public static void LogCritical(this ISnkLogger logger, System.Exception exception, string message = "", params object[] args)
            => logger.Log(SnkLogLevel.Critical, exception, message, args);
    }
}