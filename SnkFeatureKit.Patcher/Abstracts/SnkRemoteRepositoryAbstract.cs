using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SnkFeatureKit.Logging;
using SnkFeatureKit.Patcher.Extensions;
using SnkFeatureKit.Patcher.Implements;
using SnkFeatureKit.Patcher.Interfaces;

namespace SnkFeatureKit.Patcher
{
    namespace Abstracts
    {
        public abstract class SnkRemoteRepositoryAbstract : ISnkRemotePatchRepository
        {
            private static readonly long RECORD_SPEED_TIME = 10;
            
            private ISnkLogger _logger;
            protected ISnkLogger logger => _logger ??= SnkLogHost.GetLogger(this.GetType().Name);

            protected ISnkPatchController patchController { get; private set; }
            protected ISnkJsonParser jsonParser => patchController.jsonParser;

            private readonly object _locker = new object();

            
            private Stopwatch _stopwatch;
            private System.Threading.Timer _timer;
            
            protected List<int> AppVersionHistories;
            protected List<SnkVersionMeta> ResVersionHistories;
            
            protected Queue<Tuple<string, string, string>> willDownloadTaskQueue = new Queue<Tuple<string, string, string>>();
            protected List<Tuple<long, long>> speedCacheList = new List<Tuple<long, long>>();

            private int _urlIndex;
            public string ExceptionString { get; private set; }
            public int AppVersion { get; private set; } = -1;

            ushort ISnkPatchRepository.ResVersion => _resVersion;

            public int ResVersion { get; private set;} = -1;
            public bool IsError { get; private set; }
            
            
            private List<ISnkDownloadTask> downloadingList = new List<ISnkDownloadTask>();
            private Queue<Tuple<string, string, string>> exceptionQueue = new Queue<Tuple<string, string, string>>();

         
            public virtual long AverageSpeed { get; protected set; }
            public virtual long InstantaneousSpeed  { get; protected set; }
            public virtual long RecentSpeed { get; protected set; }
            public virtual long LimitDownloadSpeed { get; set; }

            private long _currDownloadedSize;
            private long _prevDownloadSize;
            private int _totalTaskCount;
            private int _finishTaskCount;
            private int _maxThreadNumber;
            private bool _disposed;
            private int _threadTickInterval = 100;
            private ushort _resVersion;

            public virtual long DownloadedSize
            {
                get
                {
                    var downloadingSize = 0L;
                    lock (_locker)
                    {
                        downloadingSize = downloadingList.Sum(a => a.DownloadedSize);
                    }
                    return _currDownloadedSize + downloadingSize;
                }
            }
            
            
            protected virtual ISnkDownloadTask CreateDownloadTask() => new SnkDownloadTask();

            protected abstract string RemoteManifestUrl { get; }

            
            
            private List<SnkSourceInfo> _sourceInfoList = new List<SnkSourceInfo>();

