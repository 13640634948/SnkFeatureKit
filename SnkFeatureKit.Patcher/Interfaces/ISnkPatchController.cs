using System.Threading.Tasks;

namespace SnkFeatureKit.Patcher
{
    namespace Interfaces
    {
        public interface ISnkPatchController : System.IDisposable
        {
            string ChannelName { get; }
            int AppVersion { get; }
            ushort LocalResVersion { get; }
            ushort RemoteResVersion { get; }
            SnkPatchControlSettings Settings { get; }
            long TotalDownloadSize { get; }
            long DownloadedSize { get; }

            double DownloadProgress { get; }
            long DownloadSpeed { get; }

            Task Initialize();
            Task<long> TryUpdate();
            //Task<(List<SnkSourceInfo>, List<string>)> PreviewDiff(ushort remoteResVersion);
            Task Apply(System.Func<Task<bool>> onExceptionCallBack);
            bool SourceExists(string key, bool fromLocalResp = true);
            bool TryGetSourceInfo(string key, bool fromLocalResp, out SnkSourceInfo sourceInfo);
        }
    }
}