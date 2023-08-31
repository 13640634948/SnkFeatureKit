using System;
using System.IO;
using SnkFeatureKit.Patcher.Abstracts;

namespace SnkFeatureKit.Patcher
{
    namespace Implements
    {
        public class SnkSourceRemoteRepository : SnkRemoteRepositoryAbstract
        {
            protected override string RemoteManifestUrl 
                => Path.Combine(GetCurrURL(), patchController.ChannelName, AppVersion.ToString(), ResVersion.ToString(), patchController.Settings.manifestFileName);

            public override void EnqueueDownloadQueue(string dirPath, string key, int resVersion)
            {
                var basicUrl = GetCurrURL();
                var url = Path.Combine(basicUrl, patchController.ChannelName, AppVersion.ToString(), resVersion.ToString(), patchController.Settings.assetsDirName, key);
                willDownloadTaskQueue.Enqueue(new Tuple<string, string, string>(url, Path.Combine(dirPath, key), key));
            }
        }
    }
}