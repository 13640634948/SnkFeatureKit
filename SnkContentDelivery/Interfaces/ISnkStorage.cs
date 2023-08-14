using System.Collections.Generic;

namespace SnkToolKit.Features.ContentDelivery
{
    namespace Interfaces
    {
        public interface ISnkStorage
        {
            string StorageName { get; }

            STORAGE_STATE mStorageState { get; }

            System.Exception mException { get; }
            void SetProgressCallbackHandle(OnProgressCallback progressCallback);

            StorageObject[] LoadObjects(string prefixKey = null);
            string[] TakeObjects(List<string> keyList, string localDirPath);
            string[] PutObjects(List<SnkPutObject> keyList);
            string[] DeleteObjects(List<string> keyList);
        }
    }
}