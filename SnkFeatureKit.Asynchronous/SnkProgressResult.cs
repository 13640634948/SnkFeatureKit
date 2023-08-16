using SnkFeatureKit.Asynchronous.Interfaces;
using SnkFeatureKit.Asynchronous.Internals;

namespace SnkFeatureKit.Asynchronous
{
    public class SnkProgressResult<TProgress> : SnkAsyncResult, ISnkProgressResult<TProgress>, ISnkProgressPromise<TProgress>
        {
            private SnkProgressCallbackable<TProgress> callbackable;
            protected TProgress _progress;
            public virtual TProgress Progress => this._progress;

            public SnkProgressResult() : this(false)
            {
            }

            public SnkProgressResult(bool cancelable) : base(cancelable)
            {
            }

            protected override void RaiseOnCallback()
            {
                base.RaiseOnCallback();
                if (this.callbackable != null)
                    this.callbackable.RaiseOnCallback();
            }

            protected virtual void RaiseOnProgressCallback(TProgress progress)
            {
                if (this.callbackable != null)
                    this.callbackable.RaiseOnProgressCallback(progress);
            }

            public new virtual ISnkProgressCallbackable<TProgress> Callbackable()
            {
                lock (_lock)
                {
                    return this.callbackable ?? (this.callbackable = new SnkProgressCallbackable<TProgress>(this));
                }
            }

            public virtual void UpdateProgress(TProgress progress)
            {
                this._progress = progress;
                this.RaiseOnProgressCallback(progress);
            }
        }

        public class SnkProgressResult<TProgress, TResult> : SnkProgressResult<TProgress>, ISnkProgressResult<TProgress, TResult>, ISnkProgressPromise<TProgress, TResult>
        {
            //private static readonly ILog log = LogManager.GetLogger(typeof(ProgressResult<TProgress, TResult>));

            private SnkCallbackable<TResult> callbackable;
            private SnkProgressCallbackable<TProgress, TResult> progressCallbackable;
            private SnkSynchronizable<TResult> synchronizable;

            public SnkProgressResult() : this(false)
            {
            }

            public SnkProgressResult(bool cancelable) : base(cancelable)
            {
            }

            public virtual new TResult Result
            {
                get
                {
                    var result = base.Result;
                    return result != null ? (TResult)result : default(TResult);
                }
            }

            public virtual void SetResult(TResult result)
            {
                base.SetResult(result);
            }

            protected override void RaiseOnCallback()
            {
                base.RaiseOnCallback();
                if (this.callbackable != null)
                    this.callbackable.RaiseOnCallback();
                if (this.progressCallbackable != null)
                    this.progressCallbackable.RaiseOnCallback();
            }

            protected override void RaiseOnProgressCallback(TProgress progress)
            {
                base.RaiseOnProgressCallback(progress);
                if (this.progressCallbackable != null)
                    this.progressCallbackable.RaiseOnProgressCallback(progress);
            }

            public new virtual ISnkProgressCallbackable<TProgress, TResult> Callbackable()
            {
                lock (_lock)
                {
                    return this.progressCallbackable ?? (this.progressCallbackable = new SnkProgressCallbackable<TProgress, TResult>(this));
                }
            }
            public new virtual ISnkSynchronizable<TResult> Synchronized()
            {
                lock (_lock)
                {
                    return this.synchronizable ?? (this.synchronizable = new SnkSynchronizable<TResult>(this, this._lock));
                }
            }

            ISnkCallbackable<TResult> ISnkAsyncResult<TResult>.Callbackable()
            {
                lock (_lock)
                {
                    return this.callbackable ?? (this.callbackable = new SnkCallbackable<TResult>(this));
                }
            }
        
    }
}