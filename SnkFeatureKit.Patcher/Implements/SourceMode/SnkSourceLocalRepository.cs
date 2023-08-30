using System.Collections.Generic;
using SnkFeatureKit.Patcher.Interfaces;

namespace SnkFeatureKit.Patcher
{
    namespace Implements
    {
        public class SnkSourceLocalRepository : SnkLocalRepositoryAbstract
        {
            protected override List<SnkSourceInfo> LoadSourceInfo()
            {
                var count = 0;
                var currNum = 0;
                var finder = new SnkFileFinder(this.LocalPath);
                return SnkPatch.GenerateSourceInfoList(0, finder, ref count, ref currNum) ?? new List<SnkSourceInfo>();
            }
        }
    }
}