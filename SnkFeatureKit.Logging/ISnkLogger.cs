namespace SnkFeatureKit.Logging
{
    public interface ISnkLogger
    {
        public string CategoryName { get; }

        bool IsEnabled(SnkLogLevel logLevel);
        void Log(SnkLogLevel logLevel, System.Exception exception = null, string message = "", params object[] args);
    }
}