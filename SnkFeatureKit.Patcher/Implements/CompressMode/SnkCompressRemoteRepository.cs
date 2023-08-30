using System;
using System.Collections.Generic;
using SnkFeatureKit.Patcher.Abstracts;
using SnkFeatureKit.Patcher.Interfaces;

namespace SnkFeatureKit.Patcher.Implements
{
    public class SnkCompressRemoteRepository : SnkRemoteRepositoryAbstract
    {
        public override List<SnkSourceInfo> GetSourceInfoList(ushort version)
        {
            throw new NotImplementedException();
        }
    }
}