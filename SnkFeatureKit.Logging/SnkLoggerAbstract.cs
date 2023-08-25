using System;

namespace SnkFeatureKit.Logging
{
    public abstract class SnkLoggerAbstract : ISnkLogger
    {
        private readonly ISnkLoggerFactory _loggerFactory;
        public string CategoryName { get; }

        protected SnkLoggerAbstract(ISnkLoggerFactory loggerFactory, string categoryName)
        {
            this._loggerFactory = loggerFactory;
            this.CategoryName = categoryName;
        }

        public bool IsEnabled(SnkLogLevel logLevel)
            => logLevel >= _loggerFactory.LogLevel;

        public abstract void Log(SnkLogLevel logLevel, Exception exception = null, string message = "", params object[] args);
    }
}