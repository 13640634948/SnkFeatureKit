using System;

namespace SnkFeatureKit.Logging
{
    public class SnkLogFactory : ISnkLogFactory
    {
        public SnkLogLevel Level => throw new NotImplementedException();

        public virtual ISnkLogger GetLogger(string name) => new SnkConsoleLog(name, this);
        public virtual ISnkLogger GetLogger<T>() => GetLogger(typeof(T).FullName);
        public virtual ISnkLogger GetLogger(Type type) => GetLogger(type.FullName);
    }
}
