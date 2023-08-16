namespace SnkFeatureKit.Asynchronous
{
    namespace Interfaces
    {
        public interface ISnkProgressResult<TProgress> : ISnkAsyncResult
        {
            TProgress Progress { get; }

            new ISnkProgressCallbackable<TProgress> Callbackable();
        }


        public interface ISnkProgressResult<TProgress, TResult> : ISnkAsyncResult<TResult>, ISnkProgressResult<TProgress>
        {
            new ISnkProgressCallbackable<TProgress, TResult> Callbackable();
        }
    }
}