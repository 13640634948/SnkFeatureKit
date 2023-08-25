namespace SnkFeatureKit.Logging
{
    public abstract class SnkLoggerFactoryAbstract : ISnkLoggerFactory
    {
        public SnkLogLevel LogLevel { get; protected set; }

        public void SetLogLevel(SnkLogLevel logLevel)
            => this.LogLevel = logLevel;

        public abstract ISnkLogger CreateLogger(string categoryName);

        public abstract ISnkLogger CreateLogger<T>();
    }
}