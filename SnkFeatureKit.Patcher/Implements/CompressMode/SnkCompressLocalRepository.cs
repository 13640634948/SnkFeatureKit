using System.Collections.Generic;
using SnkFeatureKit.Patcher.Interfaces;

namespace SnkFeatureKit.Patcher.Implements
{
    public class SnkCompressLocalRepository : SnkLocalRepositoryAbstract
    {
        protected override List<SnkSourceInfo> LoadSourceInfo()
        {
            return new List<SnkSourceInfo>();
        }
    }
}