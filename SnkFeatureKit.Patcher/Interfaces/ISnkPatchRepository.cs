using System.Collections.Generic;
using System.Threading.Tasks;

namespace SnkFeatureKit.Patcher
{
    namespace Interfaces
    {
        public interface ISnkPatchRepository : System.IDisposable
        {
            int AppVersion { get; }
            ushort ResVersion { get; }

            bool IsError { get; }

            string ExceptionString { get; }

            Task Initialize(ISnkPatchController patchController);
            List<SnkSourceInfo> GetSourceInfoList(ushort version);
        }
    }
}