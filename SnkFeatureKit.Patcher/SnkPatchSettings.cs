namespace SnkFeatureKit.Patcher
{
    public class SnkPatchSettings
        {
            /// <summary>
            /// 资源清单文件名
            /// </summary>
            public string manifestFileName = "manifest.json";

            /// <summary>
            /// 资源版本信息文件
            /// </summary>
            public string appVersionInfoFileName = "app_version.json";
            
            /// <summary>
            /// 资源版本信息文件
            /// </summary>
            public string resVersionInfoFileName = "res_version.json";

            /// <summary>
            /// 版本资源中间目录
            /// </summary>
            public string assetsDirName = "assets";

            /// <summary>
            /// 本地版本文件存放路径
            /// </summary>
            public string localVersionDir = string.Empty;
        
    }
}