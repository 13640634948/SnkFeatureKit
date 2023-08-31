namespace SnkFeatureKit.Logging
{
    public class SnkConsoleLoggerFactory : SnkLoggerFactoryAbstract
    {
        public override ISnkLogger CreateLogger(string categoryName) => new SnkConsoleLogger(this, categoryName);
    }
}