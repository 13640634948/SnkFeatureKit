using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;

using SnkFeatureKit.Patcher.Interfaces;
using SnkFeatureKit.Logging;

namespace SnkFeatureKit.Patcher
{
    namespace Implements
    {
        public class SnkLocalPatchRepository : ISnkLocalPatchRepository
        {
            protected static readonly ISnkLogger logger = SnkLogHost.GetLogger<SnkLocalPatchRepository>();

            public ushort Version => _localSourceInfos.resVersion;

            public bool IsError { get; protected set; } = false;

            public string ExceptionString { get; protected set; } = string.Empty;

            public string LocalPath => _patchCtrl.Settings.localPatchRepoPath;

            protected ISnkPatchController _patchCtrl;

            protected SnkLocalSourceInfos _localSourceInfos;

            protected virtual List<SnkSourceInfo> sourceInfoList => _localSourceInfos.localSourceInfoList;

            private bool _disposed = false;

            protected string _localVersionFullName;
            protected ISnkJsonParser _jsonParser;

            public virtual Task<bool> Initialize(ISnkPatchController patchController, ISnkJsonParser jsonParser)
            {
                this._patchCtrl = patchController;
                this._jsonParser = jsonParser;

                _localVersionFullName = Path.Combine(this.LocalPath, this._patchCtrl.Settings.localVersionDir, this._patchCtrl.Settings.resVersionInfoFileName);
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
                    this._localSourceInfos = _jsonParser.FromJson<SnkLocalSourceInfos>(text);
                }
                return Task.FromResult(true);
            }

            public virtual Task<List<SnkSourceInfo>> GetSourceInfoList(ushort version = 0)
            {
                if(logger != null && logger.IsEnabled(SnkLogLevel.Info))
                    logger.LogInfo($"[RemoteRepo]GetSourceInfoList.fromVersion:{version}");

                return Task.FromResult(sourceInfoList);
            }

            public virtual void UpdateLocalSourceInfo(SnkSourceInfo sourceInfo, bool add = true)
            {
                if (this.sourceInfoList == null)
                {
                    throw new System.NullReferenceException("sourceInfoList is null");
                }

                var index = this.sourceInfoList.FindIndex(a => a.key == sourceInfo.key);

                if (add && index < 0)
                {
                    this.sourceInfoList.Add(sourceInfo);
                }
                else if (add == false && index >= 0)
                {
                    this.sourceInfoList.RemoveAt(index);
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
                var json = _jsonParser.ToJson(this._localSourceInfos);
                File.WriteAllText(fileInfo.FullName, json);
            }

            public bool Exists(string key)
                => this.sourceInfoList.Exists(a => a.key.Equals(key));

            public bool TryGetSourceInfo(string key, out SnkSourceInfo sourceInfo)
            {
                sourceInfo = new SnkSourceInfo();
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