            public virtual async Task Initialize(ISnkPatchController patchController)
            {
                this.patchController = patchController;
                var rootUrl = Path.Combine(GetCurrURL(), patchController.ChannelName);
                var appVersionUrl = Path.Combine(rootUrl, patchController.Settings.appVersionInfoFileName);
                var resVersionUrl = Path.Combine(rootUrl, patchController.Settings.appVersion.ToString(), patchController.Settings.resVersionInfoFileName);

                var appVersionReqTask = Task.Run(() =>
                {
                    try
                    {
                        var content = SnkHttpWeb.Get(appVersionUrl);
                        if (string.IsNullOrEmpty(content))
                            throw new System.Exception("content is null or empty.");
                        logger?.LogInfo($"AppVersionReqTask.Content:\n" + content);
                        AppVersionHistories = jsonParser.FromJson<List<int>>(content);
                        if(AppVersionHistories != null && AppVersionHistories.Count > 0)
                            AppVersion = AppVersionHistories[AppVersionHistories.Count - 1];
                    }
                    catch (Exception e)
                    {
                        var tag = $"web request remote {patchController.Settings.appVersionInfoFileName} failed. url:{appVersionUrl}";
                        this.SetException(e, tag);
                    }
                });
                
                var resVersionReqTask = Task.Run(() =>
                {
                    try
                    {
                        var content = SnkHttpWeb.Get(resVersionUrl);
                        if (string.IsNullOrEmpty(content))
                            throw new System.Exception("content is null or empty.");
                        logger?.LogInfo($"ResVersionReqTask.Content:\n" + content);
                        ResVersionHistories = jsonParser.FromJson<List<SnkVersionMeta>>(content);
                        if(ResVersionHistories != null && ResVersionHistories.Count > 0)
                            ResVersion = ResVersionHistories[ResVersionHistories.Count - 1].version;
                    }
                    catch (Exception e)
                    {
                        var tag = $"web request remote {patchController.Settings.resVersionInfoFileName} failed. url:{resVersionUrl}";
                        this.SetException(e, tag);
                    }
                });

                await Task.WhenAll(appVersionReqTask, resVersionReqTask).ConfigureAwait(false);

                if (AppVersion <= 0 || ResVersion <= 0)
                {
                    var tag = $"AppVersion:{AppVersion}, ResVersion:{ResVersion}";
                    this.SetException(new AggregateException($"appVersion or resVersion is error"), tag);
                    return;
                }

                await Task.Run(async () =>
                {
                    try
                    {   
                        var content = await SnkHttpWeb.GetAsync(RemoteManifestUrl);
                        this._sourceInfoList = jsonParser.FromJson<List<SnkSourceInfo>>(content);
                        logger?.LogInfo($"RemoteManifestReqTask.Content:\n{content}");
                    }
                    catch (Exception exception)
                    {
                        var tag = $"web request remote {patchController.Settings.manifestFileName} failed. url:{RemoteManifestUrl}";
                        this.SetException(exception, tag);
                    }
                });
                
                logger?.LogInfo($"AppVersion:{AppVersion}, ResVersion:{ResVersion}");
            }

            public virtual List<SnkSourceInfo> GetSourceInfoList(ushort version) => this._sourceInfoList;

            protected virtual string GetCurrURL(bool moveNext = false)
            {
                if (!moveNext)
                    return this.patchController.Settings.remoteURLs[_urlIndex];
                var result = _urlIndex + 1 >= this.patchController.Settings.remoteURLs.Length;
                _urlIndex = result ? 0 : (_urlIndex + 1);
                return this.patchController.Settings.remoteURLs[_urlIndex];
            }
            
            protected virtual void SetException(Exception exception, string tag)
            {
                IsError = true;
                ExceptionString = tag;
                logger?.LogError(exception,$"{tag}");
            }

            public virtual List<int> GetAppVersionHistories() 
                => this.AppVersionHistories;
            
            public virtual List<SnkVersionMeta> GetResVersionHistories() 
                => this.ResVersionHistories;

            public virtual void SetDownloadThreadNumber(int threadNum) 
                => this._maxThreadNumber = threadNum;

            public virtual void SetThreadTickIntervalMilliseconds(int intervalMilliseconds)
                => this._threadTickInterval = intervalMilliseconds;

            public abstract void EnqueueDownloadQueue(string dirPath, string key, int resVersion);

            private void StartRecordDownloadSpeed()
            {
                _stopwatch = Stopwatch.StartNew();
                _timer = new System.Threading.Timer(OnRecordDownloadSpeed, null, 0, 1000);
                OnRecordDownloadSpeed(null);
            }

            private void OnRecordDownloadSpeed(object state)
            {
                if (DownloadedSize > 0)
                {
                    AverageSpeed = DownloadedSize / _stopwatch.ElapsedMilliseconds * 1000;
                    InstantaneousSpeed = DownloadedSize - _prevDownloadSize;
                    _prevDownloadSize = DownloadedSize;

                    speedCacheList.Add(new Tuple<long, long>(_stopwatch.ElapsedMilliseconds, DownloadedSize));
                    while (speedCacheList.Count > RECORD_SPEED_TIME)
                        speedCacheList.RemoveAt(0);

                    if (speedCacheList.Count > 1)
                    {
                        var last = speedCacheList[speedCacheList.Count - 1];
                        var first = speedCacheList[0];
                        RecentSpeed = (last.Item2 - first.Item2) / (last.Item1 - first.Item1) * 1000;
                    }
                    else
                    {
                        RecentSpeed = speedCacheList[0].Item2;
                    }
                }
            }

