using System.Collections.Generic;
using System.Net;
using System;

namespace SnkFeatureKit.Patcher
{
    /// <summary>
    /// http结果
    /// </summary>
    public class SnkHttpResult
    {
        /// <summary>
        /// 是否错误
        /// </summary>
        public bool IsError => Code != SNK_HTTP_ERROR_CODE.succeed;

        /// <summary>
        /// 错误信息
        /// </summary>
        public System.Exception Exception;

        /// <summary>
        /// 异常信息
        /// </summary>
        public SNK_HTTP_ERROR_CODE Code { get; }

        /// <summary>
        /// http状态吗
        /// </summary>
        public HttpStatusCode HttpStatusCode { get; }

        /// <summary>
        /// 错误信息
        /// </summary>
        /// <param name="code"></param>
        /// <param name="statusCode"></param>
        /// <param name="errorMessage"></param>
        public SnkHttpResult(SNK_HTTP_ERROR_CODE code, HttpStatusCode statusCode, Exception exception)
        {
            Code = code;
            HttpStatusCode = statusCode;
            Exception = exception;
        }
    }

    /// <summary>
    /// Head结果
    /// </summary>
    public class SnkHttpHeadResult : SnkHttpResult
    {
        /// <summary>
        /// 返回的长度
        /// </summary>
        public long Length { get; }

        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="code"></param>
        /// <param name="statusCode"></param>
        /// <param name="errorMessage"></param>
        /// <param name="length"></param>
        public SnkHttpHeadResult(SNK_HTTP_ERROR_CODE code, HttpStatusCode statusCode, Exception exception, long length) : base(code, statusCode, exception)
        {
            Length = length;
        }
    }

    /// <summary>
    /// Get结果
    /// </summary>
    public class SnkHttpGetResult : SnkHttpResult
    {
        /// <summary>
        /// 返回的数据
        /// </summary>
        public string ContentData { get; }

        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="code"></param>
        /// <param name="statusCode"></param>
        /// <param name="errorMessage"></param>
        /// <param name="data"></param>
        public SnkHttpGetResult(SNK_HTTP_ERROR_CODE code, HttpStatusCode statusCode, Exception exception, string contentData) : base(code, statusCode, exception)
        {
            ContentData = contentData;
        }
    }

    /// <summary>
    /// post结果
    /// </summary>
    public class SnkHttpPostResult : SnkHttpResult
    {
        /// <summary>
        /// Head字典
        /// </summary>
        public Dictionary<string, string> HeadDict { get; }

        /// <summary>
        /// 返回内容
        /// </summary>
        public byte[] Data { get; }

        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="code"></param>
        /// <param name="statusCode"></param>
        /// <param name="errorMessage"></param>
        /// <param name="headDict"></param>
        /// <param name="data"></param>
        public SnkHttpPostResult(SNK_HTTP_ERROR_CODE code, HttpStatusCode statusCode, Exception exception, Dictionary<string, string> headDict, byte[] data) : base(code, statusCode, exception)
        {
            HeadDict = headDict;
            Data = data;
        }
    }

    /// <summary>
    /// 下载结果
    /// </summary>
    public class SnkHttpDownloadResult : SnkHttpResult
    {
        /// <summary>
        /// 是否是取消下载
        /// </summary>
        public bool IsCancelDownload => Exception is OperationCanceledException;
        public SnkHttpDownloadResult(SNK_HTTP_ERROR_CODE code, HttpStatusCode statusCode, Exception exception) : base(code, statusCode, exception)
        {

        }
    }
}
