namespace SnkFeatureKit.Logging
{
    public interface ISnkLoggerFactory
    {
        SnkLogLevel LogLevel { get; }
        void SetLogLevel(SnkLogLevel logLevel);
        ISnkLogger CreateLogger(string categoryName);
        ISnkLogger CreateLogger<T>();
    }
}