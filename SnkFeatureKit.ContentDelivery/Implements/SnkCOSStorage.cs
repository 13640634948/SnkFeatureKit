using COSXML;
using COSXML.Model.Object;
using COSXML.Model.Bucket;

using System;
using System.Collections.Generic;
using System.Linq;
using COSXML.Auth;

using SnkFeatureKit.ContentDelivery.Abstract;

namespace SnkFeatureKit.ContentDelivery
{
    namespace Implements
    {
        public class SnkCOSStorage : SnkStorage
        {
            private readonly CosXml _cosClient;

            public SnkCOSStorage(string bucketName, string endPoint, string accessKeyId, string secretKey, bool isQuiteDelete = true, long keyDurationSecond = 600L)
                : base(bucketName, endPoint, accessKeyId, secretKey, isQuiteDelete)
            {
                var config = new CosXmlConfig.Builder()
                    .SetRegion(this.endPoint)
                    .Build();

                var credentialProvider = new DefaultQCloudCredentialProvider(
                    accessKeyId,
                    secretKey,
                    keyDurationSecond);

                _cosClient = new CosXmlServer(config, credentialProvider);
            }

            protected override string[] doDeleteObjects(List<string> keyList)
            {
                try
                {
                    var request = new DeleteMultiObjectRequest(this.bucketName);
                    request.SetDeleteQuiet(this.isQuiteDelete);
                    request.SetObjectKeys(keyList);
                    DeleteMultiObjectResult result = _cosClient.DeleteMultiObjects(request);
                }
                catch (COSXML.CosException.CosClientException cosException)
                {
                    this.setException(cosException);
                }
                catch (Exception exception)
                {
                    this.setException(exception);
                }

                return keyList.ToArray();
            }

            protected override StorageObject[] doLoadObjects(string prefixKey = null)
            {
                var keyList = new List<StorageObject>();
                try
                {
                    GetBucketRequest request = new GetBucketRequest(this.bucketName);
                    if (!string.IsNullOrEmpty(prefixKey))
                    {
                        request.SetPrefix(prefixKey + "/");
                    }
                    //执行请求
                    GetBucketResult result = _cosClient.GetBucket(request);
                    keyList.AddRange(result.listBucket.contentsList.Select(entry =>
                    {
                        return new StorageObject()
                        {
                            key = entry.key,
                            size = entry.size,
                            code = entry.eTag,
                        };
                    }));
                }
                catch (COSXML.CosException.CosClientException cosException)
                {
                    this.setException(cosException);
                }
                catch (Exception exception)
                {
                    this.setException(exception);
                }

                return keyList.ToArray();
            }

            protected override string[] doPutObjects(List<SnkPutObject> objectList)
            {
                var keyList = new List<string>();
                for (var i = 0; i < objectList.Count; i++)
                {
                    var storageObject = objectList[i];
                    try
                    {
                        var request = new PutObjectRequest(this.bucketName, storageObject.key, storageObject.path);
                        this._cosClient.PutObject(request);
                        keyList.Add(storageObject.key);
                    }
                    catch (COSXML.CosException.CosClientException cosException)
                    {
                        this.setException(cosException);
                    }
                    catch (Exception exception)
                    {
                        this.setException(exception);
                    }
                }
                return keyList.ToArray();

            }

            protected override string[] doTakeObjects(List<string> keyList, string localDirPath)
            {
                List<string> localFilePathList = new List<string>();
                try
                {
                    for (var i = 0; i < keyList.Count; i++)
                    {
                        GetObjectRequest request = new GetObjectRequest(this.bucketName, keyList[i], localDirPath, keyList[i]);
                        GetObjectResult result = this._cosClient.GetObject(request);
                    }
                }
                catch (COSXML.CosException.CosClientException cosException)
                {
                    this.setException(cosException);
                }
                catch (Exception exception)
                {
                    this.setException(exception);
                }
                return localFilePathList.ToArray();
            }
        }
    }
}