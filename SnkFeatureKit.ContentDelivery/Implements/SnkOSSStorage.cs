using Aliyun.OSS;
using Aliyun.OSS.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SnkFeatureKit.ContentDelivery.Abstract;

namespace SnkFeatureKit.ContentDelivery
{
    namespace Implements
    {
        public class SnkOSSStorage : SnkStorage
        {
            private readonly int _buffSize;
            private readonly IOss _ossClient;

            public SnkOSSStorage(string bucketName, string endPoint, string accessKeyId, string accessKeySecret, bool isQuiteDelete = true, int buffSize = 1024 * 1024 * 8)
                : base(bucketName, endPoint, accessKeyId, accessKeySecret, isQuiteDelete)
            {
                _ossClient = new OssClient(endPoint, accessKeyId, accessKeySecret);
                this._buffSize = buffSize;
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
                    var request = new DeleteObjectsRequest(this.bucketName, keyList, this.isQuiteDelete);
                    this._ossClient.DeleteObjects(request);
                }
                catch (OssException ossException)
                {
                    this.setException(ossException);
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
                    ObjectListing result = null;
                    string nextMarker = string.Empty;
                    do
                    {
                        // 每页列举的文件个数通过MaxKeys指定，超出指定数量的文件将分页显示。
                        var listObjectsRequest = new ListObjectsRequest(bucketName)
                        {
                            Marker = nextMarker,
                            MaxKeys = 100,
                            Prefix = prefixKey
                        };
                        result = _ossClient.ListObjects(listObjectsRequest);
                        keyList.AddRange(result.ObjectSummaries.Select(entry =>
                        {
                            return new StorageObject()
                            {
                                key = entry.Key,
                                size = entry.Size,
                                code = entry.ETag,
                            };
                        }));

                        nextMarker = result.NextMarker;
                    } while (result.IsTruncated);
                }
                catch (OssException ossException)
                {
                    this.setException(ossException);
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
                        _ossClient.PutObject(this.bucketName, storageObject.key, storageObject.path);
                        keyList.Add(storageObject.key);
                    }
                }
                catch (OssException obsException)
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
                        var objectName = keyList[i];
                        var downloadFilename = Path.Combine(localDirPath, objectName);

                        var obj = _ossClient.GetObject(bucketName, objectName);
                        using (var requestStream = obj.Content)
                        {
                            var buf = new byte[this._buffSize];
                            var fs = File.Open(downloadFilename, FileMode.OpenOrCreate);
                            var len = 0;
                            // 通过输入流将文件的内容读取到文件或者内存中。
                            while ((len = requestStream.Read(buf, 0, buf.Length)) != 0)
                            {
                                fs.Write(buf, 0, len);
                            }
                            fs.Close();
                        }
                    }
                }
                catch (OssException obsException)
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