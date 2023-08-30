using System.Collections.Generic;
using System.Threading.Tasks;

namespace SnkFeatureKit.Patcher
{
    namespace Interfaces
    {
        public interface ISnkRemotePatchRepository : ISnkPatchRepository
        {
            List<SnkVersionMeta> GetResVersionHistories();

            List<int> GetAppVersionHistories();

            void EnqueueDownloadQueue(string dirPath, string key, int resVersion);

            Task<bool> StartupDownload(System.Action<string> onPreDownloadTask);

            void SetDownloadThreadNumber(int threadNum);

            void SetThreadTickIntervalMilliseconds(int intervalMilliseconds);

            long DownloadedSize { get; }

            /// <summary>
            /// 平均速度
            /// </summary>
            long AverageSpeed { get; }
            /// <summary>
            /// 瞬时速度
            /// </summary>
            long InstantaneousSpeed { get; }
            /// <summary>
            /// 近期速度
            /// </summary>
            long RecentSpeed { get; }

            /// <summary>
            /// 限制下载速度
            /// </summary>
            long LimitDownloadSpeed { get; set; }
        }
    }
}