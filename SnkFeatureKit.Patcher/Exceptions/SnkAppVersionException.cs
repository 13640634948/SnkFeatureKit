namespace SnkFeatureKit.Patcher.Exceptions
{
    public class SnkAppVersionException : SnkException
    {
        public SnkAppVersionException(int lastAppVersion) : base($"new version:{lastAppVersion}")
        {
            
        }
    }
}