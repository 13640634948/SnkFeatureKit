using System;
using SnkFeatureKit.Asynchronous.Interfaces;
using Microsoft.Extensions.Logging;

namespace SnkFeatureKit.Asynchronous
{
    namespace Internals
    {
        internal class SnkProgressCallbackable<TProgress> : ISnkProgressCallbackable<TProgress>
        {
            private static readonly ILogger logger = SnkLogHost.GetLogger<SnkProgressCallbackable<TProgress>>();

            private ISnkProgressResult<TProgress> result;
            private readonly object _lock = new object();
            private Action<ISnkProgressResult<TProgress>> callback;
            private Action<TProgress> progressCallback;

            public SnkProgressCallbackable(ISnkProgressResult<TProgress> result)
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

                        foreach (Action<ISnkProgressResult<TProgress>> action in list)
                        {
                            try
                            {
                                action(this.result);
                            }
                            catch (Exception e)
                            {
                                if (logger != null && logger.IsEnabled(LogLevel.Warning))
                                    logger.LogWarning("Class[{0}] callback exception.Error:{1}", this.GetType(), e);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        if (logger != null && logger.IsEnabled(LogLevel.Warning))
                            logger.LogWarning("Class[{0}] callback exception.Error:{1}", this.GetType(), e);
                    }
                    finally
                    {
                        this.progressCallback = null;
                    }
                }
            }

            public void RaiseOnProgressCallback(TProgress progress)
            {
                lock (_lock)
                {
                    try
                    {
                        if (this.progressCallback == null)
                            return;

                        var list = this.progressCallback.GetInvocationList();
                        foreach (Action<TProgress> action in list)
                        {
                            try
                            {
                                action(progress);
                            }
                            catch (Exception e)
                            {
                                if (logger != null && logger.IsEnabled(LogLevel.Warning))
                                    logger.LogWarning("Class[{0}] progress callback exception.Error:{1}", this.GetType(), e);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        if (logger != null && logger.IsEnabled(LogLevel.Warning))
                            logger.LogWarning("Class[{0}] progress callback exception.Error:{1}", this.GetType(), e);
                    }
                }
            }

            public void OnCallback(Action<ISnkProgressResult<TProgress>> callback)
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
                            if (logger != null && logger.IsEnabled(LogLevel.Warning))
                                logger.LogWarning("Class[{0}] callback exception.Error:{1}", this.GetType(), e);
                        }
                        return;
                    }

                    this.callback += callback;
                }
            }

            public void OnProgressCallback(Action<TProgress> callback)
            {
                lock (_lock)
                {
                    if (callback == null)
                        return;

                    if (this.result.IsDone)
                    {
                        try
                        {
                            callback(this.result.Progress);
                        }
                        catch (Exception e)
                        {
                            if (logger != null && logger.IsEnabled(LogLevel.Warning))
                                logger.LogWarning("Class[{0}] progress callback exception.Error:{1}", this.GetType(), e);
                        }
                        return;
                    }

                    this.progressCallback += callback;
                }
            }
        }

        internal class SnkProgressCallbackable<TProgress, TResult> : ISnkProgressCallbackable<TProgress, TResult>
        {
            private static readonly ILogger logger = SnkLogHost.GetLogger<SnkProgressCallbackable<TProgress, TResult>>();

            private ISnkProgressResult<TProgress, TResult> result;
            private readonly object _lock = new object();
            private Action<ISnkProgressResult<TProgress, TResult>> callback;
            private Action<TProgress> progressCallback;

            public SnkProgressCallbackable(ISnkProgressResult<TProgress, TResult> result)
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

                        foreach (Action<ISnkProgressResult<TProgress, TResult>> action in list)
                        {
                            try
                            {
                                action(this.result);
                            }
                            catch (Exception e)
                            {
                                if (logger != null && logger.IsEnabled(LogLevel.Warning))
                                    logger.LogWarning("Class[{0}] callback exception.Error:{1}", this.GetType(), e);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        if (logger != null && logger.IsEnabled(LogLevel.Warning))
                            logger.LogWarning("Class[{0}] callback exception.Error:{1}", this.GetType(), e);
                    }
                    finally
                    {
                        this.progressCallback = null;
                    }
                }
            }

            public void RaiseOnProgressCallback(TProgress progress)
            {
                lock (_lock)
                {
                    try
                    {
                        if (this.progressCallback == null)
                            return;

                        var list = this.progressCallback.GetInvocationList();
                        foreach (Action<TProgress> action in list)
                        {
                            try
                            {
                                action(progress);
                            }
                            catch (Exception e)
                            {
                                if (logger != null && logger.IsEnabled(LogLevel.Warning))
                                    logger.LogWarning("Class[{0}] progress callback exception.Error:{1}", this.GetType(), e);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        if (logger != null && logger.IsEnabled(LogLevel.Warning))
                            logger.LogWarning("Class[{0}] progress callback exception.Error:{1}", this.GetType(), e);
                    }
                }
            }

            public void OnCallback(Action<ISnkProgressResult<TProgress, TResult>> callback)
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
                            if (logger != null && logger.IsEnabled(LogLevel.Warning))
                                logger.LogWarning("Class[{0}] callback exception.Error:{1}", this.GetType(), e);
                        }
                        return;
                    }

                    this.callback += callback;
                }
            }

            public void OnProgressCallback(Action<TProgress> callback)
            {
                lock (_lock)
                {
                    if (callback == null)
                        return;

                    if (this.result.IsDone)
                    {
                        try
                        {
                            callback(this.result.Progress);
                        }
                        catch (Exception e)
                        {
                            if (logger != null && logger.IsEnabled(LogLevel.Warning))
                                logger.LogWarning("Class[{0}] progress callback exception.Error:{1}", this.GetType(), e);
                        }
                        return;
                    }

                    this.progressCallback += callback;
                }
            }
        }
    }
}