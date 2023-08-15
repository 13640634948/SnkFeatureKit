using System;
using System.Collections.Generic;
using System.IO;

using SnkFeatureKit.ContentDelivery.Interfaces;

namespace SnkFeatureKit.ContentDelivery
{
    namespace Abstract
    {
        public abstract class SnkStorage : ISnkStorage
        {
            protected OnProgressCallback onProgressCallbackHandle;

            public virtual string StorageName => this.GetType().Name;
            public STORAGE_STATE mStorageState { get; protected set; }

            public Exception mException { get; protected set; }

            protected string bucketName { get; private set; }
            protected string endPoint { get; private set; }
            protected string accessKeyId { get; private set; }
            protected string accessKeySecret { get; private set; }
            protected bool isQuiteDelete { get; private set; }

            protected abstract StorageObject[] doLoadObjects(string prefixKey = null);

            protected abstract string[] doTakeObjects(List<string> keyList, string localDirPath);

            protected abstract string[] doPutObjects(List<SnkPutObject> objectList);

            protected abstract string[] doDeleteObjects(List<string> keyList);

            public SnkStorage(string bucketName, string endPoint, string accessKeyId, string accessKeySecret, bool isQuiteDelete)
            {
                this.bucketName = bucketName;
                this.endPoint = endPoint;
                this.accessKeyId = accessKeyId;
                this.accessKeySecret = accessKeySecret;
                this.isQuiteDelete = isQuiteDelete;
            }

            protected void setException(Exception exception)
            {
                this.mException = exception;
            }

            public void SetProgressCallbackHandle(OnProgressCallback progressCallback)
            {
                this.onProgressCallbackHandle = progressCallback;
            }

            protected void EnsurePathExists(string fullPath)
            {
                FileInfo fileInfo = new FileInfo(fullPath);
                if (fileInfo.Exists)
                    fileInfo.Delete();
                if (fileInfo.Directory.Exists == false)
                    fileInfo.Directory.Create();
            }

            public StorageObject[] LoadObjects(string prefixKey = null)
            {
                if (this.mStorageState != STORAGE_STATE.none)
                    return Array.Empty<StorageObject>();
                mStorageState = STORAGE_STATE.loading;
                var result = doLoadObjects(prefixKey);
                mStorageState = STORAGE_STATE.none;
                return result;
            }

            public string[] TakeObjects(List<string> keyList, string localDirPath)
            {
                if (this.mStorageState != STORAGE_STATE.none || keyList == null || keyList.Count == 0 || string.IsNullOrEmpty(localDirPath))
                    return Array.Empty<string>();
                mStorageState = STORAGE_STATE.takeing;
                var result = doTakeObjects(keyList, localDirPath);
                mStorageState = STORAGE_STATE.none;
                return result;
            }

            public string[] PutObjects(List<SnkPutObject> keyList)
            {
                if (this.mStorageState != STORAGE_STATE.none || keyList == null || keyList.Count == 0)
                    return Array.Empty<string>();
                mStorageState = STORAGE_STATE.putting;
                var result = doPutObjects(keyList);
                mStorageState = STORAGE_STATE.none;
                return result;
            }

            public string[] DeleteObjects(List<string> keyList)
            {
                if (this.mStorageState != STORAGE_STATE.none || keyList == null || keyList.Count == 0)
                    return Array.Empty<string>();
                mStorageState = STORAGE_STATE.deleting;
                var result = doDeleteObjects(keyList);
                mStorageState = STORAGE_STATE.none;
                return result;
            }

        }

    }
}