using System.Collections.Generic;

namespace SnkFeatureKit.Patcher
{
    namespace Interfaces
    {
        public interface ISnkCompressor
        {
            void Compress(string destinationDirectoryName, string sourceArchiveFileName);
            void Decompress(string sourceArchiveFileName, string destinationDirectoryName);
            void EntryExtractToFile(string sourceArchiveFileName, string destinationDirectoryName, List<string> entryKeyList);
            List<string> GetEntryKeyList(string sourceArchiveFileName);
        }
    }    
}
