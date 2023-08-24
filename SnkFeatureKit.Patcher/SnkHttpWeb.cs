using SnkFeatureKit.Patcher.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SnkFeatureKit.Patcher
{
    /// <summary>
    /// web请求
    /// </summary>
    public class SnkHttpWeb
    {
        /// <summary>
        /// Head方法
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="timeoutMilliseconds"></param>
        /// <returns></returns>
        public static long GetContentLength(string uri, int timeoutMilliseconds = 5 * 1000)
        {
            uri = uri.FixSlash();
            var request = WebRequest.CreateHttp(uri);
            request.Method = "HEAD";
            request.Timeout = timeoutMilliseconds;
            request.KeepAlive = true;
            long length;
            using (var response = request.GetResponse())
            {
                if (response.Headers != null)
                {
                    length = response.ContentLength;
                    response.Close();
                }
                else
                {
                    throw new Exception($"服务器没有返回content-length,无法获取文件长度\n访问地址:{uri}");
                }
            }
            return length;
        }

        /// <summary>
        /// Get方法
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="timeoutMilliseconds"></param>
        /// <returns></returns>
        public static string Get(string uri, int timeoutMilliseconds = 5 * 1000)
        {
            uri = uri.FixSlash();
            var request = WebRequest.CreateHttp(uri);
            request.Method = "GET";
            request.Timeout = timeoutMilliseconds;
            request.KeepAlive = true;

            string contentData;
            using (var response = request.GetResponse())
            {
                using (var responseStream = response.GetResponseStream())
                {
                    if (responseStream == null)
                        throw new ArgumentNullException($"responseStream is null.Url:{uri}");
                    
                    using (var readStream = new StreamReader(responseStream))
                    {
                        contentData = readStream.ReadToEnd();
                        readStream.Close();
                    }
                    responseStream.Close();
                }
                response.Close();
            }

            return contentData;
        }

        /// <summary>
        /// Post方法
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="postContent"></param>
        /// <param name="timeoutMilliseconds"></param>
        /// <returns></returns>
        public static Tuple<Dictionary<string, string>, string> Post(string uri, string postContent, int timeoutMilliseconds = 5 * 1000)
        {
            uri = uri.FixSlash();

            var request = WebRequest.CreateHttp(uri);
            request.Method = "POST";
            request.Timeout = timeoutMilliseconds;
            request.KeepAlive = true;
            request.ContentType = "application/json";

            Dictionary<string, string> rspHeadDict = null;
            string contentData = null;

            if (string.IsNullOrEmpty(postContent) == false)
            {
                var postContentBytes = Encoding.UTF8.GetBytes(postContent);
                request.ContentLength = postContentBytes.Length;
                using (var requestStream = request.GetRequestStream())
                {
                    requestStream.Write(postContentBytes, 0, postContentBytes.Length);
                    requestStream.Close();
                }
            }

            using (var response = request.GetResponse())
            {
                if (response.Headers != null && response.Headers.Count > 0)
                {
                    rspHeadDict = new Dictionary<string, string>();
                    for (var i = 0; i < response.Headers.Count; i++)
                    {
                        var key = response.Headers.GetKey(i);
                        var value = response.Headers.Get(key);
                        rspHeadDict[key] = value;
                    }
                }

                using (var responseStream = response.GetResponseStream())
                {
                    if (responseStream == null)
                        throw new ArgumentNullException($"responseStream is null.Url:{uri}");
                    
                    using (var readStream = new StreamReader(responseStream))
                    {
                        contentData = readStream.ReadToEnd();
                        readStream.Close();
                    }
                    responseStream.Close();
                }
                response.Close();
            }
            return new Tuple<Dictionary<string, string>, string>(rspHeadDict, contentData);
        }

        public static Task<long> GetContentLengthAsync(string uri, int timeoutMilliseconds = 10 * 1000)
            => Task.Run(() => GetContentLength(uri, timeoutMilliseconds));
        public static Task<string> GetAsync(string uri, int timeoutMilliseconds = 10 * 1000)
            => Task.Run(() => Get(uri, timeoutMilliseconds));

        public static Task<Tuple<Dictionary<string, string>, string>> PostAsync(string uri, string postContent, int timeoutMilliseconds = 10 * 1000)
            => Task.Run(() => Post(uri, postContent, timeoutMilliseconds));
    }
}
