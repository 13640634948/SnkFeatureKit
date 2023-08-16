using System;

namespace SnkFeatureKit.Asynchronous
{
    namespace Interfaces
    {
        public interface ISnkPromise
        {
            object Result { get; }

            Exception Exception { get; }

            bool IsDone { get; }

            bool IsCancelled { get; }

            bool IsCancellationRequested { get; }

            void SetCancelled();

            void SetException(string error);

            void SetException(Exception exception);

            void SetResult(object result = null);
        }

        public interface ISnkPromise<TResult> : ISnkPromise
        {
            new TResult Result { get; }

            void SetResult(TResult result);
        }
    }
}