using System;

namespace SnkFeatureKit.Asynchronous
{
    namespace Interfaces
    {
        public interface ISnkCallbackable
        {
            void OnCallback(Action<ISnkAsyncResult> callback);
        }

        public interface ISnkCallbackable<TResult>
        {
            void OnCallback(Action<ISnkAsyncResult<TResult>> callback);
        }
    }
}