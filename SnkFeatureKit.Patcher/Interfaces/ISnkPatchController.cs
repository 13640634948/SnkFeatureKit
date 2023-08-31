using System.Threading.Tasks;

namespace SnkFeatureKit.Patcher
{
    namespace Interfaces
    {
        public interface ISnkPatchController : System.IDisposable
        {
            ISnkJsonParser jsonParser { get; }
            
            string ChannelName { get; }
            int LocalAppVersion { get; }
            int RemoteAppVersion { get; }
            ushort LocalResVersion { get; }
            ushort RemoteResVersion { get; }
            
            SnkPatchControlSettings Settings { get; }
            long TotalDownloadSize { get; }
            long DownloadedSize { get; }

            double DownloadProgress { get; }
            long DownloadSpeed { get; }

            Task Initialize();
            long TryUpdate();
            
            Task Apply(System.Func<Task<bool>> onExceptionCallBack);

            void UpdateLocalResVersion(ushort resVersion);

        }
    }
}