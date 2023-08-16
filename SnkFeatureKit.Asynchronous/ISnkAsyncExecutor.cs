using System;

namespace SnkFeatureKit.Asynchronous
{
    public interface ISnkAsyncExecutor
    {
        object WaitWhile(Func<bool> predicate);
        bool IsMainThread { get; }
    }
}