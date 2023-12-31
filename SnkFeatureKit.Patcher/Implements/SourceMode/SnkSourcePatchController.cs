using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SnkFeatureKit.Logging;
using SnkFeatureKit.Patcher.Abstracts;
using SnkFeatureKit.Patcher.Interfaces;

namespace SnkFeatureKit.Patcher
{
    namespace Implements
    {
        public class SnkSourcePatchController<TLocalRepo, TRemoteRepo> : SnkPatchControllerAbstract
            where TLocalRepo : class, ISnkLocalPatchRepository, new()
            where TRemoteRepo : class, ISnkRemotePatchRepository, new()
        {

            private long totalDownloadSize;
            public override long TotalDownloadSize => totalDownloadSize;

            private List<SnkSourceInfo> _addList;
            private List<string> _delList;

            public SnkSourcePatchController(SnkPatchControlSettings settings, ISnkJsonParser jsonParser, int threadNumber = 12) : base(new TLocalRepo(), new TRemoteRepo(), settings, jsonParser, threadNumber)
            {
            }
            
            public override long TryUpdate()
            {
                ValidityInitialization();
                ValidityAppVersion();
                
                status = SNK_PATCH_CTRL_STATE.preview_diff_begin;
                var tuple = PreviewDiff(this.RemoteResVersion);
                _addList = tuple.Item1;
                _delList = tuple.Item2;
                status = SNK_PATCH_CTRL_STATE.preview_diff_end;

                //0版本的都在底包，所以认为不是差异
                _addList.RemoveAll(t => t.version == 0);

                if (_addList.Count > 0)
                    totalDownloadSize = _addList.Sum(item => item.size);

                if (_delList.Count > 0)
                    totalDownloadSize = -_delList.Count;

                return totalDownloadSize;
            }

            public override async Task Apply(System.Func<Task<bool>> onExceptionCallBack)
            {
                ValidityInitialization();

                status = SNK_PATCH_CTRL_STATE.downloading;

                if(logger != null && logger.IsEnabled(SnkLogLevel.Info))
                    logger.LogInfo("LocalPath:" + this.localRepo.LocalPath);

                var delTask = Task.Run(() =>
                {
                    foreach (var key in _delList)
                    {
                        var fileInfo = new FileInfo(System.IO.Path.Combine(this.localRepo.LocalPath, key));
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
                        remoteRepo.EnqueueDownloadQueue(localRepo.LocalPath, sourceInfo.key, sourceInfo.version);
                    }
                });

                await Task.WhenAll(delTask, addTask).ConfigureAwait(false);

                while (await remoteRepo.StartupDownload(OnPreDownloadTask) == false)
                {
                    if (_disposed)
                        return;

                    bool result = await onExceptionCallBack?.Invoke();
                    if (result == false)
                        return;
                }

                if (logger != null && logger.IsEnabled(SnkLogLevel.Info))
                    logger?.LogInfo($"UpdateLocalResVersion:{remoteRepo.ResVersion}");

                localRepo.UpdateLocalResVersion(remoteRepo.ResVersion);

                status = SNK_PATCH_CTRL_STATE.downloaded;
            }

    

            protected Tuple<List<SnkSourceInfo>, List<string>> PreviewDiff(ushort remoteResVersion)
            {
                var localManifest = localRepo.GetSourceInfoList(0) ?? new List<SnkSourceInfo>();
                var remoteManifest = remoteRepo.GetSourceInfoList(remoteResVersion);
                var tuple = SnkPatch.CompareToDiff(localManifest, remoteManifest);
                if (logger != null && logger.IsEnabled(SnkLogLevel.Info))
                {
                    logger.LogInfo($"localManifest:{localManifest.Count}");
                    logger.LogInfo($"remoteManifest:{remoteManifest.Count}");
                    logger.LogInfo($"addManifest:{tuple.Item1.Count}");
                    logger.LogInfo($"delManifest:{tuple.Item2.Count}");
                }
                return tuple;
            }
        }
    }
}