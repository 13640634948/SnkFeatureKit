using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SnkFeatureKit.ContentDelivery.Abstract;
using SnkFeatureKit.ContentDelivery.Extensions;
using SnkFeatureKit.Patcher;

namespace SnkFeatureKit.ContentDelivery
{
    namespace Implements
    {
        /// <summary>
        /// 内网存储
        /// </summary>
        public class SnkBFSStorage : SnkStorage
        {
            /// <summary>
            /// 访问所有文件
            /// </summary>
            private const string CommandFileList = "updir";

            /// <summary>
            /// 删除文件
            /// </summary>
            private const string CommandDeleteFile = "delete";

            /// <summary>
            /// 上传文件
            /// </summary>
            private const string CommandUploadFile = "upload";

            /// <summary>
            /// 下载文件
            /// </summary>
            private const string CommandDownloadFile = "downfile";

            /// <summary>
            /// 换行符
            /// </summary>
            const string CRLF = "\r\n";

            /// <summary>
            /// json解析器
            /// </summary>
            private ISnkJsonParser _jsonParser;

            /// <summary>
            /// 构造方法
            /// </summary>
            /// <param name="bucketName"></param>
            /// <param name="endPoint"></param>
            /// <param name="accessKeyId"></param>
            /// <param name="accessKeySecret"></param>
            /// <param name="isQuiteDelete"></param>
            /// <param name="jsonParser"></param>
            public SnkBFSStorage(string bucketName, string endPoint, string accessKeyId, string accessKeySecret,
                bool isQuiteDelete, ISnkJsonParser jsonParser) : base(bucketName, endPoint, accessKeyId,
                accessKeySecret, isQuiteDelete)
            {
                _jsonParser = jsonParser;
            }

            /// <summary>
            /// 列举文件
            /// </summary>
            /// <param name="prefixKey"></param>
            /// <param name="key"></param>
            /// <returns></returns>
            protected override StorageObject[] doLoadObjects(string key = null)
            {
                if (key.EndsWith("/"))
                    key = key.Remove(key.Length - 1, 1);
                var remotePath = endPoint;
                remotePath = remotePath.FixSlash();

                var url = Path.Combine(remotePath, CommandFileList, bucketName, key).FixSlash();

                Console.WriteLine($"获取文件列表路径:{url}");
                var result = Task.Run(() => SnkHttpWeb.Post(url, null, 5000)).Result;

                if (string.IsNullOrEmpty(result.Item2))
                {
                    Debug.WriteLine($"获取本地服务器上的文件列表报错\n报错地址:{url}");
                    return null;
                }

                List<BFSvrFileInfo> fileInfoList = null;
                var content = result.Item2;
                try
                {
                    fileInfoList = _jsonParser.FromJson<List<BFSvrFileInfo>>(content);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"获取本地服务器上的文件列表报错 报错原因:{e.Message}\n地址:{url}");
                    return null;
                }


                var keyList = new List<StorageObject>();

                var fileList = new List<BFSvrFileInfo>();
                foreach (var item in fileInfoList)
                {
                    fileList.AddRange(CollectChildNode(item));
                }

                foreach (var item in fileList)
                {
                    var fullKey = item.fullpath.FixSlash();
                    var needRemove = fullKey.IndexOf(this.bucketName) + bucketName.Length + 1;

                    fullKey = fullKey.Remove(0, needRemove);
                    // needRemove = fullKey.IndexOf(key) + key.Length;
                    // fullKey = fullKey.Remove(0, needRemove);
                    // if (string.IsNullOrEmpty(fullKey))
                    // {
                    //     //说明是文件
                    //     fullKey = Path.GetFileName(item.fullpath.FixSlash());
                    // }

                    var so = new StorageObject
                    {
                        key = fullKey,
                        size = item.size,
                        code = item.md5
                    };
                    keyList.Add(so);
                }

                return keyList.ToArray();
            }

            /// <summary>
            /// 收集子节点
            /// </summary>
            /// <param name="node"></param>
            /// <returns></returns>
            private List<BFSvrFileInfo> CollectChildNode(BFSvrFileInfo node)
            {
                var resultList = new List<BFSvrFileInfo>();
                if (node.nodeType == "file")
                {
                    resultList.Add(node);
                }

                for (var i = 0; i < node.mChildLst.Count; i++)
                {
                    var item = node.mChildLst[i];
                    if (item.nodeType == "file")
                    {
                        resultList.Add(item);
                    }
                    else
                    {
                        var tmp = CollectChildNode(item);
                        resultList.AddRange(tmp);
                    }
                }

                return resultList;
            }

