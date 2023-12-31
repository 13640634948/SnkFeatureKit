﻿using System.Threading;
using System.Threading.Tasks;

namespace SnkFeatureKit.Patcher
{
    namespace Interfaces
    {
        /// <summary>
        /// 下载任务接口
        /// </summary>
        public interface ISnkDownloadTask : System.IDisposable
        {
            SNK_DOWNLOAD_STATE State { get; }
            string Name { get; set; }

            /// <summary>
            /// 下载地址
            /// </summary>
            string Url { get;set; }

            /// <summary>
            /// 保存地址
            /// </summary>
            string SavePath { get; set;}

            /// <summary>
            /// 文件总大小
            /// </summary>
            /// <returns></returns>
            long TotalSize { get; }

            /// <summary>
            /// 已下载大小
            /// </summary>
            /// <returns></returns>
            long DownloadedSize { get; }

            /// <summary>
            /// 异常
            /// </summary>
            System.Exception DownloadException { get; }

            /// <summary>
            /// 下载文件
            /// </summary>
            /// <param name="buffSize"></param>
            /// <param name="resume"></param>
            /// <returns></returns>
            void DownloadFile(int buffSize = 1024 * 64, bool resume = false);

            /// <summary>
            /// 下载文件（异步）
            /// </summary>
            /// <param name="buffSize">下载缓存大小（默认：1024 * 64）</param>
            /// <param name="resume">断点继传</param>
            /// <returns></returns>
            Task DownloadFileAsync(int buffSize = 1024 * 64, bool resume = false);

            /// <summary>
            /// 取消下载
            /// </summary>
            void Cancel();

            void Pause();
            
            void Resume(); 
        }
    }
}