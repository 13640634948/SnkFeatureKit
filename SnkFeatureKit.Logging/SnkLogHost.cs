namespace SnkFeatureKit.Logging
{
    public static class SnkLogHost
    {
        public static ISnkLogger Default { get; private set; }
        private static ISnkLoggerFactory _loggerFactory;

        public static void InitializeLogging(ISnkLoggerFactory loggerFactory = null, string defaultCategoryName = "DEFAULT")
        {
            _loggerFactory = loggerFactory ?? new SnkConsoleLoggerFactory();
            Default = _loggerFactory.CreateLogger(defaultCategoryName);
        }

        public static ISnkLogger GetLogger(string categoryName)
            => _loggerFactory?.CreateLogger(categoryName);

        public static ISnkLogger GetLogger<T>()
            => _loggerFactory?.CreateLogger<T>();
    }
}
