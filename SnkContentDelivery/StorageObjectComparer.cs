using System.Collections.Generic;

namespace SnkToolKit.Features.ContentDelivery
{
    public class StorageObjectComparer : IEqualityComparer<StorageObject>
    {
        public bool Equals(StorageObject x, StorageObject y)
            => x.key == y.key && x.size == y.size && x.code == y.code;

        public int GetHashCode(StorageObject obj)
            => obj.key.GetHashCode() ^ obj.size.GetHashCode() ^ obj.code.GetHashCode();
    }
}