using System.Threading.Tasks;

namespace SnkFeatureKit.Patcher
{
    namespace Interfaces
    {
        public interface ISnkCompressor
        {
            Task Compress(string folderPath, string zipPath);
            Task Decompress(string file, string dir);
        }
    }    
}
