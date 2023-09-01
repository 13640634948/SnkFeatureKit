namespace SnkFeatureKit.Patcher.Exceptions
{
    public class SnkAppVersionException : SnkException
    {
        public int LastAppVersion { get; }

        public SnkAppVersionException(int lastAppVersion) : base($"new version:{lastAppVersion}")
        {
            LastAppVersion = lastAppVersion;
        }
    }
}