using SnkFeatureKit.Patcher.Extension;
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
        /// <param name="timeout"></param>
        /// <returns></returns>
        public static SnkHttpHeadResult Head(string uri, int timeoutMilliseconds = 10 * 1000)
        {
            uri = uri.FixSlash();
            SnkHttpHeadResult result = null;
            var code = SNK_HTTP_ERROR_CODE.succeed;
            var httpCode = HttpStatusCode.OK;
            Exception exception = null;
            var length = 0L;
            try
            {
                var request = WebRequest.CreateHttp(uri);
                request.Method = "HEAD";
                request.Timeout = timeoutMilliseconds;
                request.KeepAlive = true;

                using (var response = request.GetResponse())
                {
                    if (response == null && response.Headers != null)
                    {
                        length = response.ContentLength;
                        response.Close();
                    }
                    else
                    {
                        code = SNK_HTTP_ERROR_CODE.can_not_find_length;
                        exception = new System.Exception($"服务器没有返回content-length,无法获取文件长度\n访问地址:{uri}");
                    }
                }
            }
            catch (Exception e)
            {
                code = SNK_HTTP_ERROR_CODE.error;
                exception = new System.Exception($"请求{uri}出现异常\n异常信息:{e.Message}\n异常堆栈:{e.StackTrace}");
            }
            finally
            {
                result = new SnkHttpHeadResult(code, httpCode, exception, length);
            }
            return result;
        }

        /// <summary>
        /// Get方法
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public static SnkHttpGetResult Get(string uri, int timeoutMilliseconds = 10 * 1000, int buffSize = 1024 * 8)
        {
            uri = uri.FixSlash();
            SnkHttpGetResult result = null;
            var code = SNK_HTTP_ERROR_CODE.succeed;
            var httpCode = HttpStatusCode.OK;
            Exception exception = null;
            string contentData = null;
            try
            {
                var request = WebRequest.CreateHttp(uri);
                request.Method = "GET";
                request.Timeout = timeoutMilliseconds;
                request.KeepAlive = true;

                using (var response = request.GetResponse())
                {
                    using (var responseStream = response.GetResponseStream())
                    {
                        using (var readStream = new StreamReader(responseStream))
                        {
                            contentData = readStream.ReadToEnd();
                            readStream.Close();
                        }
                        responseStream.Close();
                    }
                    response.Close();
                }
            }
            catch (Exception e)
            {
                code = SNK_HTTP_ERROR_CODE.error;
                exception = new System.Exception($"请求{uri}出现异常\n异常信息:{e.Message}\n异常堆栈:{e.StackTrace}");
            }
            finally
            {
                result = new SnkHttpGetResult(code, httpCode, exception, contentData);
            }
            return result;
        }

        /// <summary>
        /// Post方法
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="content"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public static SnkHttpPostResult Post(string uri, string postContent, int timeoutMilliseconds = 10 * 1000)
        {
            uri = uri.FixSlash();
            SnkHttpPostResult result = null;
            var code = SNK_HTTP_ERROR_CODE.succeed;
            var httpCode = HttpStatusCode.OK;
            Exception exception = null;


            byte[] postContentBytes = null;
            Dictionary<string, string> rspHeadDict = null;

            try
            {
                var request = WebRequest.CreateHttp(uri);
                request.Method = "POST";
                request.Timeout = timeoutMilliseconds;
                request.KeepAlive = true;
                request.ContentType = "application/json";

                if (string.IsNullOrEmpty(postContent))
                {
                    postContentBytes = Encoding.UTF8.GetBytes(postContent);
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
                        for (int i = 0; i < response.Headers.Count; i++)
                        {
                            var key = response.Headers.GetKey(i);
                            var value = response.Headers.Get(key);
                            rspHeadDict[key] = value;
                        }
                    }
                    response.Close();
                }
            }
            catch (Exception e)
            {
                code = SNK_HTTP_ERROR_CODE.error;
                exception = new System.Exception($"请求{uri}出现异常\n异常信息:{e.Message}\n异常堆栈:{e.StackTrace}");
            }
            finally
            {
                result = new SnkHttpPostResult(code, httpCode, exception, rspHeadDict, postContentBytes);
            }

            return result;
        }

        public static Task<SnkHttpHeadResult> HeadAsync(string uri, int timeoutMilliseconds = 10 * 1000)
            => Task.Run(() => Head(uri, timeoutMilliseconds));

        public static Task<SnkHttpGetResult> GetAsync(string uri, int timeoutMilliseconds = 10 * 1000)
            => Task.Run(() => Get(uri, timeoutMilliseconds));

        public static Task<SnkHttpPostResult> PostAsync(string uri, string postContent, int timeoutMilliseconds = 10 * 1000)
            => Task.Run(() => Post(uri, postContent, timeoutMilliseconds));
    }
}
