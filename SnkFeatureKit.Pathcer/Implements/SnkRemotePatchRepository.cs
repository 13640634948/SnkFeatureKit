using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;

using SnkFeatureKit.Logging;
using SnkFeatureKit.Patcher.Interfaces;

namespace SnkFeatureKit.Patcher
{
    namespace Implements
    {
        public class SnkRemotePatchRepository : ISnkRemotePatchRepository
        {
            private static readonly ISnkLogger s_log = SnkLogHost.GetLogger<SnkRemotePatchRepository>();

            public ushort Version { get; private set; }

            public bool IsError { get; private set; }

            public string ExceptionString { get; private set; } = string.Empty;

            private ISnkPatchController _patchCtrl;

            private SnkVersionInfos _versionInfos;

            private int _urlIndex;

            private int _maxThreadNumber;
            private int _threadTickInterval = 100;
            private long _currDownloadedSize;
            private readonly object _locker = new object();
            public long DownloadedSize
            {
                get
                {
                    var downloadingSize = 0L;
                    lock (_locker)
                    {
                        downloadingSize = _downloadingList.Sum(a => a.DownloadedSize);
                    }
                    return _currDownloadedSize + downloadingSize;
                }
            }

            private Queue<Tuple<string, string, string>> _willDownloadTaskQueue = new Queue<Tuple<string, string, string>>();
            private Queue<Tuple<string, string, string>> _exceptionQueue = new Queue<Tuple<string, string, string>>();
            private List<ISnkDownloadTask> _downloadingList = new List<ISnkDownloadTask>();

            private int _totalTaskCount;
            private int _finishTaskCount;

            private List<SnkSourceInfo> _sourceInfoList = new List<SnkSourceInfo>();

            private bool _disposed;

            private int _logTaskCount;

            private System.Threading.Timer _timer;
            private Stopwatch _stopwatch;

            private List<Tuple<long, long>> _speedCacheList = new List<Tuple<long, long>>();

            /// <summary>
            /// 平均速度
            /// </summary>
            public long AverageSpeed { get; protected set; }
            /// <summary>
            /// 瞬时下载速度
            /// </summary>
            public long InstantaneousSpeed { get; protected set; }
            /// <summary>
            /// 近期下载速度
            /// </summary>
            public long RecentSpeed { get; protected set; }

            private long RecordSpeedTime = 10;

            private long prevDownloadSize;

            private Dictionary<ISnkDownloadTask, string> taskKeyDict = new Dictionary<ISnkDownloadTask, string>();

            public SnkRemotePatchRepository()
            {
                _logTaskCount = 0;
            }

            public async Task<bool> Initialize(ISnkPatchController patchController)
            {
                try
                {
                    this._patchCtrl = patchController;
                    var basicURL = GetCurrURL();
                    var url = Path.Combine(basicURL, _patchCtrl.ChannelName, _patchCtrl.AppVersion.ToString(), _patchCtrl.Settings.versionInfoFileName);
                    var result = await SnkHttpWeb.GetAsync(url);
                    if (result.IsError)
                    {
                        IsError = true;
                        ExceptionString = "获取远端版本信息失败";
                        s_log?.Error($"获取远端版本信息失败。URL:{url}\nerrText:{result.Exception.Message}\nStackTrace{result.Exception.StackTrace}");
                        return false;
                    }
                    
                    //var content = result.ContentData;
                    //_versionInfos = SnkVersionInfos.ValueOf(content);
                    _versionInfos = SnkPatch.JsonParser.FromJson<SnkVersionInfos>(result.ContentData);

                    var lastVersionIndex = _versionInfos.histories.Count - 1;
                    Version = _versionInfos.histories[lastVersionIndex].version;

                    if (s_log.IsInfoEnabled)
                    {
                        var initializeLog = new StringBuilder();
                        initializeLog.AppendLine($"[RemoteInit]AppVersion:{_versionInfos.appVersion}");
                        foreach (var a in _versionInfos.histories)
                        {
                            initializeLog.AppendLine($"[RemoteInit]AppVersion:{a.version}|{a.size}|{a.count}|{a.code}");
                        }
                        initializeLog.AppendLine($"[RemoteInit]Version:{Version}");
                        s_log.Info(initializeLog.ToString());
                    }
                }
                catch (Exception exception)
                {
                    IsError = true;
                    ExceptionString = "初始化远端仓库出现未知异常";
                    if (s_log.IsErrorEnabled)
                        s_log?.Error($"[Exception]\n{exception.Message}\n{exception.StackTrace}");
                    return false;
                }
                return true;
            }

