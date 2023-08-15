using System;
using System.Collections.Generic;

namespace SnkFeatureKit.Logging
{
    public class SnkLogHost
    {
        private const string DEFAULT = "DEFAULT";

        public static ISnkLogger s_defaultLogger;

        public static ISnkLogger Default 
        {
            get
            {
                if (s_defaultLogger == null)
                    s_defaultLogger = GetLogger(DEFAULT);
                return s_defaultLogger;
            }
        }

        private static readonly Dictionary<string, ISnkLogFactory> s_factoryDict = new Dictionary<string, ISnkLogFactory>();

        public static ISnkLogger GetLogger<T>(string factoryName = DEFAULT) => GetLoggerInternal(typeof(T).FullName, factoryName);

        public static ISnkLogger GetLogger(Type type, string factoryName = DEFAULT) => GetLoggerInternal(type.FullName, factoryName);

        public static ISnkLogger GetLogger(string name, string factoryName = DEFAULT) => GetLoggerInternal(name.ToLower(), factoryName);

        private static ISnkLogger GetLoggerInternal(string name, string factoryName)
        {
            if (s_factoryDict.TryGetValue(factoryName, out var factory))
                return factory.GetLogger(name);
            return null;
        }

        public static void RegistryFactory<TFactory>(string key = DEFAULT)
            where TFactory : class, ISnkLogFactory, new()
        {
            if(string.IsNullOrEmpty(key))
                key = typeof(TFactory).FullName;
            s_factoryDict[key] = new TFactory();
        }   
    }
}
