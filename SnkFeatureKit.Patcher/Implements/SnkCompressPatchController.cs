using System;
using System.Threading.Tasks;
using SnkFeatureKit.Patcher.Interfaces;

namespace SnkFeatureKit.Patcher.Implements
{
    public class SnkCompressPatchController<TLocalRepo, TRemoteRepo> : SnkPatchControllerAbstract
        where TLocalRepo : class, ISnkLocalPatchRepository, new()
        where TRemoteRepo : class, ISnkRemotePatchRepository, new()
    {
        public override long TotalDownloadSize { get; }

        public SnkCompressPatchController(SnkPatchControlSettings settings, ISnkJsonParser jsonParser) : base(new TLocalRepo(), new TRemoteRepo(), settings, jsonParser)
        {
            this.remoteRepo.SetDownloadThreadNumber(1);
        }

        public override Task<long> TryUpdate()
        {
            throw new NotImplementedException();
        }

        public override Task Apply(Func<Task<bool>> onExceptionCallBack)
        {
            throw new NotImplementedException();
        }
    }
}