            public List<SnkVersionMeta> GetResVersionHistories() => this._versionInfos.histories;

            public bool Exists(string key) => this._sourceInfoList.Exists(a => a.key.Equals(key));

            public bool TryGetSourceInfo(string key, out SnkSourceInfo sourceInfo)
            {
                sourceInfo = new SnkSourceInfo();
                var index = this._sourceInfoList.FindIndex(a => a.key.Equals(key));
                if (index < 0)
                    return false;
                sourceInfo = this._sourceInfoList[index];
                return true;
            }

            private string GetCurrURL(bool moveNext = false)
            {
                if (!moveNext)
                    return this._patchCtrl.Settings.remoteURLs[_urlIndex];
                var result = _urlIndex + 1 >= this._patchCtrl.Settings.remoteURLs.Length;
                _urlIndex = result ? 0 : (_urlIndex + 1);
                return this._patchCtrl.Settings.remoteURLs[_urlIndex];
            }

            public void SetDownloadThreadNumber(int threadNum) => this._maxThreadNumber = threadNum;

            public void SetThreadTickIntervalMilliseconds(int intervalMilliseconds)
                => this._threadTickInterval = intervalMilliseconds;

            public async Task<List<SnkSourceInfo>> GetSourceInfoList(ushort version)
            {
                var basicURL = GetCurrURL();
                var url = Path.Combine(basicURL, _patchCtrl.ChannelName, _patchCtrl.AppVersion.ToString(), version.ToString(), _patchCtrl.Settings.manifestFileName);
                var result = await SnkHttpWeb.GetAsync(url);
                if (result.IsError)
                {
                    IsError = true;
                    ExceptionString = "获取远端资源列表失败";
                    throw new AggregateException("获取远端资源列表失败。URL:" + url + "\nerrText:" + result.Exception.Message + "\n" + result.Exception.StackTrace);
                }
                s_log?.Info("url:" + url);

                var content = result.ContentData;
                this._sourceInfoList = SnkPatch.SnkSourceInfoListValueOf(content);
                return this._sourceInfoList;
            }

            public void EnqueueDownloadQueue(string dirPath, string key, int resVersion)
            {
                var basicURL = GetCurrURL();
                var url = Path.Combine(basicURL, _patchCtrl.ChannelName, _patchCtrl.AppVersion.ToString(), resVersion.ToString(), _patchCtrl.Settings.assetsDirName, key);
                _willDownloadTaskQueue.Enqueue(new Tuple<string, string, string>(url, Path.Combine(dirPath, key), key));
            }

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
                    InstantaneousSpeed = DownloadedSize - prevDownloadSize;
                    prevDownloadSize = DownloadedSize;

                    _speedCacheList.Add(new Tuple<long, long>(_stopwatch.ElapsedMilliseconds, DownloadedSize));
                    while (_speedCacheList.Count > RecordSpeedTime)
                        _speedCacheList.RemoveAt(0);

                    if (_speedCacheList.Count > 1)
                    {
                        var last = _speedCacheList[_speedCacheList.Count - 1];
                        var first = _speedCacheList[0];
                        RecentSpeed = (last.Item2 - first.Item2) / (last.Item1 - first.Item1) * 1000;
                    }
                    else
                    {
                        RecentSpeed = _speedCacheList[0].Item2;
                    }
                }
            }

