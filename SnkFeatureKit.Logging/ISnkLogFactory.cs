using System;

namespace SnkFeatureKit.Logging
{
    public interface ISnkLogFactory 
    {
        SnkLogLevel Level { get; }
        ISnkLogger GetLogger<T>();

        ISnkLogger GetLogger(Type type);

        ISnkLogger GetLogger(string name);
    }
}
