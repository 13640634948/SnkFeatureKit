namespace SnkFeatureKit.Patcher
{
    namespace Extensions
    {
        internal static class StringExtension
        {
            public static string FixLongPath(this string content)
            {
                content = System.IO.Path.GetFullPath(content);
                if (content.Length >= 256)
                    return @"\\?\" + content;
                return content;
            }

            public static string FixSlash(this string content)
                => content.Replace("\\", "/");
        }
    }
}