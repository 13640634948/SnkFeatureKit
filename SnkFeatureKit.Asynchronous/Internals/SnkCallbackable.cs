using System;
using SnkFeatureKit.Asynchronous.Interfaces;
using SnkFeatureKit.Logging;

namespace SnkFeatureKit.Asynchronous
{
    namespace Internals
    {
        internal class SnkCallbackable : ISnkCallbackable
        {
            private static readonly ISnkLogger log = SnkLogHost.GetLogger<SnkCallbackable>();

            private ISnkAsyncResult result;
            private readonly object _lock = new object();
            private Action<ISnkAsyncResult> callback;

            public SnkCallbackable(ISnkAsyncResult result)
            {
                this.result = result;
            }

            public void RaiseOnCallback()
            {
                lock (_lock)
                {
                    try
                    {
                        if (this.callback == null)
                            return;

                        var list = this.callback.GetInvocationList();
                        this.callback = null;

                        foreach (Action<ISnkAsyncResult> action in list)
                        {
                            try
                            {
                                action(this.result);
                            }
                            catch (Exception e)
                            {
                                if (log.IsWarnEnabled)
                                    log.WarnFormat("Class[{0}] callback exception.Error:{1}", this.GetType(), e);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        if (log.IsWarnEnabled)
                            log.WarnFormat("Class[{0}] callback exception.Error:{1}", this.GetType(), e);
                    }
                }
            }

            public void OnCallback(Action<ISnkAsyncResult> callback)
            {
                lock (_lock)
                {
                    if (callback == null)
                        return;

                    if (this.result.IsDone)
                    {
                        try
                        {
                            callback(this.result);
                        }
                        catch (Exception e)
                        {
                            if (log.IsWarnEnabled)
                                log.WarnFormat("Class[{0}] callback exception.Error:{1}", this.GetType(), e);
                        }
                        return;
                    }

                    this.callback += callback;
                }
            }
        }

        internal class SnkCallbackable<TResult> : ISnkCallbackable<TResult>
        {
            private static readonly ISnkLogger log = SnkLogHost.GetLogger<SnkCallbackable<TResult>>();

            private ISnkAsyncResult<TResult> result;
            private readonly object _lock = new object();
            private Action<ISnkAsyncResult<TResult>> callback;

            public SnkCallbackable(ISnkAsyncResult<TResult> result)
            {
                this.result = result;
            }

            public void RaiseOnCallback()
            {
                lock (_lock)
                {
                    try
                    {
                        if (this.callback == null)
                            return;

                        var list = this.callback.GetInvocationList();
                        this.callback = null;

                        foreach (Action<ISnkAsyncResult<TResult>> action in list)
                        {
                            try
                            {
                                action(this.result);
                            }
                            catch (Exception e)
                            {
                                if (log.IsWarnEnabled)
                                    log.WarnFormat("Class[{0}] callback exception.Error:{1}", this.GetType(), e);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        if (log.IsWarnEnabled)
                            log.WarnFormat("Class[{0}] callback exception.Error:{1}", this.GetType(), e);
                    }
                }
            }

            public void OnCallback(Action<ISnkAsyncResult<TResult>> callback)
            {
                lock (_lock)
                {
                    if (callback == null)
                        return;

                    if (this.result.IsDone)
                    {
                        try
                        {
                            callback(this.result);
                        }
                        catch (Exception e)
                        {
                            if (log.IsWarnEnabled)
                                log.WarnFormat("Class[{0}] callback exception.Error:{1}", this.GetType(), e);
                        }
                        return;
                    }

                    this.callback += callback;
                }
            }
        }
    }
}