using SnkFeatureKit.Patcher.Interfaces;

namespace SnkFeatureKit.Patcher.Delegates
{
    public delegate ISnkDownloadTask CreateDownloadTaskDelegate(string url, string savePath);
}