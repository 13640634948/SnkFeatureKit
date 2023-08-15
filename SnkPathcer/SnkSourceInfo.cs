namespace SnkFeatureKit.Patcher
{
    /// <summary>
    /// 补丁源(PatchSource)
    /// </summary>
    public struct SnkSourceInfo
    {
        /// <summary>
        /// 源文件键值
        /// </summary>
        public string key;

        /// <summary>
        /// 源文件版本
        /// </summary>
        public ushort version;

        /// <summary>
        /// 源文件大小
        /// </summary>
        public long size;

        /// <summary>
        /// 源文件验证值
        /// </summary>
        public string code;

        public override string ToString()
        {
            return $"{key}|{version}|{size}|{code}";
        }

        public static SnkSourceInfo ValueOf(string content)
        {
            var sourceInfo = new SnkSourceInfo();
            var array = content.Trim().Split('|');
            if (array.Length != 4)
            {
                throw new System.Exception($"SnkSourceInfo Parse Error. len is not 4. len:{array.Length}. content:{content}");
            }

            if (string.IsNullOrEmpty(array[0]))
            {
                throw new System.Exception($"Parse SnkSourceInfo.key Error. param is null or empty. content:{content}");
            }
            sourceInfo.key = array[0];

            if (ushort.TryParse(array[1], out sourceInfo.version) == false)
            {
                throw new System.Exception($"Parse SnkSourceInfo.version Error. param:{array[1]}. content:{content}");
            }

            if (long.TryParse(array[2], out sourceInfo.size) == false)
            {
                throw new System.Exception($"Parse SnkSourceInfo.size Error. param:{array[2]}. content:{content}");
            }

            if (string.IsNullOrEmpty(array[3]))
            {
                throw new System.Exception($"Parse SnkSourceInfo.code Error. param is null or empty. content:{content}");
            }
            sourceInfo.code = array[3];
            return sourceInfo;
        }
    }
}    
