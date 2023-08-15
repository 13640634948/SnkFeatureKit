using System;

namespace SnkFeatureKit.Logging
{
    public class SnkConsoleLog : SnkLogger
    {
        public SnkConsoleLog(string name, ISnkLogFactory factory) : base(name, factory)
        {
        }

        public override void Debug(object message)=> Console.WriteLine(Format(message, "DEBUG"));
        public override void Info(object message) => Console.WriteLine(Format(message, "INFO"));
        public override void Warn(object message) => Console.WriteLine(Format(message, "WARN"));
        public override void Error(object message) => Console.WriteLine(Format(message, "ERROR"));
        public override void Fatal(object message) => Console.WriteLine(Format(message, "FATAL"));
    }
}
