using Microsoft.Extensions.Logging;

namespace SnkFeatureKit.Patcher
{
    public class SnkLogHost
    {
        public static ILogger Default { get; private set; }
        private static ILoggerFactory _loggerFactory;

        public static void InitializeLogging(ILoggerFactory loggerFactory, ILoggerProvider logProvider, string defaultCategoryName = "PATCH")
        {
            if (loggerFactory == null || logProvider == null)
                return;

            loggerFactory.AddProvider(logProvider);
            _loggerFactory = loggerFactory;

            Default = _loggerFactory.CreateLogger(defaultCategoryName);
        }

        public static ILogger GetLogger(string categoryName)
            => _loggerFactory?.CreateLogger(categoryName);

        public static ILogger GetLogger<T>()
            => _loggerFactory?.CreateLogger<T>();
    }
}
