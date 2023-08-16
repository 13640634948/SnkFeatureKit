using System.Runtime.CompilerServices;

namespace SnkFeatureKit.Asynchronous
{
    namespace Interfaces
    {
        public interface ISnkAwaiter : ICriticalNotifyCompletion
        {
            bool IsCompleted { get; }

            void GetResult();
        }

        public interface IAwaiter<T> : ISnkAwaiter
        {
            new T GetResult();
        }
    }
}