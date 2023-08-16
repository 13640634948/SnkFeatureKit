using System;

namespace SnkFeatureKit.Asynchronous
{
    public sealed class SnkAsyncExecutor
        {
            private static ISnkAsyncExecutor s_instance;

            public bool IsMainThread => s_instance.IsMainThread;

            public static object WaitWhile(Func<bool> predicate)
                => s_instance.WaitWhile(predicate);

            public static void RegiestAsyncExecutor<TAsyncExecutor>()
                where TAsyncExecutor : class, ISnkAsyncExecutor, new()
            {
                s_instance = new TAsyncExecutor();
            }
        } 
}