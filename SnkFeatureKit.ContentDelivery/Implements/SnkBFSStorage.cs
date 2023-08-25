using System;
using System.Collections.Generic;
using System.IO;
using SnkFeatureKit.ContentDelivery.Abstract;

namespace SnkFeatureKit.ContentDelivery
{
    namespace Implements
    {
        public class SnkBFSStorage : SnkStorage
        {
            public SnkBFSStorage(string bucketName, string endPoint, string accessKeyId, string accessKeySecret, bool isQuiteDelete) : base(bucketName, endPoint, accessKeyId, accessKeySecret, isQuiteDelete)
            {
            }

            protected override StorageObject[] doLoadObjects(string prefixKey = null)
            {
                throw new NotImplementedException();
            }

            protected override string[] doTakeObjects(List<string> keyList, string localDirPath)
            {
                throw new NotImplementedException();
            }

            protected override string[] doPutObjects(List<SnkPutObject> objectList)
            {
                throw new NotImplementedException();
            }

            protected override string[] doDeleteObjects(List<string> keyList)
            {
                throw new NotImplementedException();
            }
            
              /*
        private const string CommandFileList = "updir";
        private const string CommandDeleteFile = "delete";
        private const string CommandUploadFile = "upload";
        private const string CommandDownloadFile = "downfile";

        private static SnkJsonParser _parser = new SnkJsonParser();

        private List<HaloSvrFileInfo> CollectChildNode(HaloSvrFileInfo node)
        {
            var resultList = new List<HaloSvrFileInfo>();
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

        protected override IEnumerable<(string, long)> doLoadObjects(string key = null)
        {
            key ??= "";

            var remotePath = VWeb.server_url;
            remotePath = remotePath.FixSlash();
            var url = Path.Combine(remotePath, CommandFileList, key).FixSlash();
            if (string.IsNullOrEmpty(key))
            {
                url += "/";
            }

            Debug.Log($"获取文件列表路径:{url}");
            var result = Task.Run(() => SnkHttpWeb.Post(url, null, new CancellationTokenSource(5000))).Result;
            var code = result.Code;
            if (code != SNK_HTTP_ERROR_CODE.succeed)
            {
                Debug.LogError($"获取本地服务器上的文件列表报错 报错原因:{result.Exception}\n地址:{url}");
                return null;
            }


            List<HaloSvrFileInfo> fileInfoList = null;

            var content = Encoding.UTF8.GetString(result.Data);
            Debug.Log("content " + content);


            try
            {
                if (string.IsNullOrEmpty(key))
                {
                    var node = _parser.FromJson<HaloSvrFileInfo>(content);
                    fileInfoList = new List<HaloSvrFileInfo>();
                    fileInfoList.Add(node);
                }
                else
                {
                    fileInfoList = _parser.FromJson<List<HaloSvrFileInfo>>(content);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"获取本地服务器上的文件列表报错 报错原因:{e.Message}\n地址:{url}");
                return null;
            }

            var fileList = new List<HaloSvrFileInfo>();
            foreach (var item in fileInfoList)
            {
                fileList.AddRange(CollectChildNode(item));
            }

            var keyList = new List<(string, long)>();
            foreach (var item in fileList)
            {
                var relativeTo = Path.Combine("upload", key).FixSlash();
                var relativePath = Path.GetRelativePath(relativeTo, item.fullpath);

                Debug.LogError(relativePath);
                keyList.Add((relativePath, item.size));
            }

            return keyList;
        }

        protected override IEnumerable<byte> doTakeObject(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                Debug.LogError("[VInternalStorage][doTakeObject]传入key为空");
                return null;
            }

            var remotePath = VWeb.server_url;
            var url = Path.Combine(remotePath, CommandDownloadFile, key).FixSlash();
            Debug.Log($"下载地址:{url}");

            var result = Task.Run(() => SnkHttpWeb.Get(url)).Result;
            if (result.IsError)
            {
                Debug.LogError(
                    $"[VInternalStorage][doTakeObject]下载文件出现异常\n错误信息:{result.Exception}\nhttp状态码:{result.HttpStatusCode}");
                return null;
            }

            var text = Encoding.UTF8.GetString(result.Data);
            Debug.LogError(text);

            return null;
        }

        protected override string[] doTakeObjects(List<string> keyList, string localDirPath)
        {
            for (var i = 0; i < keyList.Count; i++)
            {
                var item = keyList[i];
                var data = doTakeObject(item) as byte[];

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

                File.WriteAllBytes(path, data);
            }

            return keyList.ToArray();
        }

        protected override string[] doPutObjects(List<Tuple<string, string>> keyList)
        {
            var list = new List<string>();
            for (var i = 0; i < keyList.Count; i++)
            {
                var item = keyList[i];
                var localPath = item.Item1;
                var remoteRelativeUrl = item.Item2;
                doPutObject(localPath, remoteRelativeUrl);
                list.Add(localPath);
            }

            return list.ToArray();
        }

        private void doPutObject(string localPath, string remoteRelativeUrl)
        {
            if (string.IsNullOrEmpty(localPath))
            {
                Debug.LogError("[VInternalStorage][doPutObject]传入key为空");
                return;
            }

            if (!System.IO.File.Exists(localPath))
            {
                Debug.LogError($"[VInternalStorage][doPutObject]给出路径找不到文件:{localPath}");
                return;
            }

            var remotePath = VWeb.server_url;
            var url = Path.Combine(remotePath, CommandUploadFile, remoteRelativeUrl).FixSlash();
            Debug.Log($"上传地址:{url}");
            var data = File.ReadAllBytes(localPath);
            var fileInfo = new FileInfo(localPath);
            var content = new MultipartFormDataContent();
            content.Add(new ByteArrayContent(data, 0, (int) data.Length), "file", fileInfo.Name);

            var result = Task.Run(() => SnkHttpWeb.Post(url, content, new CancellationTokenSource())).Result;
            var code = result.Code;
            if (code != SNK_HTTP_ERROR_CODE.succeed)
            {
                Debug.LogError($"获取本地服务器上的文件列表报错 报错原因:{result.Exception}\n地址:{url}");
                return;
            }

            Debug.Log($"{remoteRelativeUrl}上传成功");
        }


        protected override string[] doDeleteObjects(List<string> keyList)
        {
            throw new NotImplementedException();
        }

        private void doDeleteObject(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                Debug.LogError("[VInternalStorage][doPutObject]传入key为空");
                return;
            }


            var remotePath = VWeb.server_url;
            var url = Path.Combine(remotePath, CommandDeleteFile, key).FixSlash();
            Debug.Log($"删除文件列表路径:{url}");
            var result = Task.Run(() => SnkHttpWeb.Post(url, null, new CancellationTokenSource(5000))).Result;
            var code = result.Code;
            if (code != SNK_HTTP_ERROR_CODE.succeed)
            {
                Debug.LogError($"删除文件列表路径报错 报错原因:{result.Exception}\n地址:{url}");
                return;
            }

            Debug.Log($"删除成功:{key}");
        }
        */
        }
    }
}