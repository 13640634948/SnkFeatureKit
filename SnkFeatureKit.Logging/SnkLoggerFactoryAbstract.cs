namespace SnkFeatureKit.Logging
{
    public abstract class SnkLoggerFactoryAbstract : ISnkLoggerFactory
    {
        public SnkLogLevel LogLevel { get; private set; }

        public void SetLogLevel(SnkLogLevel logLevel)
            => this.LogLevel = logLevel;

        public abstract ISnkLogger CreateLogger(string categoryName);

        public virtual ISnkLogger CreateLogger<T>()
            => CreateLogger(typeof(T).Name);
    }
}