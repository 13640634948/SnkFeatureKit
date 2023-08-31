using System.Collections.Generic;
using SnkFeatureKit.Patcher.Interfaces;

namespace SnkFeatureKit.Patcher.Implements
{
    public class SnkCompressLocalRepository : SnkLocalRepositoryAbstract
    {
        protected override List<SnkSourceInfo> LoadSourceInfo()
        {
            var count = 0;
            var currNum = 0;
            var finder = new SnkFileFinder(this.LocalPath);
            return SnkPatch.GenerateSourceInfoList(0, finder, ref count, ref currNum, calculateMD5:false) ?? new List<SnkSourceInfo>();
        }
    }
}