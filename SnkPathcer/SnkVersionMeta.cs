namespace SnkFeatureKit.Patcher
{
    public struct SnkVersionMeta
    {
        public ushort version;
        public long size;
        public int count;
        public string code;

        public override string ToString()
        {
            return $"{version}|{size}|{count}|{code}";
        }

        public static SnkVersionMeta ValueOf(string content)
        {
            var meta = new SnkVersionMeta();
            var array = content.Trim().Split('|');
            if (array.Length != 4)
            {
                throw new System.Exception($"SnkVersionMeta Parse Error. len is not 4. len:{array.Length}. content:{content}");
            }

            if (ushort.TryParse(array[0], out meta.version) == false)
            {
                throw new System.Exception($"Parse SnkVersionMeta.version Error. param:{array[0]}. content:{content}");
            }

            if (long.TryParse(array[1], out meta.size) == false)
            {
                throw new System.Exception($"Parse SnkVersionMeta.size Error. param:{array[1]}. content:{content}");
            }

            if (int.TryParse(array[2], out meta.count) == false)
            {
                throw new System.Exception($"Parse SnkVersionMeta.count Error. param:{array[2]}. content:{content}");
            }

            if (string.IsNullOrEmpty(array[3]))
            {
                throw new System.Exception($"Parse SnkVersionMeta.code Error. param is null or empty. content:{content}");
            }
            meta.code = array[3];
            return meta;
        }
    }
}
