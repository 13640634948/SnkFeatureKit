using System;
using System.Threading.Tasks;
using SnkFeatureKit.Logging;
using SnkFeatureKit.Patcher.Interfaces;

namespace SnkFeatureKit.Patcher
{
    public abstract class SnkPatchControllerAbstract : ISnkPatchController
    {
        protected readonly ISnkLogger logger;
        protected readonly SnkPatchControlSettings settings;
        protected readonly ISnkJsonParser jsonParser;

        protected ISnkLocalPatchRepository localRepo;
        protected ISnkRemotePatchRepository remoteRepo;

        public SNK_PATCH_CTRL_STATE status { get; protected set; }

        public virtual string ChannelName => this.settings.channelName;
        public virtual int AppVersion => this.settings.appVersion;
        public virtual SnkPatchControlSettings Settings => this.settings;

        public virtual ushort LocalResVersion => this.localRepo.Version;
        public virtual ushort RemoteResVersion => this.remoteRepo.Version;
        public virtual long DownloadedSize => this.remoteRepo.DownloadedSize;
        public virtual double DownloadProgress 
        {
            get
            {
                if (TotalDownloadSize == 0 || DownloadedSize == 0)
                    return 0;
                var progress = DownloadedSize / (double)TotalDownloadSize;
                if (progress < 0)
                    return 0;
                if (progress > 1)
                    return 1;
                return progress;
            }
        }
        public virtual long DownloadSpeed  => this.remoteRepo.RecentSpeed;


        protected bool _initialized { get; private set; } = false;
        protected bool _disposed { get; private set; } = false;

        protected SnkPatchControllerAbstract(ISnkLocalPatchRepository localRepo, ISnkRemotePatchRepository remoteRepo, SnkPatchControlSettings settings, ISnkJsonParser jsonParser)
        {
            this.localRepo = localRepo;
            this.remoteRepo = remoteRepo;
            this.settings = settings;
            this.jsonParser = jsonParser;
            this.logger = SnkLogHost.GetLogger(this.GetType().Name);
        }
        
        public virtual async Task Initialize()
        {
            status = SNK_PATCH_CTRL_STATE.initializing;
            var localInitTask = this.localRepo.Initialize(this, jsonParser);
            var remoteInfoTask = this.remoteRepo.Initialize(this, jsonParser);
            await Task.WhenAll(localInitTask, remoteInfoTask).ConfigureAwait(false);

            if (this.localRepo.IsError)
                throw new System.Exception(this.localRepo.ExceptionString);

            if (this.remoteRepo.IsError)
                throw new System.Exception(this.remoteRepo.ExceptionString);
            
            status = SNK_PATCH_CTRL_STATE.initialized;
            _initialized = true;
        }

        public virtual bool TryGetSourceInfo(string key, bool fromLocalResp, out SnkSourceInfo sourceInfo)
        {
            if (fromLocalResp)
                return this.localRepo.TryGetSourceInfo(key, out sourceInfo);
            return this.remoteRepo.TryGetSourceInfo(key, out sourceInfo);
        }
        
        public virtual bool SourceExists(string key, bool fromLocalResp = true)
        {
            if (fromLocalResp)
                return this.localRepo.Exists(key);
            return this.remoteRepo.Exists(key);
        }

        protected async Task ValidityInitialization()
        {
            if (this._initialized == false)
            {
                if (this.status == SNK_PATCH_CTRL_STATE.none)
                    await this.Initialize();
                else
                {
                    throw new Exception("No Initialize or Initializing");
                }
            }
        }

        ~SnkPatchControllerAbstract()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_disposed == false)
                {
                    this.localRepo.Dispose();
                    this.remoteRepo.Dispose();
                    _disposed = true;
                }
            }
        }
        
        public abstract long TotalDownloadSize { get; }
        public abstract Task<long> TryUpdate();
        public abstract Task Apply(Func<Task<bool>> onExceptionCallBack);
    }
}