            /// <summary>
            /// 下载单个文件
            /// </summary>
            /// <param name="key"></param>
            /// <returns></returns>
            protected string doTakeObject(string key)
            {
                if (string.IsNullOrEmpty(key))
                {
                    Console.WriteLine("[VInternalStorage][doTakeObject]传入key为空");
                    return null;
                }

                var remotePath = $"{endPoint}/{CommandDownloadFile}/{bucketName}";
                var url = Path.Combine(remotePath, key).FixSlash();
                Console.WriteLine($"下载地址:{url}");

                var result = Task.Run(() => SnkHttpWeb.Get(url)).Result;
                if (string.IsNullOrEmpty(result))
                {
                    Console.WriteLine(
                        $"[VInternalStorage][doTakeObject]下载文件出现异常\n错误key:{key}");
                    return null;
                }

                return result;
            }


            /// <summary>
            /// 下载文件
            /// </summary>
            /// <param name="keyList"></param>
            /// <param name="localDirPath"></param>
            /// <returns></returns>
            protected override string[] doTakeObjects(List<string> keyList, string localDirPath)
            {
                for (var i = 0; i < keyList.Count; i++)
                {
                    var item = keyList[i];
                    var dataContent = doTakeObject(item);

                    var data = dataContent;

                    var path = Path.Combine(localDirPath, item).FixSlash();
                    var fileInfo = new FileInfo(path);

                    if (!fileInfo.Directory.Exists)
                    {
                        fileInfo.Directory.Create();
                    }
                    else
                    {
                        if (fileInfo.Exists)
                            fileInfo.Delete();
                    }

                    File.WriteAllText(path, data);
                }

                return keyList.ToArray();
            }

            /// <summary>
            /// 上传文件列表
            /// </summary>
            /// <param name="objectList"></param>
            /// <returns></returns>
            protected override string[] doPutObjects(List<SnkPutObject> objectList)
            {
                var list = new List<string>();
                for (var i = 0; i < objectList.Count; i++)
                {
                    var item = objectList[i];
                    var localPath = item.path;
                    var remoteRelativeUrl = item.key;
                    doPutObject(localPath, remoteRelativeUrl);
                    list.Add(localPath);
                }

                return list.ToArray();
            }

            /// <summary>
            /// 上传单个文件
            /// </summary>
            /// <param name="localPath"></param>
            /// <param name="remoteRelativeUrl"></param>
            private void doPutObject(string localPath, string remoteRelativeUrl)
            {
                if (string.IsNullOrEmpty(localPath))
                {
                    Console.WriteLine("[VInternalStorage][doPutObject]传入key为空");
                    return;
                }

                if (!System.IO.File.Exists(localPath))
                {
                    Console.WriteLine($"[VInternalStorage][doPutObject]给出路径找不到文件:{localPath}");
                    return;
                }

                var remotePath = $"{this.endPoint}/{CommandUploadFile}/{this.bucketName}";
                var url = Path.Combine(remotePath, remoteRelativeUrl).FixSlash();
                Console.WriteLine($"上传地址:{url}");
                var fileInfo = new FileInfo(localPath);

                var nameNvc = new NameValueCollection();
                //nameNvc.Add("file", fileInfo.Name);

                var fileNvc = new NameValueCollection {{fileInfo.Name, fileInfo.FullName}};
                var result = UploadMultipartFormData(url, "POST", null, nameNvc, fileNvc);

                if (string.IsNullOrEmpty(result))
                {
                    Console.WriteLine($"获取本地服务器上的文件列表报错 \n地址:{url}");
                    return;
                }

                Console.WriteLine($"{remoteRelativeUrl}上传成功");
            }

            /// <summary>
            /// 批量删除文件
            /// </summary>
            /// <param name="keyList"></param>
            /// <returns></returns>
            protected override string[] doDeleteObjects(List<string> keyList)
            {
                var resultList = new List<string>();
                for (var i = 0; i < keyList.Count; i++)
                {
                    var item = keyList[i];
                    var result = doDeleteObject(item);
                    var resultContent = string.Empty;
                    if (result)
                    {
                        resultContent = $"key:{item}-----删除成功";
                    }
                    else
                    {
                        resultContent = $"key:{item}-----删除失败";
                    }

                    resultList.Add($"key:{item}--{resultContent}");
                }

                return resultList.ToArray();
            }

            /// <summary>
            /// 删除单个文件
            /// </summary>
            /// <param name="key"></param>
            /// <returns></returns>
            private bool doDeleteObject(string key)
            {
                if (string.IsNullOrEmpty(key))
                {
                    Console.WriteLine("[VInternalStorage][doPutObject]传入key为空");
                    return false;
                }

                var remotePath = $"{endPoint}/{CommandDeleteFile}/{bucketName}";
                var url = Path.Combine(remotePath, key).FixSlash();
                Console.WriteLine($"删除文件列表路径:{url}");
                var result = Task.Run(() => SnkHttpWeb.Post(url, null, 5000)).Result;

                if (result == null)
                {
                    Console.WriteLine($"删除文件列表路径报错 \n地址:{url}");
                    return false;
                }

                Console.WriteLine($"删除成功:{key}");
                return true;
            }

