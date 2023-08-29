namespace SnkFeatureKit.Patcher
{
    public struct SnkVersionMeta
    {
        public ushort version;
        public long size;
        public int count;
        public string code;
        
        public override string ToString()
            => $"ver:{version}, size:{size}, count:{count}, code:{code}";
    }
}
