namespace SnkFeatureKit.Logging
{
    public interface ISnkLogger
    {
        string CategoryName { get; }

        bool IsEnabled(SnkLogLevel logLevel);
        void Log(SnkLogLevel logLevel, System.Exception exception = null, string message = "", params object[] args);
    }
}