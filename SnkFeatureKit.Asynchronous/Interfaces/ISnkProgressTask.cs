using System;

namespace SnkFeatureKit.Asynchronous
{
    namespace Interfaces
    {
        public interface ISnkProgressTask<TProgress> : ISnkProgressResult<TProgress>
        {
            ISnkProgressTask<TProgress> OnPreExecute(Action callback, bool runOnMainThread = true);

            ISnkProgressTask<TProgress> OnPostExecute(Action callback, bool runOnMainThread = true);

            ISnkProgressTask<TProgress> OnError(Action<Exception> callback, bool runOnMainThread = true);

            ISnkProgressTask<TProgress> OnFinish(Action callback, bool runOnMainThread = true);

            ISnkProgressTask<TProgress> OnProgressUpdate(Action<TProgress> callback, bool runOnMainThread = true);

            ISnkProgressTask<TProgress> Start(int delay);

            ISnkProgressTask<TProgress> Start();
        }

        public interface ISnkProgressTask<TProgress, TResult> : ISnkProgressResult<TProgress, TResult>
        {
            ISnkProgressTask<TProgress, TResult> OnPreExecute(Action callback, bool runOnMainThread = true);

            ISnkProgressTask<TProgress, TResult> OnPostExecute(Action<TResult> callback, bool runOnMainThread = true);

            ISnkProgressTask<TProgress, TResult> OnError(Action<Exception> callback, bool runOnMainThread = true);

            ISnkProgressTask<TProgress, TResult> OnFinish(Action callback, bool runOnMainThread = true);

            ISnkProgressTask<TProgress, TResult> OnProgressUpdate(Action<TProgress> callback, bool runOnMainThread = true);

            ISnkProgressTask<TProgress, TResult> Start(int delay);

            ISnkProgressTask<TProgress, TResult> Start();
        }
    }
}