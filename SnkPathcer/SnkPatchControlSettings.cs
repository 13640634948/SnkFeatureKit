namespace SnkToolKit.Features.Patcher
{
    public class SnkPatchControlSettings : SnkPatchSettings
        {
            /// <summary>
            /// 渠道名字
            /// </summary>
            public string channelName;

            /// <summary>
            /// 当前应用版本号
            /// </summary>
            public int appVersion;

            /// <summary>
            /// URL下载地址
            /// </summary>
            public string[] remoteURLs;

            /// <summary>
            /// 本地仓库路径
            /// </summary>
            public string localPatchRepoPath;
        
    }
}