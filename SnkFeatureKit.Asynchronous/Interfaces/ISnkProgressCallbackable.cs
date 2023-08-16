using System;

namespace SnkFeatureKit.Asynchronous
{
    namespace Interfaces
    {
        public interface ISnkProgressCallbackable<TProgress>
        {
            void OnCallback(Action<ISnkProgressResult<TProgress>> callback);

            void OnProgressCallback(Action<TProgress> callback);
        }

        public interface ISnkProgressCallbackable<TProgress, TResult>
        {
            void OnCallback(Action<ISnkProgressResult<TProgress, TResult>> callback);

            void OnProgressCallback(Action<TProgress> callback);
        }
    }
}