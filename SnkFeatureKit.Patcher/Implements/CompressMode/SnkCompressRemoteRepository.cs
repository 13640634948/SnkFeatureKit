using System;
using System.IO;
using SnkFeatureKit.Patcher.Abstracts;

namespace SnkFeatureKit.Patcher.Implements
{
    public class SnkCompressRemoteRepository : SnkRemoteRepositoryAbstract
    {
        protected override string RemoteManifestUrl 
            => Path.Combine(GetCurrURL(), patchController.ChannelName, AppVersion.ToString(), patchController.Settings.manifestFileName);


        public override void EnqueueDownloadQueue(string dirPath, string key, int resVersion)
        {
            var basicUrl = GetCurrURL();
            var fileName = $"patcher_{resVersion}.zip";
            var url = Path.Combine(basicUrl, patchController.ChannelName, AppVersion.ToString(), fileName);
            willDownloadTaskQueue.Enqueue(new Tuple<string, string, string>(url, Path.Combine(dirPath, fileName), fileName));
        }
    }
}