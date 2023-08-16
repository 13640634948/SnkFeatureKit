using System;

namespace SnkFeatureKit.Logging
{
    public class SnkLogFactory : ISnkLogFactory
    {
        protected SnkLogLevel _level = SnkLogLevel.DEBUG;
        public SnkLogLevel Level
        {
            get => _level;
            set => _level = value;
        }

        public virtual ISnkLogger GetLogger(string name) => new SnkConsoleLog(name, this);
        public virtual ISnkLogger GetLogger<T>() => GetLogger(typeof(T).FullName);
        public virtual ISnkLogger GetLogger(Type type) => GetLogger(type.FullName);
    }
}
