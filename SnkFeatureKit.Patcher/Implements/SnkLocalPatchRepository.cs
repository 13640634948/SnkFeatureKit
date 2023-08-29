using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
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

            protected virtual List<SnkSourceInfo> sourceInfoList { get; set; }

            private bool _disposed = false;

            protected ISnkJsonParser _jsonParser;

            protected virtual string resVersionFilePath { get; } = "res_version.json";

            protected void LoadLocalSourceInfos()
            {
                SnkLocalSourceInfos localSourceInfos = null;
                if (System.IO.File.Exists(resVersionFilePath))
                {
                    var json = System.IO.File.ReadAllText(resVersionFilePath);
                    localSourceInfos = _jsonParser.FromJson<SnkLocalSourceInfos>(json);
                }
                _localSourceInfos = localSourceInfos ?? new SnkLocalSourceInfos();
            }
 
            public virtual async Task<bool> Initialize(ISnkPatchController patchController, ISnkJsonParser jsonParser)
            {
                this._patchCtrl = patchController;
                this._jsonParser = jsonParser;

                LoadLocalSourceInfos();
                
                var finder = new SnkFileFinder(this.LocalPath);
                sourceInfoList = await SnkPatch.GenerateSourceInfoList(0, finder) ?? new List<SnkSourceInfo>();

                if (logger != null && logger.IsEnabled(SnkLogLevel.Info))
                {
                    if (sourceInfoList.Count == 0)
                    {
                        logger.LogInfo("LocalRepoVersion.Initialize:sourceInfoList.Count:0");
                    }
                    else
                    {
                        logger.LogInfo("LocalRepoVersion.Initialize:Start");
                        foreach (var t in sourceInfoList)
                            logger.LogInfo(t.ToString());
                        logger.LogInfo("LocalRepoVersion.Initialize:End");
                    }
                }
            
                return true;
            }

            public virtual Task<List<SnkSourceInfo>> GetSourceInfoList(ushort version = 0)
                => Task.FromResult(sourceInfoList);

            public virtual void UpdateLocalResVersion(ushort resVersion)
            {
                this._localSourceInfos.resVersion = resVersion;
                System.IO.File.WriteAllText("res_version.json", _jsonParser.ToJson(this._localSourceInfos));
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