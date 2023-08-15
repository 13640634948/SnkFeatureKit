using System;
using System.Collections.Generic;

namespace SnkFeatureKit.Logging
{
    public static class SnkLogHost
    {
        private const string DEFAULT = "DEFAULT";

        private static ISnkLogger s_defaultLogger;

        public static ISnkLogger Default => s_defaultLogger ?? (s_defaultLogger = InternalGetLogger(DEFAULT));

        private static readonly Dictionary<string, ISnkLogFactory> s_factoryDict = new Dictionary<string, ISnkLogFactory>();

        public static ISnkLogger GetLogger<T>(string factoryName = DEFAULT) => GetLoggerInternal(typeof(T).FullName, factoryName);

        public static ISnkLogger GetLogger(Type type, string factoryName = DEFAULT) => GetLoggerInternal(type.FullName, factoryName);

        private static ISnkLogger InternalGetLogger(string name, string factoryName = DEFAULT) => GetLoggerInternal(name.ToLower(), factoryName);

        private static ISnkLogger GetLoggerInternal(string name, string factoryName)
            => s_factoryDict.TryGetValue(factoryName, out var factory) ? factory.GetLogger(name) : null;

        public static void RegistryFactory<TFactory>(string key = DEFAULT)
            where TFactory : class, ISnkLogFactory, new()
        {
            if(string.IsNullOrEmpty(key))
                key = typeof(TFactory).FullName;
            s_factoryDict[key] = new TFactory();
        }   
    }
}
