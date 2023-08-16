using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using SnkFeatureKit.Logging;
using SnkFeatureKit.Patcher.Interfaces;

namespace SnkFeatureKit.Patcher
{
    namespace Implements
    {
        public class SnkPatchController<TLocalRepo, TRemoteRepo> : ISnkPatchController
            where TLocalRepo : class, ISnkLocalPatchRepository, new()
            where TRemoteRepo : class, ISnkRemotePatchRepository, new()
        {
            public enum STATE
            {
                none,
                initializing,
                initialized,
                preview_diff_begin,
                preview_diff_end,
                downloading,
                downloaded
            }

            private static readonly ISnkLogger s_log = SnkLogHost.GetLogger<SnkPatchController<TLocalRepo, TRemoteRepo>>();
            private TLocalRepo _localRepo;
            private TRemoteRepo _remoteRepo;
            private SnkPatchControlSettings _settings;

            public STATE status { get; private set; }

            private bool _initializd = false;

            public virtual string ChannelName => this._settings.channelName;
            public virtual int AppVersion => this._settings.appVersion;
            public virtual SnkPatchControlSettings Settings => this._settings;
            public ushort LocalResVersion => this._localRepo.Version;
            public ushort RemoteResVersion => this._remoteRepo.Version;

            private bool _disposed = false;

            private long totalDownloadSize;
            private List<SnkSourceInfo> _addList;
            private List<string> _delList;

            public long TotalDownloadSize => totalDownloadSize;
            public long DownloadedSize => this._remoteRepo.DownloadedSize;

            public double DownloadProgress
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

            public long DownloadSpeed => this._remoteRepo.RecentSpeed;

            public Exception Exception;

            public SnkPatchController(SnkPatchControlSettings settings, int threadNumber)
            {
                this._settings = settings;

                this._localRepo = new TLocalRepo();
                this._remoteRepo = new TRemoteRepo();
                this._remoteRepo.SetDownloadThreadNumber(threadNumber);
            }

            public async Task Initialize()
            {
                status = STATE.initializing;
                var localInitTask = this._localRepo.Initialize(this);
                var remoteInfoTask = this._remoteRepo.Initialize(this);
                await Task.WhenAll(localInitTask, remoteInfoTask).ConfigureAwait(false);

                if (this._localRepo.IsError)
                {
                    throw new System.Exception(this._localRepo.ExceptionString);
                }

                if (this._remoteRepo.IsError)
                {
                    throw new System.Exception(this._remoteRepo.ExceptionString);
                }
                status = STATE.initialized;
                _initializd = true;
            }

            private async Task ValidityInitialization()
            {
                if (this._initializd == false)
                {
                    if (this.status == STATE.none)
                        await this.Initialize();
                    else
                    {
                        throw new Exception("No Initialize or Initializing");
                    }
                }
            }

            public async Task<long> TryUpdate()
            {
                await ValidityInitialization();

                status = STATE.preview_diff_begin;
                var tuple = await PreviewDiff(this.RemoteResVersion);
                _addList = tuple.Item1;
                _delList = tuple.Item2;
                status = STATE.preview_diff_end;

                //0版本的都在底包，所以认为不是差异
                _addList.RemoveAll(t => t.version == 0);

                if (_addList.Count > 0)
                    return _addList.Sum(item => item.size);

                if (_delList.Count > 0)
                    return -_delList.Count;

                return 0;
            }

            public async Task Apply(System.Func<Task<bool>> onExceptionCallBack)
            {
                await ValidityInitialization();

                status = STATE.downloading;

                if (s_log.IsInfoEnabled)
                    s_log?.Info("LocalPath:" + this._localRepo.LocalPath);

                var delTask = Task.Run(() =>
                {
                    foreach (var key in _delList)
                    {
                        var fileInfo = new FileInfo(System.IO.Path.Combine(this._localRepo.LocalPath, key));
                        if (fileInfo.Exists)
                        {
                            fileInfo.Delete();
                        }
                    }
                });

                var addTask = Task.Run(() =>
                {
                    for (var i = 0; i < _addList.Count; i++)
                    {
                        var sourceInfo = _addList[i];
                        _remoteRepo.EnqueueDownloadQueue(_localRepo.LocalPath, sourceInfo.key, sourceInfo.version);
                        totalDownloadSize += sourceInfo.size;
                    }
                });

                await Task.WhenAll(delTask, addTask).ConfigureAwait(false);

                while (await _remoteRepo.StartupDownload(OnPreDownloadTask) == false)
                {
                    if (_disposed)
                        return;

                    bool result = await onExceptionCallBack?.Invoke();
                    if (result == false)
                        return;
                }

                if (s_log.IsInfoEnabled)
                    s_log?.Info($"UpdateLocalResVersion:{_remoteRepo.Version}");

                _localRepo.UpdateLocalResVersion(_remoteRepo.Version);

                status = STATE.downloaded;
            }

            public void OnPreDownloadTask(string key)
            {
                var index = _addList.FindIndex(a => a.key == key);
                if (index >= 0)
                {
                    _localRepo.UpdateLocalSourceInfo(_addList[index]);
                }
            }

            public async Task<Tuple<List<SnkSourceInfo>, List<string>>> PreviewDiff(ushort remoteResVersion)
            {
                var localManifest = await _localRepo.GetSourceInfoList(0) ?? new List<SnkSourceInfo>();

                if (remoteResVersion < 0)
                    remoteResVersion = this._remoteRepo.Version;
                var remoteManifest = await _remoteRepo.GetSourceInfoList(remoteResVersion);

                return SnkPatch.CompareToDiff(localManifest, remoteManifest);
            }

            public bool LocalRepoExists(string key) => this._localRepo.Exists(key);

            public bool SourceExists(string key, bool fromLocalResp = true)
            {
                if (fromLocalResp)
                    return this._localRepo.Exists(key);
                return this._remoteRepo.Exists(key);
            }

            public bool TryGetSourceInfo(string key, bool fromLocalResp, out SnkSourceInfo sourceInfo)
            {
                if (fromLocalResp)
                    return this._localRepo.TryGetSourceInfo(key, out sourceInfo);
                return this._remoteRepo.TryGetSourceInfo(key, out sourceInfo);
            }

            //析构
            ~SnkPatchController()
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
                        this._localRepo.Dispose();
                        this._remoteRepo.Dispose();
                        _disposed = true;
                    }
                }
            }
        }
    }
}