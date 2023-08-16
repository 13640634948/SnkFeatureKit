namespace SnkFeatureKit.Asynchronous
{
    namespace Interfaces
    {
        public interface ISnkProgressPromise<TProgress> : ISnkPromise
        {
            TProgress Progress { get; }

            void UpdateProgress(TProgress progress);
        }

        public interface ISnkProgressPromise<TProgress, TResult> : ISnkProgressPromise<TProgress>, ISnkPromise<TResult>
        {
        }
    }
}