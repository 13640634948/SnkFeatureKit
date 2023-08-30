using System;
using System.Threading.Tasks;
using SnkFeatureKit.Patcher.Abstracts;
using SnkFeatureKit.Patcher.Interfaces;

namespace SnkFeatureKit.Patcher.Implements
{
    public class SnkCompressPatchController<TLocalRepo, TRemoteRepo> : SnkPatchControllerAbstract
        where TLocalRepo : class, ISnkLocalPatchRepository, new()
        where TRemoteRepo : class, ISnkRemotePatchRepository, new()
    {
        public SnkCompressPatchController(ISnkLocalPatchRepository localRepo, ISnkRemotePatchRepository remoteRepo, SnkPatchControlSettings settings, ISnkJsonParser jsonParser) : base(localRepo, remoteRepo, settings, jsonParser)
        {
        }

        public override long TotalDownloadSize { get; }
        public override long TryUpdate()
        {
            throw new NotImplementedException();
        }

        public override Task Apply(Func<Task<bool>> onExceptionCallBack)
        {
            throw new NotImplementedException();
        }
    }
}