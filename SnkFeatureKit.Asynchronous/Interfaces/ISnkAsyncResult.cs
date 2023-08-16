using System;

namespace SnkFeatureKit.Asynchronous
{
    namespace Interfaces
    {
        public interface ISnkAsyncResult
        {
            object Result { get; }

            Exception Exception { get; }

            bool IsDone { get; }

            bool IsCancelled { get; }

            bool Cancel();

            ISnkCallbackable Callbackable();

            ISnkSynchronizable Synchronized();

            object WaitForDone();
        }

        public interface ISnkAsyncResult<TResult> : ISnkAsyncResult
        {
            new TResult Result { get; }

            new ISnkCallbackable<TResult> Callbackable();

            new ISnkSynchronizable<TResult> Synchronized();
        }
    }
}