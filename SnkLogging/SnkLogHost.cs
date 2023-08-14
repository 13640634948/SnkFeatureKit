using System;
using System.Collections.Generic;

namespace SnkToolKit.Features.Logging
{
    public interface ISnkLogger
    {
        void Debug(object message);

        void Debug(object message, Exception exception);

        void DebugFormat(string format, params object[] args);


        void Info(object message);

        void Info(object message, Exception exception);

        void InfoFormat(string format, params object[] args);


        void Warn(object message);

        void Warn(object message, Exception exception);

        void WarnFormat(string format, params object[] args);


        void Error(object message);

        void Error(object message, Exception exception);

        void ErrorFormat(string format, params object[] args);


        void Fatal(object message);

        void Fatal(object message, Exception exception);

        void FatalFormat(string format, params object[] args);


        bool IsDebugEnabled { get; }

        bool IsInfoEnabled { get; }

        bool IsWarnEnabled { get; }

        bool IsErrorEnabled { get; }

        bool IsFatalEnabled { get; }
    }

    public interface ISnkLogFactory 
    {
        ISnkLogger GetLogger<T>();

        ISnkLogger GetLogger(Type type);

        ISnkLogger GetLogger(string name);
    }

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
