using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using SnkFeatureKit.Patcher;
using SnkFeatureKit.Patcher.Interfaces;

namespace SnkFeatureKit.DownloadTask
{
        public class SnkDownloadTask : ISnkDownloadTask
        {
            public string Name { get; set; }
            public string URL { get; set; }

            public string SavePath { get; set; }

            public long TotalSize { get; set; }

            public long DownloadedSize { get; set; }

            public bool Resume { get; private set; } = false;

            //public SNK_DOWNLOAD_STATUS Status { get; private set; } = SNK_DOWNLOAD_STATUS.none;

            //public SnkHttpDownloadResult DownloadResult { get; private set; } = null;

            public bool IsCompleted { get; private set; } = false;

            private CancellationTokenSource _cancellationTokenSource = null;

            private bool _disposed = false;

            public void CancelDownload()
            {
                _cancellationTokenSource?.Cancel();
            }

            public SnkHttpDownloadResult DownloadResult { get; }

            public void DownloadFile(int buffSize = 65536, bool resume = false)
            {
                var code = SNK_HTTP_ERROR_CODE.succeed;
                var httpCode = HttpStatusCode.OK;
                Exception exception = null;
                FileStream fileStream = null;

                var fileInfo = new FileInfo(this.SavePath);
                if (fileInfo.Exists)
                {
                    if (resume)
                    {
                        fileStream = fileInfo.Open(FileMode.Open, FileAccess.ReadWrite);
                        fileStream.Seek(0, SeekOrigin.End);
                    }
                    else
                        fileInfo.Delete();
                }

                try
                {
                    var request = WebRequest.CreateHttp(URL);
                    request.Method = "GET";

                    if (fileStream != null)
                        request.AddRange(fileStream.Length);

                    using (var response = request.GetResponse())
                    {
                        this.TotalSize = response.ContentLength;
                        var buffer = new byte[buffSize];
                        var len = 0;
                        using (var responseStream = response.GetResponseStream())
                        {
                            if(fileInfo.Directory.Exists == false)
                                fileInfo.Directory.Create();

                            using (fileStream = fileStream ?? fileInfo.Open(FileMode.Create, FileAccess.ReadWrite))
                            {
                                while ((len = responseStream.Read(buffer, 0, buffSize)) > 0)
                                {
                                    if (_cancellationTokenSource != null && _cancellationTokenSource.Token.IsCancellationRequested)
                                    {
                                        code = SNK_HTTP_ERROR_CODE.user_cancel;
                                        break;
                                    }
                                    fileStream.Write(buffer, 0, len);
                                    this.DownloadedSize += len;
                                }
                                fileStream.Flush();
                                fileStream.Close();
                            }
                        }
                    }
                }
                catch (System.Exception e)
                {
                    code = SNK_HTTP_ERROR_CODE.download_error;
                    exception = new Exception($"下载出现异常\n下载地址:{URL}\n错误信息:{e.Message}\n堆栈:{e.StackTrace}", e);
                }
                finally
                {
                    this.ReleaseCancellationTokenSource();
                    //DownloadResult = new SnkHttpDownloadResult(code, httpCode, exception);
                    //_status = SNK_DOWNLOAD_STATUS.completed;
                    IsCompleted = true;
                }
            }

            private void ReleaseCancellationTokenSource()
            {
                if (_cancellationTokenSource != null)
                {
                    _cancellationTokenSource.Cancel();
                    _cancellationTokenSource.Dispose();
                    _cancellationTokenSource = null;
                }
            }

            public Task DownloadFileAsync(int buffSize = 1024 * 64, bool resume = false, CancellationTokenSource cancellationTokenSource = null)
            {
                if(cancellationTokenSource == null)
                    return Task.Run(() => DownloadFile(buffSize, resume));
                
                _cancellationTokenSource = cancellationTokenSource;
                return Task.Run(() => DownloadFile(buffSize, resume), _cancellationTokenSource.Token);
            }

            ~SnkDownloadTask()
            {
                Dispose(false);
            }

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            protected virtual void Dispose(bool disposing)
            {
                if (disposing)
                {
                    if (_disposed == false)
                    {
                        _disposed = true;

                        this.ReleaseCancellationTokenSource();
                        URL = string.Empty;
                        SavePath = string.Empty;
                        TotalSize = 0L;
                        DownloadedSize = 0L;
                        Resume = false;
                        IsCompleted = false;
                        //DownloadResult = null;
                    }
                }
            }
        }

}