using System.IO;

namespace SnkFeatureKit.Patcher
{
    namespace Interfaces
    {
        public interface ISnkFileFinder
        {
            SearchOption SearchOption { get; }

            string SourceDirPath { get; }

            bool TrySurvey(out FileInfo[] fileInfos);
        }
    }
}