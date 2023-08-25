namespace SnkFeatureKit.Logging
{
    public static class SnkLogHost
    {
        public static ISnkLogger Default { get; private set; }
        private static ISnkLoggerFactory _loggerFactory;

        public static void InitializeLogging(ISnkLoggerFactory loggerFactory, string defaultCategoryName = "DEFAULT")
        {
            if (loggerFactory == null)
                return;
            _loggerFactory = loggerFactory;
            Default = _loggerFactory.CreateLogger(defaultCategoryName);
        }

        public static ISnkLogger GetLogger(string categoryName)
            => _loggerFactory?.CreateLogger(categoryName);

        public static ISnkLogger GetLogger<T>()
            => _loggerFactory?.CreateLogger<T>();
    }
}
