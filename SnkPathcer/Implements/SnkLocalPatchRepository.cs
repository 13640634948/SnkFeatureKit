using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;

using SnkFeatureKit.Logging;
using SnkFeatureKit.Patcher.Interfaces;

namespace SnkFeatureKit.Patcher
{
    namespace Implements
    {
        public class SnkLocalPatchRepository : ISnkLocalPatchRepository
        {
            protected static readonly ISnkLogger s_log = SnkLogHost.GetLogger<SnkLocalPatchRepository>();
            public ushort Version => _localSourceInfos.resVersion;

            public bool IsError { get; protected set; } = false;

            public string ExceptionString { get; protected set; } = string.Empty;

            public string LocalPath => _patchCtrl.Settings.localPatchRepoPath;

            protected ISnkPatchController _patchCtrl;

            protected SnkLocalSourceInfos _localSourceInfos;

            protected virtual List<SnkSourceInfo> sourceInfoList => _localSourceInfos.localSourceInfoList;

            private bool _disposed = false;

            protected string _localVersionFullName;

            public virtual Task<bool> Initialize(ISnkPatchController patchController)
            {
                this._patchCtrl = patchController;

                _localVersionFullName = Path.Combine(this.LocalPath, this._patchCtrl.Settings.localVersionDir, this._patchCtrl.Settings.versionInfoFileName);
                var fileInfo = new FileInfo(_localVersionFullName);
                if (fileInfo.Exists == false)
                {
                    var versionInfos = new SnkLocalSourceInfos();
                    versionInfos.resVersion = 0;
                    versionInfos.localSourceInfoList = new List<SnkSourceInfo>();
                    this._localSourceInfos = versionInfos;
                }
                else
                {
                    string text = File.ReadAllText(fileInfo.FullName).Trim();
                    this._localSourceInfos = SnkLocalSourceInfos.ValueOf(text);
                }
                return Task.FromResult(true);
            }

            public virtual Task<List<SnkSourceInfo>> GetSourceInfoList(ushort version = 0)
            {
                if (s_log.IsInfoEnabled)
                    s_log?.Info($"[RemoteRepo]GetSourceInfoList.fromVersion:{version}");
                return Task.FromResult(sourceInfoList);
            }

            public virtual void UpdateLocalSourceInfo(SnkSourceInfo sourceInfo, bool add = true)
            {
                var index = this.sourceInfoList.FindIndex(a => a.key == sourceInfo.key);

                if (add && index < 0)
                {
                    this.sourceInfoList.Add(sourceInfo);
                }
                else if (add == false && index >= 0)
                {
                    this.sourceInfoList.RemoveAt(index);
                }
                else
                {
                    if (s_log.IsErrorEnabled)
                        s_log.Error($"û���ҵ���Ӧ�Ĳ�������add:{add}, sourceInfo:{sourceInfo}");
                }
                SaveSourceInfo();
            }

            public virtual void UpdateLocalResVersion(ushort resVersion)
            {
                this._localSourceInfos.resVersion = resVersion;
                SaveSourceInfo();
            }

            protected virtual void SaveSourceInfo()
            {
                var fileInfo = new FileInfo(_localVersionFullName);
                if (fileInfo.Exists)
                    fileInfo.Delete();

                if (fileInfo.Directory.Exists == false)
                    fileInfo.Directory.Create();
                var json = this._localSourceInfos.ToString();
                File.WriteAllText(fileInfo.FullName, json);
            }

            public bool Exists(string key)
                => this.sourceInfoList.Exists(a => a.key.Equals(key));

            public bool TryGetSourceInfo(string key, out SnkSourceInfo sourceInfo)
            {
                sourceInfo = default;
                var index = this.sourceInfoList.FindIndex(a => a.key.Equals(key));
                if (index < 0)
                    return false;
                sourceInfo = this.sourceInfoList[index];
                return true;
            }

            ~SnkLocalPatchRepository()
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

                        if (sourceInfoList != null)
                        {
                            sourceInfoList.Clear();
                        }
                    }
                }
            }
        }
    }
}