            public virtual async Task<bool> StartupDownload(Action<string> onPreDownloadTask)
            {
                if (logger != null && logger.IsEnabled(SnkLogLevel.Info))
                        logger.LogInfo($"[StartDownload] thread number:{this._maxThreadNumber}");

                    StartRecordDownloadSpeed();

                    return await Task.Run(() =>
                    {
                        try
                        {
                            _totalTaskCount = willDownloadTaskQueue.Count;
                            while (this._finishTaskCount < _totalTaskCount)
                            {
                                if (_disposed)
                                    return false;

                                lock (_locker)
                                {
                                    for (var i = 0; i < downloadingList.Count; i++)
                                    {
                                        var task = downloadingList[i];

                                        if (task.State != SNK_DOWNLOAD_STATE.completed)
                                            continue;

                                        if (task.DownloadException != null)
                                        {
                                            exceptionQueue.Enqueue(new Tuple<string, string, string>(task.Url, task.SavePath, task.Name));
                                            downloadingList[i] = null;
                                            
                                            if (logger != null && logger.IsEnabled(SnkLogLevel.Error))
                                                logger.LogError($"[DownloadException]{task.Url}\n{task.DownloadException.Message}");
                                        }
                                        else
                                        {
                                            this._finishTaskCount++;
                                            this._currDownloadedSize += task.TotalSize;
                                            onPreDownloadTask(task.Name);
                                            
                                            if (logger != null && logger.IsEnabled(SnkLogLevel.Info))
                                                logger.LogInfo($"[DownloadFinish]{task.Url}");
                                        }
                                        task.Dispose();
                                        downloadingList.RemoveAt(i--);
                                        
                                    }
                                    
                                    var implementTaskCount = this._maxThreadNumber - downloadingList.Count;
                                    for (var i = 0; i < implementTaskCount; i++)
                                    {
                                        var task = GetPrepareDownloadTask();
                                        if (task == null)
                                            continue;
                                        task.DownloadFileAsync().ConfigureAwait(false);
                                        downloadingList.Add(task);
                                    }
                                }

                                if (logger != null && logger.IsEnabled(SnkLogLevel.Info))
                                {
                                    var stringBuilder = new StringBuilder();
                                    stringBuilder.AppendLine($"[DownloadingState]cnt:{downloadingList.Count}");
                                    foreach (var downloadTask in downloadingList)
                                    {
                                        stringBuilder.AppendLine($"{downloadTask.Url}:{downloadTask.State}");
                                    }
                                    logger.LogInfo($"{stringBuilder}");
                                }

                                
                                
                                System.Threading.Thread.Sleep(_threadTickInterval);
                                if (_disposed)
                                    return false;
                            }
                        }
                        catch (Exception ex)
                        {
                            if (logger != null && logger.IsEnabled(SnkLogLevel.Error))
                                logger.LogError(ex);
                            throw;
                        }
                        return true;
                    });
            }

            private ISnkDownloadTask GetPrepareDownloadTask()
            {
                while (true)
                {
                    if (willDownloadTaskQueue.Count > 0)
                    {
                        var tuple = willDownloadTaskQueue.Dequeue();
                        var downloadTask = CreateDownloadTask();
                        downloadTask.Url = tuple.Item1.FixSlash();
                        downloadTask.SavePath = tuple.Item2.FixSlash().FixLongPath();
                        downloadTask.Name = tuple.Item3;
                            
                        if (logger != null && logger.IsEnabled(SnkLogLevel.Info))
                            logger.LogInfo($"[CreateDownloadTask]{downloadTask.Url}");
                        return downloadTask;
                    }

                    if (exceptionQueue.Count == 0) 
                        return null;
                        
                    while (exceptionQueue.Count > 0) 
                        willDownloadTaskQueue.Enqueue(exceptionQueue.Dequeue());
                }
            }

            ~SnkRemoteRepositoryAbstract()
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
                        _disposed = true;

                        if (_timer != null)
                        {
                            _timer.Dispose();
                            _timer = null;
                        }
                        if (exceptionQueue != null)
                        {
                            exceptionQueue.Clear();
                            exceptionQueue = null;
                        }

                        if (willDownloadTaskQueue != null)
                        {
                            willDownloadTaskQueue.Clear();
                            willDownloadTaskQueue = null;
                        }

                        if (downloadingList != null)
                        {
                            foreach (var task in downloadingList)
                                task.Cancel();
                            downloadingList.Clear();
                            downloadingList = null;
                        }
                    }
                }
            }
        }
    }
}