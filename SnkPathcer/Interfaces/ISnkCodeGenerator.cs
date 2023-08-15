namespace SnkFeatureKit.Patcher
{
    namespace Interfaces
    {
        public interface ISnkCodeGenerator
        {
            string CalculateFileMD5(string filePath);

            string CalculateContentMD5(string content);
        }
    }
}