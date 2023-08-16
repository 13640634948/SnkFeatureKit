using System;
using System.Collections.Generic;

namespace SnkFeatureKit.Patcher
{
    [Serializable]
    public class SnkVersionInfos
    {
        /// <summary>
        /// 应用版本
        /// </summary>
        public int appVersion;

        /// <summary>
        /// 历史版本
        /// </summary>
        public List<SnkVersionMeta> histories;
    }
}    