            /// <summary>
            /// 使用multipart/form-data方式上传文件及提交其他数据
            /// </summary>
            /// <param name="headers">请求头参数</param>
            /// <param name="nameValueCollection">键值对参数</param>
            /// <param name="fileCollection">文件参数：参数名，文件路径</param>
            /// <returns>接口返回结果</returns>
            public static string UploadMultipartFormData(string url, string httpMethod,
                Dictionary<string, string> headers, NameValueCollection nameValueCollection,
                NameValueCollection fileCollection)
            {
                var boundary = string.Format("batch_{0}", Guid.NewGuid());
                var startBoundary = string.Format("--{0}", boundary);

                // Set up Request body.
                WebRequest request = HttpWebRequest.Create(url);
                if (headers != null)
                {
                    foreach (var item in headers)
                    {
                        request.Headers.Add(item.Key, item.Value);
                    }
                }

                request.Method = httpMethod;

                request.ContentType = $"multipart/form-data; boundary={boundary}";
                var result = string.Empty;
                // Writes the boundary and Content-Disposition header, then writes
                // the file binary, and finishes by writing the closing boundary.
                using (Stream requestStream = request.GetRequestStream())
                {
                    StreamWriter writer = new StreamWriter(requestStream);

                    // 处理文件内容
                    var fileKeys = fileCollection.AllKeys;
                    foreach (var key in fileKeys)
                    {
                        WriteFileToStream(writer, startBoundary, key, fileCollection[key]);
                    }

                    // 键值对参数
                    var allKeys = nameValueCollection.AllKeys;
                    foreach (var key in allKeys)
                    {
                        WriteNvToStream(writer, startBoundary, key, nameValueCollection[key]);
                    }

                    var endFormData = CRLF + $"--{boundary}--" + CRLF;

                    writer.Write(endFormData);
                    writer.Flush();
                    writer.Close();

                    var response = (HttpWebResponse) request.GetResponse();
                    result = new StreamReader(response.GetResponseStream()).ReadToEnd();
                }

                return result;
            }

            static void WriteFileToStream(StreamWriter writer, string startBoundary, string name, string filePath)
            {
                var filename = Path.GetFileName(filePath);
                var fileRequestBody = startBoundary + CRLF;
                fileRequestBody += $"Content-Disposition: form-data; name=\"{name}\"; filename=\"{filename}\"" + CRLF +
                                   CRLF;

                writer.Write(fileRequestBody);
                writer.Flush();

                var bmpBytes = File.ReadAllBytes(filePath);
                writer.BaseStream.Write(bmpBytes, 0, bmpBytes.Length);
            }

            static void WriteNvToStream(StreamWriter writer, string startBoundary, string name, string value)
            {
                var nvFormData = CRLF + startBoundary + CRLF;
                nvFormData += $"Content-Disposition: form-data; name=\"{name}\"" + CRLF + CRLF;
                nvFormData += value /*+ CRLF*/;

                writer.Write(nvFormData);
                writer.Flush();
            }


            [Serializable]
            public class BFSvrFileInfo
            {
                public string fullpath;
                public int depth;
                public string nodeType;
                public long size;
                public string md5;
                public string createDate;
                public string lastModifyDate;
                public List<BFSvrFileInfo> mChildLst;

                public BFSvrFileInfo()
                {
                    this.mChildLst = new List<BFSvrFileInfo>();
                }

                public List<string> GetAllFilesUrl()
                {
                    var list = new List<string>();
                    var count = mChildLst.Count;
                    for (var i = 0; i < count; i++)
                    {
                        var item = mChildLst[i];
                        if (item.nodeType == "file") list.Add(item.fullpath);
                        else if (item.nodeType == "directory")
                        {
                            if (item.mChildLst.Count != 0)
                                list.AddRange(item.GetAllFilesUrl());
                        }
                        else
                        {
                            Console.WriteLine("GetAllFilesUrl  Error");
                            return null;
                        }
                    }

                    return list;
                }

                public List<string> GetAllDirectorysUrl()
                {
                    var list = new List<string>();
                    var count = mChildLst.Count;
                    for (var i = 0; i < count; i++)
                    {
                        var item = mChildLst[i];
                        if (item.nodeType != "directory") continue;
                        list.Add(item.fullpath);
                        if (item.mChildLst.Count == 0) continue;
                        list.AddRange(item.GetAllDirectorysUrl());
                    }

                    return list;
                }
            }
        }
    }
}