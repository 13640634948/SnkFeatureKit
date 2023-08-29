using System.Threading.Tasks;
using SnkFeatureKit.Patcher.Interfaces;
using System.IO.Compression;

namespace SnkFeatureKit.Patcher
{
    namespace Implements
    {
        public class SnkCompressor : ISnkCompressor
        {
            public Task Compress(string folderPath, string zipPath)
            {
                throw new System.NotImplementedException();
            }

            public Task Decompress(string file, string dir)
            {
                throw new System.NotImplementedException();
            }
        }
    }    
}
