using System.Collections.Generic;
using System.Threading.Tasks;

namespace SnkFeatureKit.Patcher
{
    namespace Interfaces
    {
        public interface ISnkPatchRepository : System.IDisposable
        {
            /// <summary>
            /// 版本号
            /// </summary>
            ushort Version { get; }

            bool IsError { get; }

            string ExceptionString { get; }

            /// <summary>
            /// 仓库初始化
            /// </summary>
            /// <param name="patchController">补丁控制器</param>
            /// <returns>任务</returns>
            Task<bool> Initialize(ISnkPatchController patchController);

            /// <summary>
            /// 获取资源信息列表
            /// </summary>
            /// <param name="version">版本号</param>
            /// <returns>资源信息列表</returns>
            Task<List<SnkSourceInfo>> GetSourceInfoList(ushort version);

            /// <summary>
            /// 存库中是否存在
            /// </summary>
            /// <param name="key"></param>
            /// <returns></returns>
            bool Exists(string key);

            /// <summary>
            /// 获取SourceInfo
            /// </summary>
            /// <param name="key"></param>
            /// <param name="sourceInfo"></param>
            /// <returns></returns>
            bool TryGetSourceInfo(string key, out SnkSourceInfo sourceInfo);
        }
    }
}