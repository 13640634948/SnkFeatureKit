using System;

namespace SnkFeatureKit.Logging
{
    public abstract class SnkLogger : ISnkLogger 
    {
        
        public abstract void Debug(object message);
        public abstract void Info(object message);
        public abstract void Warn(object message);
        public abstract void Error(object message);
        public abstract void Fatal(object message);
        
        private readonly ISnkLogFactory _factory;
        private readonly string _name;

        protected SnkLogger(string name, ISnkLogFactory factory) 
        {
            this._name = name;
            this._factory = factory;
        }
        
        private bool IsEnabled(SnkLogLevel level) => SnkLogLevel.DEBUG >= this._factory.Level;

        protected virtual string Format(object message, string level)
            => $"{System.DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} [{level}] {_name} - {message}";
        
        public virtual bool IsDebugEnabled => IsEnabled(SnkLogLevel.DEBUG);
        public virtual bool IsInfoEnabled => IsEnabled(SnkLogLevel.INFO);
        public virtual bool IsWarnEnabled => IsEnabled(SnkLogLevel.WARN);
        public virtual bool IsErrorEnabled => IsEnabled(SnkLogLevel.ERROR);
        public virtual bool IsFatalEnabled => IsEnabled(SnkLogLevel.FATAL);
        
        public virtual void Debug(object message, Exception exception) => Debug($"{message} Exception:{exception}");
        public virtual void DebugFormat(string format, params object[] args) => Debug(string.Format(format, args));

        public virtual void Info(object message, Exception exception) => Info($"{message} Exception:{exception}");
        public virtual void InfoFormat(string format, params object[] args) => Info(string.Format(format, args));

        public virtual void Warn(object message, Exception exception) => Warn($"{message} Exception:{exception}");
        public virtual void WarnFormat(string format, params object[] args) => Warn(string.Format(format, args));

        public virtual void Error(object message, Exception exception) => Error($"{message} Exception:{exception}");
        public virtual void ErrorFormat(string format, params object[] args) => Error(string.Format(format, args));

        public virtual void Fatal(object message, Exception exception) => Fatal($"{message} Exception:{exception}");
        public virtual void FatalFormat(string format, params object[] args) => Fatal(string.Format(format, args));
    }
}