            public async Task<bool> StartupDownload(System.Action<string> onPreDownloadTask)
            {
                if (s_log.IsInfoEnabled)
                    s_log?.Info($"[StartDownload] thread number:{this._maxThreadNumber}");

                StartRecordDownloadSpeed();

                return await Task.Run(() =>
                {
                    try
                    {
                        _totalTaskCount = _willDownloadTaskQueue.Count;
                        while (this._finishTaskCount < _totalTaskCount)
                        {
                            if (_disposed)
                                return false;

                            lock (_locker)
                            {
                                for (var i = 0; i < _downloadingList.Count; i++)
                                {
                                    var task = _downloadingList[i];
                                    if (task.IsCompleted == false)
                                        continue;

                                    var key = taskKeyDict[task];

                                    if (task.DownloadResult.Code == SNK_HTTP_ERROR_CODE.succeed)
                                    {
                                        this._finishTaskCount++;
                                        this._currDownloadedSize += task.TotalSize;
                                        onPreDownloadTask(key);
                                    }
                                    else
                                    {
                                        _exceptionQueue.Enqueue(new Tuple<string, string, string>(task.URL, task.SavePath, key));
                                        task.CancelDownload();
                                        task.Dispose();
                                        _downloadingList[i] = null;
                                    }
                                    _downloadingList.RemoveAt(i--);
                                }

                                var implementTaskCount = this._maxThreadNumber - _downloadingList.Count;
                                for (var i = 0; i < implementTaskCount; i++)
                                {
                                    var task = GetPrepareDownloadTask();
                                    if (task == null)
                                        continue;
                                    task.DownloadFileAsync().ConfigureAwait(false);
                                    _downloadingList.Add(task);
                                }
                            }

                            System.Threading.Thread.Sleep(_threadTickInterval);
                            if (_disposed)
                                return false;

                            PrintDownloadInfo();
                        }
                    }
                    catch (Exception ex)
                    {
                        s_log?.Error(ex);
                    }
                    return true;
                });
            }

            private void PrintDownloadInfo()
            {
                if (!s_log.IsInfoEnabled)
                    return;
                var exceptionCnt = _exceptionQueue.Count;
                var downloadingCnt = _downloadingList.Count;

                var logString = string.Empty;
                for (var i = 0; i < downloadingCnt; i++)
                    logString += _downloadingList[i].URL + "\n";

                var tmpLogTaskCount = _finishTaskCount + exceptionCnt + downloadingCnt;
                if (_logTaskCount == tmpLogTaskCount)
                    return;
                _logTaskCount = tmpLogTaskCount;
                s_log?.Info($"[DownloadInfo]-total:{_totalTaskCount}, finish:{_finishTaskCount}, exception:{exceptionCnt}, downloading:{downloadingCnt} => {_logTaskCount}\n[Downloading]\n{logString.Trim()}");
            }

            private ISnkDownloadTask GetPrepareDownloadTask()
            {
                while (true)
                {
                    if (_willDownloadTaskQueue.Count > 0)
                    {
                        var tuple = _willDownloadTaskQueue.Dequeue();
                        var task = new SnkDownloadTask(tuple.Item1, tuple.Item2);
                        taskKeyDict[task] = tuple.Item3;
                        return task;
                    }

                    if (_exceptionQueue.Count == 0) return null;

                    if (s_log.IsInfoEnabled)
                    {
                        var taskKey = $"[回收异常下载任务]数量：{_exceptionQueue.Count}\n";
                        taskKey = _exceptionQueue.Aggregate(taskKey, (current, task) => $"{current}{task.Item1}\n");
                        s_log.Info(taskKey.Trim());
                    }

                    while (_exceptionQueue.Count > 0) _willDownloadTaskQueue.Enqueue(_exceptionQueue.Dequeue());
                }
            }

            ~SnkRemotePatchRepository()
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
                        if (_exceptionQueue != null)
                        {
                            _exceptionQueue.Clear();
                            _exceptionQueue = null;
                        }

                        if (_willDownloadTaskQueue != null)
                        {
                            _willDownloadTaskQueue.Clear();
                            _willDownloadTaskQueue = null;
                        }

                        if (_downloadingList != null)
                        {
                            foreach (var task in _downloadingList)
                                task.CancelDownload();
                            _downloadingList.Clear();
                            _downloadingList = null;
                        }
                    }
                }
            }
        }
    }
}