using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SnkFeatureKit.Logging;
using SnkFeatureKit.Patcher.Abstracts;
using SnkFeatureKit.Patcher.Interfaces;

namespace SnkFeatureKit.Patcher.Implements
{
    public class SnkCompressPatchController<TLocalRepo, TRemoteRepo> : SnkPatchControllerAbstract
        where TLocalRepo : class, ISnkLocalPatchRepository, new()
        where TRemoteRepo : class, ISnkRemotePatchRepository, new()
    {
        public SnkCompressPatchController(SnkPatchControlSettings settings, ISnkJsonParser jsonParser, int threadNumber = 12)
            : base(new TLocalRepo(), new TRemoteRepo(), settings, jsonParser, threadNumber)
        {
        }
        
        private long totalDownloadSize;
        public override long TotalDownloadSize => totalDownloadSize;


        private List<SnkVersionMeta> _upgradeRouteList = new List<SnkVersionMeta>();
        public override long TryUpdate()
        {
            ValidityInitialization();
            ValidityAppVersion();
            
            status = SNK_PATCH_CTRL_STATE.preview_diff_begin;
            _upgradeRouteList = CalculateUpgradeRoute();
            status = SNK_PATCH_CTRL_STATE.preview_diff_end;

            if (_upgradeRouteList != null && _upgradeRouteList.Count > 0)
                totalDownloadSize = _upgradeRouteList.Sum(a => a.size);
            
            logger?.LogInfo($"totalDownloadSize:{totalDownloadSize}");
            return totalDownloadSize;
        }

        public override  async Task Apply(Func<Task<bool>> onExceptionCallBack)
        {
            ValidityInitialization();

            status = SNK_PATCH_CTRL_STATE.downloading;

            for (int i = 0; i < _upgradeRouteList.Count; i++)
            {
                var meta = _upgradeRouteList[i];
                remoteRepo.EnqueueDownloadQueue(localRepo.LocalPath, meta.code, meta.version);
            }
            
            while (await remoteRepo.StartupDownload(OnPreDownloadTask) == false)
            {
                if (_disposed)
                    return;

                bool result = await onExceptionCallBack?.Invoke();
                if (result == false)
                    return;
            }

            status = SNK_PATCH_CTRL_STATE.downloaded;
        }
        
        private List<SnkVersionMeta> CalculateUpgradeRoute()
        {
            var list = new List<SnkVersionMeta>();
            var resVersionHistories = this.remoteRepo.GetResVersionHistories();
            for (var i = 0; i < resVersionHistories.Count; i++)
            {
                if (resVersionHistories[i].version > this.LocalResVersion)
                {
                    list.Add(resVersionHistories[i]);
                }
            }

            if (logger != null && logger.IsEnabled(SnkLogLevel.Info))
            {
                var outputString = $"LocalResVersion:{LocalResVersion}\n";
                if (list.Count == 0)
                {
                    outputString += "CalculateUpgradeRoute()-list.count = 0.";
                }
                else
                {
                    foreach (var t in list)
                    {
                        outputString += t + "\n";
                    }
                }

                logger.LogInfo(outputString.Trim());
            }

            return list;
        }

    }
}