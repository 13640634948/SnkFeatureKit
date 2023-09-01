using System.Collections.Generic;

namespace SnkFeatureKit.Patcher
{
    namespace Interfaces
    {
        public interface ISnkCompressor
        {
            void Compress(string folderPath, string zipPath);
            void Decompress(string file, string dir);
            void ExtractEntry(string file, string dir, List<string> keyList);
        }
    }    
}
