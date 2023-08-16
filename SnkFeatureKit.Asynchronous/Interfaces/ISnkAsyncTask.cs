using System;

namespace SnkFeatureKit.Asynchronous
{
    namespace Interfaces
    {
        public interface ISnkAsyncTask : ISnkAsyncResult
        {
            ISnkAsyncTask OnPreExecute(Action callback, bool runOnMainThread = true);

            ISnkAsyncTask OnPostExecute(Action callback, bool runOnMainThread = true);

            ISnkAsyncTask OnError(Action<Exception> callback, bool runOnMainThread = true);

            ISnkAsyncTask OnFinish(Action callback, bool runOnMainThread = true);

            ISnkAsyncTask Start(int delay);

            ISnkAsyncTask Start();
        }

        public interface ISnkAsyncTask<TResult> : ISnkAsyncResult<TResult>
        {
            ISnkAsyncTask<TResult> OnPreExecute(Action callback, bool runOnMainThread = true);

            ISnkAsyncTask<TResult> OnPostExecute(Action<TResult> callback, bool runOnMainThread = true);

            ISnkAsyncTask<TResult> OnError(Action<Exception> callback, bool runOnMainThread = true);

            ISnkAsyncTask<TResult> OnFinish(Action callback, bool runOnMainThread = true);

            ISnkAsyncTask<TResult> Start(int delay);

            ISnkAsyncTask<TResult> Start();
        }
    }
}