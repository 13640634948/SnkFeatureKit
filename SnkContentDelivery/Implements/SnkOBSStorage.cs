using OBS;
using OBS.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using SnkFeatureKit.ContentDelivery.Abstract;

namespace SnkFeatureKit.ContentDelivery
{
    namespace Implements
    {
        public class SnkOBSStorage : SnkStorage
        {
            private readonly ObsClient _obsClient;

            public SnkOBSStorage(string bucketName, string endPoint, string accessKeyId, string accessKeySecret, bool isQuiteDelete = true)
                : base(bucketName, endPoint, accessKeyId, accessKeySecret, isQuiteDelete)
            {
                _obsClient = new ObsClient(accessKeyId, accessKeySecret, endPoint);
            }

            /// <summary>
            /// 从仓库中删除对象
            /// </summary>
            /// <param name="keyList">待对象Key列表</param>
            /// <returns>已删除的对象Key列表</returns>
            protected override string[] doDeleteObjects(List<string> keyList)
            {
                try
                {
                    var request = new DeleteObjectsRequest
                    {
                        BucketName = this.bucketName,
                        Quiet = this.isQuiteDelete,
                    };
                    foreach (var key in keyList)
                    {
                        request.AddKey(key);
                        Console.WriteLine($"[DEL]{key}");
                    }
                    this._obsClient.DeleteObjects(request);
                }
                catch (ObsException obsException)
                {
                    this.setException(obsException);
                }
                catch (Exception exception)
                {
                    this.setException(exception);
                }

                return keyList.ToArray();
            }

            /// <summary>
            /// 列举对象
            /// </summary>
            /// <param name="prefixKey">对象Key前缀</param>
            /// <returns>仓库对象</returns>
            protected override StorageObject[] doLoadObjects(string prefixKey = null)
            {
                var keyList = new List<StorageObject>();
                try
                {
                    ListObjectsResponse response;
                    var request = new ListObjectsRequest
                    {
                        BucketName = this.bucketName,
                        MaxKeys = 1000,
                        Prefix = prefixKey,
                    };

                    do
                    {
                        response = this._obsClient.ListObjects(request);
                        keyList.AddRange(response.ObsObjects.Select(entry =>
                        {
                            return new StorageObject()
                            {
                                key = entry.ObjectKey,
                                size = entry.Size,
                                code = entry.ETag.ToUpper().Substring(1, entry.ETag.Length - 2),
                            };
                        }));
                        request.Marker = response.NextMarker;
                    } while (response.IsTruncated);
                }
                catch (ObsException obsException)
                {
                    this.setException(obsException);
                }
                catch (Exception exception)
                {
                    this.setException(exception);
                }

                return keyList.ToArray();
            }

            /// <summary>
            /// 放置对象到仓库中
            /// </summary>
            /// <param name="objectList">对象列表</param>
            /// <returns>仓库中Key的列表</returns>
            protected override string[] doPutObjects(List<SnkPutObject> objectList)
            {
                var keyList = new List<string>();
                try
                {
                    for (var i = 0; i < objectList.Count; i++)
                    {
                        var storageObject = objectList[i];

                        if (System.IO.File.Exists(storageObject.path) == false)
                        {
                            throw new System.IO.FileNotFoundException($"key:{storageObject.key}\npath:{storageObject.path}");
                        }

                        var request = new PutObjectRequest
                        {
                            BucketName = this.bucketName,
                            ObjectKey = storageObject.key,
                            FilePath = storageObject.path
                        };
                        request.UploadProgress += (_, status) => this.onProgressCallbackHandle?.Invoke(storageObject.key, status.TransferredBytes, status.TotalBytes, i, objectList.Count);
                        _obsClient.PutObject(request);
                        keyList.Add(storageObject.key);
                        Console.WriteLine($"[PUT]{storageObject.key}");
                    }
                }
                catch (ObsException obsException)
                {
                    this.setException(obsException);
                }
                catch (Exception exception)
                {
                    this.setException(exception);
                }
                return keyList.ToArray();
            }

            /// <summary>
            /// 从仓库获取对象到本地
            /// </summary>
            /// <param name="keyList">对象KEY</param>
            /// <param name="localDirPath">本地目录</param>
            /// <returns>本地文件地址</returns>
            protected override string[] doTakeObjects(List<string> keyList, string localDirPath)
            {
                List<string> localFilePathList = new List<string>();
                try
                {
                    for (var i = 0; i < keyList.Count; i++)
                    {
                        var request = new GetObjectRequest
                        {
                            BucketName = this.bucketName,
                            ObjectKey = keyList[i]
                        };
                        request.DownloadProgress += (_, status) =>
                            this.onProgressCallbackHandle?.Invoke(keyList[i], status.TransferredBytes,
                                status.TotalBytes, i, keyList.Count);

                        var response = _obsClient.GetObject(request);
                        var localFullName = System.IO.Path.Combine(localDirPath, request.ObjectKey);
                        response.WriteResponseStreamToFile(localFullName);
                        localFilePathList.Add(localFullName);
                        Console.WriteLine($"[TAKE]{localFullName}");
                    }
                }
                catch (ObsException obsException)
                {
                    this.setException(obsException);
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