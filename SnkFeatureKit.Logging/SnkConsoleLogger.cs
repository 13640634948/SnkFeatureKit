using System;

namespace SnkFeatureKit.Logging
{
    public class SnkConsoleLogger : SnkLoggerAbstract
    {
        public SnkConsoleLogger(ISnkLoggerFactory loggerFactory, string categoryName) : base(loggerFactory, categoryName)
        {
        }

        public override void Log(SnkLogLevel logLevel, Exception exception = null, string message = "", params object[] args)
        {
            if (string.IsNullOrEmpty(message))
                Console.WriteLine($"[{this.GetType()}][{logLevel}]Exception:{exception?.Message}\n{exception?.StackTrace}");
            else
            {
                if(args == null || args.Length == 0)
                    Console.WriteLine($"[{this.GetType()}][{logLevel}]{message}");
                else
                    Console.WriteLine($"[{this.GetType()}][{logLevel}]{string.Format(message, args)}");
            }
        }
    }
}