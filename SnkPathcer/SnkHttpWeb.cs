using SnkFeatureKit.Patcher.Extension;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;

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
        public static async Task<SnkHttpHeadResult> Head(string uri, TimeSpan timeout = default)
        {
            uri = uri.FixSlash();
            return await Task.Run(() =>
            {
                SnkHttpHeadResult result = default;
                var code = SNK_HTTP_ERROR_CODE.succeed;
                var httpCode = HttpStatusCode.OK;
                Exception exception = null;
                var length = 0L;
                try
                {
                    var httpClient = new HttpClient();
                    if (timeout != default)
                    {
                        httpClient.Timeout = timeout;
                    }

                    using (var hrm = new HttpRequestMessage(HttpMethod.Head, uri))
                    {
                        using (var rsp = httpClient.SendAsync(hrm).Result)
                        {
                            httpCode = rsp.StatusCode;
                            rsp.EnsureSuccessStatusCode();
                            if (rsp.Content != null && rsp.Content.Headers != null)
                            {
                                length = (long)rsp.Content.Headers.ContentLength;
                            }
                            else
                            {
                                code = SNK_HTTP_ERROR_CODE.can_not_find_length;
                                exception = new System.Exception($"服务器没有返回content-length,无法获取文件长度\n访问地址:{uri}");
                            }
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
            });
        }

        /// <summary>
        /// Get方法
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public static async Task<SnkHttpGetResult> Get(string uri, TimeSpan timeout = default)
        {
            uri = uri.FixSlash();
            return await Task.Run(() =>
            {
                SnkHttpGetResult result = default;
                var code = SNK_HTTP_ERROR_CODE.succeed;
                var httpCode = HttpStatusCode.OK;
                Exception exception = null;
                byte[] data = null;
                //Dictionary<string, string> headDict = null;
                try
                {
                    var httpClient = new HttpClient();
                    if (timeout != default)
                    {
                        httpClient.Timeout = timeout;
                    }

                    using (var hrm = new HttpRequestMessage(HttpMethod.Get, uri))
                    {
                        using (var rsp = httpClient.SendAsync(hrm).Result)
                        {
                            httpCode = rsp.StatusCode;
                            rsp.EnsureSuccessStatusCode();
                            rsp.Headers.GetEnumerator();
                            data = rsp.Content.ReadAsByteArrayAsync().Result;
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
                    result = new SnkHttpGetResult(code, httpCode, exception, data);
                }

                return result;
            });
        }

        /// <summary>
        /// Post方法
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="content"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public static async Task<SnkHttpPostResult> Post(string uri, HttpContent content,
            CancellationTokenSource cts)
        {
            uri = uri.FixSlash();
            SnkHttpPostResult result = default;
            var code = SNK_HTTP_ERROR_CODE.succeed;
            var httpCode = HttpStatusCode.OK;
            Exception exception = null;

            Dictionary<string, string> rspHeadDict = null;
            byte[] data = null;
            HttpRequestMessage hrm = null;
            HttpResponseMessage rsp = null;

            try
            {
                var httpClient = new HttpClient();
                hrm = new HttpRequestMessage(HttpMethod.Post, uri);
                if (content != null)
                    hrm.Content = content;
                rsp = await httpClient.SendAsync(hrm, cts.Token);

                httpCode = rsp.StatusCode;
                rsp.EnsureSuccessStatusCode();
                if (rsp.Headers != null)
                {
                    foreach (var kv in rsp.Headers)
                    {
                        if (rspHeadDict == null)
                        {
                            rspHeadDict = new Dictionary<string, string>();
                        }

                        rspHeadDict.Add(kv.Key, kv.Value.ToString());
                    }
                }

                data = await rsp.Content.ReadAsByteArrayAsync();
            }
            catch (Exception e)
            {
                code = SNK_HTTP_ERROR_CODE.error;
                exception = new System.Exception($"请求{uri}出现异常\n异常信息:{e.Message}\n异常堆栈:{e.StackTrace}");
            }
            finally
            {
                hrm?.Dispose();
                rsp?.Dispose();
                result = new SnkHttpPostResult(code, httpCode, exception, rspHeadDict, data);
            }

            return result;
        }
    }
}
