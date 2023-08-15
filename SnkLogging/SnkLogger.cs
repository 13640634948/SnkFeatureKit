using System;

namespace SnkFeatureKit.Logging
{
    public abstract class SnkLogger : ISnkLogger 
    {
        protected bool IsEnabled(SnkLogLevel level) => level >= this._factory.Level;

        public virtual bool IsDebugEnabled => IsEnabled(SnkLogLevel.DEBUG);
        public virtual bool IsInfoEnabled => IsEnabled(SnkLogLevel.INFO);
        public virtual bool IsWarnEnabled => IsEnabled(SnkLogLevel.WARN);
        public virtual bool IsErrorEnabled => IsEnabled(SnkLogLevel.ERROR);
        public virtual bool IsFatalEnabled => IsEnabled(SnkLogLevel.FATAL);

        private ISnkLogFactory _factory;
        private string _name;
        public SnkLogger(string name, ISnkLogFactory factory) 
        {
            this._name = name;
            this._factory = factory;
        }

        protected virtual string Format(object message, string level)
            => string.Format("{0:yyyy-MM-dd HH:mm:ss.fff} [{1}] {2} - {3}", System.DateTime.Now, level, _name, message);

        public abstract void Debug(object message);
        public abstract void Info(object message);
        public abstract void Warn(object message);
        public abstract void Error(object message);
        public abstract void Fatal(object message);

        public virtual void Debug(object message, Exception exception) => Debug(string.Format("{0} Exception:{1}", message, exception));
        public virtual void DebugFormat(string format, params object[] args) => Debug(string.Format(format, args));

        public virtual void Info(object message, Exception exception) => Info(string.Format("{0} Exception:{1}", message, exception));
        public virtual void InfoFormat(string format, params object[] args) => Info(string.Format(format, args));

        public virtual void Warn(object message, Exception exception) => Warn(string.Format("{0} Exception:{1}", message, exception));
        public virtual void WarnFormat(string format, params object[] args) => Warn(string.Format(format, args));

        public virtual void Error(object message, Exception exception) => Error(string.Format("{0} Exception:{1}", message, exception));
        public virtual void ErrorFormat(string format, params object[] args) => Error(string.Format(format, args));

        public virtual void Fatal(object message, Exception exception) => Fatal(string.Format("{0} Exception:{1}", message, exception));
        public virtual void FatalFormat(string format, params object[] args) => Fatal(string.Format(format, args));
    }
}
